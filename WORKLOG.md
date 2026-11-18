# Worklog - GIN-PDF-RW

## 2026-11-18 - v010 Left-Click Drag Panning, Mouse Wheel Scroll & Cyclical Fullscreen Toggle

- **Target:** Deliver refined navigation requested by Vladimir:
  1. Left-click drag panning: user can hold left mouse button and smoothly drag the document up, down, left, and right when zoomed in.
  2. Clicking mouse wheel (middle click) or pressing Enter cyclically toggles between borderless fullscreen mode and normal window mode with top menu buttons visible.
  3. Mouse wheel scrolling: rolling wheel scrolls the document down and up naturally; when reaching the bottom or top of a page, it flips to the next/previous page of multi-page PDFs.
  4. Quick zoom keys: `+` and `-` on numeric keypad (Numpad) or top-right of main keyboard (near Backspace) zoom in/out with 5% step, with or without Ctrl.
  5. Ctrl + Mouse Wheel provides instant 5% incremental zoom.
  6. PageUp and PageDown flip through PDF files in the current folder.
  7. Escape smoothly restores window from fullscreen to maximized view.
- **Repository:** `https://github.com/GinCz/GIN-PDF-RW`
- **Actions:**
  - Updated `src/App.cs`:
    - Implemented mouse drag panning via `PreviewMouseLeftButtonDown`, `PreviewMouseMove`, `PreviewMouseLeftButtonUp`, and `LostMouseCapture` on `docScroll`.
    - Implemented middle-click mouse wheel toggle via `PreviewMouseDown` on `docScroll` and `MainWindow`.
    - Implemented natural vertical document scrolling with page boundary transition in `docScroll.PreviewMouseWheel`.
    - Added Enter/Return cyclical fullscreen toggle and `+`/`-` zoom hotkeys in `KeyDown`.
    - Updated `container` alignment in `ApplyZoomToPage()` to ensure full edge-to-edge scrolling when zoomed.
    - Updated version metadata to `0.010.0.0` (`GIN-PDF-RW_v010`).
  - Updated `src/Setup.cs` and `src/Uninstall.cs` version metadata to `0.010.0.0` (`GIN-PDF-RW_v010`).
  - Compiled binaries: `GIN-PDF-RW.exe`, `Uninstall.exe`, and `GIN-PDF-RW_v010_Setup.exe`.
  - Delivered installer to `D:\MEGA\DOCS\desktop\GIN-PDF-RW_v010_Setup.exe` (retained 2 latest: `v009` and `v010`).
  - Updated `version.json`, `README.md`, and `WORKLOG.md`.

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
