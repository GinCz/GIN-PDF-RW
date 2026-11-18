# Worklog - GIN-PDF-RW

## 2026-11-18 - v009 Fit Page, Mouse Wheel Folder Navigation & Crisp Welcome

- **Target:** Deliver refinements requested by Vladimir:
  1. Full page fit without distortion (`DoFitPage()`): PDF document scales to fit both width and height inside the monitor screen/viewport without vertical or horizontal clipping.
  2. Mouse wheel navigation: scrolling up/down flips to previous/next PDF file in the current folder.
  3. Keyboard file navigation: PageUp flips to previous file, PageDown and Space flip to next file.
  4. Fullscreen exit keys: Escape and Enter smoothly restore window to maximized view (`WindowState.Maximized`) with full toolbars visible.
  5. High-resolution crisp welcome sheet: rendered at 200 DPI (1588x2246) with ClearTypeGridFit and AntiAlias for razor-sharp typography without blur or fuzzy scaling.
  6. Multilingual welcome sheets for each language.
- **Repository:** `https://github.com/GinCz/GIN-PDF-RW`
- **Actions:**
  - Updated `src/App.cs`:
    - Replaced width-only fit with `DoFitPage()`: `zoom = Math.Min(scaleX, scaleY)`.
    - Added folder file enumeration and sorting in `NavigateFile(direction)`.
    - Added `PreviewMouseWheel` on `docScroll` to flip through PDF files in the folder (or zoom when `Ctrl` is held).
    - Added `PageUp`, `PageDown`, and `Space` hotkeys for file navigation.
    - Updated `Escape` and `Enter` to restore from fullscreen to maximized window.
    - Redesigned `RenderWelcomeSheet()` with high-resolution vector-like graphics and clean fonts.
  - Updated `src/Setup.cs` and `src/Uninstall.cs` version metadata to `0.009.0.0` (`GIN-PDF-RW_v009`).
  - Compiled binaries: `GIN-PDF-RW.exe`, `Uninstall.exe`, and `GIN-PDF-RW_v009_Setup.exe`.
  - Delivered single installer strictly to `D:\MEGA\DOCS\desktop\GIN-PDF-RW_v009_Setup.exe` and cleaned up `v008`.
  - Published GitHub Release `v009`.

## 2026-11-18 - v008 Day Theme Default, Any-Key Exit to Maximized View, Multilingual Welcome & Localized Uninstall

- **Target:** Implement feedback from Vladimir:
  1. Default theme set to Light (Day).
  2. In fullscreen: pressing ANY key (or left-clicking mouse) exits fullscreen into a fully MAXIMIZED window.
  3. Multilingual Welcome Sheet.
  4. Clean typography: replaced GDI+ unsupported emoji glyphs.
  5. Multilingual Uninstaller (`Uninstall.exe`).
  6. Persistent language persistence: saved to Registry, application directory, and `%APPDATA%`.
