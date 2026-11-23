# GIN-PDF-RW — High-Performance Zen PDF Reader & Editor

[![Latest Release](https://img.shields.io/github/v/release/GinCz/GIN-PDF-RW?color=89B4FA&label=Latest%20Version&logo=github&style=for-the-badge)](https://github.com/GinCz/GIN-PDF-RW/releases/latest)
[![Direct Download](https://img.shields.io/badge/Download-Latest%20Installer%20(1--Click)-a6e3a1?style=for-the-badge&logo=windows&logoColor=1e1e2e)](https://github.com/GinCz/GIN-PDF-RW/releases/latest/download/GIN-PDF-RW_v014_Setup.cmd)
[![Platform](https://img.shields.io/badge/Platform-Windows%2010%20%2F%2011%20(x64%2Farm64)-89b4fa?style=for-the-badge&logo=windows)](https://github.com/GinCz/GIN-PDF-RW)
[![License](https://img.shields.io/badge/License-MIT-f9e2af?style=for-the-badge)](LICENSE)
[![Author](https://img.shields.io/badge/Author-Vladimir%20Bulantsev%20(@GinCz)-cba6f7?style=for-the-badge&logo=github)](https://github.com/GinCz)

> **Fast, distraction-free Open Source PDF Reader & Interactive Editor engineered in pure C# / WPF with hardware WinRT vector rendering, instant Escape exit, AcroForms fill-in, cryptographic PAdES X.509 digital signatures, and dynamic Hi-DPI sub-pixel typography.**

---

## ⚡ Direct 1-Click Download

Get the official latest release directly without complicated package managers or setups:

| Release Package | Type | Target Architecture | Direct Link |
| :--- | :--- | :--- | :--- |
| **`GIN-PDF-RW_v014_Setup.cmd`** | Standalone Self-Compiling Installer | Windows 10 / 11 (x64, x86, ARM64) | [📥 Download Now (1-Click)](https://github.com/GinCz/GIN-PDF-RW/releases/latest/download/GIN-PDF-RW_v014_Setup.cmd) |
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
- `↷ Rotate Right 90°` (`Повернуть направо на 90°`)
- `↶ Rotate Left 90°` (`Повернуть налево на 90°`)
- `↻ Rotate 180°` (`Повернуть на 180°`)
- `⇆ Flip Horizontal` (`Отразить по горизонтали`)
- `⇅ Flip Vertical` (`Отразить по вертикали`)
- `🖨️ Print Document` (`Печать документа`)
- `❌ Close Application` (`Закрыть программу`)
All transforms execute via hardware GPU acceleration across all pages simultaneously.

### 📝 Interactive AcroForms & Click-to-Fill Engine
- Complete interactive fillable forms directly inside the document.
- Click to place checkboxes (`✓`), date fields, and custom text inputs in-place.
- Ready for digital workflows without cumbersome cloud subscriptions.

### 🔏 Cryptographic PAdES X.509 Digital Signatures
- Compliant with European eIDAS and international PAdES cryptographic standards.
- Select your personal hardware or software X.509 certificate directly from the Windows Certificate Store (`CurrentUser\My`) to sign documents cryptographically.

### 🎨 3 Adaptive Ergonomic Themes
Switch instantly between three calibrated palettes engineered for day and night workflows:
- ☀️ **Day (Light Theme):** High-contrast crisp paper white.
- ☁️ **Cloud (Neutral Theme):** Balanced gray tone for reduced eye strain during extended reading.
- 🌙 **Night (Dark Theme):** Deep contrast Catppuccin Mocha aesthetic.
Theme and language choices persist permanently across reboots and sessions.

---

## ⌨️ Productivity Keyboard & Mouse Shortcuts

| Shortcut | Action |
| :--- | :--- |
| `Escape` | **Instant Exit / Close Application** |
| `Ctrl + Mouse Wheel` | Smooth 5% incremental zoom with dynamic vector re-rendering |
| `+` / `-` (Numpad or Keyboard) | Fast 5% zoom in / zoom out |
| `Enter` / `Return` | Toggle borderless fullscreen reading mode |
| `Middle Click (Wheel)` | Cyclical toggle between Zen fullscreen and normal window |
| `Left Mouse Drag` | Smooth viewport panning when zoomed in |
| `Mouse Wheel Scroll` | Natural vertical document scrolling with automatic page flips |
| `PageUp` / `PageDown` | Flip through PDF documents in the current directory |
| `Ctrl + O` | Open document file dialog |
| `Ctrl + S` | Save document |
| `Ctrl + P` | Fast native Windows printing |
| `Right Click` | Open 16pt Context Menu with rotation and flip tools |

---

## 🛠️ Architecture & Standalone Build

**GIN-PDF-RW** strictly adheres to zero-dependency principles:
- **Language & Runtime:** C# (.NET Framework 4.0 / 4.8 / Windows 10 & 11 native runtime).
- **GUI Engine:** Hardware-accelerated Windows Presentation Foundation (WPF).
- **PDF Core:** Native Windows WinRT COM pipeline (`Windows.Data.Pdf`).
- **Compilation:** Self-contained polyglot script that compiles instantaneously using the built-in Microsoft compiler (`csc.exe`):

```cmd
:: Quick compilation from developer console
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /nologo /target:winexe /optimize+ /platform:anycpu ^
    /win32icon:app.ico ^
    /r:PresentationFramework.dll,PresentationCore.dll,WindowsBase.dll,System.Xaml.dll,System.dll,System.Drawing.dll,System.Windows.Forms.dll,System.Core.dll,Microsoft.CSharp.dll ^
    /out:GIN-PDF-RW.exe src\App.cs
```

---

## 👨‍💻 Author & Project Ecosystem

Developed by **Vladimir Bulantsev** ([@GinCz](https://github.com/GinCz)).

Explore the full open-source utility suite by Vladimir Bulantsev:
- [GIN-PDF-RW](https://github.com/GinCz/GIN-PDF-RW) — High-Performance Zen PDF Reader & Editor
- [GIN-DNS-Switch](https://github.com/GinCz/GIN-DNS-Switch) — Fast Windows DNS Profile Switcher & Anti-Censorship Tool
- [GIN-Dock-Wallpaper](https://github.com/GinCz/Windows_scripts) — Dynamic Wallpaper Engine & Desktop Dock Bar

---

## 📄 License

This project is licensed under the terms of the [MIT License](LICENSE).
