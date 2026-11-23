@echo off
setlocal EnableExtensions
cd /d "%~dp0"
echo [GIN-PDF-RW] Compiling v014 binaries via Microsoft .NET csc.exe...

set "CSC="
if exist "%SystemRoot%\Microsoft.NET\Framework64\v4.0.30319\csc.exe" set "CSC=%SystemRoot%\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if not defined CSC if exist "%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\csc.exe" set "CSC=%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\csc.exe"

if not defined CSC (
    echo [ERROR] csc.exe not found!
    exit /b 1
)

echo [1/3] Compiling Uninstall.exe...
"%CSC%" /nologo /target:winexe /optimize+ /platform:anycpu ^
    /win32icon:app.ico ^
    /r:System.dll,System.Windows.Forms.dll,Microsoft.Win32.Primitives.dll ^
    /out:Uninstall.exe Uninstall.cs

echo [2/3] Compiling GIN-PDF-RW.exe...
"%CSC%" /nologo /target:winexe /optimize+ /platform:anycpu ^
    /win32icon:app.ico ^
    /r:PresentationFramework.dll,PresentationCore.dll,WindowsBase.dll,System.Xaml.dll,System.dll,System.Drawing.dll,System.Windows.Forms.dll,System.Core.dll,Microsoft.CSharp.dll ^
    /out:GIN-PDF-RW.exe App.cs

echo [3/3] Compiling Setup.exe with embedded resources...
"%CSC%" /nologo /target:winexe /optimize+ /platform:anycpu ^
    /win32icon:app.ico ^
    /resource:GIN-PDF-RW.exe,GIN-PDF-RW.exe ^
    /resource:Uninstall.exe,Uninstall.exe ^
    /resource:app.ico,app.ico ^
    /r:PresentationFramework.dll,PresentationCore.dll,WindowsBase.dll,System.Xaml.dll,System.dll,System.Drawing.dll,System.Windows.Forms.dll,System.Core.dll,Microsoft.CSharp.dll ^
    /out:..\GIN-PDF-RW_v014_Setup.exe Setup.cs

if %ERRORLEVEL% equ 0 (
    echo [SUCCESS] Build completed successfully! GIN-PDF-RW_v014_Setup.exe created.
) else (
    echo [ERROR] Build failed!
    exit /b %ERRORLEVEL%
)
