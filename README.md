# Switch 🚀

**An ultra-fast, lightweight Alt-Tab alternative for Windows.**  
**Created by Gamer OG**

Tired of the slight delay when using Alt-Tab? Want to switch between your last used applications faster than mechanically possible? **Switch** is the answer!

## What is Switch?
Switch is a tiny, standalone Windows application that runs in the background. It uses raw Windows APIs to completely bypass the visual Alt-Tab menu. When you press the hotkey, it instantly swaps your current window with your previous window.

*   **Zero Delay:** Instantaneous window switching without any UI lag.
*   **Lightweight:** Less than 20 KB in size, uses virtually no RAM or CPU.
*   **Customizable:** Set any hotkey combination you prefer (default is `Alt + Q`).
*   **Non-Intrusive:** Lives silently in your system tray.

## Installation 💾

You have two simple ways to get started:

### 1. The Easy Way (Installer)
1.  Download the [`Switch_Installer.exe`](https://github.com/Gamerogalt/switch/raw/main/Switch_Installer.exe) from this repository.
2.  Double-click it and click **Yes**.
3.  The installer will automatically place the app in your system data folder and set it to run on startup!

### 2. The Portable Way (Standalone)
1.  Download the [`Switch.exe`](https://github.com/Gamerogalt/switch/raw/main/Switch.exe) file.
2.  Place it anywhere on your computer (like your Desktop or Documents).
3.  Double-click it to run it. (If you want it to run on startup, you can manually place a shortcut in your `shell:startup` folder).

## How to Use 🎮
1.  Ensure **Switch** is running (you will see a small `(i)` icon in your system tray at the bottom right).
2.  Open at least two windows (e.g., your browser and a game).
3.  Press the hotkey (**`Alt + Q`** by default).
4.  Watch your windows swap instantly!

### Changing the Hotkey
If you want to use a different key combination (like `F4`, `Caps Lock`, or `Ctrl + Tab`):
1.  Right-click the `(i)` icon in your system tray.
2.  Click **"Change Hotkey..."**.
3.  Press your new desired key combination. The app will save it automatically!

## For Developers 👨‍💻
Want to see how it works or modify it yourself?
*   `Switch.cs` contains the entire source code (C#).
*   Run `build.bat` to compile the `Switch.exe` yourself using the built-in Windows C# compiler (no Visual Studio needed!).
*   `generate_installer.ps1` is the script used to package the standalone `.exe` into the convenient installer.

---
**Enjoy the speed! - Gamer OG**
