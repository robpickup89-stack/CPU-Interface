// Form1.cs - Improved UI with fullscreen support and detection flag parsing
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO.Ports;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace CPU_Interface
{
    public partial class Form1 : Form
    {
        private SerialPort serialPort1;   // 9600 baud (PTC1)
        private SerialPort serialPort2;   // 1200 baud (Siemens)

        private readonly StringBuilder rxBuffer = new StringBuilder();
        private readonly List<string> allReceivedLines = new List<string>();

        // FFS (Fault log) mode state
        private bool ffsMode = false;
        private bool flfMode = false;
        private System.Windows.Forms.Timer? ffsTimer;
        private string? flfStartEntry = null;
        private int flfEntryCount = 0;

        // Siemens init sequence state
        private bool siemensInitActive = false;
        private int siemensInitCount = 0;
        private const int MaxSiemensInitCommands = 10;

        // Fullscreen state
        private bool isFullscreen = false;
        private FormWindowState previousWindowState;
        private FormBorderStyle previousBorderStyle;

        // Detection flag patterns
        private static readonly Regex DetectorFlagRegex = new Regex(
            @"DET\.\s+(\S+)\s+([FONS\-]{5})",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // FFS fault log pattern: FFS [FLF_NUMBER]:[255] [DESCRIPTION]
        private static readonly Regex FfsFaultRegex = new Regex(
            @"^FFS\s+(\d+):(\d+)\s+(.+)$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // ==============================
        // CPU FAULT DICTIONARY
        // ==============================
        private readonly Dictionary<string, string> cpuMessages = new Dictionary<string, string>()
        {
            { "000:NO LAMP POWER.MDU", "POWERFAIL: Mains power failure detected. Check mains supply, fuses, PSU." },
            { "001:LOW LAMP VOLTAGE.MDU", "MAINS LOW: Supply voltage below threshold. Check incoming voltage." },
            { "002:CPU-B CRC CODE", "CPUB Firmware CRC Error. Possible CPU-B HW/firmware fault." },
            { "003:CPU-B CRC DATA", "CPUB Configuration CRC Error. Possible CPU-B HW/firmware fault." },
            { "004:CPU-B LCM", "LCM internal error detected." },
            { "005:CPU-B OVERLOAD", "CPU-B insufficient processing power." },
            { "006:CPU-B LCM DIAG (F)", "Comms issue between CPU-B and LCM." },
            { "007:CPU-B LOT SLOW", "Timeout on LOT bits." },
            { "008:CPU-A ALARM", "CPUA requested alarm." },
            { "009:XP ALARM", "Stream alarm detected." },
            { "010:USER ALARM", "LCM units reporting no mains supply." },
            { "011:NO MAINS SYNC.MDU", "MAINS SYNC error detected. Check mains quality / sync circuit." },
            { "013:LOC-LOC", "CPUA and LCM LOC mismatch." },
            { "014:CPU-B COM CPU-A", "No comms between CPUB and CPUA." },
            { "020:LCM CONF ERR (1) (F)", "Failed to configure LCM units." },
            { "021:LCM CONF ERR (F)", "Failed to configure LCM units." },
            { "025:CPU-A COM CPU-B", "CPUA has no communication with CPUB." },
            { "028:CPU-A (EXT)", "CPUA not receiving BMAINS interrupts." },
            { "052:DEADLOCK SG", "Deadlock detected on signal group." },
            { "080:OTU # - # #", "OTU bit stuck at STREAM - STAGE ERROR/OK." },
            { "081:PHASE INH # #", "Phase inhibit - likely lamp failure or inhibit condition." },
            { "099:LOGBOOK CLEARED", "Fault log cleared - user action." },
            { "100:CPU-A XLM name", "Lamp configuration mismatch (CPUA/B)." },
            { "110:PARAM LOG 1", "Parameter log overflow - too many changes." },
            { "151:BATTERY ERROR", "Battery backup error - recharge or replace battery/CPU card." },
            { "190:CPU-B DOWNLOAD STARTED", "CPUA-B download started - user action." },
            { "191:CPU-B DOWNLOAD FAILED", "CPUA-B download failed - user action." },
            { "192:CPU-B DOWNLOAD OK", "CPUA-B download OK - user action." },
            { "193:DIMMING FAIL", "Dimming failed - possible MDU fault." },
            { "194:DIMMING OK", "Dimming OK - issue cleared." },
            { "195:MANSTEP ACTIVE", "Manual step mode CLF active - user action." },
            { "196:MANSTEP INACTIVE", "Manual step mode CLF inactive - user action." },
            { "197:MANSTEP FAIL", "Manual step mode CLF failed - user action." },
            { "201:BATTERY OK", "Battery backup OK - power restored." },
            { "202:RESET ALARMS", "Manual reset - alarms reset by operator." },
            { "203:DET. UNIT OK", "Detector OK - behaving correctly." },
            { "204:RESET DETAL", "Detector log reset - user action." },
            { "205:PT VEH. LOST", "PSV ignored - tracking buffer overflow." },
            { "206:LAMP AUTOSET", "Lamp monitoring auto-calibration started." },
            { "208:LAMP AUTOSET ERR", "Lamp auto-calibration failed - minimum load not detected." },
            { "212:TIME MASTER ERROR", "Time updated via XKOP - configuration error." },
            { "213:AUTO-RESET", "Application restarted - user/system action." },
            { "214:AUTO-RESET LOCK", "Max restarts reached - config may be corrupt." },
            { "215:LAMP AUTOSET OK", "Lamp auto-calibration complete." },
            { "217:MIMIC MODE ON / MIMIC MODE OFF", "Mimic mode started/exited - user action." },
            { "218:CPU-A/B ID # #", "CPU A & B ID mismatch - shutdown." },
            { "219:UG405 LICENCE ERROR", "UG405 licence missing/invalid." },
            { "238:DOOR #", "Cabinet door open." },
            { "239:DOOR CP #", "Control panel door open." },
            { "240:ENERGY COMP. #", "Generator compartment door open." },
            { "241:TRY", "Reset 2nd lamp failures - user action." },
            { "242:ZAP", "Configuration value restored - user action." },
            { "243:WATCHDOG #", "Task watchdog timeout - check configuration/load." },
            { "245:SOFT RESET", "Soft reset - controller resets to safe state." },
            { "246:RESET PROG PARM", "Program slot parameter reset - user action." },
            { "247:PROGRAM #", "Program slot active - reports configuration number." },
            { "248:RESET PROGRAM", "Reset program - user action." },
            { "249:RESET CPU", "Reset CPU - button/terminal action." },
            { "250:MAINS OFF", "Mains supply failed - power supply problem." },
            { "251:OLD TIME", "Old time - previous controller time." },
            { "252:NEW TIME", "New time - TOD updated." },
            { "253:MAINS ON", "System restart - mains restored." },
            { "255:FATAL ALARM", "Fatal fault reported - check other fault entries." },

            // FLF CODES
            { "FLF:0",  "Not used - No action required." },
            { "FLF:1",  "Conflict configuration error - Reload configuration and recheck conflict matrix." },
            { "FLF:2",  "Phase Bus Safety Fault (PBUS) - Signals shut down. Power cycle; replace CPU/cards if repeats." },
            { "FLF:3",  "Correspondence Fault (CORR) - Outputs not matching command. Check flash/relays/wiring; reset." },
            { "FLF:4",  "Relay Test Failed (RLAY) - Signals won't start. Check/replace relay hardware." },
            { "FLF:5",  "Conflict Fault (CFT) - Conflicting greens detected. Check wiring/outputs/config." },
            { "FLF:6",  "Power supply fault - Check PSU, fuses, internal voltages." },
            { "FLF:7",  "Over temperature - Check ventilation, fans, cabinet heat." },
            { "FLF:8",  "Watchdog trip (WDOG) - Power cycle. Replace CPU if recurring." },
            { "FLF:9",  "Clock fault - Reset time / check battery." },
            { "FLF:10", "Internal comms fault - Power cycle controller." },
            { "FLF:11", "Memory fault (MEM) - Power cycle or replace CPU." },
            { "FLF:12", "Detector fault monitor (DFM) - Check loops/cards/wiring." },
            { "FLF:13", "I/O board fault - Check expansion cards match configuration." },
            { "FLF:14", "CPU communication fault - Reseat/replace CPU; power cycle." },
            { "FLF:15", "Configuration CRC error - Reload known good config." },
            { "FLF:16", "Non-volatile memory fault - Replace CPU board." },
            { "FLF:17", "Lamp supply fault - Check PSU/fuses/mains." },
            { "FLF:18", "Ped detector fault - Check push button/wiring." },
            { "FLF:19", "I/O failure - Check wiring and I/O cards." },
            { "FLF:20", "Configuration error - Reload and recommission." },
            { "FLF:21", "Initialisation failure - Power cycle; check hardware vs config." },
            { "FLF:22", "Red lamp failure (RLM) - Check red aspect/LED module/wiring." },
            { "FLF:23", "Amber lamp failure - Check amber aspect/LED module/wiring." },
            { "FLF:24", "Green lamp failure - Check green aspect/LED module/wiring." },
            { "FLF:25", "CPU link fault - Power cycle; replace CPU if repeats." },
            { "FLF:26", "Timing error - Reload config; check timings." },
            { "FLF:27", "Time-of-day fault - Check timetable/clock settings." },
            { "FLF:28", "Mains sync fault - Check supply quality / sync circuits." },
            { "FLF:29", "Flash mode fault - Check flash transitions/outputs/relays." },
            { "FLF:30", "Manual control active - Return to auto." },
            { "FLF:31", "Cabinet door open - Close door / check switch." },
            { "FLF:32", "Panel open - Close panel / check switch." },
            { "FLF:33", "Lamp output load fault - Check wiring / LED type." },
            { "FLF:34", "Fuse failure - Check signal/PSU fuses." },
            { "FLF:35", "Battery fault - Replace backup battery." },
            { "FLF:36", "EEPROM write error - Settings not saving; replace CPU." },
            { "FLF:37", "Aux supply fault - Check auxiliary PSU outputs." },
            { "FLF:38", "External comms fault - Check OTU/UTC comms equipment." },
            { "FLF:39", "OTU fault - Check OTU power/comms." },
            { "FLF:40", "Invalid mode - Reset or reload config." },
            { "FLF:41", "Detector config error - Check detector setup/config." },
            { "FLF:42", "Input fault - Check wiring/inputs." },
            { "FLF:43", "Output fault - Check driver card/output channel." },
            { "FLF:44", "Stage fault - Check demands/config." },
            { "FLF:45", "Stream fault - Check stream config." },
            { "FLF:46", "Lamp config error - Check KLT/load settings; reload." },
            { "FLF:47", "PSU monitoring fault - Check voltages/feedback." },
            { "FLF:48", "CPU over temp - Improve cooling / replace CPU." },
            { "FLF:49", "RAM test failure - Replace CPU." },
            { "FLF:50", "Conflict data logged - Review details before reset." },
            { "FLF:51", "Fault log full - Clear after investigation." },
            { "FLF:52", "Deadlock detected - Check demands/config." },
            { "FLF:53", "Detector load monitor fault - Check detector wiring/card." },
            { "FLF:54", "Input data error - Power cycle." },
            { "FLF:55", "Lamp monitor fault - Check lamps/loads." },
            { "FLF:56", "Lamp monitor update required - Run KLR reset." },
            { "FLF:57", "Lamp monitor reset failed - Retry with healthy lamps." },
            { "FLF:58", "Integral OTU fault - Check or disable if not fitted." },
            { "FLF:59", "Integral IMU fault - Check or disable if not fitted." },
            { "FLF:60", "Redundant PSU fault - Check backup supply." },
            { "FLF:61", "System warning - Review log." },
            { "FLF:62", "Non-FLF fault present - Use WIZ/web fault table." },

            // TEXTUAL ERROR CODES
            { "LCM-1-255 ERR", "LCM unit failed or missing." },
            { "IO1616-1 ERR", "IO16 Card 1 error - failed/missing. Check addressing." },
            { "IO1616-2 ERR", "IO16 Card 2 error - failed/missing. Check addressing." },
            { "IO1616-3 ERR", "IO16 Card 3 error - failed/missing. Check addressing." },
            { "IO1616-4 ERR", "IO16 Card 4 error - failed/missing. Check addressing." },
            { "IO1616-5 ERR", "IO16 Card 5 error - failed/missing. Check addressing." },
            { "IO1616-6 ERR", "IO16 Card 6 error - failed/missing. Check addressing." },
            { "IO1616-7 ERR", "IO16 Card 7 error - failed/missing. Check addressing." },
            { "MP-1 ERR", "Manual panel error - failed or missing." },
            { "level 3", "Level 3 not pressed on CPU." },
            { "LAMP name ERR", "Lamp failure - last lamp failure / broken lamp." },
            { "02:FATAL ALARM", "Fatal fault detected by CPUB - check associated faults." },
            { "03:SG MIN TIMER", "Signal group changed state too quickly - possible CPU-A fault." },
            { "11:OMS-ERR", "Signal on when not required - check wiring." },
            { ".04:SG MAX TIMER", "Signal group kept state too long - check CPU reset/events." },
            { ".05:SG SEQUENCE", "Illegal state transition - check wiring." },
            { ".06:SG CONFLICT", "Conflicting signal groups - check wiring/config." },
            { ".08:LOV-LOV", "Voltage measurement circuits disagree - faulty LCM." },
            { ".09:HALF-WAVING", "Half-waving detected - faulty LCM/output switch." },
            { "12:LAMP ERR", "Lamp failure - broken lamp." },
            { "IOTU-1 ERR", "UG405 issue - check OTU configuration." },
            { "MTS4BP-1 ERR", "Backplane issue - check addressing." },
            { "SWOF XP", "Stream switch off - stage fault detected." },

            // OMS ERRORS
            {"OMS ERR G01 (0)", "A Phase green - Unknown. Fuse blown, Triac failure, Voltage sensor failure" },
            {"OMS ERR G01 (1)", "A Phase green - illegal ON. Fuse blown, Triac failure, Voltage sensor failure" },
            {"OMS ERR G01 (2)", "A Phase green - illegal OFF. Fuse blown, Triac failure, Voltage sensor failure" },
            {"OMS ERR G01 (3)", "A Phase green - Half Waving. Fuse blown, Triac failure, Voltage sensor failure" },
            {"OMS ERR G01 (4)", "A Phase green - Green Non Equivalence. Fuse blown, Triac failure, Voltage sensor failure" },
            {"OMS ERR G01 (5)", "A Phase green - LOC/LOC Difference. Fuse blown, Triac failure, Voltage sensor failure" },
            {"OMS ERR G02 (0)", "B Phase green - Unknown. Fuse blown, Triac failure, Voltage sensor failure"},
            {"OMS ERR G02 (1)", "B Phase green - illegal ON. Fuse blown, Triac failure, Voltage sensor failure"},
            {"OMS ERR G02 (2)", "B Phase green - illegal OFF. Fuse blown, Triac failure, Voltage sensor failure"},
            {"OMS ERR G02 (3)", "B Phase green - Half Waving. Fuse blown, Triac failure, Voltage sensor failure"},
            {"OMS ERR G02 (4)", "B Phase green - Green Non Equivalence. Fuse blown, Triac failure, Voltage sensor failure"},
            {"OMS ERR G02 (5)", "B Phase green - LOC/LOC Difference. Fuse blown, Triac failure, Voltage sensor failure"},
            {"OMS ERR G03 (0)", "C Phase green - Unknown. Fuse blown, Triac failure, Voltage sensor failure"},
            {"OMS ERR G03 (1)", "C Phase green - illegal ON. Fuse blown, Triac failure, Voltage sensor failure"},
            {"OMS ERR G03 (2)", "C Phase green - illegal OFF. Fuse blown, Triac failure, Voltage sensor failure"},
            {"OMS ERR G03 (3)", "C Phase green - Half Waving. Fuse blown, Triac failure, Voltage sensor failure"},
            {"OMS ERR G03 (4)", "C Phase green - Green Non Equivalence. Fuse blown, Triac failure, Voltage sensor failure"},
            {"OMS ERR G03 (5)", "C Phase green - LOC/LOC Difference. Fuse blown, Triac failure, Voltage sensor failure"},
            {"OMS ERR G04 (0)", "D Phase green - Unknown. Fuse blown, Triac failure, Voltage sensor failure"},
            {"OMS ERR G04 (1)", "D Phase green - illegal ON. Fuse blown, Triac failure, Voltage sensor failure"},
            {"OMS ERR G04 (2)", "D Phase green - illegal OFF. Fuse blown, Triac failure, Voltage sensor failure"},
            {"OMS ERR G04 (3)", "D Phase green - Half Waving. Fuse blown, Triac failure, Voltage sensor failure"},
            {"OMS ERR G04 (4)", "D Phase green - Green Non Equivalence. Fuse blown, Triac failure, Voltage sensor failure"},
            {"OMS ERR G04 (5)", "D Phase green - LOC/LOC Difference. Fuse blown, Triac failure, Voltage sensor failure"},
            {"OMS ERR G05 (0)", "E Phase green - Unknown. Fuse blown, Triac failure, Voltage sensor failure"},
            {"OMS ERR G05 (1)", "E Phase green - illegal ON. Fuse blown, Triac failure, Voltage sensor failure"},
            {"OMS ERR G05 (2)", "E Phase green - illegal OFF. Fuse blown, Triac failure, Voltage sensor failure"},
            {"OMS ERR G05 (3)", "E Phase green - Half Waving. Fuse blown, Triac failure, Voltage sensor failure"},
            {"OMS ERR G05 (4)", "E Phase green - Green Non Equivalence. Fuse blown, Triac failure, Voltage sensor failure"},
            {"OMS ERR G05 (5)", "E Phase green - LOC/LOC Difference. Fuse blown, Triac failure, Voltage sensor failure"},
            {"OMS ERR G06 (0)", "F Phase green - Unknown. Fuse blown, Triac failure, Voltage sensor failure"},
            {"OMS ERR G06 (1)", "F Phase green - illegal ON. Fuse blown, Triac failure, Voltage sensor failure"},
            {"OMS ERR G06 (2)", "F Phase green - illegal OFF. Fuse blown, Triac failure, Voltage sensor failure"},
            {"OMS ERR G06 (3)", "F Phase green - Half Waving. Fuse blown, Triac failure, Voltage sensor failure"},
            {"OMS ERR G06 (4)", "F Phase green - Green Non Equivalence. Fuse blown, Triac failure, Voltage sensor failure"},
            {"OMS ERR G06 (5)", "F Phase green - LOC/LOC Difference. Fuse blown, Triac failure, Voltage sensor failure"},
            {"OMS ERR G07 (0)", "G Phase green - Unknown. Fuse blown, Triac failure, Voltage sensor failure"},
            {"OMS ERR G07 (1)", "G Phase green - illegal ON. Fuse blown, Triac failure, Voltage sensor failure"},
            {"OMS ERR G07 (2)", "G Phase green - illegal OFF. Fuse blown, Triac failure, Voltage sensor failure"},
            {"OMS ERR G07 (3)", "G Phase green - Half Waving. Fuse blown, Triac failure, Voltage sensor failure"},
            {"OMS ERR G07 (4)", "G Phase green - Green Non Equivalence. Fuse blown, Triac failure, Voltage sensor failure"},
            {"OMS ERR G07 (5)", "G Phase green - LOC/LOC Difference. Fuse blown, Triac failure, Voltage sensor failure"},
            {"OMS ERR G08 (0)", "H Phase green - Unknown. Fuse blown, Triac failure, Voltage sensor failure"},
            {"OMS ERR G08 (1)", "H Phase green - illegal ON. Fuse blown, Triac failure, Voltage sensor failure"},
            {"OMS ERR G08 (2)", "H Phase green - illegal OFF. Fuse blown, Triac failure, Voltage sensor failure"},
            {"OMS ERR G08 (3)", "H Phase green - Half Waving. Fuse blown, Triac failure, Voltage sensor failure"},
            {"OMS ERR G08 (4)", "H Phase green - Green Non Equivalence. Fuse blown, Triac failure, Voltage sensor failure"},
            {"OMS ERR G08 (5)", "H Phase green - LOC/LOC Difference. Fuse blown, Triac failure, Voltage sensor failure"},
            {"OMS ERR G09 (0)", "I Phase green - Unknown. Fuse blown, Triac failure, Voltage sensor failure"},
            {"OMS ERR G09 (1)", "I Phase green - illegal ON. Fuse blown, Triac failure, Voltage sensor failure"},
            {"OMS ERR G09 (2)", "I Phase green - illegal OFF. Fuse blown, Triac failure, Voltage sensor failure"},
            {"OMS ERR G09 (3)", "I Phase green - Half Waving. Fuse blown, Triac failure, Voltage sensor failure"},
            {"OMS ERR G09 (4)", "I Phase green - Green Non Equivalence. Fuse blown, Triac failure, Voltage sensor failure"},
            {"OMS ERR G09 (5)", "I Phase green - LOC/LOC Difference. Fuse blown, Triac failure, Voltage sensor failure"},
            {"OMS ERR G10 (0)", "J Phase green - Unknown. Fuse blown, Triac failure, Voltage sensor failure"},
            {"OMS ERR G10 (1)", "J Phase green - illegal ON. Fuse blown, Triac failure, Voltage sensor failure"},
            {"OMS ERR G10 (2)", "J Phase green - illegal OFF. Fuse blown, Triac failure, Voltage sensor failure"},
            {"OMS ERR G10 (3)", "J Phase green - Half Waving. Fuse blown, Triac failure, Voltage sensor failure"},
            {"OMS ERR G10 (4)", "J Phase green - Green Non Equivalence. Fuse blown, Triac failure, Voltage sensor failure"},
            {"OMS ERR G10 (5)", "J Phase green - LOC/LOC Difference. Fuse blown, Triac failure, Voltage sensor failure"},
            {"OMS ERR G11 (0)", "K Phase green - Unknown. Fuse blown, Triac failure, Voltage sensor failure"},
            {"OMS ERR G11 (1)", "K Phase green - illegal ON. Fuse blown, Triac failure, Voltage sensor failure"},
            {"OMS ERR G11 (2)", "K Phase green - illegal OFF. Fuse blown, Triac failure, Voltage sensor failure"},
            {"OMS ERR G11 (3)", "K Phase green - Half Waving. Fuse blown, Triac failure, Voltage sensor failure"},
            {"OMS ERR G11 (4)", "K Phase green - Green Non Equivalence. Fuse blown, Triac failure, Voltage sensor failure"},
            {"OMS ERR G11 (5)", "K Phase green - LOC/LOC Difference. Fuse blown, Triac failure, Voltage sensor failure"},
            {"OMS ERR G12 (0)", "L Phase green - Unknown. Fuse blown, Triac failure, Voltage sensor failure"},
            {"OMS ERR G12 (1)", "L Phase green - illegal ON. Fuse blown, Triac failure, Voltage sensor failure"},
            {"OMS ERR G12 (2)", "L Phase green - illegal OFF. Fuse blown, Triac failure, Voltage sensor failure"},
            {"OMS ERR G12 (3)", "L Phase green - Half Waving. Fuse blown, Triac failure, Voltage sensor failure"},
            {"OMS ERR G12 (4)", "L Phase green - Green Non Equivalence. Fuse blown, Triac failure, Voltage sensor failure"},
            {"OMS ERR G12 (5)", "L Phase green - LOC/LOC Difference. Fuse blown, Triac failure, Voltage sensor failure"},
            {"OMS ERR G13 (0)", "M Phase green - Unknown. Fuse blown, Triac failure, Voltage sensor failure"},
            {"OMS ERR G13 (1)", "M Phase green - illegal ON. Fuse blown, Triac failure, Voltage sensor failure"},
            {"OMS ERR G13 (2)", "M Phase green - illegal OFF. Fuse blown, Triac failure, Voltage sensor failure"},
            {"OMS ERR G13 (3)", "M Phase green - Half Waving. Fuse blown, Triac failure, Voltage sensor failure"},
            {"OMS ERR G13 (4)", "M Phase green - Green Non Equivalence. Fuse blown, Triac failure, Voltage sensor failure"},
            {"OMS ERR G13 (5)", "M Phase green - LOC/LOC Difference. Fuse blown, Triac failure, Voltage sensor failure"},
        };

        // ==============================
        // CONSTRUCTOR
        // ==============================
        public Form1()
        {
            InitializeComponent();

            // Initialize COM port dropdown
            comboBoxPTC1.Items.Clear();
            for (int i = 1; i <= 20; i++)
                comboBoxPTC1.Items.Add("COM" + i);
            comboBoxPTC1.SelectedIndex = 0;

            // Wire up keyboard events
            textBox2.KeyDown += textBox2_KeyDown;
            KeyDown += Form1_KeyDown;

            // SERIAL PORT 1 (9600)
            serialPort1 = new SerialPort
            {
                BaudRate = 9600,
                DataBits = 7,
                Parity = Parity.Even,
                StopBits = StopBits.One,
                Handshake = Handshake.None,
                Encoding = Encoding.ASCII
            };
            serialPort1.DataReceived += SerialPort1_DataReceived;

            // SERIAL PORT 2 (1200) - Siemens
            // Settings: 1200 baud, 7 data bits, Even parity, 1 stop bit, No flow control
            // DTR enabled for proper handshaking with Siemens controllers
            serialPort2 = new SerialPort
            {
                BaudRate = 1200,
                DataBits = 7,
                Parity = Parity.Even,
                StopBits = StopBits.One,
                Handshake = Handshake.None,
                Encoding = Encoding.ASCII,
                DtrEnable = true,
                RtsEnable = true
            };
            serialPort2.DataReceived += SerialPort2_DataReceived;

            // Store initial window state for fullscreen toggle
            previousWindowState = WindowState;
            previousBorderStyle = FormBorderStyle;

            // Initialize FFS timer for auto-sending "+" during fault log retrieval
            ffsTimer = new System.Windows.Forms.Timer();
            ffsTimer.Interval = 500; // 500ms delay between "+" sends
            ffsTimer.Tick += FfsTimer_Tick;

            // Set 50/50 split on load
            Load += Form1_Load;
        }

        private void Form1_Load(object? sender, EventArgs e)
        {
            // Set splitter to 50% of container width for equal split
            mainSplitContainer.SplitterDistance = mainSplitContainer.Width / 2;
        }

        // ==============================
        // FULLSCREEN TOGGLE (F11)
        // ==============================
        private void Form1_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F11)
            {
                ToggleFullscreen();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Escape && isFullscreen)
            {
                ToggleFullscreen();
                e.Handled = true;
            }
            // Shift++ sends "+" command when connected to Siemens
            else if (e.Shift && (e.KeyCode == Keys.Oemplus || e.KeyCode == Keys.Add))
            {
                if (serialPort2 != null && serialPort2.IsOpen)
                {
                    serialPort2.Write("+\r\n");
                    textBox1.AppendText($"> [{serialPort2.PortName}] +{Environment.NewLine}");
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
            }
        }

        private void ToggleFullscreen()
        {
            if (!isFullscreen)
            {
                // Enter fullscreen
                previousWindowState = WindowState;
                previousBorderStyle = FormBorderStyle;

                WindowState = FormWindowState.Normal;
                FormBorderStyle = FormBorderStyle.None;
                WindowState = FormWindowState.Maximized;

                btnFullscreen.Text = "Exit (F11)";
                statusLabelFullscreen.Text = "Press F11 or ESC to exit fullscreen";
                isFullscreen = true;
            }
            else
            {
                // Exit fullscreen
                FormBorderStyle = previousBorderStyle;
                WindowState = previousWindowState;

                btnFullscreen.Text = "Fullscreen";
                statusLabelFullscreen.Text = "Press F11 for fullscreen";
                isFullscreen = false;
            }
        }

        private void btnFullscreen_Click(object? sender, EventArgs e)
        {
            ToggleFullscreen();
        }

        // ==============================
        // FFS/FLF TIMER (Auto-send "+" for fault log)
        // ==============================
        private void FfsTimer_Tick(object? sender, EventArgs e)
        {
            bool shouldSend = false;

            // Check if FFS auto-scroll is enabled and active
            if (ffsMode && chkFfsAutoScroll != null && chkFfsAutoScroll.Checked)
            {
                shouldSend = true;
            }
            // Check if FLF auto-scroll is enabled and active
            else if (flfMode && chkFlfAutoScroll != null && chkFlfAutoScroll.Checked)
            {
                shouldSend = true;
            }

            if (shouldSend && serialPort2 != null && serialPort2.IsOpen)
            {
                serialPort2.Write("+\r\n");
                textBox1.AppendText($"> [{serialPort2.PortName}] +{Environment.NewLine}");
            }
            ffsTimer?.Stop();
        }

        // ==============================
        // SIEMENS INITIAL CONNECT SEQUENCE
        // ==============================
        private async void SendSiemensInitSequence()
        {
            if (serialPort2 == null || !serialPort2.IsOpen) return;

            siemensInitActive = true;
            siemensInitCount = 0;

            // Send "f" characters to wake up the Siemens controller
            // Stop when: message received OR MaxSiemensInitCommands reached
            while (siemensInitActive && siemensInitCount < MaxSiemensInitCommands)
            {
                await System.Threading.Tasks.Task.Delay(300);
                try
                {
                    if (serialPort2.IsOpen && siemensInitActive)
                    {
                        serialPort2.Write("f\r\n");
                        siemensInitCount++;
                        textBox1.AppendText($"> [{serialPort2.PortName}] f (init {siemensInitCount}/{MaxSiemensInitCommands}){Environment.NewLine}");
                    }
                    else
                    {
                        break;
                    }
                }
                catch { break; }
            }

            if (siemensInitCount >= MaxSiemensInitCommands && siemensInitActive)
            {
                textBox1.AppendText($"[INIT TIMEOUT] No response after {MaxSiemensInitCommands} attempts{Environment.NewLine}");
            }
            siemensInitActive = false;
        }

        // Stop init sequence when we receive a valid response
        private void StopSiemensInit()
        {
            if (siemensInitActive)
            {
                siemensInitActive = false;
                textBox1.AppendText($"[INIT COMPLETE] Response received{Environment.NewLine}");
            }
        }

        // ==============================
        // CLEAR BUTTON
        // ==============================
        private void btnClear_Click(object? sender, EventArgs e)
        {
            textBox1.Clear();
            textBox3.Clear();
            allReceivedLines.Clear();
            rxBuffer.Clear();
            ffsMode = false;
            flfMode = false;
            ffsTimer?.Stop();
            flfStartEntry = null;
            flfEntryCount = 0;
            UpdateStatusBar();
        }

        // ==============================
        // BUTTON 1 (9600 baud)
        // ==============================
        private void button1_Click(object? sender, EventArgs e)
        {
            try
            {
                if (!serialPort1.IsOpen && serialPort2.IsOpen)
                {
                    MessageBox.Show("COM port already in use.\nPlease disconnect the other connection.",
                        "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!serialPort1.IsOpen)
                {
                    var port = comboBoxPTC1.SelectedItem as string;
                    if (string.IsNullOrWhiteSpace(port))
                    {
                        MessageBox.Show("Please select a COM port.",
                            "Port Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    serialPort1.PortName = port;
                    serialPort1.Open();

                    button1.Text = "DISCONNECT";
                    button1.BackColor = Color.FromArgb(144, 238, 144);
                    button1.ForeColor = SystemColors.ControlText;

                    textBox1.AppendText($"[CONNECTED] PTC1 @ 9600 baud on {serialPort1.PortName}{Environment.NewLine}");
                    UpdateStatusBar();
                }
                else
                {
                    serialPort1.Close();
                    button1.Text = "PTC1 (9600)";
                    button1.BackColor = SystemColors.Control;
                    button1.ForeColor = SystemColors.ControlText;

                    textBox1.AppendText($"[DISCONNECTED] PTC1{Environment.NewLine}");
                    UpdateStatusBar();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Connection error: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ==============================
        // BUTTON 2 (1200 baud)
        // ==============================
        private void button2_Click(object? sender, EventArgs e)
        {
            try
            {
                if (!serialPort2.IsOpen && serialPort1.IsOpen)
                {
                    MessageBox.Show("COM port already in use.\nPlease disconnect the other connection.",
                        "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!serialPort2.IsOpen)
                {
                    var port = comboBoxPTC1.SelectedItem as string;
                    if (string.IsNullOrWhiteSpace(port))
                    {
                        MessageBox.Show("Please select a COM port.",
                            "Port Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    serialPort2.PortName = port;
                    serialPort2.Open();

                    button2.Text = "DISCONNECT";
                    button2.BackColor = Color.FromArgb(135, 206, 250);
                    button2.ForeColor = SystemColors.ControlText;

                    textBox1.AppendText($"[CONNECTED] Siemens @ 1200 baud on {serialPort2.PortName}{Environment.NewLine}");
                    UpdateStatusBar();

                    // Send initial sequence to get "SIEMENS" response
                    SendSiemensInitSequence();
                }
                else
                {
                    serialPort2.Close();
                    button2.Text = "Siemens (1200)";
                    button2.BackColor = SystemColors.Control;
                    button2.ForeColor = SystemColors.ControlText;

                    // Reset FFS/FLF mode on disconnect
                    ffsMode = false;
                    flfMode = false;
                    ffsTimer?.Stop();
                    siemensInitActive = false;
                    seenFlfEntries.Clear();
                    lastFlfEntry = null;

                    textBox1.AppendText($"[DISCONNECTED] Siemens{Environment.NewLine}");
                    UpdateStatusBar();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Connection error: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ==============================
        // STATUS BAR UPDATE
        // ==============================
        private void UpdateStatusBar()
        {
            string connectionStatus = "DISCONNECTED";
            string baudRate = "--";

            if (serialPort1 != null && serialPort1.IsOpen)
            {
                connectionStatus = $"CONNECTED ({serialPort1.PortName})";
                baudRate = "9600";
            }
            else if (serialPort2 != null && serialPort2.IsOpen)
            {
                connectionStatus = $"CONNECTED ({serialPort2.PortName})";
                baudRate = "1200";
            }

            statusLabelConnection.Text = connectionStatus;
            statusLabelBaud.Text = $"Baud: {baudRate}";
            statusLabelMessages.Text = $"Messages: {allReceivedLines.Count}";
        }

        // ==============================
        // SEND COMMAND
        // ==============================
        private void textBox2_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;

            SendCommand();
            e.SuppressKeyPress = true;
        }

        private void SendCommand()
        {
            SerialPort? activePort = null;

            if (serialPort1 != null && serialPort1.IsOpen)
                activePort = serialPort1;
            else if (serialPort2 != null && serialPort2.IsOpen)
                activePort = serialPort2;

            if (activePort == null)
            {
                MessageBox.Show("No COM port connected.",
                    "Not Connected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string cmd = textBox2.Text.Trim();
            if (cmd.Length == 0) return;

            textBox1.AppendText($"> [{activePort.PortName}] {cmd}{Environment.NewLine}");
            activePort.Write(cmd + "\r\n");
            textBox2.Clear();
        }

        // ==============================
        // RECEIVE DATA (9600)
        // ==============================
        private void SerialPort1_DataReceived(object? sender, SerialDataReceivedEventArgs e)
        {
            if (!serialPort1.IsOpen) return;
            HandleIncoming(serialPort1.ReadExisting());
        }

        // ==============================
        // RECEIVE DATA (1200)
        // ==============================
        private void SerialPort2_DataReceived(object? sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                int count = serialPort2.BytesToRead;
                if (count <= 0) return;

                byte[] buffer = new byte[count];
                serialPort2.Read(buffer, 0, count);

                BeginInvoke(new Action(() =>
                {
                    string ascii = Encoding.ASCII.GetString(buffer);
                    rxBuffer.Append(ascii);

                    while (true)
                    {
                        int idx = rxBuffer.ToString().IndexOfAny(new[] { '\r', '\n' });
                        if (idx < 0) break;

                        string line = rxBuffer.ToString(0, idx).Trim();
                        rxBuffer.Remove(0, idx + 1);

                        if (!string.IsNullOrEmpty(line))
                        {
                            AppendRawLine(line);
                            allReceivedLines.Add(line);

                            // Stop init sequence when we receive any response
                            StopSiemensInit();

                            // Handle FFS/FLF fault log mode
                            HandleFfsResponse(line);
                        }
                    }

                    UpdateCodeDescriptions();
                    UpdateStatusBar();

                    // Auto-scroll to end of raw data
                    textBox1.SelectionStart = textBox1.Text.Length;
                    textBox1.ScrollToCaret();
                }));
            }
            catch (Exception ex)
            {
                BeginInvoke(new Action(() =>
                {
                    textBox1.AppendText($"[RX ERROR] {ex.Message}{Environment.NewLine}");
                }));
            }
        }

        private void AppendRawLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                return;
            }

            textBox1.AppendText(line + Environment.NewLine);
        }

        // ==============================
        // HANDLE FFS/FLF (FAULT LOG) RESPONSES
        // ==============================
        private void HandleFfsResponse(string line)
        {
            string upperLine = line.ToUpper().Trim();

            // Check for END OF LOG - stop FFS auto-sending "+"
            if (upperLine.Contains("END OF LOG"))
            {
                ffsMode = false;
                ffsTimer?.Stop();
                textBox1.AppendText($"[FFS FAULT LOG COMPLETE]{Environment.NewLine}");
                return;
            }

            // Check for FFS fault entry - need to send "+" to continue until END OF LOG
            // FFS format: FFS 12:255 DFM-DSF
            bool isFfsEntry = upperLine.StartsWith("FFS") && !upperLine.Equals("FFS");

            // Check for FLF fault entry - need to send "+" until we see a repeat
            // FLF format: FLF 0:0
            bool isFlFEntry = upperLine.StartsWith("FLF") && upperLine.Contains(":");

            if (isFfsEntry)
            {
                ffsMode = true;
                flfMode = false;
                flfStartEntry = null;
                flfEntryCount = 0;
                // Start timer to send "+" after a short delay
                ffsTimer?.Stop();
                ffsTimer?.Start();
            }
            else if (isFlFEntry)
            {
                string flfKey = upperLine;
                if (!flfMode)
                {
                    flfStartEntry = null;
                    flfEntryCount = 0;
                }

                if (flfEntryCount > 0 && flfStartEntry != null && flfKey == flfStartEntry)
                {
                    // Repeating entry detected - stop auto-scroll
                    flfMode = false;
                    ffsTimer?.Stop();
                    textBox1.AppendText($"[FLF LOG COMPLETE - Repeat detected]{Environment.NewLine}");
                    flfStartEntry = null;
                    flfEntryCount = 0;
                    return;
                }

                if (flfEntryCount == 0)
                {
                    flfStartEntry = flfKey;
                }

                flfEntryCount++;
                flfMode = true;
                ffsMode = false;

                // Start timer to send "+" after a short delay
                ffsTimer?.Stop();
                ffsTimer?.Start();
            }
        }

        // ==============================
        // SHARED RX HANDLER
        // ==============================
        private void HandleIncoming(string data)
        {
            BeginInvoke(new Action(() =>
            {
                rxBuffer.Append(data);

                while (true)
                {
                    int idx = rxBuffer.ToString().IndexOfAny(new[] { '\r', '\n' });
                    if (idx < 0) break;

                    string line = rxBuffer.ToString(0, idx).Trim();
                    rxBuffer.Remove(0, idx + 1);

                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        textBox1.AppendText(line + Environment.NewLine);
                        allReceivedLines.Add(line);
                    }
                }

                UpdateCodeDescriptions();
                UpdateStatusBar();

                // Auto-scroll to end of raw data
                textBox1.SelectionStart = textBox1.Text.Length;
                textBox1.ScrollToCaret();
            }));
        }

        // ==============================
        // PARSE DETECTION FLAGS (F, O, N, S, -)
        // Format: DET. NAME XXXXX where X is F/O/N/S/-
        // F = Fault, O = Open, N = Normal, S = Stuck/Override
        // ==============================
        private string? ParseDetectorFlags(string line)
        {
            var match = DetectorFlagRegex.Match(line);
            if (!match.Success) return null;

            string detectorName = match.Groups[1].Value;
            string flags = match.Groups[2].Value.ToUpper();

            var statusList = new List<string>();
            var flagDescriptions = new Dictionary<char, (string name, string color)>
            {
                { 'F', ("FAULT", "RED") },
                { 'O', ("OPEN", "YELLOW") },
                { 'N', ("NORMAL", "GREEN") },
                { 'S', ("STUCK/OVERRIDE", "ORANGE") }
            };

            for (int i = 0; i < flags.Length && i < 5; i++)
            {
                char flag = flags[i];
                if (flagDescriptions.TryGetValue(flag, out var desc))
                {
                    statusList.Add($"[{desc.name}]");
                }
            }

            if (statusList.Count == 0)
                return null;

            string statusStr = string.Join(" ", statusList);
            string explanation = GetDetectorExplanation(flags);

            return $"DETECTOR: {detectorName}\n" +
                   $"  Flags: {flags}\n" +
                   $"  Status: {statusStr}\n" +
                   $"  {explanation}";
        }

        private string GetDetectorExplanation(string flags)
        {
            if (flags.Contains('F'))
                return "Detector FAULT - Check loop/card/wiring. Loop may be shorted.";
            if (flags.Contains('O'))
                return "Detector OPEN - Loop circuit open. Check connections/cable.";
            if (flags.Contains('S'))
                return "Detector STUCK/OVERRIDE - Manual override or stuck detection.";
            if (flags.Contains('N'))
                return "Detector NORMAL - Operating correctly.";
            return "Unknown detector state.";
        }

        // ==============================
        // EXTRACT KEY FOR LOOKUP
        // ==============================
        private string ExtractCodeKey(string line)
        {
            if (string.IsNullOrWhiteSpace(line)) return "UNKNOWN";

            var tokens = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

            // 0) Check for FFS format: FFS [NUMBER]:255 [DESCRIPTION]
            //    Convert to FLF:[NUMBER] for lookup
            var ffsMatch = FfsFaultRegex.Match(line);
            if (ffsMatch.Success)
            {
                string flfNumber = ffsMatch.Groups[1].Value;
                return $"FLF:{flfNumber}";
            }

            // 0b) Check for FLF format: FLF [NUMBER]:[NUMBER]
            //     Convert to FLF:[NUMBER] for lookup
            if (tokens.Length >= 2 && tokens[0].Equals("FLF", StringComparison.OrdinalIgnoreCase))
            {
                string[] parts = tokens[1].Split(':');
                if (parts.Length >= 1 && int.TryParse(parts[0], out int flfNum))
                {
                    return $"FLF:{flfNum}";
                }
            }

            // 1) Prefer FLF:#
            var flfToken = Array.Find(tokens, t => t.StartsWith("FLF:", StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrEmpty(flfToken))
                return flfToken.Trim().TrimEnd(',', ';');

            // 2) Check for OMS ERR pattern
            if (line.Contains("OMS ERR"))
            {
                var omsMatch = Regex.Match(line, @"OMS ERR G\d{2}\s*\(\d\)");
                if (omsMatch.Success)
                    return omsMatch.Value;
            }

            // 3) Check for SWOF XP pattern
            if (line.Contains("SWOF XP"))
                return "SWOF XP";

            // 4) Prefer numeric code tokens like 011:...
            var numToken = Array.Find(tokens, t =>
                t.Length >= 4 &&
                char.IsDigit(t[0]) && char.IsDigit(t[1]) && char.IsDigit(t[2]) &&
                t[3] == ':');

            if (!string.IsNullOrEmpty(numToken))
            {
                int start = Array.IndexOf(tokens, numToken);
                var sb = new StringBuilder(tokens[start].Trim().TrimEnd(',', ';'));
                for (int i = start + 1; i < tokens.Length; i++)
                {
                    string t = tokens[i];
                    if (t.StartsWith("[") || t.StartsWith("(")) break;
                    sb.Append(' ').Append(t);
                }
                return sb.ToString().Trim();
            }

            // 5) Known textual prefixes
            string[] knownPrefixes = { "LCM-", "IO1616-", "MP-", "IOTU-", "MTS4BP-" };
            foreach (var t in tokens)
            {
                foreach (var p in knownPrefixes)
                {
                    if (t.StartsWith(p, StringComparison.OrdinalIgnoreCase))
                    {
                        // Include ERR suffix if present
                        int idx = Array.IndexOf(tokens, t);
                        if (idx < tokens.Length - 1 && tokens[idx + 1].Equals("ERR", StringComparison.OrdinalIgnoreCase))
                            return t.Trim() + " ERR";
                        return t.Trim().TrimEnd(',', ';');
                    }
                }
            }

            // 6) Fallback - whole line
            return tokens.Length == 1 ? tokens[0].Trim() : line.Trim();
        }

        // ==============================
        // UPDATE FAULT DESCRIPTIONS
        // ==============================
        private void UpdateCodeDescriptions()
        {
            var criticalCodes = new HashSet<string>
            {
                "000:NO LAMP POWER.MDU",
                "006:CPU-B LCM DIAG (F)",
                "208:LAMP AUTOSET ERR",
                "255:FATAL ALARM",
                "LCM-1-255 ERR",
                "IO1616-1 ERR",
                "IO1616-2 ERR",
                "MP-1 ERR",
                "FLF:2",
                "FLF:3",
                "FLF:4",
                "FLF:5"
            };

            var sb = new StringBuilder();

            foreach (string line in allReceivedLines)
            {
                // First check for detector flags (DET. NAME FLAGS)
                string? detectorParsed = ParseDetectorFlags(line);
                if (detectorParsed != null)
                {
                    // Check if it's a fault condition
                    bool isFault = line.Contains("F") && !line.Contains("F----".Replace("F", "-"));
                    string prefix = isFault ? "[DETECTOR FAULT] " : "";
                    sb.AppendLine($"{prefix}{detectorParsed}");
                    continue;
                }

                // Otherwise use standard code lookup
                string codeKey = ExtractCodeKey(line);
                string matchedDesc;

                if (cpuMessages.TryGetValue(codeKey, out var descExact))
                {
                    matchedDesc = descExact;
                }
                else
                {
                    // Try partial matching for numeric codes
                    if (codeKey.Length > 4 && char.IsDigit(codeKey[0]) && char.IsDigit(codeKey[1]) &&
                        char.IsDigit(codeKey[2]) && codeKey[3] == ':')
                    {
                        var toks = codeKey.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (toks.Length >= 3)
                        {
                            string tryKey = toks[0] + " " + toks[1] + " " + toks[2];
                            if (cpuMessages.TryGetValue(tryKey, out var descShort))
                                matchedDesc = descShort;
                            else
                                matchedDesc = "No matching code found.";
                        }
                        else
                        {
                            matchedDesc = "No matching code found.";
                        }
                    }
                    else
                    {
                        matchedDesc = "No matching code found.";
                    }
                }

                string displayLine = $"{codeKey}: {matchedDesc}";
                if (criticalCodes.Contains(codeKey))
                    displayLine = "[CRITICAL] " + displayLine;

                sb.AppendLine(displayLine);
            }

            textBox3.Text = sb.ToString();

            if (textBox3.Text.Length > 0)
            {
                textBox3.SelectionStart = textBox3.Text.Length;
                textBox3.ScrollToCaret();
            }
        }

        // ==============================
        // CLEAN EXIT
        // ==============================
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            try
            {
                if (serialPort1 != null && serialPort1.IsOpen) serialPort1.Close();
                if (serialPort2 != null && serialPort2.IsOpen) serialPort2.Close();
            }
            catch
            {
                // ignore close errors on shutdown
            }

            base.OnFormClosing(e);
        }
    }
}
