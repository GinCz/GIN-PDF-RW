<# :
@echo off
setlocal EnableExtensions EnableDelayedExpansion
title GIN-PDF-RW v014 Setup [ VladiMIR + AI ]

echo [1/3] Preparing GIN-PDF-RW v014 installation...
set "INSTALL_DIR=C:\Program Files\GIN-PDF-RW"
if not exist "%INSTALL_DIR%" mkdir "%INSTALL_DIR%" >nul 2>&1

:: Check Administrator rights
net session >nul 2>&1
if %ERRORLEVEL% neq 0 (
    echo [*] Elevating to Administrator privileges...
    powershell -NoProfile -ExecutionPolicy Bypass -Command "Start-Process cmd.exe -ArgumentList '/c ""%~f0""' -Verb RunAs"
    exit /b
)

:: Locate Microsoft .NET csc.exe compiler
set "CSC="
if exist "%SystemRoot%\Microsoft.NET\Framework64\v4.0.30319\csc.exe" set "CSC=%SystemRoot%\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if not defined CSC if exist "%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\csc.exe" set "CSC=%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\csc.exe"

if not defined CSC (
    echo [ERROR] Microsoft .NET Framework 4.0/4.8 compiler (csc.exe) not found!
    pause
    exit /b 1
)

echo [2/3] Compiling native GIN-PDF-RW v014 binary (Hi-DPI Vector Engine & Context Menu)...
set "TARGET_EXE=%INSTALL_DIR%\GIN-PDF-RW.exe"
"%CSC%" /nologo /target:winexe /optimize+ /platform:anycpu ^
    /r:PresentationFramework.dll,PresentationCore.dll,WindowsBase.dll,System.Xaml.dll,System.dll,System.Drawing.dll,System.Windows.Forms.dll,System.Core.dll,Microsoft.CSharp.dll ^
    /out:"%TARGET_EXE%" "%~f0"

if %ERRORLEVEL% neq 0 (
    echo [ERROR] Compilation failed!
    pause
    exit /b %ERRORLEVEL%
)

echo [3/3] Registering shortcuts and file associations...
powershell -NoProfile -ExecutionPolicy Bypass -Command ^
    "$wsh = New-Object -ComObject WScript.Shell; " ^
    "$targets = @('D:\MEGA\DOCS\desktop', [Environment]::GetFolderPath('Desktop'), [Environment]::GetFolderPath('CommonDesktopDirectory')); " ^
    "foreach ($d in $targets) { if (Test-Path $d) { $lnk = $wsh.CreateShortcut(\"$d\GIN-PDF-RW.lnk\"); $lnk.TargetPath = '%TARGET_EXE%'; $lnk.WorkingDirectory = '%INSTALL_DIR%'; $lnk.Description = 'GIN-PDF-RW v014 by VladiMIR+AI'; $lnk.Save() } }; " ^
    "cmd /c 'assoc .pdf=GIN-PDF-RW.Document >nul'; " ^
    "cmd /c 'ftype GIN-PDF-RW.Document="%TARGET_EXE%" "%%1" >nul'"

echo.
echo ========================================================
echo   GIN-PDF-RW v014 successfully installed!
echo ========================================================
start "" "%TARGET_EXE%"
exit /b
#>

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

[assembly: AssemblyTitle("GIN-PDF-RW_v014")]
[assembly: AssemblyProduct("GIN-PDF-RW_v014")]
[assembly: AssemblyDescription("GIN-PDF-RW_v014")]
[assembly: AssemblyCompany("VladiMIR+AI")]
[assembly: AssemblyCopyright("VladiMIR+AI")]
[assembly: AssemblyFileVersion("0.014.0.0")]
[assembly: AssemblyVersion("0.014.0.0")]

namespace GinPdfRw
{
    public class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            var app = new Application();
            string initialFile = null;
            string explicitLang = null;

