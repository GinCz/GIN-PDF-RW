# GIN-PDF-RW

**High-Performance, Distraction-Free Open Source PDF Reader & Editor**  
*Author:* Vladimir Bulantsev ([@GinCz](https://github.com/GinCz)) | *Version:* `v002` | *Standard:* UTF-8

---

## 🎯 Vision & Objective

`GIN-PDF-RW` is engineered to provide an ultra-lightweight, lightning-fast, and completely distraction-free PDF reading and editing experience on Windows. It eliminates cluttered legacy toolbars, mandatory cloud subscriptions, telemetry, and background bloat, giving 95%+ of screen space to the document itself.

---

## ⚡ Key Highlights in `v002`

* **Distraction-Free Zen View:** Ultra-slim top toolbar designed for maximum document readability.
* **Instant Start (< 0.2s):** Minimal memory footprint, portable standalone execution without background services.
* **Interactive AcroForms Engine:** Full support for form fields, checkboxes, radio buttons, and dropdown lists.
* **Unified Dual-Mode Workflow:**
  * **Reader & Form Fill Mode (Default):** Fluid zooming, search, and 1-click form filling.
  * **Editor Mode (1-Click Switch):** In-place text modification, stamp/image insertion, and checkmarks anywhere on the sheet.
* **Electronic & Cryptographic Signatures:**
  * Visual facsimile drawing (mouse, stylus, or transparent image).
  * X.509 / PAdES cryptographic signing using local Windows Certificate Store.
* **Strict UTF-8 Standard:** Native support for all global languages, European diacritics (Czech `ě, š, č, ř, ž`), Cyrillic, and Asian alphabets.
* **Multilingual Setup:** Smart installer with automatic Windows locale detection and manual language override (English, Русский, Čeština).
* **Zero Telemetry & 100% Privacy:** All processing remains strictly local and offline.

---

## ⌨️ Core Hotkeys

| Shortcut | Action |
| :--- | :--- |
| `F11` | Toggle borderless Fullscreen reading mode |
| `Ctrl + P` | Native Windows Print dialog |
| `Ctrl + S` | Fast Save changes |
| `Ctrl + O` | Open document |
| `Ctrl + +` / `Ctrl + -` | Zoom in / Zoom out |
| `Ctrl + 0` | Fit to width / page |
| `Esc` | Exit fullscreen or cancel tool |

---

## 🚀 Quick Start & Installation

### Option 1: Direct Portable Run
Simply double-click `GIN-PDF-RW.cmd` in the repository root.

### Option 2: Full System Installation
Double-click `Setup.cmd` to:
1. Auto-detect your Windows language (English, Russian, or Czech) with manual override.
2. Install into `C:\Program Files\GIN-PDF-RW`.
3. Associate `.pdf` extension so all PDFs open in GIN-PDF-RW by default.
4. Place shortcuts on Desktop and Vladimir's MEGA Desktop (`D:\MEGA\DOCS\desktop\`).

---

## 🏗️ Technical Architecture

* **UI Layer:** Hardware-accelerated WPF with Segoe UI Variable, dark Catppuccin theme (`#181825`), and responsive canvas scaling.
* **Form & Interaction Layer:** Dynamic XAML overlay canvas mapping AcroForm dictionaries to interactive controls.
* **Cryptography Layer:** Windows Certificate Store integration (`System.Security.Cryptography.X509Certificates`) for legal PAdES digital signatures.
* **Localization:** Modular UTF-8 dictionary manager with live on-the-fly switching.
