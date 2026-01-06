namespace CPU_Interface
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            components = new System.ComponentModel.Container();

            // Main panels and containers
            mainTableLayout = new TableLayoutPanel();
            topPanel = new Panel();
            mainSplitContainer = new SplitContainer();
            statusStrip = new StatusStrip();

            // Top panel controls
            panelConnection = new Panel();
            button1 = new Button();
            button2 = new Button();
            comboBoxPTC1 = new ComboBox();
            labelComPort = new Label();
            panelCommand = new Panel();
            textBox2 = new TextBox();
            labelCommand = new Label();
            btnClear = new Button();
            btnFullscreen = new Button();

            // Left panel (raw data)
            panelLeft = new Panel();
            labelRawData = new Label();
            textBox1 = new TextBox();

            // Center panel (image)
            pictureBox1 = new PictureBox();

            // Right panel (decoded output)
            panelRight = new Panel();
            labelDecoded = new Label();
            textBox3 = new TextBox();

            // Status strip items
            statusLabelConnection = new ToolStripStatusLabel();
            statusLabelBaud = new ToolStripStatusLabel();
            statusLabelMessages = new ToolStripStatusLabel();
            statusLabelFullscreen = new ToolStripStatusLabel();

            // Suspend layout
            mainTableLayout.SuspendLayout();
            topPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)mainSplitContainer).BeginInit();
            mainSplitContainer.Panel1.SuspendLayout();
            mainSplitContainer.Panel2.SuspendLayout();
            mainSplitContainer.SuspendLayout();
            panelConnection.SuspendLayout();
            panelCommand.SuspendLayout();
            panelLeft.SuspendLayout();
            panelRight.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            statusStrip.SuspendLayout();
            SuspendLayout();

            // ========================================
            // MAIN TABLE LAYOUT
            // ========================================
            mainTableLayout.ColumnCount = 1;
            mainTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            mainTableLayout.RowCount = 3;
            mainTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 90F));
            mainTableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            mainTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 26F));
            mainTableLayout.Controls.Add(topPanel, 0, 0);
            mainTableLayout.Controls.Add(mainSplitContainer, 0, 1);
            mainTableLayout.Controls.Add(statusStrip, 0, 2);
            mainTableLayout.Dock = DockStyle.Fill;
            mainTableLayout.Location = new Point(0, 0);
            mainTableLayout.Margin = new Padding(0);
            mainTableLayout.Name = "mainTableLayout";
            mainTableLayout.Padding = new Padding(4);
            mainTableLayout.BackColor = SystemColors.Control;

            // ========================================
            // TOP PANEL
            // ========================================
            topPanel.BackColor = SystemColors.Control;
            topPanel.Controls.Add(panelConnection);
            topPanel.Controls.Add(panelCommand);
            topPanel.Controls.Add(btnFullscreen);
            topPanel.Dock = DockStyle.Fill;
            topPanel.Name = "topPanel";
            topPanel.Padding = new Padding(8);

            // ========================================
            // CONNECTION PANEL
            // ========================================
            panelConnection.BackColor = SystemColors.ControlLight;
            panelConnection.Controls.Add(labelComPort);
            panelConnection.Controls.Add(comboBoxPTC1);
            panelConnection.Controls.Add(button1);
            panelConnection.Controls.Add(button2);
            panelConnection.Location = new Point(8, 8);
            panelConnection.Size = new Size(450, 74);
            panelConnection.Name = "panelConnection";
            panelConnection.Padding = new Padding(10);
            panelConnection.BorderStyle = BorderStyle.FixedSingle;

            // Label: COM Port
            labelComPort.AutoSize = true;
            labelComPort.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            labelComPort.ForeColor = SystemColors.ControlText;
            labelComPort.Location = new Point(12, 12);
            labelComPort.Name = "labelComPort";
            labelComPort.Text = "COM PORT";

            // ComboBox: COM Port selector
            comboBoxPTC1.BackColor = SystemColors.Window;
            comboBoxPTC1.ForeColor = SystemColors.WindowText;
            comboBoxPTC1.Font = new Font("Segoe UI", 10F);
            comboBoxPTC1.FormattingEnabled = true;
            comboBoxPTC1.Location = new Point(12, 35);
            comboBoxPTC1.Name = "comboBoxPTC1";
            comboBoxPTC1.Size = new Size(100, 28);
            comboBoxPTC1.DropDownStyle = ComboBoxStyle.DropDownList;

            // Button: PTC1 (9600 baud)
            button1.BackColor = SystemColors.Control;
            button1.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            button1.ForeColor = SystemColors.ControlText;
            button1.Location = new Point(125, 25);
            button1.Name = "button1";
            button1.Size = new Size(150, 40);
            button1.TabIndex = 1;
            button1.Text = "PTC1 (9600)";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;

            // Button: Siemens (1200 baud)
            button2.BackColor = SystemColors.Control;
            button2.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            button2.ForeColor = SystemColors.ControlText;
            button2.Location = new Point(285, 25);
            button2.Name = "button2";
            button2.Size = new Size(150, 40);
            button2.TabIndex = 2;
            button2.Text = "Siemens (1200)";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;

            // ========================================
            // COMMAND PANEL
            // ========================================
            panelCommand.BackColor = SystemColors.ControlLight;
            panelCommand.Controls.Add(labelCommand);
            panelCommand.Controls.Add(textBox2);
            panelCommand.Controls.Add(btnClear);
            panelCommand.Location = new Point(468, 8);
            panelCommand.Size = new Size(400, 74);
            panelCommand.Name = "panelCommand";
            panelCommand.Padding = new Padding(10);
            panelCommand.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            panelCommand.BorderStyle = BorderStyle.FixedSingle;

            // Label: Command
            labelCommand.AutoSize = true;
            labelCommand.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            labelCommand.ForeColor = SystemColors.ControlText;
            labelCommand.Location = new Point(12, 12);
            labelCommand.Name = "labelCommand";
            labelCommand.Text = "COMMAND INPUT";

            // TextBox: Command input
            textBox2.BackColor = SystemColors.Window;
            textBox2.BorderStyle = BorderStyle.FixedSingle;
            textBox2.Font = new Font("Consolas", 11F);
            textBox2.ForeColor = SystemColors.WindowText;
            textBox2.Location = new Point(12, 35);
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(280, 28);
            textBox2.TabIndex = 3;

            // Button: Clear
            btnClear.BackColor = Color.FromArgb(255, 192, 192);
            btnClear.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnClear.ForeColor = SystemColors.ControlText;
            btnClear.Location = new Point(300, 32);
            btnClear.Name = "btnClear";
            btnClear.Size = new Size(85, 32);
            btnClear.Text = "CLEAR";
            btnClear.UseVisualStyleBackColor = false;
            btnClear.Click += btnClear_Click;

            // Button: Fullscreen toggle
            btnFullscreen.BackColor = SystemColors.Control;
            btnFullscreen.Font = new Font("Segoe UI", 9F);
            btnFullscreen.ForeColor = SystemColors.ControlText;
            btnFullscreen.Location = new Point(878, 32);
            btnFullscreen.Name = "btnFullscreen";
            btnFullscreen.Size = new Size(100, 32);
            btnFullscreen.Text = "Fullscreen";
            btnFullscreen.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnFullscreen.UseVisualStyleBackColor = true;
            btnFullscreen.Click += btnFullscreen_Click;

            // ========================================
            // MAIN SPLIT CONTAINER
            // ========================================
            mainSplitContainer.BackColor = SystemColors.Control;
            mainSplitContainer.Dock = DockStyle.Fill;
            mainSplitContainer.Name = "mainSplitContainer";
            mainSplitContainer.SplitterDistance = 500;
            mainSplitContainer.SplitterWidth = 6;
            mainSplitContainer.Panel1.BackColor = SystemColors.ControlLight;
            mainSplitContainer.Panel2.BackColor = SystemColors.ControlLight;

            // ========================================
            // LEFT PANEL (Raw Data)
            // ========================================
            mainSplitContainer.Panel1.Controls.Add(panelLeft);
            mainSplitContainer.Panel1.Controls.Add(pictureBox1);

            panelLeft.BackColor = SystemColors.ControlLight;
            panelLeft.Controls.Add(labelRawData);
            panelLeft.Controls.Add(textBox1);
            panelLeft.Dock = DockStyle.Fill;
            panelLeft.Name = "panelLeft";
            panelLeft.Padding = new Padding(8);

            // Label: Raw Data
            labelRawData.AutoSize = true;
            labelRawData.BackColor = SystemColors.ControlLight;
            labelRawData.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            labelRawData.ForeColor = Color.FromArgb(0, 128, 64);
            labelRawData.Location = new Point(10, 8);
            labelRawData.Name = "labelRawData";
            labelRawData.Text = "RAW SERIAL DATA";

            // TextBox: Raw data display
            textBox1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            textBox1.BackColor = SystemColors.Window;
            textBox1.BorderStyle = BorderStyle.FixedSingle;
            textBox1.Font = new Font("Consolas", 9.75F, FontStyle.Bold);
            textBox1.ForeColor = SystemColors.WindowText;
            textBox1.Location = new Point(8, 32);
            textBox1.Multiline = true;
            textBox1.Name = "textBox1";
            textBox1.ReadOnly = true;
            textBox1.ScrollBars = ScrollBars.Both;
            textBox1.Size = new Size(350, 600);
            textBox1.TabIndex = 0;
            textBox1.WordWrap = false;

            // ========================================
            // PICTURE BOX (Center Image)
            // ========================================
            pictureBox1.Image = (Image)resources.GetObject("pictureBox1.Image");
            pictureBox1.Location = new Point(365, 100);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(130, 400);
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.TabIndex = 8;
            pictureBox1.TabStop = false;
            pictureBox1.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            // ========================================
            // RIGHT PANEL (Decoded Output)
            // ========================================
            mainSplitContainer.Panel2.Controls.Add(panelRight);

            panelRight.BackColor = SystemColors.ControlLight;
            panelRight.Controls.Add(labelDecoded);
            panelRight.Controls.Add(textBox3);
            panelRight.Dock = DockStyle.Fill;
            panelRight.Name = "panelRight";
            panelRight.Padding = new Padding(8);

            // Label: Decoded
            labelDecoded.AutoSize = true;
            labelDecoded.BackColor = SystemColors.ControlLight;
            labelDecoded.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            labelDecoded.ForeColor = Color.FromArgb(192, 64, 0);
            labelDecoded.Location = new Point(10, 8);
            labelDecoded.Name = "labelDecoded";
            labelDecoded.Text = "DECODED FAULTS & DETECTORS";

            // TextBox: Decoded display
            textBox3.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            textBox3.BackColor = SystemColors.Window;
            textBox3.BorderStyle = BorderStyle.FixedSingle;
            textBox3.Font = new Font("Consolas", 9.75F, FontStyle.Bold);
            textBox3.ForeColor = SystemColors.WindowText;
            textBox3.Location = new Point(8, 32);
            textBox3.Multiline = true;
            textBox3.Name = "textBox3";
            textBox3.ReadOnly = true;
            textBox3.ScrollBars = ScrollBars.Both;
            textBox3.Size = new Size(484, 600);
            textBox3.TabIndex = 4;
            textBox3.WordWrap = false;

            // ========================================
            // STATUS STRIP
            // ========================================
            statusStrip.BackColor = Color.FromArgb(0, 122, 204);
            statusStrip.Dock = DockStyle.Fill;
            statusStrip.GripStyle = ToolStripGripStyle.Hidden;
            statusStrip.Items.AddRange(new ToolStripItem[] {
                statusLabelConnection,
                statusLabelBaud,
                statusLabelMessages,
                statusLabelFullscreen
            });
            statusStrip.Name = "statusStrip";
            statusStrip.RenderMode = ToolStripRenderMode.Professional;
            statusStrip.SizingGrip = false;

            // Status: Connection
            statusLabelConnection.ForeColor = Color.White;
            statusLabelConnection.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            statusLabelConnection.Name = "statusLabelConnection";
            statusLabelConnection.Text = "DISCONNECTED";
            statusLabelConnection.Padding = new Padding(5, 0, 20, 0);

            // Status: Baud rate
            statusLabelBaud.ForeColor = Color.White;
            statusLabelBaud.Font = new Font("Segoe UI", 9F);
            statusLabelBaud.Name = "statusLabelBaud";
            statusLabelBaud.Text = "Baud: --";
            statusLabelBaud.Padding = new Padding(5, 0, 20, 0);

            // Status: Message count
            statusLabelMessages.ForeColor = Color.White;
            statusLabelMessages.Font = new Font("Segoe UI", 9F);
            statusLabelMessages.Name = "statusLabelMessages";
            statusLabelMessages.Text = "Messages: 0";
            statusLabelMessages.Padding = new Padding(5, 0, 20, 0);

            // Status: Fullscreen hint
            statusLabelFullscreen.ForeColor = Color.FromArgb(200, 200, 200);
            statusLabelFullscreen.Font = new Font("Segoe UI", 9F);
            statusLabelFullscreen.Name = "statusLabelFullscreen";
            statusLabelFullscreen.Text = "Press F11 for fullscreen";
            statusLabelFullscreen.Alignment = ToolStripItemAlignment.Right;
            statusLabelFullscreen.Spring = true;

            // ========================================
            // FORM1
            // ========================================
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.Control;
            ClientSize = new Size(1400, 850);
            Controls.Add(mainTableLayout);
            DoubleBuffered = true;
            KeyPreview = true;
            MinimumSize = new Size(1000, 600);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "CPU Interface - Traffic Controller Monitor";
            WindowState = FormWindowState.Maximized;

            // Resume layout
            mainTableLayout.ResumeLayout(false);
            topPanel.ResumeLayout(false);
            mainSplitContainer.Panel1.ResumeLayout(false);
            mainSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)mainSplitContainer).EndInit();
            mainSplitContainer.ResumeLayout(false);
            panelConnection.ResumeLayout(false);
            panelConnection.PerformLayout();
            panelCommand.ResumeLayout(false);
            panelCommand.PerformLayout();
            panelLeft.ResumeLayout(false);
            panelLeft.PerformLayout();
            panelRight.ResumeLayout(false);
            panelRight.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        // Main layout containers
        private TableLayoutPanel mainTableLayout;
        private Panel topPanel;
        private SplitContainer mainSplitContainer;
        private StatusStrip statusStrip;

        // Top panel controls
        private Panel panelConnection;
        private Panel panelCommand;
        private Button button1;
        private Button button2;
        private ComboBox comboBoxPTC1;
        private Label labelComPort;
        private TextBox textBox2;
        private Label labelCommand;
        private Button btnClear;
        private Button btnFullscreen;

        // Left panel (raw data)
        private Panel panelLeft;
        private Label labelRawData;
        private TextBox textBox1;

        // Center image
        private PictureBox pictureBox1;

        // Right panel (decoded)
        private Panel panelRight;
        private Label labelDecoded;
        private TextBox textBox3;

        // Status strip items
        private ToolStripStatusLabel statusLabelConnection;
        private ToolStripStatusLabel statusLabelBaud;
        private ToolStripStatusLabel statusLabelMessages;
        private ToolStripStatusLabel statusLabelFullscreen;
    }
}
