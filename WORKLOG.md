# Worklog - GIN-PDF-RW

## 2026-11-18 - v007 Embedded Payload Extraction Fix & Turnkey Setup Delivery

- **Target:** Fix setup payload extraction error (`Installation error: Не удается найти указанный файл`) when installing from `GIN-PDF-RW_Setup.exe`.
- **Repository:** `https://github.com/GinCz/GIN-PDF-RW`
- **Actions:**
  - Diagnosed extraction failure:
    - Root cause: In `build.ps1`, the CSC resource argument passed `/resource:...GIN_PDF_RW.exe` (with underscores), while `Setup.cs` requested `GIN-PDF-RW.exe` (with dashes). Because `stream` was null and no separate file was present alongside the standalone setup on Vladimir's desktop, `Process.Start(targetExe)` failed with `FileNotFoundException`.
    - Solution:
      1. Updated `build.ps1` to embed both naming variations (`GIN-PDF-RW.exe` and `GIN_PDF_RW.exe`).
      2. Enhanced `ExtractResource` in `src/Setup.cs` with dynamic resource name resolution across dash/underscore variants.
      3. Added fallback verification before attempting to start the process.
  - Incremented version metadata across source and assemblies to `0.007.0.0` (`GIN-PDF-RW_v007`).
  - Compiled binaries: `GIN-PDF-RW.exe`, `Uninstall.exe`, and `GIN-PDF-RW_v007_Setup.exe`.
  - Delivered single installer strictly to `D:\MEGA\DOCS\desktop\GIN-PDF-RW_v007_Setup.exe` and cleaned up `v006`.
  - Published GitHub Release `v007`.

## 2026-11-18 - v006 Fullscreen Default, Left-Click Flyout, Esc Windowed Mode, 5% Smooth Zoom, Auto FitWidth & 3 Themes

- **Target:** Deliver complete v006 features requested by Vladimir:
  1. Default borderless fullscreen mode on PDF open.
  2. Left click anywhere on document/viewer toggles top menu flyout in fullscreen.
  3. `Esc` key restores normal windowed mode, immediately stretched to full width (`FitWidth`).
  4. Smooth 5% zoom steps (`0.05`) on `+`, `-`, and mouse wheel.
  5. `FitWidth` and `F11` responsive layout stretching PDF to fill full viewport width.
  6. 3-way theme system: Day ☀️, Neutral Cloud ☁️ (in the middle), and Night 🌙.
  7. High-contrast ComboBox styling: dark text on light backgrounds in Day mode, bright text in Night/Cloud modes.
  8. Single installer `GIN-PDF-RW_v006_Setup.exe` with PE version properties (`VladiMIR+AI`).