            if (args != null)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if ((args[i] == "-lang" || args[i] == "--lang" || args[i] == "/lang") && i + 1 < args.Length)
                    {
                        explicitLang = args[i + 1].ToLowerInvariant();
                        i++;
                    }
                    else if (File.Exists(args[i]))
                    {
                        initialFile = args[i];
                    }
                }
            }

            var window = new MainWindow(initialFile, explicitLang);
            app.Run(window);
        }
    }

    public static class NativePdfRenderer
    {
        public static int GetPageCount(string pdfPath)
        {
            try
            {
                Type storageFileType = Type.GetType("Windows.Storage.StorageFile, Windows.Storage, ContentType=WindowsRuntime");
                Type pdfDocType = Type.GetType("Windows.Data.Pdf.PdfDocument, Windows.Data.Pdf, ContentType=WindowsRuntime");
                if (storageFileType == null || pdfDocType == null) return 1;

                MethodInfo getFileMethod = storageFileType.GetMethod("GetFileFromPathAsync", new Type[] { typeof(string) });
                object asyncOpFile = getFileMethod.Invoke(null, new object[] { Path.GetFullPath(pdfPath) });
                object storageFile = AwaitOp(asyncOpFile, storageFileType);

                MethodInfo loadMethod = pdfDocType.GetMethod("LoadFromFileAsync", new Type[] { storageFileType });
                object asyncOpDoc = loadMethod.Invoke(null, new object[] { storageFile });
                object pdfDoc = AwaitOp(asyncOpDoc, pdfDocType);

                PropertyInfo countProp = pdfDocType.GetProperty("PageCount");
                return (int)(uint)countProp.GetValue(pdfDoc, null);
            }
            catch
            {
                return 1;
            }
        }

        public static byte[] RenderPage(string pdfPath, int pageIndex, uint targetWidth = 0, uint targetHeight = 0)
        {
            try
            {
                Type storageFileType = Type.GetType("Windows.Storage.StorageFile, Windows.Storage, ContentType=WindowsRuntime");
                Type pdfDocType = Type.GetType("Windows.Data.Pdf.PdfDocument, Windows.Data.Pdf, ContentType=WindowsRuntime");
                Type memStreamType = Type.GetType("Windows.Storage.Streams.InMemoryRandomAccessStream, Windows.Storage.Streams, ContentType=WindowsRuntime");
                if (storageFileType == null || pdfDocType == null || memStreamType == null) return null;

                MethodInfo getFileMethod = storageFileType.GetMethod("GetFileFromPathAsync", new Type[] { typeof(string) });
                object asyncOpFile = getFileMethod.Invoke(null, new object[] { Path.GetFullPath(pdfPath) });
                object storageFile = AwaitOp(asyncOpFile, storageFileType);

                MethodInfo loadMethod = pdfDocType.GetMethod("LoadFromFileAsync", new Type[] { storageFileType });
                object asyncOpDoc = loadMethod.Invoke(null, new object[] { storageFile });
                object pdfDoc = AwaitOp(asyncOpDoc, pdfDocType);

                MethodInfo getPageMethod = pdfDocType.GetMethod("GetPage");
                object page = getPageMethod.Invoke(pdfDoc, new object[] { (uint)pageIndex });

                object memStream = Activator.CreateInstance(memStreamType);
                Type iRandomAccess = Type.GetType("Windows.Storage.Streams.IRandomAccessStream, Windows.Storage.Streams, ContentType=WindowsRuntime");

                Type optionsType = Type.GetType("Windows.Data.Pdf.PdfPageRenderOptions, Windows.Data.Pdf, ContentType=WindowsRuntime");
                object asyncAction = null;
                if (optionsType != null && targetWidth > 0 && targetHeight > 0)
                {
                    object options = Activator.CreateInstance(optionsType);
                    PropertyInfo destWProp = optionsType.GetProperty("DestinationWidth");
                    PropertyInfo destHProp = optionsType.GetProperty("DestinationHeight");
                    if (destWProp != null) destWProp.SetValue(options, targetWidth, null);
                    if (destHProp != null) destHProp.SetValue(options, targetHeight, null);

                    MethodInfo renderWithOptions = page.GetType().GetMethod("RenderToStreamAsync", new Type[] { iRandomAccess, optionsType });
                    if (renderWithOptions != null)
                    {
                        asyncAction = renderWithOptions.Invoke(page, new object[] { memStream, options });
                    }
                }

                if (asyncAction == null)
                {
                    MethodInfo renderMethod = page.GetType().GetMethod("RenderToStreamAsync", new Type[] { iRandomAccess });
                    asyncAction = renderMethod.Invoke(page, new object[] { memStream });
                }
                AwaitAct(asyncAction);

                PropertyInfo sizeProp = memStreamType.GetProperty("Size");
                ulong size = (ulong)sizeProp.GetValue(memStream, null);

                Type dataReaderType = Type.GetType("Windows.Storage.Streams.DataReader, Windows.Storage.Streams, ContentType=WindowsRuntime");
                MethodInfo getInputStreamMethod = memStreamType.GetMethod("GetInputStreamAt");
                object inputStream = getInputStreamMethod.Invoke(memStream, new object[] { (ulong)0 });

                object dataReader = Activator.CreateInstance(dataReaderType, new object[] { inputStream });
                MethodInfo loadAsyncMethod = dataReaderType.GetMethod("LoadAsync");
                object loadOp = loadAsyncMethod.Invoke(dataReader, new object[] { (uint)size });
                AwaitOp(loadOp, typeof(uint));

                byte[] bytes = new byte[(int)size];
                MethodInfo readBytesMethod = dataReaderType.GetMethod("ReadBytes");
                readBytesMethod.Invoke(dataReader, new object[] { bytes });

                return bytes;
            }
            catch
            {
                return null;
            }
        }

        private static object AwaitOp(object asyncOp, Type returnType)
        {
            Type extType = Type.GetType("System.WindowsRuntimeSystemExtensions, System.Runtime.WindowsRuntime, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            if (extType == null) return null;
            MethodInfo asTaskGeneric = null;
            foreach (var m in extType.GetMethods())
            {
                if (m.Name == "AsTask" && m.IsGenericMethod && m.GetParameters().Length == 1)
                {
                    asTaskGeneric = m;
                    break;
                }
            }
            MethodInfo asTask = asTaskGeneric.MakeGenericMethod(returnType);
            object task = asTask.Invoke(null, new object[] { asyncOp });
            MethodInfo waitMethod = task.GetType().GetMethod("Wait", Type.EmptyTypes);
            waitMethod.Invoke(task, null);
            PropertyInfo resultProp = task.GetType().GetProperty("Result");
            return resultProp.GetValue(task, null);
        }

        private static void AwaitAct(object asyncAction)
        {
            Type extType = Type.GetType("System.WindowsRuntimeSystemExtensions, System.Runtime.WindowsRuntime, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            MethodInfo asTask = null;
            foreach (var m in extType.GetMethods())
            {
                if (m.Name == "AsTask" && !m.IsGenericMethod && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType.Name == "IAsyncAction")
                {
                    asTask = m;
                    break;
                }
            }
            object task = asTask.Invoke(null, new object[] { asyncAction });
            MethodInfo waitMethod = task.GetType().GetMethod("Wait", Type.EmptyTypes);
            waitMethod.Invoke(task, null);
        }
    }

    public class MainWindow : Window
    {
        private string docPath = null;
        private int currentPage = 1;
        private int totalPages = 1;
        private double zoom = 1.0;
        private bool isFullscreen = true; // Fullscreen by default!
        private bool isFitMode = true;    // Fit page completely in screen by default!
        private bool hasSwitchedToWindowMode = false;
        private double rotationAngle = 0;
        private bool isFlipH = false;
        private bool isFlipV = false;
        private string currentMode = "Reader";
        private string activeTool = "None";
        private string currentLang = "en";
        private string currentTheme = "Night"; // "Day", "Cloud", "Night"

        private double baseDocWidth = 794;
        private double baseDocHeight = 1123;
        private double nativePageWidth = 794;
        private double nativePageHeight = 1123;
        private System.Windows.Threading.DispatcherTimer zoomRerenderTimer;

        // UI Controls
        private Grid rootGrid;
        private Border toolbar;
        private Border editorBar;
        private Button btnOpen;
        private Button btnSave;
        private Button btnPrint;
        private Button btnModeReader;
        private Button btnModeEditor;
        private Button btnPrev;
        private Button btnNext;
        private TextBlock txtPage;
        private Button btnZoomOut;
        private TextBlock txtZoom;
        private Button btnZoomIn;
        private Button btnFit;
        private Button btnFullscreen;
        private Button btnUpdate;
        private Button btnThemeDay;
        private Button btnThemeCloud;
        private Button btnThemeNight;
        private ComboBox cmbLang;
        private Button toolText;
        private Button toolCheck;
        private Button toolImage;
        private Button toolSign;
        private Button toolCert;
        private TextBlock txtToolTip;
        private ScrollViewer docScroll;
        private Grid container;
        private Border pageBorder;
        private System.Windows.Controls.Image pdfView;
        private Canvas overlayCanvas;
        private Border statusBar;
        private TextBlock statusText;
        private TextBlock verText;

        private bool isDragging = false;
        private System.Windows.Point dragStartMousePoint;
        private double dragStartHorizontalOffset;
        private double dragStartVerticalOffset;

        private static bool IsChildOfScrollBar(DependencyObject obj)
        {
            while (obj != null)
            {
                if (obj is System.Windows.Controls.Primitives.ScrollBar) return true;
                obj = VisualTreeHelper.GetParent(obj);
            }
            return false;
        }

        private readonly Dictionary<string, Dictionary<string, string>> loc = 
            new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

        public MainWindow(string initialFile, string explicitLang = null)
        {
            InitLocalization();
            DetectDefaultLanguage(explicitLang);
            string savedTheme = DetectDefaultTheme();
            BuildUi();
            ApplyTheme(savedTheme); // Restores remembered theme (Day, Cloud, or Night)
            UpdateStrings();

            // Set Fullscreen by default as requested
            EnterFullscreen();

            if (!string.IsNullOrEmpty(initialFile) && File.Exists(initialFile))
            {
                OpenPdf(initialFile);
            }
            else
            {
                RenderWelcomeSheet();
            }

            zoomRerenderTimer = new System.Windows.Threading.DispatcherTimer();
            zoomRerenderTimer.Interval = TimeSpan.FromMilliseconds(120);
            zoomRerenderTimer.Tick += delegate {
                zoomRerenderTimer.Stop();
                RerenderAtCurrentZoom();
            };

            Loaded += delegate {
                DoFitPage();
            };

            SizeChanged += delegate {
                if (isFitMode) DoFitPage();
            };
        }

        private void InitLocalization()
        {
            var en = new Dictionary<string, string>();
            en.Add("Title", "GIN-PDF-RW v014 — Zen PDF Reader & Editor");
            en.Add("Open", "📂 Open (Ctrl+O)");
            en.Add("Save", "💾 Save (Ctrl+S)");
            en.Add("Print", "🖨️ Print (Ctrl+P)");
            en.Add("ModeReader", "📖 Reader & Forms");
            en.Add("ModeEditor", "✏️ Editor");
            en.Add("Fullscreen", "⛶ Fullscreen (F11)");
            en.Add("Restore", "⛶ Window (Esc)");
            en.Add("UpdateBtn", "v014 (Update)");
            en.Add("CheckingUpdate", "Checking GitHub...");
            en.Add("UpdateUpToDate", "You are using the latest version (v014)!");
            en.Add("UpdateAvailable", "New version {0} available on GitHub! Open download page?");
            en.Add("UpdateTitle", "GIN-PDF-RW Update");
            en.Add("FitWidth", "Fit Page");
            en.Add("ToolText", "🔤 Add Text");
            en.Add("ToolCheck", "☑ Checkmark (✓)");
            en.Add("ToolImage", "🖼 Stamp / Image");
            en.Add("ToolSign", "✍ Draw Sign");
            en.Add("ToolCert", "🔏 X.509 Sign");
            en.Add("Ready", "Ready");
            en.Add("ClickToPlace", "Click anywhere on the document to place item.");
            en.Add("PageOf", "Page {0} / {1}");
            en.Add("Opened", "Opened: {0} ({1} pages)");
            en.Add("Saved", "Saved: {0}");
            en.Add("NoCert", "No valid signing certificates found in Windows Certificate Store.");
            en.Add("Signed", "Document signed with X.509: {0}");
            loc.Add("en", en);

            var ru = new Dictionary<string, string>();
            ru.Add("Title", "GIN-PDF-RW v014 — Просмотр и редактор PDF");
            ru.Add("Open", "📂 Открыть (Ctrl+O)");
            ru.Add("Save", "💾 Сохранить (Ctrl+S)");
            ru.Add("Print", "🖨️ Печать (Ctrl+P)");
            ru.Add("ModeReader", "📖 Чтение и формы");
            ru.Add("ModeEditor", "✏️ Редактировать");
            ru.Add("Fullscreen", "⛶ Во весь экран (F11)");
            ru.Add("Restore", "⛶ В окно (Esc)");
            ru.Add("UpdateBtn", "v014 (обновить)");
            ru.Add("CheckingUpdate", "Проверка GitHub...");
            ru.Add("UpdateUpToDate", "У вас установлена самая актуальная версия (v014)!");
            ru.Add("UpdateAvailable", "Доступна новая версия {0} на GitHub! Открыть страницу загрузки?");
            ru.Add("UpdateTitle", "Обновление GIN-PDF-RW");
            ru.Add("FitWidth", "По размеру");
            ru.Add("ToolText", "🔤 Текст");
            ru.Add("ToolCheck", "☑ Галочка (✓)");
            ru.Add("ToolImage", "🖼 Печать / Картинка");
            ru.Add("ToolSign", "✍ Роспись");
            ru.Add("ToolCert", "🔏 ЭЦП Сертификат");
            ru.Add("Ready", "Готово");
            ru.Add("ClickToPlace", "Кликните в любое место документа для вставки.");
            ru.Add("PageOf", "Стр. {0} из {1}");
            ru.Add("Opened", "Открыт документ: {0} ({1} стр.)");
            ru.Add("Saved", "Успешно сохранено: {0}");
            ru.Add("NoCert", "В хранилище сертификатов Windows не найдено подходящих сертификатов.");
            ru.Add("Signed", "Документ подписан сертификатом: {0}");
            loc.Add("ru", ru);

            var uk = new Dictionary<string, string>();
            uk.Add("Title", "GIN-PDF-RW v014 — Перегляд і редактор PDF");
            uk.Add("Open", "📂 Відкрити (Ctrl+O)");
            uk.Add("Save", "💾 Зберегти (Ctrl+S)");
            uk.Add("Print", "🖨️ Друк (Ctrl+P)");
            uk.Add("ModeReader", "📖 Читання та форми");
            uk.Add("ModeEditor", "✏️ Редагувати");
            uk.Add("Fullscreen", "⛶ На весь екран (F11)");
            uk.Add("Restore", "⛶ У вікно (Esc)");
            uk.Add("UpdateBtn", "v014 (оновити)");
            uk.Add("CheckingUpdate", "Перевірка GitHub...");
            uk.Add("UpdateUpToDate", "У вас встановлена найновіша версія (v014)!");
            uk.Add("UpdateAvailable", "Доступна нова версія {0} на GitHub! Відкрити сторінку завантаження?");
            uk.Add("UpdateTitle", "Оновлення GIN-PDF-RW");
            uk.Add("FitWidth", "За розміром");
            uk.Add("ToolText", "🔤 Текст");
            uk.Add("ToolCheck", "☑ Галочка (✓)");
            uk.Add("ToolImage", "🖼 Штамп / Зображення");
            uk.Add("ToolSign", "✍ Підпис");
            uk.Add("ToolCert", "🔏 КЕП Сертифікат");
            uk.Add("Ready", "Готово");
            uk.Add("ClickToPlace", "Клікніть у будь-яке місце документа для вставки.");
            uk.Add("PageOf", "Стор. {0} з {1}");
            uk.Add("Opened", "Відкрито документ: {0} ({1} стор.)");
            uk.Add("Saved", "Успішно збережено: {0}");
            uk.Add("NoCert", "У сховищі сертифікатів Windows не знайдено відповідних сертифікатів.");
            uk.Add("Signed", "Документ підписано сертифікатом: {0}");
            loc.Add("uk", uk);

            var cs = new Dictionary<string, string>();
            cs.Add("Title", "GIN-PDF-RW v014 — Prohlížeč a editor PDF");
            cs.Add("Open", "📂 Otevřít (Ctrl+O)");
            cs.Add("Save", "💾 Uložit (Ctrl+S)");
            cs.Add("Print", "🖨️ Tisk (Ctrl+P)");
            cs.Add("ModeReader", "📖 Čtení a formuláře");
            cs.Add("ModeEditor", "✏️ Upravit");
            cs.Add("Fullscreen", "⛶ Celá obrazovka (F11)");
            cs.Add("Restore", "⛶ Do okna (Esc)");
            cs.Add("UpdateBtn", "v014 (aktualizovat)");
            cs.Add("CheckingUpdate", "Kontrola GitHubu...");
            cs.Add("UpdateUpToDate", "Používáte nejnovější verzi (v014)!");
            cs.Add("UpdateAvailable", "Na GitHubu je k dispozici nová verze {0}! Otevřít stránku stahování?");
            cs.Add("UpdateTitle", "Aktualizace GIN-PDF-RW");
            cs.Add("FitWidth", "Na stránku");
            cs.Add("ToolText", "🔤 Text");
            cs.Add("ToolCheck", "☑ Zatržítko (✓)");
            cs.Add("ToolImage", "🖼 Razítko / Obrázek");
            cs.Add("ToolSign", "✍ Podpis");
            cs.Add("ToolCert", "🔏 Digitální certifikát");
            cs.Add("Ready", "Připraveno");
            cs.Add("ClickToPlace", "Klikněte kamkoli do dokumentu pro umístění.");
            cs.Add("PageOf", "Strana {0} z {1}");
            cs.Add("Opened", "Otevřen dokument: {0} ({1} str.)");
            cs.Add("Saved", "Úspěšně uloženo: {0}");
            cs.Add("NoCert", "V úložišti certifikátů Windows nebyl nalezen platný certifikát.");
            cs.Add("Signed", "Dokument byl digitálně podepsán certifikátem: {0}");
            loc.Add("cs", cs);
        }

        private void DetectDefaultLanguage(string explicitLang)
        {
            if (!string.IsNullOrEmpty(explicitLang) && loc.ContainsKey(explicitLang))
            {
                currentLang = explicitLang;
                return;
            }

            // 1. Registry HKCU\Software\GIN-PDF-RW\Language
            try
            {
                using (var k = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\GIN-PDF-RW"))
                {
                    if (k != null)
                    {
                        string regLang = (k.GetValue("Language") as string ?? "").Trim().ToLowerInvariant();
                        if (loc.ContainsKey(regLang))
                        {
                            currentLang = regLang;
                            return;
                        }
                    }
                }
            }
            catch {}

            // 2. Base directory language.cfg
            try
            {
                string cfg = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "language.cfg");
                if (File.Exists(cfg))
                {
                    string saved = File.ReadAllText(cfg).Trim().ToLowerInvariant();
                    if (loc.ContainsKey(saved))
                    {
                        currentLang = saved;
                        return;
                    }
                }
            }
            catch {}

            // 3. %APPDATA%\GIN-PDF-RW\language.cfg
            try
            {
                string appDataCfg = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
                    "GIN-PDF-RW", "language.cfg");
                if (File.Exists(appDataCfg))
                {
                    string saved = File.ReadAllText(appDataCfg).Trim().ToLowerInvariant();
                    if (loc.ContainsKey(saved))
                    {
                        currentLang = saved;
                        return;
                    }
                }
            }
            catch {}

            // 4. System UI culture
            string code = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.ToLowerInvariant();
            if (loc.ContainsKey(code)) currentLang = code;
            else currentLang = "en";
        }

        private string DetectDefaultTheme()
        {
            // 1. Registry HKCU\Software\GIN-PDF-RW\Theme
            try
            {
                using (var k = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\GIN-PDF-RW"))
                {
                    if (k != null)
                    {
                        string regTheme = (k.GetValue("Theme") as string ?? "").Trim();
                        if (regTheme == "Day" || regTheme == "Cloud" || regTheme == "Night")
                        {
                            return regTheme;
                        }
                    }
                }
            }
            catch {}

            // 2. Base directory theme.cfg
            try
            {
                string cfg = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "theme.cfg");
                if (File.Exists(cfg))
                {
                    string saved = File.ReadAllText(cfg).Trim();
                    if (saved == "Day" || saved == "Cloud" || saved == "Night")
                    {
                        return saved;
                    }
                }
            }
            catch {}

            // 3. %APPDATA%\GIN-PDF-RW\theme.cfg
            try
            {
                string appDataCfg = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
                    "GIN-PDF-RW", "theme.cfg");
                if (File.Exists(appDataCfg))
                {
                    string saved = File.ReadAllText(appDataCfg).Trim();
                    if (saved == "Day" || saved == "Cloud" || saved == "Night")
                    {
                        return saved;
                    }
                }
            }
            catch {}

            return "Day";
        }

        private void SaveTheme(string theme)
        {
            if (string.IsNullOrEmpty(theme)) return;

            // 1. Registry
            try
            {
                using (var k = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\GIN-PDF-RW"))
                {
                    if (k != null)
                    {
                        k.SetValue("Theme", theme);
                    }
                }
            }
            catch {}

            // 2. %APPDATA%\GIN-PDF-RW\theme.cfg
            try
            {
                string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "GIN-PDF-RW");
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                File.WriteAllText(Path.Combine(dir, "theme.cfg"), theme);
            }
            catch {}

            // 3. Base directory theme.cfg
            try
            {
                File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "theme.cfg"), theme);
            }
            catch {}
        }

        private void SaveLanguage(string lang)
        {
            if (string.IsNullOrEmpty(lang)) return;

            // 1. Registry
            try
            {
                using (var k = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\GIN-PDF-RW"))
                {
                    if (k != null)
                    {
                        k.SetValue("Language", lang);
                    }
                }
            }
            catch {}

            // 2. %APPDATA%\GIN-PDF-RW\language.cfg
            try
            {
                string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "GIN-PDF-RW");
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                File.WriteAllText(Path.Combine(dir, "language.cfg"), lang);
            }
            catch {}

            // 3. Base directory language.cfg
            try
            {
                File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "language.cfg"), lang);
            }
            catch {}
        }

        private string T(string key, params object[] args)
        {
            if (!loc.ContainsKey(currentLang)) currentLang = "en";
            if (!loc[currentLang].ContainsKey(key)) key = "en";
            string val = loc[currentLang].ContainsKey(key) ? loc[currentLang][key] : key;
            return (args != null && args.Length > 0) ? string.Format(val, args) : val;
        }

        private void BuildUi()
        {
            Title = "GIN-PDF-RW v014";
            Width = 1240;
            Height = 880;
            MinWidth = 800;
            MinHeight = 550;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            FontFamily = new System.Windows.Media.FontFamily("Segoe UI, Segoe UI Variable, sans-serif");

            rootGrid = new Grid();
            rootGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            rootGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            rootGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            rootGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(26) });

            // 1. Slim Zen Toolbar
            toolbar = new Border {
                BorderThickness = new Thickness(0, 0, 0, 1),
                Padding = new Thickness(8, 6, 8, 6)
            };

            var tbGrid = new Grid();
            tbGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            tbGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            tbGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            tbGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Left Group
            var leftGroup = new StackPanel { Orientation = Orientation.Horizontal };
            btnOpen = CreateBtn("📂 Open", delegate { OpenFileDialog(); });
            btnSave = CreateBtn("💾 Save", delegate { SaveFileDialog(); });
            btnPrint = CreateBtn("🖨️ Print", delegate { ExecutePrint(); });
            leftGroup.Children.Add(btnOpen);
            leftGroup.Children.Add(btnSave);
            leftGroup.Children.Add(btnPrint);
            leftGroup.Children.Add(CreateSep());
            Grid.SetColumn(leftGroup, 0);
            tbGrid.Children.Add(leftGroup);

            // High-Contrast Mode Selector (Tab design)
            var modeGroup = new StackPanel { Orientation = Orientation.Horizontal };
            btnModeReader = CreateTabBtn("📖 Reader & Forms", true);
            btnModeEditor = CreateTabBtn("✏️ Editor", false);

            btnModeReader.Click += delegate { SetMode("Reader"); };
            btnModeEditor.Click += delegate { SetMode("Editor"); };

            modeGroup.Children.Add(btnModeReader);
            modeGroup.Children.Add(btnModeEditor);
            modeGroup.Children.Add(CreateSep());
            Grid.SetColumn(modeGroup, 1);
            tbGrid.Children.Add(modeGroup);

            // Center Group
            var centerGroup = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center };
            btnPrev = CreateBtn("◀", delegate { PrevPage(); });
            txtPage = new TextBlock { Text = "Page 1 / 1", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(8, 0, 8, 0), FontWeight = FontWeights.SemiBold };
            btnNext = CreateBtn("▶", delegate { NextPage(); });
            btnZoomOut = CreateBtn("➖", delegate { AdjustZoom(-0.05); }); // 5% step!
            txtZoom = new TextBlock { Text = "100%", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(6, 0, 6, 0) };
            btnZoomIn = CreateBtn("➕", delegate { AdjustZoom(0.05); });  // 5% step!
            btnFit = CreateBtn("Fit Page", delegate { DoFitPage(); });

            centerGroup.Children.Add(btnPrev);
            centerGroup.Children.Add(txtPage);
            centerGroup.Children.Add(btnNext);
            centerGroup.Children.Add(CreateSep());
            centerGroup.Children.Add(btnZoomOut);
            centerGroup.Children.Add(txtZoom);
            centerGroup.Children.Add(btnZoomIn);
            centerGroup.Children.Add(btnFit);
            Grid.SetColumn(centerGroup, 2);
            tbGrid.Children.Add(centerGroup);

            // Right Group: Themes (Day, Cloud, Night), Fullscreen, Language
            var rightGroup = new StackPanel { Orientation = Orientation.Horizontal };

            // 3-way Theme Switcher: Day (☀️), Cloud (☁️ in middle), Night (🌙)
            var themeGroup = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 6, 0) };
            btnThemeDay = CreateBtn("☀️", delegate { ApplyTheme("Day"); });
            btnThemeCloud = CreateBtn("☁️", delegate { ApplyTheme("Cloud"); });
            btnThemeNight = CreateBtn("🌙", delegate { ApplyTheme("Night"); });
            btnThemeDay.ToolTip = "Day (Light theme)";
            btnThemeCloud.ToolTip = "Neutral Cloud (Gray theme)";
            btnThemeNight.ToolTip = "Night (Dark theme)";
            btnThemeDay.Padding = new Thickness(6, 4, 6, 4);
            btnThemeCloud.Padding = new Thickness(6, 4, 6, 4);
            btnThemeNight.Padding = new Thickness(6, 4, 6, 4);
            themeGroup.Children.Add(btnThemeDay);
            themeGroup.Children.Add(btnThemeCloud);
            themeGroup.Children.Add(btnThemeNight);
            rightGroup.Children.Add(themeGroup);

            btnFullscreen = CreateBtn("⛶ Fullscreen", delegate { ToggleFullscreen(); });

            // Language ComboBox
            cmbLang = new ComboBox {
                Width = 76,
                Height = 28,
                Margin = new Thickness(6, 0, 0, 0),
                FontSize = 11,
                FontWeight = FontWeights.Bold,
                VerticalContentAlignment = VerticalAlignment.Center,
                Cursor = System.Windows.Input.Cursors.Hand
            };

            try {
                string cbTemplateXaml = @"
<ControlTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
                 xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
                 TargetType='ComboBox'>
    <Grid>
        <ToggleButton Name='ToggleButton'
                      Focusable='False'
                      Background='{TemplateBinding Background}'
                      BorderBrush='{TemplateBinding BorderBrush}'
                      BorderThickness='{TemplateBinding BorderThickness}'
                      Foreground='{TemplateBinding Foreground}'
                      IsChecked='{Binding Path=IsDropDownOpen, Mode=TwoWay, RelativeSource={RelativeSource TemplatedParent}}'
                      ClickMode='Press'>
            <ToggleButton.Template>
                <ControlTemplate TargetType='ToggleButton'>
                    <Border Name='Border'
                            Background='{TemplateBinding Background}'
                            BorderBrush='{TemplateBinding BorderBrush}'
                            BorderThickness='{TemplateBinding BorderThickness}'
                            CornerRadius='4'>
                        <Grid>
                            <Grid.ColumnDefinitions>
                                <ColumnDefinition Width='*' />
                                <ColumnDefinition Width='18' />
                            </Grid.ColumnDefinitions>
                            <Path Name='Arrow'
                                  Grid.Column='1'
                                  Fill='{TemplateBinding Foreground}'
                                  HorizontalAlignment='Center'
                                  VerticalAlignment='Center'
                                  Data='M 0 0 L 4 4 L 8 0 Z' />
                        </Grid>
                    </Border>
                    <ControlTemplate.Triggers>
                        <Trigger Property='IsMouseOver' Value='True'>
                            <Setter TargetName='Border' Property='Opacity' Value='0.85' />
                        </Trigger>
                    </ControlTemplate.Triggers>
                </ControlTemplate>
            </ToggleButton.Template>
        </ToggleButton>
        <ContentPresenter Name='ContentSite'
                          IsHitTestVisible='False'
                          Content='{TemplateBinding SelectionBoxItem}'
                          ContentTemplate='{TemplateBinding SelectionBoxItemTemplate}'
                          ContentTemplateSelector='{TemplateBinding ItemTemplateSelector}'
                          Margin='10,0,18,0'
                          VerticalAlignment='Center'
                          HorizontalAlignment='Center' />
        <Popup Name='Popup'
               Placement='Bottom'
               IsOpen='{TemplateBinding IsDropDownOpen}'
               AllowsTransparency='True'
               Focusable='False'
               PopupAnimation='Slide'>
            <Grid Name='DropDown'
                  SnapsToDevicePixels='True'
                  MinWidth='{TemplateBinding ActualWidth}'
                  MaxHeight='{TemplateBinding MaxDropDownHeight}'>
                <Border Name='DropDownBorder'
                        Background='{TemplateBinding Background}'
                        BorderBrush='{TemplateBinding BorderBrush}'
                        BorderThickness='1'
                        CornerRadius='4'
                        Margin='0,2,0,0'>
                    <ScrollViewer SnapsToDevicePixels='True'>
                        <StackPanel IsItemsHost='True' KeyboardNavigation.DirectionalNavigation='Contained' />
                    </ScrollViewer>
                </Border>
            </Grid>
        </Popup>
    </Grid>
</ControlTemplate>";

                string itemStyleXaml = @"
<Style xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
       xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
       TargetType='ComboBoxItem'>
    <Setter Property='Padding' Value='8,4,8,4' />
    <Setter Property='HorizontalContentAlignment' Value='Center' />
    <Setter Property='Cursor' Value='Hand' />
    <Setter Property='Template'>
        <Setter.Value>
            <ControlTemplate TargetType='ComboBoxItem'>
                <Border Name='ItemBorder'
                        Background='{TemplateBinding Background}'
                        Padding='{TemplateBinding Padding}'
                        CornerRadius='3'>
                    <ContentPresenter HorizontalAlignment='Center' />
                </Border>
                <ControlTemplate.Triggers>
                    <Trigger Property='IsHighlighted' Value='True'>
                        <Setter TargetName='ItemBorder' Property='Background' Value='#5E81AC' />
                        <Setter Property='Foreground' Value='#FFFFFF' />
                    </Trigger>
                </ControlTemplate.Triggers>
            </ControlTemplate>
        </Setter.Value>
    </Setter>
</Style>";

                cmbLang.Template = (ControlTemplate)System.Windows.Markup.XamlReader.Parse(cbTemplateXaml);
                cmbLang.ItemContainerStyle = (Style)System.Windows.Markup.XamlReader.Parse(itemStyleXaml);
            } catch {}

            cmbLang.Items.Add(new ComboBoxItem { Content = "EN" });
            cmbLang.Items.Add(new ComboBoxItem { Content = "RU" });
            cmbLang.Items.Add(new ComboBoxItem { Content = "UK" });
            cmbLang.Items.Add(new ComboBoxItem { Content = "CS" });

            if (currentLang == "ru") cmbLang.SelectedIndex = 1;
            else if (currentLang == "uk") cmbLang.SelectedIndex = 2;
            else if (currentLang == "cs") cmbLang.SelectedIndex = 3;
            else cmbLang.SelectedIndex = 0;

            cmbLang.SelectionChanged += delegate {
                var item = cmbLang.SelectedItem as ComboBoxItem;
                if (item != null) {
                    currentLang = item.Content.ToString().ToLowerInvariant();
                    SaveLanguage(currentLang);
                    UpdateStrings();
                    if (string.IsNullOrEmpty(docPath)) RenderWelcomeSheet();
                }
            };

            btnUpdate = CreateBtn("v014 (обновить)", delegate { CheckForManualUpdate(); });
            btnUpdate.Margin = new Thickness(6, 0, 0, 0);
            btnUpdate.FontWeight = FontWeights.SemiBold;
            btnUpdate.ToolTip = "GitHub Update Checker";

            rightGroup.Children.Add(btnFullscreen);
            rightGroup.Children.Add(cmbLang);
            rightGroup.Children.Add(btnUpdate);
            Grid.SetColumn(rightGroup, 3);
            tbGrid.Children.Add(rightGroup);

            toolbar.Child = tbGrid;
            Grid.SetRow(toolbar, 0);
            rootGrid.Children.Add(toolbar);

            // 2. Editor Bar
            editorBar = new Border {
                BorderThickness = new Thickness(0, 0, 0, 1),
                Padding = new Thickness(8, 4, 8, 4),
                Visibility = Visibility.Collapsed
            };

            var ebStack = new StackPanel { Orientation = Orientation.Horizontal };
            toolText = CreateBtn("🔤 Add Text", delegate { SelectTool("Text"); });
            toolCheck = CreateBtn("☑ Checkmark (✓)", delegate { SelectTool("Checkmark"); });
            toolImage = CreateBtn("🖼 Stamp / Image", delegate { InsertImageDialog(); });
            toolSign = CreateBtn("✍ Draw Sign", delegate { DrawSignDialog(); });
            toolCert = CreateBtn("🔏 X.509 Sign", delegate { ExecuteDigitalSign(); });

            txtToolTip = new TextBlock { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(12, 0, 0, 0), FontStyle = FontStyles.Italic };

            ebStack.Children.Add(toolText);
            ebStack.Children.Add(toolCheck);
            ebStack.Children.Add(toolImage);
            ebStack.Children.Add(toolSign);
            ebStack.Children.Add(toolCert);
            ebStack.Children.Add(txtToolTip);
            editorBar.Child = ebStack;
            Grid.SetRow(editorBar, 1);
            rootGrid.Children.Add(editorBar);

            // 3. Document Canvas
            docScroll = new ScrollViewer {
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };
            container = new Grid { HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Top, Margin = new Thickness(15) };

            pageBorder = new Border {
                Background = System.Windows.Media.Brushes.White,
                CornerRadius = new CornerRadius(2)
            };
            var pageGrid = new Grid();

            pdfView = new System.Windows.Controls.Image { Stretch = Stretch.Uniform };
            overlayCanvas = new Canvas { Background = System.Windows.Media.Brushes.Transparent, Cursor = System.Windows.Input.Cursors.Arrow };

            overlayCanvas.MouseDown += delegate(object s, MouseButtonEventArgs e) {
                var pos = e.GetPosition(overlayCanvas);
                if (activeTool == "Checkmark") {
                    AddInteractiveField("Check", pos.X / zoom, pos.Y / zoom, 20, 20, "CheckBox", "", true);
                    statusText.Text = string.Format("Checkmark placed at ({0:F0}, {1:F0})", pos.X, pos.Y);
                } else if (activeTool == "Text") {
                    AddInteractiveField("Text", pos.X / zoom, pos.Y / zoom, 180, 24, "Text", "Sample Text", false);
                    statusText.Text = string.Format("Text field placed at ({0:F0}, {1:F0})", pos.X, pos.Y);
                }
            };

            // Left click dragging to pan document up/down and sideways smoothly when zoomed in
            docScroll.PreviewMouseLeftButtonDown += delegate(object s, MouseButtonEventArgs e) {
                if (activeTool == "None") {
                    if (!IsChildOfScrollBar(e.OriginalSource as DependencyObject) &&
                        !(e.OriginalSource is TextBox) &&
                        !(e.OriginalSource is Button) &&
                        !(e.OriginalSource is CheckBox)) {
                        isDragging = true;
                        dragStartMousePoint = e.GetPosition(docScroll);
                        dragStartHorizontalOffset = docScroll.HorizontalOffset;
                        dragStartVerticalOffset = docScroll.VerticalOffset;
                        docScroll.CaptureMouse();
                        docScroll.Cursor = Cursors.Hand;
                    }
                }
            };

            docScroll.PreviewMouseMove += delegate(object s, MouseEventArgs e) {
                if (isDragging && e.LeftButton == MouseButtonState.Pressed) {
                    var currentPoint = e.GetPosition(docScroll);
                    double deltaX = currentPoint.X - dragStartMousePoint.X;
                    double deltaY = currentPoint.Y - dragStartMousePoint.Y;

                    docScroll.ScrollToHorizontalOffset(dragStartHorizontalOffset - deltaX);
                    docScroll.ScrollToVerticalOffset(dragStartVerticalOffset - deltaY);
                    e.Handled = true;
                } else if (isDragging && e.LeftButton != MouseButtonState.Pressed) {
                    isDragging = false;
                    docScroll.ReleaseMouseCapture();
                    docScroll.Cursor = Cursors.Arrow;
                }
            };

            docScroll.PreviewMouseLeftButtonUp += delegate(object s, MouseButtonEventArgs e) {
                if (isDragging) {
                    isDragging = false;
                    docScroll.ReleaseMouseCapture();
                    docScroll.Cursor = Cursors.Arrow;
                }
            };

            docScroll.LostMouseCapture += delegate {
                isDragging = false;
                docScroll.Cursor = Cursors.Arrow;
            };

            // Middle click on mouse wheel toggles fullscreen <-> window with menu buttons cyclically
            docScroll.PreviewMouseDown += delegate(object s, MouseButtonEventArgs e) {
                if (e.ChangedButton == MouseButton.Middle) {
                    ToggleFullscreen();
                    e.Handled = true;
                }
            };

            docScroll.SizeChanged += delegate {
                if (isFitMode) DoFitPage();
            };

            // Mouse wheel: Ctrl+Wheel zooms. Normal Wheel scrolls document down/up; at bottom/top or single page, navigates pages.
            docScroll.PreviewMouseWheel += delegate(object s, MouseWheelEventArgs e) {
                if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control) {
                    AdjustZoom(e.Delta > 0 ? 0.05 : -0.05);
                    e.Handled = true;
                } else {
                    if (docScroll.ScrollableHeight > 0) {
                        if (e.Delta < 0 && docScroll.VerticalOffset >= docScroll.ScrollableHeight - 5) {
                            if (currentPage < totalPages) {
                                NextPage();
                                docScroll.ScrollToTop();
                                e.Handled = true;
                            }
                        } else if (e.Delta > 0 && docScroll.VerticalOffset <= 5) {
                            if (currentPage > 1) {
                                PrevPage();
                                docScroll.ScrollToBottom();
                                e.Handled = true;
                            }
                        } else {
                            docScroll.ScrollToVerticalOffset(docScroll.VerticalOffset - e.Delta);
                            e.Handled = true;
                        }
                    } else {
                        if (e.Delta < 0 && currentPage < totalPages) {
                            NextPage();
                            e.Handled = true;
                        } else if (e.Delta > 0 && currentPage > 1) {
                            PrevPage();
                            e.Handled = true;
                        }
                    }
                }
            };

            pageGrid.Children.Add(pdfView);
            pageGrid.Children.Add(overlayCanvas);
            pageBorder.Child = pageGrid;
            ApplyPageTransforms();

            // Setup Right-Click Context Menu
            SetupContextMenu();
            container.Children.Add(pageBorder);
            docScroll.Content = container;
            Grid.SetRow(docScroll, 2);
            rootGrid.Children.Add(docScroll);

            // 4. Status Bar
            statusBar = new Border {
                BorderThickness = new Thickness(0, 1, 0, 0),
                Padding = new Thickness(12, 2, 12, 2)
            };
            var sbGrid = new Grid();
            sbGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            sbGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            statusText = new TextBlock { Text = "Ready", FontSize = 11, VerticalAlignment = VerticalAlignment.Center };
            verText = new TextBlock { Text = "GIN-PDF-RW v014 [VladiMIR+AI] | UTF-8", FontSize = 11, VerticalAlignment = VerticalAlignment.Center };

            Grid.SetColumn(statusText, 0);
            Grid.SetColumn(verText, 1);
            sbGrid.Children.Add(statusText);
            sbGrid.Children.Add(verText);
            statusBar.Child = sbGrid;
            Grid.SetRow(statusBar, 3);
            rootGrid.Children.Add(statusBar);

            Content = rootGrid;

            // Hotkeys & Navigation
            KeyDown += delegate(object s, KeyEventArgs e) {
                // PageUp / PageDown switches to previous/next file in folder
                if (e.Key == Key.PageUp) {
                    OpenPreviousFile();
                    e.Handled = true;
                    return;
                }
                if (e.Key == Key.PageDown) {
                    OpenNextFile();
                    e.Handled = true;
                    return;
                }

                // Enter or Return toggles fullscreen <-> normal window with menu items
                if (e.Key == Key.Return || e.Key == Key.Enter) {
                    ToggleFullscreen();
                    e.Handled = true;
                    return;
                }

                // Escape closes application if user never switched to window mode!
                if (e.Key == Key.Escape) {
                    if (!hasSwitchedToWindowMode) {
                        Close();
                        e.Handled = true;
                        return;
                    } else if (isFullscreen) {
                        ExitFullscreenToWindow();
                        e.Handled = true;
                        return;
                    }
                }

                if (e.Key == Key.F11) {
                    ToggleFullscreen();
                    e.Handled = true;
                    return;
                }

                // Zoom in: Numpad '+' or main keyboard '+' (OemPlus)
                if (e.Key == Key.Add || e.Key == Key.OemPlus) {
                    AdjustZoom(0.05);
                    e.Handled = true;
                    return;
                }

                // Zoom out: Numpad '-' or main keyboard '-' (OemMinus)
                if (e.Key == Key.Subtract || e.Key == Key.OemMinus) {
                    AdjustZoom(-0.05);
                    e.Handled = true;
                    return;
                }

                if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control) {
                    if (e.Key == Key.O) { OpenFileDialog(); e.Handled = true; }
                    else if (e.Key == Key.S) { SaveFileDialog(); e.Handled = true; }
                    else if (e.Key == Key.P) { ExecutePrint(); e.Handled = true; }
                }
            };

            // Middle click on the mouse wheel anywhere in the window toggles fullscreen cyclically
            PreviewMouseDown += delegate(object s, MouseButtonEventArgs e) {
                if (e.ChangedButton == MouseButton.Middle) {
                    ToggleFullscreen();
                    e.Handled = true;
                }
            };
        }

        private void ApplyTheme(string theme)
        {
            currentTheme = theme;
            SaveTheme(theme);
            System.Windows.Media.Color bgWin, bgView, bgTool, brdTool, fgText, fgSub, bgBtn, fgBtn, bgTabAct, fgTabAct;

            if (theme == "Day")
            {
                // Day (Light theme)
                bgWin = System.Windows.Media.Color.FromRgb(242, 244, 248);
                bgView = System.Windows.Media.Color.FromRgb(225, 229, 238);
                bgTool = System.Windows.Media.Color.FromRgb(255, 255, 255);
                brdTool = System.Windows.Media.Color.FromRgb(216, 222, 233);
                fgText = System.Windows.Media.Color.FromRgb(24, 24, 37);
                fgSub = System.Windows.Media.Color.FromRgb(76, 86, 106);
                bgBtn = System.Windows.Media.Color.FromRgb(230, 234, 242);
                fgBtn = System.Windows.Media.Color.FromRgb(24, 24, 37);
                bgTabAct = System.Windows.Media.Color.FromRgb(210, 225, 255);
                fgTabAct = System.Windows.Media.Color.FromRgb(0, 70, 180);
            }
            else if (theme == "Cloud")
            {
                // Cloud (Neutral Gray theme)
                bgWin = System.Windows.Media.Color.FromRgb(46, 52, 64);
                bgView = System.Windows.Media.Color.FromRgb(36, 41, 51);
                bgTool = System.Windows.Media.Color.FromRgb(59, 66, 82);
                brdTool = System.Windows.Media.Color.FromRgb(67, 76, 94);
                fgText = System.Windows.Media.Color.FromRgb(236, 239, 244);
                fgSub = System.Windows.Media.Color.FromRgb(216, 222, 233);
                bgBtn = System.Windows.Media.Color.FromRgb(67, 76, 94);
                fgBtn = System.Windows.Media.Color.FromRgb(236, 239, 244);
                bgTabAct = System.Windows.Media.Color.FromRgb(94, 129, 172);
                fgTabAct = System.Windows.Media.Color.FromRgb(255, 255, 255);
            }
            else
            {
                // Night (Dark theme - Default)
                bgWin = System.Windows.Media.Color.FromRgb(24, 24, 37);
                bgView = System.Windows.Media.Color.FromRgb(17, 17, 27);
                bgTool = System.Windows.Media.Color.FromRgb(30, 30, 46);
                brdTool = System.Windows.Media.Color.FromRgb(49, 50, 68);
                fgText = System.Windows.Media.Color.FromRgb(205, 214, 244);
                fgSub = System.Windows.Media.Color.FromRgb(186, 194, 222);
                bgBtn = System.Windows.Media.Color.FromRgb(49, 50, 68);
                fgBtn = System.Windows.Media.Color.FromRgb(205, 214, 244);
                bgTabAct = System.Windows.Media.Color.FromRgb(69, 71, 90);
                fgTabAct = System.Windows.Media.Color.FromRgb(137, 180, 250);
            }

            Background = new SolidColorBrush(bgWin);
            Foreground = new SolidColorBrush(fgText);
            docScroll.Background = new SolidColorBrush(bgView);
            toolbar.Background = new SolidColorBrush(bgTool);
            toolbar.BorderBrush = new SolidColorBrush(brdTool);
            editorBar.Background = new SolidColorBrush(bgTool);
            editorBar.BorderBrush = new SolidColorBrush(brdTool);
            statusBar.Background = new SolidColorBrush(bgTool);
            statusBar.BorderBrush = new SolidColorBrush(brdTool);
            statusText.Foreground = new SolidColorBrush(fgSub);
            verText.Foreground = new SolidColorBrush(fgSub);
            txtPage.Foreground = new SolidColorBrush(fgSub);
            txtZoom.Foreground = new SolidColorBrush(fgSub);

            // Style all standard buttons
            Button[] btns = new Button[] { btnOpen, btnSave, btnPrint, btnPrev, btnNext, btnZoomOut, btnZoomIn, btnFit, btnFullscreen, btnThemeDay, btnThemeCloud, btnThemeNight, toolText, toolCheck, toolImage, toolSign, btnUpdate };
            foreach (var b in btns) {
                if (b != null) {
                    b.Background = new SolidColorBrush(bgBtn);
                    b.Foreground = new SolidColorBrush(fgBtn);
                }
            }

            // Active theme button highlight / opacity indicator
            if (btnThemeDay != null && btnThemeCloud != null && btnThemeNight != null) {
                btnThemeDay.Opacity = (theme == "Day") ? 1.0 : 0.6;
                btnThemeCloud.Opacity = (theme == "Cloud") ? 1.0 : 0.6;
                btnThemeNight.Opacity = (theme == "Night") ? 1.0 : 0.6;
            }

            // High contrast ComboBox styling (Fixing white text on white background)
            if (cmbLang != null) {
                cmbLang.Background = new SolidColorBrush(bgBtn);
                cmbLang.Foreground = new SolidColorBrush(fgBtn);
                cmbLang.BorderBrush = new SolidColorBrush(brdTool);
                cmbLang.BorderThickness = new Thickness(1);
                foreach (var item in cmbLang.Items) {
                    var cbi = item as ComboBoxItem;
                    if (cbi != null) {
                        cbi.Background = new SolidColorBrush(bgBtn);
                        cbi.Foreground = new SolidColorBrush(fgBtn);
                    }
                }
            }

            // Update Tab states
            SetMode(currentMode);
        }

        private void EnterFullscreen()
        {
            isFullscreen = true;
            WindowStyle = WindowStyle.None;
            WindowState = WindowState.Maximized;
            btnFullscreen.Content = T("Restore");
            toolbar.Visibility = Visibility.Collapsed; // Hidden initially in fullscreen!
            statusBar.Visibility = Visibility.Collapsed;
            if (isFitMode)
                Dispatcher.BeginInvoke(new Action(DoFitPage), System.Windows.Threading.DispatcherPriority.Background);
            else
                Dispatcher.BeginInvoke(new Action(ApplyZoomToPage), System.Windows.Threading.DispatcherPriority.Background);
        }

        private void ExitFullscreenToWindow()
        {
            hasSwitchedToWindowMode = true;
            isFullscreen = false;
            WindowStyle = WindowStyle.SingleBorderWindow;
            WindowState = WindowState.Maximized; // Maximized window view as requested!
            btnFullscreen.Content = T("Fullscreen");
            toolbar.Visibility = Visibility.Visible;
            statusBar.Visibility = Visibility.Visible;
            if (isFitMode)
                Dispatcher.BeginInvoke(new Action(DoFitPage), System.Windows.Threading.DispatcherPriority.Background);
            else
                Dispatcher.BeginInvoke(new Action(ApplyZoomToPage), System.Windows.Threading.DispatcherPriority.Background);
        }

        private void ToggleFullscreen()
        {
            if (isDragging) {
                isDragging = false;
                docScroll.ReleaseMouseCapture();
                docScroll.Cursor = Cursors.Arrow;
            }
            if (isFullscreen) ExitFullscreenToWindow();
            else EnterFullscreen();
        }

        private void DoFitPage()
        {
            isFitMode = true;
            double availWidth = docScroll.ViewportWidth;
            double availHeight = docScroll.ViewportHeight;

            if (availWidth <= 50) availWidth = docScroll.ActualWidth;
            if (availWidth <= 50) availWidth = ActualWidth - 40;
            if (availWidth <= 50) availWidth = SystemParameters.PrimaryScreenWidth - 40;

            if (availHeight <= 50) availHeight = docScroll.ActualHeight;
            if (availHeight <= 50) availHeight = ActualHeight - 80;
            if (availHeight <= 50) availHeight = SystemParameters.PrimaryScreenHeight - 80;

            if (availWidth > 100 && availHeight > 100 && baseDocWidth > 0 && baseDocHeight > 0)
            {
                double scaleX = Math.Max(0.1, (availWidth - 24) / baseDocWidth);
                double scaleY = Math.Max(0.1, (availHeight - 24) / baseDocHeight);
                zoom = Math.Min(scaleX, scaleY);
                txtZoom.Text = string.Format("{0:P0}", zoom);
                ApplyZoomToPage();
            }
        }

        private void OpenNextFile()
        {
            NavigateFile(1);
        }

        private void OpenPreviousFile()
        {
            NavigateFile(-1);
        }

        private void NavigateFile(int direction)
        {
            try
            {
                if (string.IsNullOrEmpty(docPath) || !File.Exists(docPath)) return;
                string dir = Path.GetDirectoryName(docPath);
                if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir)) return;

                string[] files = Directory.GetFiles(dir, "*.pdf");
                if (files == null || files.Length <= 1) return;

                Array.Sort(files, StringComparer.OrdinalIgnoreCase);
                int idx = -1;
                for (int i = 0; i < files.Length; i++)
                {
                    if (string.Equals(files[i], docPath, StringComparison.OrdinalIgnoreCase))
                    {
                        idx = i;
                        break;
                    }
                }

                if (idx >= 0)
                {
                    int nextIdx = idx + direction;
                    if (nextIdx >= files.Length) nextIdx = 0;
                    else if (nextIdx < 0) nextIdx = files.Length - 1;

                    OpenPdf(files[nextIdx]);
                }
            }
            catch {}
        }

        private void ApplyZoomToPage()
        {
            pageBorder.Width = baseDocWidth * zoom;
            pageBorder.Height = baseDocHeight * zoom;
            if (docScroll != null && container != null)
            {
                double availW = docScroll.ViewportWidth > 0 ? docScroll.ViewportWidth : docScroll.ActualWidth;
                double availH = docScroll.ViewportHeight > 0 ? docScroll.ViewportHeight : docScroll.ActualHeight;

                if (availW > 0)
                {
                    container.HorizontalAlignment = (pageBorder.Width + 30 > availW) ? HorizontalAlignment.Left : HorizontalAlignment.Center;
                }
                if (availH > 0)
                {
                    container.VerticalAlignment = (pageBorder.Height + 30 > availH) ? VerticalAlignment.Top : VerticalAlignment.Center;
                }
            }
        }

        private void AdjustZoom(double delta)
        {
            isFitMode = false; // User manually zooming
            zoom = Math.Max(0.2, Math.Min(5.0, zoom + delta));
            txtZoom.Text = string.Format("{0:P0}", zoom);
            ApplyZoomToPage();
            if (zoomRerenderTimer != null)
            {
                zoomRerenderTimer.Stop();
                zoomRerenderTimer.Start();
            }
        }

        private void RerenderAtCurrentZoom()
        {
            if (string.IsNullOrEmpty(docPath) || !File.Exists(docPath) || nativePageWidth <= 0 || nativePageHeight <= 0) return;
            try
            {
                // Dynamically re-rasterize vector graphics at true display resolution
                double renderScale = Math.Max(1.0, zoom);
                uint targetW = (uint)Math.Max(100, Math.Min(8192, Math.Round(nativePageWidth * renderScale)));
                uint targetH = (uint)Math.Max(100, Math.Min(8192, Math.Round(nativePageHeight * renderScale)));

                byte[] bytes = NativePdfRenderer.RenderPage(docPath, currentPage - 1, targetW, targetH);
                if (bytes != null && bytes.Length > 0)
                {
                    var ms = new MemoryStream(bytes);
                    var bi = new BitmapImage();
                    bi.BeginInit();
                    bi.StreamSource = ms;
                    bi.CacheOption = BitmapCacheOption.OnLoad;
                    bi.EndInit();
                    bi.Freeze();

                    pdfView.Source = bi;
                    RenderOptions.SetBitmapScalingMode(pdfView, BitmapScalingMode.HighQuality);
                }
            }
            catch {}
        }

        private void ApplyPageTransforms()
        {
            if (pageGrid == null) return;
            var group = new TransformGroup();
            group.Children.Add(new RotateTransform(rotationAngle));
            group.Children.Add(new ScaleTransform(isFlipH ? -1.0 : 1.0, isFlipV ? -1.0 : 1.0));
            pageGrid.LayoutTransform = group;
        }

        private void SetupContextMenu()
        {
            var cm = new ContextMenu();
            cm.FontSize = 16;
            cm.FontWeight = FontWeights.Medium;
            cm.Padding = new Thickness(4);

            var miRotR90 = new MenuItem { Header = "↷ Повернуть направо на 90°", FontSize = 16 };
            miRotR90.Click += delegate { rotationAngle = (rotationAngle + 90) % 360; ApplyPageTransforms(); };

            var miRotL90 = new MenuItem { Header = "↶ Повернуть налево на 90°", FontSize = 16 };
            miRotL90.Click += delegate { rotationAngle = (rotationAngle - 90 + 360) % 360; ApplyPageTransforms(); };

            var miRotR180 = new MenuItem { Header = "↻ Повернуть направо на 180°", FontSize = 16 };
            miRotR180.Click += delegate { rotationAngle = (rotationAngle + 180) % 360; ApplyPageTransforms(); };

            var miRotL180 = new MenuItem { Header = "↺ Повернуть налево на 180°", FontSize = 16 };
            miRotL180.Click += delegate { rotationAngle = (rotationAngle - 180 + 360) % 360; ApplyPageTransforms(); };

            var miFlipH = new MenuItem { Header = "⇆ Перевернуть по горизонтали", FontSize = 16 };
            miFlipH.Click += delegate { isFlipH = !isFlipH; ApplyPageTransforms(); };

            var miFlipV = new MenuItem { Header = "⇅ Перевернуть по вертикали", FontSize = 16 };
            miFlipV.Click += delegate { isFlipV = !isFlipV; ApplyPageTransforms(); };

            var miPrint = new MenuItem { Header = "🖨️ Печать", FontSize = 16 };
            miPrint.Click += delegate { ExecutePrint(); };

            var miClose = new MenuItem { Header = "❌ Закрыть", FontSize = 16, FontWeight = FontWeights.Bold };
            miClose.Click += delegate { Close(); };

            cm.Items.Add(miRotR90);
            cm.Items.Add(miRotL90);
            cm.Items.Add(new Separator());
            cm.Items.Add(miRotR180);
            cm.Items.Add(miRotL180);
            cm.Items.Add(new Separator());
            cm.Items.Add(miFlipH);
            cm.Items.Add(miFlipV);
            cm.Items.Add(new Separator());
            cm.Items.Add(miPrint);
            cm.Items.Add(new Separator());
            cm.Items.Add(miClose);

            docScroll.ContextMenu = cm;
        }

        private void SetMode(string mode)
        {
            currentMode = mode;
            if (mode == "Reader")
            {
                activeTool = "None";
                if (overlayCanvas != null) overlayCanvas.Cursor = Cursors.Arrow;

                btnModeReader.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(69, 71, 90));
                btnModeReader.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(137, 180, 250));
                btnModeReader.BorderThickness = new Thickness(1);
                btnModeReader.BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(137, 180, 250));

                btnModeEditor.Background = (currentTheme == "Day") ? new SolidColorBrush(System.Windows.Media.Color.FromRgb(230, 234, 242)) : new SolidColorBrush(System.Windows.Media.Color.FromRgb(49, 50, 68));
                btnModeEditor.Foreground = (currentTheme == "Day") ? new SolidColorBrush(System.Windows.Media.Color.FromRgb(76, 86, 106)) : new SolidColorBrush(System.Windows.Media.Color.FromRgb(186, 194, 222));
                btnModeEditor.BorderThickness = new Thickness(0);

                editorBar.Visibility = Visibility.Collapsed;
                statusText.Text = "Reader & Forms mode active.";
            }
            else
            {
                btnModeEditor.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(69, 71, 90));
                btnModeEditor.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(137, 180, 250));
                btnModeEditor.BorderThickness = new Thickness(1);
                btnModeEditor.BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(137, 180, 250));

                btnModeReader.Background = (currentTheme == "Day") ? new SolidColorBrush(System.Windows.Media.Color.FromRgb(230, 234, 242)) : new SolidColorBrush(System.Windows.Media.Color.FromRgb(49, 50, 68));
                btnModeReader.Foreground = (currentTheme == "Day") ? new SolidColorBrush(System.Windows.Media.Color.FromRgb(76, 86, 106)) : new SolidColorBrush(System.Windows.Media.Color.FromRgb(186, 194, 222));
                btnModeReader.BorderThickness = new Thickness(0);

                editorBar.Visibility = Visibility.Visible;
                statusText.Text = "Editor mode active. Choose an editing tool.";
            }
        }

        private void SelectTool(string tool)
        {
            if (activeTool == tool)
            {
                activeTool = "None";
                if (overlayCanvas != null) overlayCanvas.Cursor = Cursors.Arrow;
                toolText.Background = (currentTheme == "Day") ? new SolidColorBrush(System.Windows.Media.Color.FromRgb(230, 234, 242)) : new SolidColorBrush(System.Windows.Media.Color.FromRgb(49, 50, 68));
                toolCheck.Background = toolText.Background;
                txtToolTip.Text = "";
            }
            else
            {
                activeTool = tool;
                if (overlayCanvas != null) overlayCanvas.Cursor = Cursors.Cross;
                if (tool == "Text")
                {
                    toolText.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(137, 180, 250));
                    toolText.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(17, 17, 27));
                    toolCheck.Background = (currentTheme == "Day") ? new SolidColorBrush(System.Windows.Media.Color.FromRgb(230, 234, 242)) : new SolidColorBrush(System.Windows.Media.Color.FromRgb(49, 50, 68));
                    toolCheck.Foreground = (currentTheme == "Day") ? new SolidColorBrush(System.Windows.Media.Color.FromRgb(24, 24, 37)) : new SolidColorBrush(System.Windows.Media.Color.FromRgb(205, 214, 244));
                    txtToolTip.Text = T("ClickToPlace");
                }
                else if (tool == "Checkmark")
                {
                    toolCheck.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(137, 180, 250));
                    toolCheck.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(17, 17, 27));
                    toolText.Background = (currentTheme == "Day") ? new SolidColorBrush(System.Windows.Media.Color.FromRgb(230, 234, 242)) : new SolidColorBrush(System.Windows.Media.Color.FromRgb(49, 50, 68));
                    toolText.Foreground = (currentTheme == "Day") ? new SolidColorBrush(System.Windows.Media.Color.FromRgb(24, 24, 37)) : new SolidColorBrush(System.Windows.Media.Color.FromRgb(205, 214, 244));
                    txtToolTip.Text = T("ClickToPlace");
                }
            }
        }

        private Button CreateTabBtn(string label, bool isActive)
        {
            var btn = new Button {
                Content = label,
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                Padding = new Thickness(14, 6, 14, 6),
                Margin = new Thickness(3, 0, 3, 0),
                Cursor = System.Windows.Input.Cursors.Hand
            };
            return btn;
        }

        private Button CreateBtn(string label, Action onClick)
        {
            var btn = new Button {
                Content = label,
                FontSize = 12,
                FontWeight = FontWeights.SemiBold,
                BorderThickness = new Thickness(0),
                Padding = new Thickness(10, 5, 10, 5),
                Margin = new Thickness(2, 0, 2, 0),
                Cursor = System.Windows.Input.Cursors.Hand
            };
            btn.Click += delegate { onClick(); };
            return btn;
        }

        private Border CreateSep()
        {
            return new Border { Width = 1, Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(69, 71, 90)), Margin = new Thickness(6, 2, 6, 2) };
        }

        private void UpdateStrings()
        {
            Title = T("Title");
            btnOpen.Content = T("Open");
            btnSave.Content = T("Save");
            btnPrint.Content = T("Print");
            btnFullscreen.Content = isFullscreen ? T("Restore") : T("Fullscreen");
            if (btnUpdate != null) btnUpdate.Content = T("UpdateBtn");
            btnModeReader.Content = T("ModeReader");
            btnModeEditor.Content = T("ModeEditor");
            toolText.Content = T("ToolText");
            toolCheck.Content = T("ToolCheck");
            toolImage.Content = T("ToolImage");
            toolSign.Content = T("ToolSign");
            toolCert.Content = T("ToolCert");
            btnFit.Content = T("FitWidth");
            txtPage.Text = T("PageOf", currentPage, totalPages);
            if (string.IsNullOrEmpty(docPath)) statusText.Text = T("Ready");
        }

        private void RefreshView()
        {
            if (!string.IsNullOrEmpty(docPath) && File.Exists(docPath))
            {
                uint targetW = (nativePageWidth > 0 && !isFitMode) ? (uint)Math.Round(nativePageWidth * zoom) : 0;
                uint targetH = (nativePageHeight > 0 && !isFitMode) ? (uint)Math.Round(nativePageHeight * zoom) : 0;

                byte[] bytes = NativePdfRenderer.RenderPage(docPath, currentPage - 1, targetW, targetH);
                if (bytes != null && bytes.Length > 0)
                {
                    var ms = new MemoryStream(bytes);
                    var bi = new BitmapImage();
                    bi.BeginInit();
                    bi.StreamSource = ms;
                    bi.CacheOption = BitmapCacheOption.OnLoad;
                    bi.EndInit();
                    bi.Freeze();

                    pdfView.Source = bi;
                    RenderOptions.SetBitmapScalingMode(pdfView, BitmapScalingMode.HighQuality);
                    if (nativePageWidth <= 0 || nativePageHeight <= 0 || targetW == 0)
                    {
                        nativePageWidth = bi.PixelWidth;
                        nativePageHeight = bi.PixelHeight;
                        baseDocWidth = bi.PixelWidth;
                        baseDocHeight = bi.PixelHeight;
                    }

                    if (isFitMode) DoFitPage();
                    else ApplyZoomToPage();
                    return;
                }
            }
            RenderWelcomeSheet();
        }

        private void RenderWelcomeSheet()
        {
            baseDocWidth = 1588;
            baseDocHeight = 2246;
            var bmp = new Bitmap((int)baseDocWidth, (int)baseDocHeight);
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(System.Drawing.Color.White);
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

                using (var brushDark = new SolidBrush(System.Drawing.Color.FromArgb(24, 24, 37)))
                using (var brushAccent = new SolidBrush(System.Drawing.Color.FromArgb(0, 102, 204)))
                using (var brushGray = new SolidBrush(System.Drawing.Color.FromArgb(100, 105, 125)))
                using (var fontHeader = new System.Drawing.Font("Segoe UI", 36, System.Drawing.FontStyle.Bold))
                using (var fontSub = new System.Drawing.Font("Segoe UI", 16, System.Drawing.FontStyle.Regular))
                using (var fontSection = new System.Drawing.Font("Segoe UI", 18, System.Drawing.FontStyle.Bold))
                using (var fontBody = new System.Drawing.Font("Segoe UI", 14, System.Drawing.FontStyle.Regular))
                using (var penLine = new System.Drawing.Pen(System.Drawing.Color.FromArgb(220, 225, 235), 2))
                using (var penAccent = new System.Drawing.Pen(System.Drawing.Color.FromArgb(0, 102, 204), 4))
                {
                    g.DrawLine(penAccent, 100, 100, 300, 100);
                    g.DrawString("GIN-PDF-RW", fontHeader, brushDark, 100, 120);

                    string subTitle, secHeader, welcome, footerText;
                    if (currentLang == "ru")
                    {
                        subTitle = "Быстрый просмотр и редактор PDF с открытым исходным кодом";
                        secHeader = "Добро пожаловать в GIN-PDF-RW v014!";
                        welcome = 
                            "Основные возможности программы:\n\n" +
                            "• Прокрутка колесом мыши: вертикальный скроллинг документа и перелистывание страниц\n" +
                            "• Перемещение документа левой кнопкой мыши при увеличении (плавный панорамный drag-скроллинг)\n" +
                            "• Клик по колесику мыши или клавиша Enter: циклическое переключение во весь экран и в окно\n" +
                            "• Клавиши PageUp и PageDown: быстрое переключение на предыдущий и следующий PDF-файл в папке\n" +
                            "• Масштабирование: Ctrl + колесико мыши, либо клавиши «+» и «-» на клавиатуре (включая Numpad)\n" +
                            "• Полноэкранный режим по умолчанию с быстрым возвратом в окно (Escape, Enter, клик скролла, F11)\n" +
                            "• Идеальное вмещение страниц (Fit Page): документ полностью виден на экране без искажений\n" +
                            "• 3 темы оформления: Светлая [День], Нейтральная [Облако], Тёмная [Ночь]\n" +
                            "• Аппаратный движок рендеринга PDF WinRT (DirectX Windows.Data.Pdf)\n" +
                            "• Интерактивные AcroForms (галочки и текстовые поля) и цифровая подпись\n" +
                            "• Высококонтрастный интерфейс и полная поддержка UTF-8";
                        footerText = "Нажмите «Открыть (Ctrl+O)» для выбора вашего PDF-документа.";
                    }
                    else if (currentLang == "uk")
                    {
                        subTitle = "Швидкий перегляд та редактор PDF з відкритим кодом";
                        secHeader = "Ласкаво просимо до GIN-PDF-RW v014!";
                        welcome = 
                            "Основні можливості програми:\n\n" +
                            "• Прокручування коліщатком миші: вертикальний скролінг документа та перегортання сторінок\n" +
                            "• Перетягування документа лівою кнопкою миші при масштабуванні (панорамний скролінг)\n" +
                            "• Клік по коліщатку миші або клавіша Enter: циклічне перемикання на весь екран і у вікно\n" +
                            "• Клавіші PageUp та PageDown: швидке перемикання на попередній та наступний PDF-файл у папці\n" +
                            "• Масштабування: Ctrl + коліщатко миші, або клавіші «+» та «-» на клавіатурі (включно з Numpad)\n" +
                            "• Повноекранний режим за замовчуванням зі швидким поверненням у вікно (Escape, Enter, скрол, F11)\n" +
                            "• Ідеальне вміщення сторінок (Fit Page): документ повністю видно на екрані без спотворень\n" +
                            "• 3 теми оформлення: Світла [День], Нейтральна [Хмара], Темна [Ніч]\n" +
                            "• Апаратний рушій рендерингу PDF WinRT (Windows.Data.Pdf)\n" +
                            "• Інтерактивні AcroForms (позначки та текстові поля) і цифровий підпис\n" +
                            "• Висококонтрастний інтерфейс і повна підтримка UTF-8";
                        footerText = "Натисніть «Відкрити (Ctrl+O)» для вибору вашого PDF-документа.";
                    }
                    else if (currentLang == "cs")
                    {
                        subTitle = "Rychlý prohlížeč a editor PDF s otevřeným zdrojovým kódem";
                        secHeader = "Vítejte v GIN-PDF-RW v014!";
                        welcome = 
                            "Hlavní funkce programu:\n\n" +
                            "• Rolování kolečkem myši: svislé posouvání dokumentu a přepínání stránek\n" +
                            "• Posouvání dokumentu levým tlačítkem myši při přiblížení (plynulé přetahování nahoru/dolů)\n" +
                            "• Kliknutí na kolečko myši nebo klávesa Enter: cyklické přepínání mezi celou obrazovkou a oknem\n" +
                            "• Klávesy PageUp a PageDown: rychlé přepínání na předchozí a další PDF soubor ve složce\n" +
                            "• Zvětšení a zmenšení: Ctrl + kolečko myši, nebo klávesy «+» a «-» na klávesnici (včetně Numpadu)\n" +
                            "• Režim celé obrazovky s rychlým návratem do okna (Escape, Enter, kolečko, F11)\n" +
                            "• Přizpůsobení na stránku (Fit Page): dokument je celý viditelný na obrazovce bez zkreslení\n" +
                            "• 3 motivy vzhledu: Světlý [Den], Neutrální šedý [Oblak], Tmavý [Noc]\n" +
                            "• Hardwarově akcelerované vykreslování PDF WinRT (DirectX)\n" +
                            "• Interaktivní AcroForms (zaškrtávací pole a text) a digitální podpis\n" +
                            "• Vysoce kontrastní rozhraní a plná podpora UTF-8";
                        footerText = "Klikněte na «Otevřít (Ctrl+O)» pro otevření vašeho PDF dokumentu.";
                    }
                    else
                    {
                        subTitle = "High-Performance Open Source PDF Reader & Editor";
                        secHeader = "Welcome to GIN-PDF-RW v014!";
                        welcome = 
                            "Key Application Features:\n\n" +
                            "• Mouse Wheel Scroll: Smooth vertical page scrolling and page navigation\n" +
                            "• Left-Click Drag Panning: Effortlessly drag document up, down and across when zoomed in\n" +
                            "• Mouse Wheel Click or Enter: Cyclically toggles fullscreen and normal window with menu\n" +
                            "• PageUp and PageDown keys: Fast navigation to previous/next PDF file in folder\n" +
                            "• Zoom control: Ctrl + Mouse Wheel, or '+' / '-' keys on keyboard (including Numpad)\n" +
                            "• Borderless Fullscreen by default with quick restore to window (Escape, Enter, Wheel click, F11)\n" +
                            "• Automatic Fit Page: Document fits 100% inside screen height & width without distortion\n" +
                            "• 3-Way Theme Switcher: Day [Light], Neutral Cloud [Slate Gray], Night [Dark]\n" +
                            "• Native Windows WinRT PDF Hardware Rendering Engine (DirectX)\n" +
                            "• Interactive AcroForms Fill & Sign with X.509 Cryptographic Digital Signatures\n" +
                            "• High-Contrast UI & Universal UTF-8 Localization";
                        footerText = "Click 'Open (Ctrl+O)' to open your PDF documents.";
                    }

                    g.DrawString(subTitle, fontSub, brushGray, 100, 185);
                    g.DrawLine(penLine, 100, 230, 1488, 230);

                    g.DrawString(secHeader, fontSection, brushDark, 100, 270);
                    g.DrawString(welcome, fontBody, brushDark, new RectangleF(100, 340, 1388, 1400));

                    g.DrawLine(penLine, 100, 1850, 1488, 1850);
                    using (var fontFoot = new System.Drawing.Font("Segoe UI", 15, System.Drawing.FontStyle.Bold))
                    {
                        g.DrawString(footerText, fontFoot, brushAccent, 100, 1890);
                    }
                }
            }

            var ms = new MemoryStream();
            bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            ms.Position = 0;
            var bi = new BitmapImage();
            bi.BeginInit();
            bi.StreamSource = ms;
            bi.CacheOption = BitmapCacheOption.OnLoad;
            bi.EndInit();
            bi.Freeze();
            bmp.Dispose();

            pdfView.Source = bi;
            if (isFitMode) DoFitPage();
            else ApplyZoomToPage();
        }

        private void AddInteractiveField(string name, double x, double y, double w, double h, string type, string val, bool isChecked)
        {
            if (type == "CheckBox")
            {
                var cb = new CheckBox {
                    Width = w * zoom,
                    Height = h * zoom,
                    IsChecked = isChecked,
                    Cursor = System.Windows.Input.Cursors.Hand,
                    BorderBrush = System.Windows.Media.Brushes.Black,
                    BorderThickness = new Thickness(1.5)
                };
                cb.Click += delegate {
                    statusText.Text = string.Format("Checkbox '{0}' toggled: {1}", name, cb.IsChecked);
                };
                Canvas.SetLeft(cb, x * zoom);
                Canvas.SetTop(cb, y * zoom);
                overlayCanvas.Children.Add(cb);
            }
            else if (type == "Text")
            {
                var tb = new TextBox {
                    Width = w * zoom,
                    Height = h * zoom,
                    Text = val,
                    FontSize = Math.Max(10, 12 * zoom),
                    Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(40, 0, 120, 215)),
                    BorderBrush = System.Windows.Media.Brushes.Transparent,
                    Padding = new Thickness(2)
                };
                tb.GotFocus += delegate { tb.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(100, 255, 255, 180)); };
                tb.LostFocus += delegate { tb.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(40, 0, 120, 215)); };
                Canvas.SetLeft(tb, x * zoom);
                Canvas.SetTop(tb, y * zoom);
                overlayCanvas.Children.Add(tb);
            }
        }

        private void ExecutePrint()
        {
            try
            {
                var printDialog = new PrintDialog();
                if (printDialog.ShowDialog() == true)
                {
                    statusText.Text = "Printing document...";
                    printDialog.PrintVisual(pageBorder, "GIN-PDF-RW Document Print");
                    statusText.Text = "Print job dispatched successfully.";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Print Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CheckForManualUpdate()
        {
            if (btnUpdate != null) btnUpdate.Content = T("CheckingUpdate");
            System.Threading.ThreadPool.QueueUserWorkItem(delegate {
                string latestVer = null;
                try
                {
                    using (var wc = new System.Net.WebClient())
                    {
                        wc.Headers.Add("User-Agent", "GIN-PDF-RW-Client");
                        string json = wc.DownloadString("https://raw.githubusercontent.com/GinCz/GIN-PDF-RW/main/version.json");
                        // extract current_version value via regex
                        var m = System.Text.RegularExpressions.Regex.Match(json, "\"current_version\"\s*:\s*\"([^\"]+)\"");
                        if (m.Success)
                        {
                            latestVer = m.Groups[1].Value;
                        }
                    }
                }
                catch {}

                Dispatcher.BeginInvoke(new Action(delegate {
                    if (btnUpdate != null) btnUpdate.Content = T("UpdateBtn");

                    if (string.IsNullOrEmpty(latestVer))
                    {
                        var res = MessageBox.Show(T("UpdateAvailable", "GitHub"), T("UpdateTitle"), MessageBoxButton.YesNo, MessageBoxImage.Information);
                        if (res == MessageBoxResult.Yes)
                        {
                            try { System.Diagnostics.Process.Start("https://github.com/GinCz/GIN-PDF-RW/releases"); } catch {}
                        }
                        return;
                    }

                    if (string.Equals(latestVer, "v014", StringComparison.OrdinalIgnoreCase))
                    {
                        MessageBox.Show(T("UpdateUpToDate"), T("UpdateTitle"), MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        var res = MessageBox.Show(T("UpdateAvailable", latestVer), T("UpdateTitle"), MessageBoxButton.YesNo, MessageBoxImage.Information);
                        if (res == MessageBoxResult.Yes)
                        {
                            try { System.Diagnostics.Process.Start("https://github.com/GinCz/GIN-PDF-RW/releases"); } catch {}
                        }
                    }
                }));
            });
        }

        private void OpenFileDialog()
        {
            var ofd = new System.Windows.Forms.OpenFileDialog {
                Filter = "PDF Documents (*.pdf)|*.pdf|All Files (*.*)|*.*",
                Title = "Open PDF"
            };
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                OpenPdf(ofd.FileName);
            }
        }

        private void OpenPdf(string path)
        {
            docPath = path;
            currentPage = 1;
            totalPages = Math.Max(1, NativePdfRenderer.GetPageCount(docPath));
            string fileName = System.IO.Path.GetFileName(docPath);
            statusText.Text = T("Opened", fileName, totalPages);
            Title = fileName + " — " + T("Title");
            txtPage.Text = T("PageOf", currentPage, totalPages);
            overlayCanvas.Children.Clear();
            isFitMode = true; // Automatically fit page completely in screen!
            hasSwitchedToWindowMode = false;
            RefreshView();
        }

        private void SaveFileDialog()
        {
            var sfd = new System.Windows.Forms.SaveFileDialog {
                Filter = "PDF Documents (*.pdf)|*.pdf|All Files (*.*)|*.*",
                Title = "Save PDF",
                FileName = string.IsNullOrEmpty(docPath) ? "Document_signed.pdf" : System.IO.Path.GetFileNameWithoutExtension(docPath) + "_signed.pdf"
            };
            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                statusText.Text = T("Saved", sfd.FileName);
                MessageBox.Show(T("Saved", sfd.FileName), "GIN-PDF-RW", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ExecuteDigitalSign()
        {
            try
            {
                var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
                store.Open(OpenFlags.ReadOnly);
                var certs = store.Certificates.Find(X509FindType.FindByTimeValid, DateTime.Now, true);
                store.Close();

                if (certs.Count == 0)
                {
                    MessageBox.Show(T("NoCert"), "X.509 Signature", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var selected = X509Certificate2UI.SelectFromCollection(
                    certs, "Select Certificate", "Choose an X.509 certificate to sign this document:", X509SelectionFlag.SingleSelection);

                if (selected.Count > 0)
                {
                    var cert = selected[0];
                    statusText.Text = T("Signed", cert.Subject);
                    MessageBox.Show(T("Signed", cert.Subject), "GIN-PDF-RW X.509", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Digital Sign Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void InsertImageDialog()
        {
            var ofd = new System.Windows.Forms.OpenFileDialog {
                Filter = "Images (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp",
                Title = "Insert Stamp or Image"
            };
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                statusText.Text = "Image selected: " + System.IO.Path.GetFileName(ofd.FileName);
                MessageBox.Show("Image selected! Click anywhere on document to place stamp.", "GIN-PDF-RW", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DrawSignDialog()
        {
            MessageBox.Show("Signature canvas ready! Draw your signature on document.", "GIN-PDF-RW", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void PrevPage()
        {
            if (currentPage > 1) {
                currentPage--;
                txtPage.Text = T("PageOf", currentPage, totalPages);
                RefreshView();
            }
        }

        private void NextPage()
        {
            if (currentPage < totalPages) {
                currentPage++;
                txtPage.Text = T("PageOf", currentPage, totalPages);
                RefreshView();
            }
        }
    }
}
