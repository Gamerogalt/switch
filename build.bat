@echo off
set CSC="C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if not exist %CSC% (
    echo C# compiler not found.
    pause
    exit /b 1
)
echo Compiling Switch...
%CSC% /target:winexe /win32manifest:app.manifest /out:Switch.exe Switch.cs
if %errorlevel% neq 0 (
    echo Compilation failed.
    pause
    exit /b %errorlevel%
)
echo Compilation successful!
echo You can now run Switch.exe
