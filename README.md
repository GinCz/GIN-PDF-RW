# GIN-PDF-RW

**High-Performance, Distraction-Free Open Source PDF Reader & Editor**  
*Author:* Vladimir Bulantsev ([@GinCz](https://github.com/GinCz)) | *Version:* `v008` | *Standard:* UTF-8

---

## 🎯 Vision & Objective

`GIN-PDF-RW` is engineered as a native, ultra-lightweight Windows GUI application providing an expansive distraction-free PDF viewing and editing workspace. It completely replaces bloated viewers by removing telemetry, tool clutter, and background services, granting 95%+ of screen space to the document itself.

---

## ⚡ Highlights in `v008`

* **Day Mode by Default:** Clean, high-contrast light theme enabled by default for optimal readability.
* **Smart Fullscreen & Any-Key Exit to Maximized View:** 
  * Documents launch in immersive borderless fullscreen mode (`F11`).
  * Pressing **ANY key** (Escape, Space, Enter, arrows, etc.) or **Left-Clicking** immediately exits fullscreen into a fully **MAXIMIZED** window (`WindowState.Maximized`) with standard title bar and accessible top/bottom toolbars.
  * PDF document is maximally stretched to 100% of the available window width without distortion (`FitWidth`).
* **Multilingual Welcome Screen & Clean Typography:**
  * Welcome sheet text rendered in the user's chosen installation language (English, Русский, Українська, Čeština, etc.).
  * Eliminated GDI+ emoji rendering artifacts (no tofu boxes ``).
* **Fully Localized Uninstaller (`Uninstall.exe`):**
  * Automatically detects installation language from Windows Registry (`HKCU\Software\GIN-PDF-RW\Language`) or configuration files.
  * Displays prompts, dialogs, buttons, and confirmations in the user's language across 12 languages.
* **Persistent Language Architecture:**
  * Selected language is retained across UAC elevation, written to Registry, local directory, and `%APPDATA%`.
* **Ultra-Smooth 5% Zoom:** Precise `0.05` zoom steps on `+`, `-`, and mouse wheel.
* **3-Way Theme Switcher:** 
  * ☀️ **Day Mode (Default):** Clean white/light-gray background with dark high-contrast typography.
  * ☁️ **Neutral Cloud Mode (Center):** Soft balanced slate-gray theme for reduced eye strain.
  * 🌙 **Night Mode:** Deep Catppuccin Mocha dark theme for night reading.
* **High-Contrast ComboBox Styling:** High-contrast text colors across all themes (no white text on white backgrounds).
* **Real Native WinRT PDF Rendering Engine:** Integrated GPU-accelerated rendering using Windows 10/11 DirectX WinRT (`Windows.Data.Pdf`) for instant page rasterization.
* **Standalone Setup Wizard (`GIN-PDF-RW_v008_Setup.exe`):**
  * Self-extracts `GIN-PDF-RW.exe`, `Uninstall.exe`, and `app.ico` into `C:\Program Files\GIN-PDF-RW\`.
  * Auto-associates `.pdf` files and creates Desktop shortcuts.
* **Interactive AcroForms Engine:** Clickable checkboxes with instant `✓` ticks and editable text fields.
* **Dual-Mode Workflow:**
  * **Reader & Forms Mode (Default):** Rapid rendering, search, and form completion.
  * **Editor Mode:** In-place text modification, transparent PNG stamp/image insertion, and freeform annotations.
* **Electronic & Cryptographic Signatures:**
  * Visual hand-drawn signature canvas.
  * X.509 PAdES digital signing via Windows Certificate Store.
* **Universal UTF-8 Encoding:** Full support for Czech, Ukrainian, Russian, and all global alphabets.

---

## 📦 Download

Get the standalone installer directly from GitHub Releases:
* ⚙️ **[GIN-PDF-RW_v008_Setup.exe (GitHub Release v008)](https://github.com/GinCz/GIN-PDF-RW/releases/tag/v008)**
