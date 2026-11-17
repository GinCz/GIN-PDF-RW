# GIN-PDF-RW

**High-Performance, Distraction-Free Open Source PDF Reader & Editor**  
*Author:* Vladimir Bulantsev ([@GinCz](https://github.com/GinCz)) | *Version:* `v001` | *Standard:* UTF-8

---

## 🎯 Vision & Objective

`GIN-PDF-RW` is engineered to provide an ultra-lightweight, lightning-fast, and completely distraction-free PDF reading and editing experience on Windows. It eliminates cluttered legacy toolbars, mandatory cloud subscriptions, telemetry, and background bloat, giving 95%+ of screen space to the document itself.

---

## ⚡ Key Highlights

* **Distraction-Free Zen View:** Ultra-slim, auto-hiding top toolbar designed for maximum document readability.
* **Instant Start (< 0.2s):** Minimal memory footprint, portable executable without background services.
* **Interactive AcroForms Engine:** Full support for form fields, checkboxes, radio buttons, and dropdown lists.
* **Unified Dual-Mode Workflow:**
  * **Reader & Form Fill Mode (Default):** Fluid zooming, search, and form filling.
  * **Editor Mode (1-Click Switch):** In-place text modification, image insertion, stamps, and geometric shapes.
* **Electronic & Cryptographic Signatures:**
  * Visual facsimile drawing (mouse, stylus, or transparent image).
  * X.509 / PAdES cryptographic signing using local Windows Certificate Store.
* **Strict UTF-8 Standard:** Native support for all global languages, European diacritics, Cyrillic, and Asian alphabets.
* **Multilingual Setup:** Smart installer with automatic Windows locale detection and manual language override.
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

## 🏗️ Architecture Stack

* **Rendering Engine:** Google PDFium C++ core (high-speed hardware rasterization, AcroForms).
* **Application Framework:** C# / .NET 9 with WPF / Native Windows UI.
* **PDF Manipulation & Font Subsetting:** Strict UTF-8 Unicode mapping for seamless text editing.
* **Digital Signatures:** System Cryptography X.509 / PKCS#7 PAdES container compliance.
* **Packaging:** Single portable standalone executable and lightweight Inno Setup installer.

---

## 🗺️ Project Milestones

* **v001:** Core architecture specification, repository setup, and dual-mode pipeline design.
* **v002:** PDFium rendering pipeline integration, high-DPI scaling, and F11 fullscreen view.
* **v003:** AcroForm field editing, checkbox state preservation, and flat field saving.
* **v004:** In-place text and image overlay editing engine.
* **v005:** Visual and X.509 cryptographic signature engine.
* **v006:** Multilingual installer and automated release pipeline.
