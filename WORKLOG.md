# Worklog - GIN-PDF-RW

## 2026-11-18 - v006 Fullscreen Default, Left-Click Flyout, Esc Windowed Mode, 5% Smooth Zoom, Auto FitWidth & 3 Themes

- **Target:** Deliver complete v006 features requested by Vladimir:
  1. Default borderless fullscreen mode on PDF open.
  2. Left click anywhere on document/viewer toggles top menu flyout in fullscreen.
  3. `Esc` key restores normal windowed mode, immediately stretched to full width (`FitWidth`).
  4. Smooth 5% zoom steps (`0.05`) on `+`, `-`, and mouse wheel.
  5. `FitWidth` and `F11` responsive layout stretching PDF to fill full viewport width.
  6. 3-way theme system: Day ☀️, Neutral Cloud ☁️ (in the middle), and Night 🌙.
  7. High-contrast ComboBox styling: dark text on light backgrounds in Day mode, bright text in Night/Cloud modes (no white-on-white text).
  8. Single installer `GIN-PDF-RW_v006_Setup.exe` with PE version properties (`VladiMIR+AI`).
- **Repository:** `https://github.com/GinCz/GIN-PDF-RW`
- **Actions:**
  - Updated `src/App.cs`:
    - Set default mode to borderless fullscreen (`WindowStyle.None`, `WindowState.Maximized`).
    - Added `PreviewMouseLeftButtonDown` on `docScroll` to smoothly toggle top menu flyout when clicking anywhere in fullscreen.
    - Added `Esc` key shortcut to restore normal windowed mode with automatic `DoFitWidth()`.
    - Set zoom increments to `0.05` (5%) for ultra-smooth zoom precision.
    - Upgraded `DoFitWidth()` to dynamically calculate from `docScroll.ViewportWidth - 30` with `Dispatcher.BeginInvoke` layout pass and `docScroll.SizeChanged` handler.
    - Implemented 3-way theme switcher (Day ☀️, Neutral Cloud ☁️, Night 🌙) with unified button, menu, background, and ComboBox styles.
    - Fixed ComboBox foreground and background items to prevent white-on-white text.
  - Updated `src/Setup.cs` and `src/Uninstall.cs` assembly metadata to `0.006.0.0` (`GIN-PDF-RW_v006`).
  - Compiled native binaries: `Uninstall.exe`, `GIN-PDF-RW.exe`, `GIN-PDF-RW_v006_Setup.exe`.
  - Delivered single installer strictly to `D:\MEGA\DOCS\desktop\GIN-PDF-RW_v006_Setup.exe` and removed older setups.
  - Published GitHub Release `v006`.

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
  - Built all-in-one setup wizard `GIN-PDF-RW_v005_Setup.exe` (`src/Setup.cs`).
