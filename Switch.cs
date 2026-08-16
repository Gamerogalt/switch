using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Diagnostics;

namespace Switch
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new SwitchContext());
        }
    }

    public class SwitchContext : ApplicationContext
    {
        private NotifyIcon trayIcon;
        private ContextMenuStrip trayMenu;
        private ToolStripMenuItem changeHotkeyItem;
        
        // Default Hotkey: Alt + Q
        private int hotkeyModifiers = MOD_ALT;
        private Keys hotkeyKey = Keys.Q;
        
        private const string ConfigFile = "config.ini";

        public SwitchContext()
        {
            LoadConfig();
            
            trayMenu = new ContextMenuStrip();
            changeHotkeyItem = new ToolStripMenuItem("Change Hotkey...");
            changeHotkeyItem.Click += ChangeHotkeyItem_Click;
            trayMenu.Items.Add(changeHotkeyItem);
            
            ToolStripMenuItem aboutItem = new ToolStripMenuItem("About");
            aboutItem.Click += (s, e) => MessageBox.Show("Switch\n\nCreator: Gamer OG", "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
            trayMenu.Items.Add(aboutItem);
            
            trayMenu.Items.Add(new ToolStripMenuItem("Exit", null, Exit_Click));

            trayIcon = new NotifyIcon();
            trayIcon.Text = "Switch";
            trayIcon.Icon = SystemIcons.Information; // Use default icon
            trayIcon.ContextMenuStrip = trayMenu;
            trayIcon.Visible = true;

            // Dummy form to receive hotkey messages
            hiddenForm = new HiddenMessageForm(this);

            // Register global hotkey
            RegisterCurrentHotkey();
        }
        
        private void LoadConfig()
        {
            if (File.Exists(ConfigFile))
            {
                try {
                    string[] lines = File.ReadAllLines(ConfigFile);
                    if (lines.Length >= 2)
                    {
                        hotkeyModifiers = int.Parse(lines[0]);
                        hotkeyKey = (Keys)Enum.Parse(typeof(Keys), lines[1]);
                    }
                } catch { }
            }
        }
        
        private void SaveConfig()
        {
            try {
                File.WriteAllLines(ConfigFile, new string[] { hotkeyModifiers.ToString(), hotkeyKey.ToString() });
            } catch { }
        }

        private HiddenMessageForm hiddenForm;
        private const int HOTKEY_ID = 1;

        private void RegisterCurrentHotkey()
        {
            if (hiddenForm != null)
            {
                UnregisterHotKey(hiddenForm.Handle, HOTKEY_ID);
                bool success = RegisterHotKey(hiddenForm.Handle, HOTKEY_ID, hotkeyModifiers, (int)hotkeyKey);
                trayIcon.Text = string.Format("Switch (Active: {0})", GetHotkeyString());
                if (!success)
                {
                    trayIcon.ShowBalloonTip(3000, "Switch Error", "Failed to register hotkey. It might be in use by another app.", ToolTipIcon.Error);
                }
            }
        }
        
        private string GetHotkeyString()
        {
            string s = "";
            if ((hotkeyModifiers & MOD_CONTROL) != 0) s += "Ctrl + ";
            if ((hotkeyModifiers & MOD_ALT) != 0) s += "Alt + ";
            if ((hotkeyModifiers & MOD_SHIFT) != 0) s += "Shift + ";
            s += hotkeyKey.ToString();
            return s;
        }

        private void ChangeHotkeyItem_Click(object sender, EventArgs e)
        {
            // Unregister while capturing
            UnregisterHotKey(hiddenForm.Handle, HOTKEY_ID);
            
            Form captureForm = new Form();
            captureForm.Text = "Press New Hotkey...";
            captureForm.Size = new Size(300, 100);
            captureForm.StartPosition = FormStartPosition.CenterScreen;
            captureForm.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            captureForm.TopMost = true;
            
            Label lbl = new Label();
            lbl.Text = "Press any key combination now...";
            lbl.AutoSize = false;
            lbl.Dock = DockStyle.Fill;
            lbl.TextAlign = ContentAlignment.MiddleCenter;
            captureForm.Controls.Add(lbl);
            
            captureForm.KeyDown += (s, ev) => 
            {
                int modifiers = 0;
                if (ev.Control) modifiers |= MOD_CONTROL;
                if (ev.Alt) modifiers |= MOD_ALT;
                if (ev.Shift) modifiers |= MOD_SHIFT;
                
                if (ev.KeyCode != Keys.ControlKey && ev.KeyCode != Keys.Menu && ev.KeyCode != Keys.ShiftKey)
                {
                    hotkeyModifiers = modifiers;
                    hotkeyKey = ev.KeyCode;
                    SaveConfig();
                    RegisterCurrentHotkey();
                    captureForm.Close();
                }
            };
            
            captureForm.ShowDialog();
            
            // In case it was closed without setting a new key
            RegisterCurrentHotkey();
        }

        private void Exit_Click(object sender, EventArgs e)
        {
            trayIcon.Visible = false;
            if (hiddenForm != null) UnregisterHotKey(hiddenForm.Handle, HOTKEY_ID);
            Application.Exit();
        }

        public void OnHotkeyPressed()
        {
            // Perform instantaneous switch
            SwitchToLastWindow();
        }
        
        // --- Win32 API Logic for Fast Switching ---

        private void SwitchToLastWindow()
        {
            IntPtr currentHwnd = GetForegroundWindow();
            IntPtr nextHwnd = GetWindow(currentHwnd, GW_HWNDNEXT);

            while (nextHwnd != IntPtr.Zero)
            {
                if (IsAltTabWindow(nextHwnd))
                {
                    // Trick to bypass foreground lock: simulate Alt key press
                    keybd_event(VK_MENU, 0, 0, 0);
                    keybd_event(VK_MENU, 0, KEYEVENTF_KEYUP, 0);
                    
                    SetForegroundWindow(nextHwnd);
                    
                    // Sometimes windows are minimized, we need to restore them
                    if (IsIconic(nextHwnd))
                    {
                        ShowWindow(nextHwnd, SW_RESTORE);
                    }
                    return;
                }
                nextHwnd = GetWindow(nextHwnd, GW_HWNDNEXT);
            }
        }

        private bool IsAltTabWindow(IntPtr hwnd)
        {
            if (!IsWindowVisible(hwnd)) return false;

            IntPtr root = GetAncestor(hwnd, GA_ROOTOWNER);
            if (GetLastActivePopup(root) != hwnd) return false;

            int exStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            if ((exStyle & WS_EX_TOOLWINDOW) != 0) return false;

            // Check if cloaked (Windows 10/11 virtual desktops / UWP background apps)
            int cloaked;
            if (DwmGetWindowAttribute(hwnd, DWMWA_CLOAKED, out cloaked, sizeof(int)) == 0)
            {
                if (cloaked != 0) return false;
            }

            return true;
        }

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        
        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

        [DllImport("user32.dll")]
        static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern IntPtr GetAncestor(IntPtr hwnd, uint gaFlags);

        [DllImport("user32.dll")]
        static extern IntPtr GetLastActivePopup(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("dwmapi.dll")]
        static extern int DwmGetWindowAttribute(IntPtr hwnd, int dwAttribute, out int pvAttribute, int cbAttribute);

        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

        private const int MOD_ALT = 0x0001;
        private const int MOD_CONTROL = 0x0002;
        private const int MOD_SHIFT = 0x0004;
        
        private const uint GW_HWNDNEXT = 2;
        private const uint GA_ROOTOWNER = 3;
        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TOOLWINDOW = 0x00000080;
        private const int DWMWA_CLOAKED = 14;
        private const int SW_RESTORE = 9;
        
        private const byte VK_MENU = 0x12;
        private const int KEYEVENTF_KEYUP = 0x0002;

        private class HiddenMessageForm : Form
        {
            private SwitchContext ctx;
            public HiddenMessageForm(SwitchContext ctx)
            {
                this.ctx = ctx;
                this.Text = "Switch Hidden Form";
            }
            protected override void WndProc(ref Message m)
            {
                if (m.Msg == 0x0312) // WM_HOTKEY
                {
                    ctx.OnHotkeyPressed();
                }
                base.WndProc(ref m);
            }
            protected override void SetVisibleCore(bool value)
            {
                // Always hide
                base.SetVisibleCore(false);
            }
        }
    }
}
