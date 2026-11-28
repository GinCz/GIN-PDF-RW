# GIN-PDF-RW — High-Performance Zen PDF Reader & Editor

[![Latest Release](https://img.shields.io/github/v/release/GinCz/GIN-PDF-RW?color=89B4FA&label=Latest%20Version&logo=github&style=for-the-badge)](https://github.com/GinCz/GIN-PDF-RW/releases/latest)
[![Direct Download](https://img.shields.io/badge/Download-Latest%20Installer%20(1--Click)-a6e3a1?style=for-the-badge&logo=windows&logoColor=1e1e2e)](https://github.com/GinCz/GIN-PDF-RW/releases/download/v021/GIN-PDF-RW_v021_Setup.exe)
[![Platform](https://img.shields.io/badge/Platform-Windows%2010%20%2F%2011%20(x64%2Farm64)-89b4fa?style=for-the-badge&logo=windows)](https://github.com/GinCz/GIN-PDF-RW)
[![License](https://img.shields.io/badge/License-MIT-f9e2af?style=for-the-badge)](LICENSE)
[![Author](https://img.shields.io/badge/Author-Vladimir%20Bulantsev%20(@GinCz)-cba6f7?style=for-the-badge&logo=github)](https://github.com/GinCz)

> **Fast, distraction-free Open Source PDF Reader & Interactive Editor engineered in pure C# / WPF with hardware WinRT vector rendering, instant Escape exit, AcroForms fill-in, cryptographic PAdES X.509 digital signatures, and dynamic Hi-DPI sub-pixel typography.**

---

## ⚡ Direct 1-Click Download

Get the official latest release directly without complicated package managers or setups:

| Release Package | Type | Target Architecture | Direct Link |
| :--- | :--- | :--- | :--- |
| **`GIN-PDF-RW_v021_Setup.exe`** | Standalone Native GUI Installer with Icon | Windows 10 / 11 (x64, x86, ARM64) | [📥 Download Now (1-Click)](https://github.com/GinCz/GIN-PDF-RW/releases/download/v021/GIN-PDF-RW_v021_Setup.exe) |
| **All Releases & History** | GitHub Release Archive | All versions & changelogs | [📦 View All Releases](https://github.com/GinCz/GIN-PDF-RW/releases) |

---

## 🎯 Key Capabilities & Highlights

### 🔍 Dynamic Hi-DPI Vector Re-Rendering on Zoom
Unlike conventional viewers that stretch static pre-rendered raster images, **GIN-PDF-RW** dynamically interfaces with the native Windows WinRT hardware rendering engine (`Windows.Data.Pdf.PdfDocument`). 
- As you zoom in using `Ctrl + Mouse Wheel` or `+` / `-` keys, document vector curves, fonts, and complex illustrations are dynamically re-rasterized at native display sub-pixel resolution.
- Enjoy 100% crystal-clear typography without blur or fuzzy scaling, exactly matching Adobe Acrobat Pro and Foxit Reader.

### 🚪 Instant Escape Close (Zen Mode)
Designed for seamless document previews:
- Opening a PDF automatically launches into full-screen distraction-free reading mode.
- Pressing `Escape` at any time (even after scrolling down through multi-page files or zooming in) immediately exits and closes the entire application in a fraction of a millisecond.

### 🖱️ 16pt Right-Click Quick Actions Context Menu
Right-click anywhere on the document viewport to trigger a responsive, high-contrast context menu with 16pt typography and intuitive icons:
- `↷ Rotate Right 90°`
- `↶ Rotate Left 90°`
- `↻ Rotate 180°`
- `↺ Rotate Left 180°`
- `⇆ Flip Horizontal`
- `⇅ Flip Vertical`
- `🖨️ Print Document`
- `❌ Close Application`
All transforms execute via hardware GPU acceleration across all pages simultaneously.

### 📝 Interactive AcroForms & Click-to-Fill Engine
- Complete interactive fillable forms directly inside the document.
- Click to place checkboxes (`✓`), date fields, and custom text inputs in-place.
- Ready for digital workflows without cumbersome cloud subscriptions.

### 🔏 Cryptographic X.509 PAdES Digital Signatures
- Sign PDF contracts, invoices, and documents locally with Windows Personal Certificate Store or custom PKCS#12 (`.pfx`) digital certificates.
- Zero cloud uploads: complete on-device cryptographic security and GDPR compliance.

### ✏ Edit Existing Text & Insert Images
- **Edit Text** tool: click any line of real (non-scanned) PDF text, it becomes an editable text box that covers the original line.
- **Stamp / Image**: insert PNG/JPG, drag to move, drag the corner handle to resize with the original aspect ratio locked, `Delete` removes it.
- Undo / Redo (`Ctrl+Z` / `Ctrl+Y`), Save, Save As (`Ctrl+Shift+S`), Save & Exit, Cancel. Powered by [PdfPig](https://github.com/UglyToad/PdfPig) (Apache-2.0) for text positions.

### 🔄 On-Demand GitHub Version Checker
- Integrated non-intrusive `v021 (Update)` toolbar button to verify new releases on GitHub without unsolicited background network polls.

---

## 🚀 Installation & System Requirements

- **Supported OS:** Windows 10 / Windows 11 (all editions, 64-bit and 32-bit).
- **Prerequisites:** .NET Framework 4.8 (pre-installed by default on Windows 10 and 11).
- **Zero Dependencies:** Pure standalone portable architecture. No external runtimes or heavy bloated frameworks needed.

Simply download and launch **`GIN-PDF-RW_v021_Setup.exe`**.

---

## ⌨️ Keyboard & Mouse Shortcuts

| Key / Gesture | Function |
| :--- | :--- |
| `Escape` | Instant application exit (in fullscreen reading mode) |
| `Ctrl + Mouse Wheel` | Smooth zoom in / zoom out with dynamic sub-pixel re-rendering |
| `+` / `-` | Zoom in / Zoom out |
| `Enter` / `Return` | Toggle Fullscreen / Windowed Editor view |
| `Middle Mouse Click` | Cycle Fullscreen mode |
| `Right Mouse Click` | Open 16pt Quick Transform & Print Menu |
| `PageUp` / `PageDown` | Open previous / next PDF document in directory |
| `Ctrl + O` | Open PDF file picker |
| `Ctrl + S` | Save signed / edited PDF document |
| `Ctrl + P` | Print document |

---

## 📋 Version History

<details>
<summary><b>Click to expand full changelog</b></summary>

### v021 (Latest)
* Added on-demand GitHub version update checker in toolbar (`v021 Update`).
* Enhanced in-place text editing with PdfPig bounding-box coordinate detection.
* 16pt high-contrast context menu with instant GPU-accelerated page transforms.
* PAdES cryptographic digital signature engine with local Windows certificate store integration.
* Portable Setup executable packaged in `releases/`.

### v020
* Initial standalone release with native WinRT hardware-accelerated PDF rasterization.

</details>

---

## 🏷️ Tags & Topics
`#pdf-reader` `#pdf-editor` `#wpf` `#csharp` `#winrt` `#hi-dpi` `#open-source` `#acroforms` `#digital-signature` `#pades` `#windows11` `#lightweight` `#zen-mode`

---

## 📄 License
Licensed under the [MIT License](LICENSE).  
Copyright © 2026 Vladimir Bulantsev ([@GinCz](https://github.com/GinCz)). All rights reserved.
