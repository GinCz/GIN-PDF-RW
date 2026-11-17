# GIN-PDF-RW

**High-Performance, Distraction-Free Open Source PDF Reader & Editor**  
*Author:* Vladimir Bulantsev ([@GinCz](https://github.com/GinCz)) | *Version:* `v003` | *Standard:* UTF-8

---

## 🎯 Vision & Objective

`GIN-PDF-RW` is engineered as a native, ultra-lightweight Windows GUI application providing an expansive distraction-free PDF viewing and editing workspace. It completely replaces bloated viewers by removing telemetry, tool clutter, and background services, granting 95%+ of screen space to the document itself.

---

## ⚡ Highlights in `v003`

* **Native Windows GUI Executable:** Compiled directly as Win32 GUI (`winexe`) with embedded high-resolution `app.ico` and PE metadata (`VladiMIR+AI`).
* **Zero Console Flashing:** Completely silent launch with zero black terminal windows or console flicker.
* **Standalone Setup Wizard (`Setup.exe`):**
  * Clean confirmation prompt in English: *"Do you want to install GIN-PDF-RW on this computer?"*.
  * 10 language choices via radio buttons with English selected by default (English, Russian, Czech, German, French, Spanish, Italian, Polish, Chinese, Japanese).
  * Two intuitive buttons: **Cancel** and **Install**.
  * Auto-extracts into `C:\Program Files\GIN-PDF-RW`, associates `.pdf` files, and places Desktop shortcuts.
* **Interactive AcroForms Engine:** Clickable checkboxes with instant `✓` ticks and editable text fields.
* **Dual-Mode Workflow:**
  * **Reader & Forms Mode (Default):** Rapid rendering, search, and form completion.
  * **Editor Mode:** In-place text modification, transparent PNG stamp/image insertion, and freeform annotations.
* **Electronic & Cryptographic Signatures:**
  * Visual hand-drawn signature canvas.
  * X.509 PAdES digital signing via Windows Certificate Store.
* **Universal UTF-8 Encoding:** Full support for Czech diacritics (`ě, š, č, ř, ž`), Cyrillic, and Asian alphabets.
* **Hotkeys:**
  * `F11` — True borderless fullscreen mode.
  * `Ctrl + P` — Windows print dialog.
  * `Ctrl + S` — Fast Save.
  * `Ctrl + O` — Open file.

---

## 📦 Download

Get the standalone installer directly from GitHub Releases:
* ⚙️ **[Setup.exe (GitHub Release v003)](https://github.com/GinCz/GIN-PDF-RW/releases/tag/v003)**
