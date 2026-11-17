# Worklog - GIN-PDF-RW

## 2026-11-17 - v005 Complete WinRT COM Interface Fix & Standalone Delivery

- **Target:** Fix WinRT reflection async dispatch in `NativePdfRenderer` so all real PDF documents render immediately on screen, integrate high-contrast tab controls, embed `Uninstall.exe`, and deliver single versioned installer `GIN-PDF-RW_v005_Setup.exe`.
- **Repository:** `https://github.com/GinCz/GIN-PDF-RW`
- **Actions:**
  - Diagnosed `NativePdfRenderer` silent failure:
    - Root cause: `asyncOp.GetType().GetInterfaces()[0]` failed because COM RCWs (`System.__ComObject`) in .NET Framework do not expose generic interface definitions at index 0.
    - Solution: Refactored `AwaitOp` to explicitly pass target WinRT return type (`storageFileType`, `pdfDocType`, `uint`) to `AsTask<T>()` and invoke task completion wait.
    - Verified real PDF byte rasterization directly to WPF `BitmapImage`.
  - Re-architected application initialization in `src/App.cs`:
    - Automatically opens and renders files passed via command line arguments (`args[0]`).
    - Fixed contrast of Reader and Editor tabs with active accent borders and crystal-clear text.
    - Full localization support for English, Ukrainian, Russian, and Czech.
  - Built standalone native uninstaller `Uninstall.exe` (`src/Uninstall.cs`).
  - Built all-in-one setup wizard `GIN-PDF-RW_v005_Setup.exe` (`src/Setup.cs`):
    - Embeds `GIN-PDF-RW.exe`, `Uninstall.exe`, and `app.ico`.
    - Automatically extracts payload to `C:\Program Files\GIN-PDF-RW`.
    - Automatically associates `.pdf` files.
    - Creates Desktop shortcuts.
  - Enforced delivery rules:
    - Single versioned installer on Vladimir's desktop: `D:\MEGA\DOCS\desktop\GIN-PDF-RW_v005_Setup.exe`.
    - Cleaned up all older and unversioned installers.
