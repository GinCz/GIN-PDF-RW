# GIN-PDF-RW

**High-Performance, Distraction-Free Open Source PDF Reader & Editor**  
*Author:* Vladimir Bulantsev ([@GinCz](https://github.com/GinCz)) | *Version:* `v005` | *Standard:* UTF-8

---

## 🎯 Vision & Objective

`GIN-PDF-RW` is engineered as a native, ultra-lightweight Windows GUI application providing an expansive distraction-free PDF viewing and editing workspace. It completely replaces bloated viewers by removing telemetry, tool clutter, and background services, granting 95%+ of screen space to the document itself.

---

## ⚡ Highlights in `v005`

* **Real Native PDF Rendering Engine:** Integrated GPU-accelerated rendering using Windows 10/11 DirectX WinRT (`Windows.Data.Pdf`) for instant page rasterization and zoom.
* **High-Contrast Modern UI:** Redesigned toolbar tabs with high-contrast readable text in Catppuccin Mocha style.
* **Dedicated Uninstaller (`Uninstall.exe`):** Integrated uninstaller deployed to `C:\Program Files\GIN-PDF-RW\`, accessible from Windows Control Panel.
* **Standalone Setup Wizard (`GIN-PDF-RW_v005_Setup.exe`):**
  * Prompt in English: *"Do you want to install GIN-PDF-RW on this computer?"*.
  * 12 languages with radio buttons (English selected by default): English, Українська, Русский, Čeština, Deutsch, Français, Español, Italiano, Polski, Türkçe, 中文, 日本語.
  * Two intuitive buttons: **Cancel** and **Install**.
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
* **Hotkeys:**
  * `F11` — True borderless fullscreen mode.
  * `Ctrl + P` — Windows print dialog.
  * `Ctrl + S` — Fast Save.
  * `Ctrl + O` — Open file.

---

## 📦 Download

Get the standalone installer directly from GitHub Releases:
* ⚙️ **[GIN-PDF-RW_v005_Setup.exe (GitHub Release v005)](https://github.com/GinCz/GIN-PDF-RW/releases/tag/v005)**
