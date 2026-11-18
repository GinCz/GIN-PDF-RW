# GIN-PDF-RW

**High-Performance, Distraction-Free Open Source PDF Reader & Editor**  
*Author:* Vladimir Bulantsev ([@GinCz](https://github.com/GinCz)) | *Version:* `v009` | *Standard:* UTF-8

---

## 🎯 Vision & Objective

`GIN-PDF-RW` is engineered as a native, ultra-lightweight Windows GUI application providing an expansive distraction-free PDF viewing and editing workspace. It completely replaces bloated viewers by removing telemetry, tool clutter, and background services, granting 95%+ of screen space to the document itself.

---

## ⚡ Highlights in `v009`

* **Fit Page (Zero Clipping / Zero Distortion):** Automatically scales the PDF document to fit both width and height completely inside the viewport, ensuring 100% of the page is visible on the screen.
* **Rapid File Browsing in Folder:**
  * **Mouse Wheel:** Scroll up/down to flip immediately to the previous/next PDF file in the folder.
  * **Hotkeys:** Press `PageUp` for previous file, `PageDown` or `Space` for next file.
* **Fullscreen & Smooth Window Restoration:**
  * Documents launch in borderless fullscreen by default (`F11`).
  * Pressing `Escape` or `Enter` restores the app to a fully **MAXIMIZED** window (`WindowState.Maximized`) with complete standard window controls, top toolbar, and bottom status bar.
* **High-Definition, Razor-Sharp Welcome Sheet:**
  * High-resolution 200 DPI canvas (1588x2246) with ClearType anti-aliasing for crystal-clear readability without any blur or fuzziness.
  * Localized in Russian, Ukrainian, Czech, and English.
* **Day Mode by Default:** Clean, high-contrast light theme enabled by default for optimal readability.
* **Fully Localized Uninstaller (`Uninstall.exe`):**
  * Automatically detects installation language from Windows Registry (`HKCU\Software\GIN-PDF-RW\Language`) or configuration files.
  * Displays prompts, dialogs, buttons, and confirmations in the user's language across 12 languages.
* **Ultra-Smooth 5% Zoom:** Precise `0.05` zoom steps on `+`, `-`, and mouse wheel (with `Ctrl`).
* **3-Way Theme Switcher:** 
  * ☀️ **Day Mode (Default):** Clean white/light-gray background with dark high-contrast typography.
  * ☁️ **Neutral Cloud Mode (Center):** Soft balanced slate-gray theme for reduced eye strain.
  * 🌙 **Night Mode:** Deep Catppuccin Mocha dark theme for night reading.
* **Real Native WinRT PDF Rendering Engine:** Integrated GPU-accelerated rendering using Windows 10/11 DirectX WinRT (`Windows.Data.Pdf`) for instant page rasterization.
* **Standalone Setup Wizard (`GIN-PDF-RW_v009_Setup.exe`):**
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
* ⚙️ **[GIN-PDF-RW_v009_Setup.exe (GitHub Release v009)](https://github.com/GinCz/GIN-PDF-RW/releases/tag/v009)**
