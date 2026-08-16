$base64 = (Get-Content .\base64.txt -Raw).Trim()
$code = @"
using System;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;

namespace SwitchInstaller {
    static class Program {
        [STAThread]
        static void Main() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            DialogResult result = MessageBox.Show(
                "Welcome to the Switch Setup.\n\nThis will install Switch and set it to run automatically on startup.\n\nDo you want to continue?", 
                "Switch Installer", 
                MessageBoxButtons.YesNo, 
                MessageBoxIcon.Information);
                
            if (result == DialogResult.Yes) {
                try {
                    string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    string targetFolder = Path.Combine(appData, "Switch");
                    if (!Directory.Exists(targetFolder)) Directory.CreateDirectory(targetFolder);
                    
                    string exePath = Path.Combine(targetFolder, "Switch.exe");
                    string b64 = `"$base64`";
                    
                    File.WriteAllBytes(exePath, Convert.FromBase64String(b64));
                    
                    // Create Shortcut via WScript.Shell
                    string startupFolder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
                    string shortcutPath = Path.Combine(startupFolder, "Switch.lnk");
                    
                    Type t = Type.GetTypeFromProgID("WScript.Shell");
                    dynamic shell = Activator.CreateInstance(t);
                    var shortcut = shell.CreateShortcut(shortcutPath);
                    shortcut.TargetPath = exePath;
                    shortcut.WorkingDirectory = targetFolder;
                    shortcut.Save();
                    
                    // Launch
                    Process.Start(exePath);
                    
                    MessageBox.Show("Switch has been installed successfully!\n\nIt is now running in your system tray (bottom right).", "Install Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex) {
                    MessageBox.Show("Installation failed: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
"@

Set-Content .\SwitchInstaller.cs $code -Encoding UTF8
$CSC = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
& $CSC /target:winexe /out:Switch_Installer.exe SwitchInstaller.cs
