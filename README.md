# GIN-PDF-RW

**High-Performance, Distraction-Free Open Source PDF Reader & Editor**  
*Author:* Vladimir Bulantsev ([@GinCz](https://github.com/GinCz)) | *Version:* `v013` | *Standard:* UTF-8

---

## 🎯 Vision & Objective

`GIN-PDF-RW` is engineered as a native, ultra-lightweight Windows GUI application providing an expansive distraction-free PDF viewing and editing workspace. It completely replaces bloated viewers by removing telemetry, tool clutter, and background services, granting 95%+ of screen space to the document itself.

---

## ⚡ Highlights in `v013`

* **Persistent Theme Memory (Cross-Session Preservation):**
  * Switching between Day (☀️), Evening/Cloud (☁️), and Night (🌙) themes is now persistently saved to Windows Registry (`HKCU\Software\GIN-PDF-RW\Theme`), `%APPDATA%\GIN-PDF-RW\theme.cfg`, and application root.
  * Closing and reopening the application reliably preserves the user's selected theme instead of resetting back to Day mode.
* **High-Contrast Theme-Adaptive Language Switcher:**
  * Custom WPF `ControlTemplate` and item container style for the top-right language dropdown (`cmbLang`).
  * Seamlessly adapts its button background, foreground typography, borders, drop-down popup, and hover highlights to Day, Evening/Cloud, and Night modes.
  * Completely eliminates the white-on-white text readability defect when operating in Evening or Night themes.
* **Persistent Language Selection:**
  * Switching languages via the toolbar dropdown instantly saves the preference to registry and configuration files, preserving the user's selection permanently.
* **Left-Click Drag Panning (Hand Tool):** When zoomed in or when document content exceeds the viewport, click and drag with the left mouse button to smoothly pan and glide across the document up, down, left, and right.
* **Scroll Wheel Click (Middle Click) & Enter Cyclical Fullscreen Toggle:**
  * Clicking down on the mouse wheel button itself (middle click) or pressing `Enter` cyclically switches between borderless fullscreen reading mode and normal maximized window mode with full toolbar menu visible.
  * Preserves user manual zoom level and viewport state when toggling.
* **Natural Mouse Wheel Scrolling & Page Navigation:**
  * Rolling the mouse wheel scrolls the document up and down naturally.
  * When reaching the bottom of the page, continuing to scroll down flips to the next page from the top; scrolling up at the top flips to the previous page.
* **Keyboard Hotkeys & Zoom Controls:**
  * **Page Navigation / File Switching:** `PageUp` switches to previous PDF file in the folder, `PageDown` switches to next PDF file.
  * **Quick Zoom:** Pressing `+` or `-` (on the numeric keypad or top-right of main keyboard near Backspace) smoothly zooms in/out by 5%, with or without `Ctrl`.
  * **Precision Wheel Zoom:** Holding `Ctrl` while rolling the mouse wheel provides instant 5% incremental zoom.
  * **Escape Key:** Exits fullscreen into maximized window with full toolbars.
* **Fit Page (Zero Clipping / Zero Distortion):** Automatically scales the PDF document to fit both width and height completely inside the viewport, ensuring 100% of the page is visible on the screen.
* **High-Definition, Razor-Sharp Welcome Sheet:**
  * High-resolution 200 DPI canvas (1588x2246) with ClearType anti-aliasing for crystal-clear readability without any blur or fuzziness.
  * Localized in Russian, Ukrainian, Czech, and English.
* **Day Mode by Default:** Clean, high-contrast light theme enabled by default on pristine install.
* **Fully Localized Uninstaller (`Uninstall.exe`):**
  * Automatically detects installation language from Windows Registry (`HKCU\Software\GIN-PDF-RW\Language`) or configuration files.
  * Displays prompts, dialogs, buttons, and confirmations in the user's language across 12 languages.
* **3-Way Theme Switcher:** 
  * ☀️ **Day Mode (Default):** Clean white/light-gray background with dark high-contrast typography.
  * ☁️ **Neutral Cloud Mode (Center):** Soft balanced slate-gray theme for reduced eye strain.
  * 🌙 **Night Mode:** Deep Catppuccin Mocha dark theme for night reading.
* **Real Native WinRT PDF Rendering Engine:** Integrated GPU-accelerated rendering using Windows 10/11 DirectX WinRT (`Windows.Data.Pdf`) for instant page rasterization.
* **Standalone Setup Wizard (`GIN-PDF-RW_v013_Setup.exe`):**
  * Self-extracts `GIN-PDF-RW.exe`, `Uninstall.exe`, and `app.ico` into `C:\Program Files\GIN-PDF-RW\`.
  * Auto-associates `.pdf` files and creates Desktop shortcuts.
* **Interactive AcroForms Engine:** Clickable checkboxes with instant `✓` ticks and editable text fields.
* **Electronic & Cryptographic Signatures:**
  * Visual hand-drawn signature canvas.
  * X.509 PAdES digital signing via Windows Certificate Store.
* **Universal UTF-8 Encoding:** Full support for Czech, Ukrainian, Russian, and all global alphabets.

---

## 📦 Download

Get the standalone installer directly from GitHub Releases:
* ⚙️ **[GIN-PDF-RW_v013_Setup.exe (GitHub Release v013)](https://github.com/GinCz/GIN-PDF-RW/releases/tag/v013)**
