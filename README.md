# CPU-Interface
JLO CPU Interface

## Troubleshooting

### "Mark of the Web" Error

If you encounter the error:
> Couldn't process file Form1.resx due to its being in the Internet or Restricted zone or having the mark of the web on the file.

This occurs because Windows marks files downloaded from the internet as potentially unsafe. To fix this:

**Option 1: Run the included PowerShell script**
```powershell
.\Unblock-Files.ps1
```

**Option 2: Unblock files manually**
1. Right-click each blocked file (e.g., `Form1.resx`, `Properties/Resources.resx`)
2. Select "Properties"
3. At the bottom of the General tab, check "Unblock"
4. Click OK

**Option 3: Unblock all files via PowerShell**
```powershell
Get-ChildItem -Recurse | Unblock-File
```
