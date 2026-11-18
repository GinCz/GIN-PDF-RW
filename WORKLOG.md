# Worklog - GIN-PDF-RW

## 2026-11-18 - v008 Day Theme Default, Any-Key Exit to Maximized View, Multilingual Welcome & Localized Uninstall

- **Target:** Implement feedback from Vladimir:
  1. Default theme set to Light (Day).
  2. In fullscreen: pressing ANY key (or left-clicking mouse) exits fullscreen into a fully MAXIMIZED window (`WindowState.Maximized`) with standard title bar, window controls, and top/bottom toolbars visible, and PDF stretched to full width without distortion.
  3. Multilingual Welcome Sheet: localized welcome text for all languages (RU, UK, CS, EN, etc.) matching the language selected during setup or in app.
  4. Clean typography: replaced GDI+ unsupported emoji glyphs (which previously caused tofu boxes ``) with clean text brackets and bullet points.
  5. Multilingual Uninstaller (`Uninstall.exe`): detects language from Registry (`HKCU\Software\GIN-PDF-RW\Language`), local configuration, or system locale, displaying title, confirmation prompt, buttons, and status messages in the selected language.
  6. Persistent language persistence: saved to Registry, application directory, and `%APPDATA%\GIN-PDF-RW\language.cfg`, passed directly on post-install launch.
- **Repository:** `https://github.com/GinCz/GIN-PDF-RW`
- **Actions:**
  - Updated `src/App.cs`:
    - Changed default theme initialization to `ApplyTheme("Day")`.
    - Modified `KeyDown` and `PreviewMouseLeftButtonDown` so any key or left click exits fullscreen to `WindowState.Maximized`.
    - Updated `DetectDefaultLanguage(explicitLang)` to inspect command line argument (`-lang`), Registry (`HKCU\Software\GIN-PDF-RW`), local directory, and AppData.
    - Localized `RenderWelcomeSheet()` across all supported languages with clean standard typography.
  - Updated `src/Setup.cs`:
    - Added argument support `--lang <code>` and `--auto` to preserve selected language across UAC re-elevation.
    - Saved language selection to Registry and AppData.
    - Passed `-lang <code\>` upon post-install process start.
  - Updated `src/Uninstall.cs`:
    - Integrated multi-language dictionary for 12 languages (RU, UK, CS, EN, DE, FR, ES, IT, PL, TR, ZH, JA).
    - Detected configured language from Registry and configuration files.
  - Recompiled binaries: `GIN-PDF-RW.exe`, `Uninstall.exe`, and `GIN-PDF-RW_v008_Setup.exe`.
  - Delivered single installer strictly to `D:\MEGA\DOCS\desktop\GIN-PDF-RW_v008_Setup.exe` and cleaned up `v007`.
  - Published GitHub Release `v008`.

## 2026-11-18 - v007 Embedded Payload Extraction Fix & Turnkey Setup Delivery

- **Target:** Fix setup payload extraction error (`Installation error: Не удается найти указанный файл`) when installing from `GIN-PDF-RW_Setup.exe`.
- **Repository:** `https://github.com/GinCz/GIN-PDF-RW`
- **Actions:**
  - Embedded both naming variations (`GIN-PDF-RW.exe` and `GIN_PDF_RW.exe`).
  - Enhanced `ExtractResource` with dynamic resource name resolution.
