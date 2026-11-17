# Worklog - GIN-PDF-RW

## 2026-11-17 - v004 Native WinRT PDF Rendering, Contrast Fix & Uninstaller

- **Target:** Implement real GPU-accelerated PDF page rendering via native Windows WinRT (`Windows.Data.Pdf`), fix unreadable top toolbar button contrast, embed dedicated `Uninstall.exe`, add Ukrainian language support, enforce strict versioned installer naming (`GIN-PDF-RW_v004_Setup.exe`), and purge unversioned desktop binaries.
- **Repository:** `https://github.com/GinCz/GIN-PDF-RW`
- **Actions:**
  - Implemented `NativePdfRenderer` in `src/App.cs` using Windows 10/11 native `Windows.Data.Pdf.PdfDocument`:
    - Renders real PDF pages directly to DirectX/DirectWrite accelerated PNG memory streams.
    - Accurately tracks page counts and renders actual document pages when opening files.
    - Dynamically resizes page viewport according to original PDF page geometry and zoom factor.
  - Fixed top toolbar UI contrast in `src/App.cs`:
    - Replaced buggy toggle buttons with high-contrast Catppuccin Mocha tabs.
    - Active mode tab: `#45475A` background with crisp `#89B4FA` text, bold font, and accent border.
    - Inactive mode tab: `#313244` background with readable `#BAC2DE` text.
  - Created standalone native `Uninstall.exe` (`src/Uninstall.cs`):
    - Prompts user confirmation before uninstalling.
    - Removes all desktop and MEGA shortcuts (`D:\MEGA\DOCS\desktop\GIN-PDF-RW.lnk`).
    - Cleans up Windows Registry keys (`.pdf` association and `Uninstall` control panel entry).
    - Deletes `C:\Program Files\GIN-PDF-RW` on completion.
  - Enhanced installer `src/Setup.cs`:
    - Added Ukrainian (`uk`) and Turkish (`tr`) options, bringing total language options to 12.
    - Embeds `GIN-PDF-RW.exe`, `Uninstall.exe`, and `app.ico` into a single standalone installer.
    - Registers uninstallation entry in Windows Control Panel / Settings.
  - Enforced strict versioned naming:
    - Output installer named strictly `GIN-PDF-RW_v004_Setup.exe`.
    - Delivered exclusively to `D:\MEGA\DOCS\desktop\GIN-PDF-RW_v004_Setup.exe`.
    - Purged older and unversioned installers from MEGA desktop.
