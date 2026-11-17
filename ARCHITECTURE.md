# GIN-PDF-RW Architecture & Technical Specification

> Version: `v001` | Encoding: `UTF-8` | Target: Windows 10 / 11 (x64)

---

## 1. System Layers

```
+-----------------------------------------------------------+
|                      Presentation Layer                   |
|   Zen Toolbar  |  Document Viewport  |  Status / Mini-Bar  |
+-----------------------------------------------------------+
                             |
+-----------------------------------------------------------+
|                      Controller Layer                     |
|    Mode Controller (Reader <-> Editor)  |  Hotkey Manager  |
+-----------------------------------------------------------+
       |                             |               |
+---------------+            +---------------+ +---------------+
|  Form Engine  |            | Editor Engine | | Crypto Engine |
| AcroForm Sync |            | Text / Images | | X.509 / PAdES |
+---------------+            +---------------+ +---------------+
       |                             |               |
+-----------------------------------------------------------+
|                  Core Engine (Google PDFium)              |
|        Rasterization  |  Page Geometry  |  PDF Stream     |
+-----------------------------------------------------------+
```

---

## 2. Core Modules

### 2.1 PDFium Rendering Pipeline
* Interacts with native `pdfium.dll` through high-performance C# P/Invoke bindings.
* Implements tiled multi-resolution caching to provide 60 FPS viewport scrolling and zooming.
* Respects Windows Per-Monitor V2 DPI scaling for crystal clear font rendering on 4K displays.

### 2.2 Form & Annotation Engine
* Detects standard PDF AcroForm dictionaries (`/AcroForm`, `/Fields`).
* Automatically maps interactive text boxes, checkboxes, dropdown lists, and radio buttons into interactive native controls.
* Supports saving both editable interactive forms and flattened immutable archives.

### 2.3 Editor Engine (In-Place Modifications)
* When switching to Editor Mode via the toolbar button:
  * Identifies text blocks, bounding boxes, and font metrics.
  * Allows direct text editing with UTF-8 character encoding.
  * Supports inserting raster images (PNG, JPEG) with alpha transparency (stamps and company seals).

### 2.4 Cryptographic Digital Signatures
* Integrates with the Windows Certificate Store (`CurrentUser\My` and `LocalMachine\My`).
* Supports hardware security tokens (smart cards, USB tokens).
* Signs document revisions according to the PDF 1.7 / 2.0 PAdES specification without invalidating existing revisions.

### 2.5 Multilingual Installer & UI Localization
* Detects system locale at launch (`ru-RU`, `cs-CZ`, `en-US`, `de-DE`).
* Provides single-click manual override in UI and setup dialog.
* Stores translations in clean, structured UTF-8 JSON localization packs.
