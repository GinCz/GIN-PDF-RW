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

[assembly: AssemblyTitle("GIN-PDF-RW_v021")]
[assembly: AssemblyProduct("GIN-PDF-RW_v021")]
[assembly: AssemblyDescription("GIN-PDF-RW_v021")]
[assembly: AssemblyCompany("VladiMIR+AI")]
[assembly: AssemblyCopyright("VladiMIR+AI")]
[assembly: AssemblyFileVersion("0.021.0.0")]
[assembly: AssemblyVersion("0.021.0.0")]

namespace GinPdfRw
{
    public class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            AppDomain.CurrentDomain.AssemblyResolve += delegate(object sender, ResolveEventArgs ea) {
                try {
                    string n = new AssemblyName(ea.Name).Name;
                    string f = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, n + ".dll");
                    if (File.Exists(f)) return Assembly.LoadFrom(f);
                } catch {}
                return null;
            };
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
        private Button btnSaveAs;
        private Button btnUndo;
        private Button btnRedo;
        private Button btnSaveExit;
        private Button btnCancel;
        private List<UIElement> redoStack = new List<UIElement>();
        private Dictionary<int, List<UIElement>> pageEdits = new Dictionary<int, List<UIElement>>();
        private Dictionary<int, PdfPageText> textCache = new Dictionary<int, PdfPageText>();
        private Viewbox overlayHost;
        private Button toolEdit;
        private bool textEngineWarned = false;
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
        private Grid pageGrid;
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
            en.Add("Title", "GIN-PDF-RW v021 — Zen PDF Reader & Editor");
            en.Add("Open", "📂 Open (Ctrl+O)");
            en.Add("Save", "💾 Save (Ctrl+S)");
            en.Add("Print", "🖨️ Print (Ctrl+P)");
            en.Add("ModeReader", "📖 Reader & Forms");
            en.Add("ModeEditor", "✏️ Editor");
            en.Add("Fullscreen", "⛶ Fullscreen (F11)");
            en.Add("Restore", "⛶ Window (Esc)");
            en.Add("UpdateBtn", "v021 (Update)");
            en.Add("CheckingUpdate", "Checking GitHub...");
            en.Add("UpdateUpToDate", "You are using the latest version (v021)!");
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
            en.Add("CmR90", "↷ Rotate right 90°");
            en.Add("CmL90", "↶ Rotate left 90°");
            en.Add("CmR180", "↻ Rotate right 180°");
            en.Add("CmL180", "↺ Rotate left 180°");
            en.Add("CmFlipH", "⇆ Flip horizontally");
            en.Add("CmFlipV", "⇅ Flip vertically");
            en.Add("CmPrint", "🖨️ Print");
            en.Add("CmClose", "❌ Close");
            en.Add("SaveAs", "💾 Save As (Ctrl+Shift+S)");
            en.Add("Undo", "↩ Undo (Ctrl+Z)");
            en.Add("Redo", "↪ Redo (Ctrl+Y)");
            en.Add("SaveExit", "✔ Save & Exit");
            en.Add("Cancel", "✖ Cancel");
            en.Add("ToolEdit", "✏ Edit Text");
            en.Add("ClickToEdit", "Click on existing text to edit it.");
            en.Add("NoTextHere", "No text found here (scanned page?).");
            loc.Add("en", en);

            var ru = new Dictionary<string, string>();
            ru.Add("Title", "GIN-PDF-RW v021 — Просмотр и редактор PDF");
            ru.Add("Open", "📂 Открыть (Ctrl+O)");
            ru.Add("Save", "💾 Сохранить (Ctrl+S)");
            ru.Add("Print", "🖨️ Печать (Ctrl+P)");
            ru.Add("ModeReader", "📖 Чтение и формы");
            ru.Add("ModeEditor", "✏️ Редактировать");
            ru.Add("Fullscreen", "⛶ Во весь экран (F11)");
            ru.Add("Restore", "⛶ В окно (Esc)");
            ru.Add("UpdateBtn", "v021 (обновить)");
            ru.Add("CheckingUpdate", "Проверка GitHub...");
            ru.Add("UpdateUpToDate", "У вас установлена самая актуальная версия (v021)!");
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
            ru.Add("CmR90", "↷ Повернуть направо на 90°");
            ru.Add("CmL90", "↶ Повернуть налево на 90°");
            ru.Add("CmR180", "↻ Повернуть направо на 180°");
            ru.Add("CmL180", "↺ Повернуть налево на 180°");
            ru.Add("CmFlipH", "⇆ Перевернуть по горизонтали");
            ru.Add("CmFlipV", "⇅ Перевернуть по вертикали");
            ru.Add("CmPrint", "🖨️ Печать");
            ru.Add("CmClose", "❌ Закрыть");
            ru.Add("SaveAs", "💾 Сохранить как (Ctrl+Shift+S)");
            ru.Add("Undo", "↩ Назад (Ctrl+Z)");
            ru.Add("Redo", "↪ Вперёд (Ctrl+Y)");
            ru.Add("SaveExit", "✔ Сохранить и выйти");
            ru.Add("Cancel", "✖ Отмена");
            ru.Add("ToolEdit", "✏ Править текст");
            ru.Add("ClickToEdit", "Кликните по тексту документа, чтобы его изменить.");
            ru.Add("NoTextHere", "Здесь нет текста (скан?).");
            loc.Add("ru", ru);

            var uk = new Dictionary<string, string>();
            uk.Add("Title", "GIN-PDF-RW v021 — Перегляд і редактор PDF");
            uk.Add("Open", "📂 Відкрити (Ctrl+O)");
            uk.Add("Save", "💾 Зберегти (Ctrl+S)");
            uk.Add("Print", "🖨️ Друк (Ctrl+P)");
            uk.Add("ModeReader", "📖 Читання та форми");
            uk.Add("ModeEditor", "✏️ Редагувати");
            uk.Add("Fullscreen", "⛶ На весь екран (F11)");
            uk.Add("Restore", "⛶ У вікно (Esc)");
            uk.Add("UpdateBtn", "v021 (оновити)");
            uk.Add("CheckingUpdate", "Перевірка GitHub...");
            uk.Add("UpdateUpToDate", "У вас встановлена найновіша версія (v021)!");
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
            uk.Add("CmR90", "↷ Повернути праворуч на 90°");
            uk.Add("CmL90", "↶ Повернути ліворуч на 90°");
            uk.Add("CmR180", "↻ Повернути праворуч на 180°");
            uk.Add("CmL180", "↺ Повернути ліворуч на 180°");
            uk.Add("CmFlipH", "⇆ Перевернути по горизонталі");
            uk.Add("CmFlipV", "⇅ Перевернути по вертикалі");
            uk.Add("CmPrint", "🖨️ Друк");
            uk.Add("CmClose", "❌ Закрити");
            uk.Add("SaveAs", "💾 Зберегти як (Ctrl+Shift+S)");
            uk.Add("Undo", "↩ Назад (Ctrl+Z)");
            uk.Add("Redo", "↪ Вперед (Ctrl+Y)");
            uk.Add("SaveExit", "✔ Зберегти і вийти");
            uk.Add("Cancel", "✖ Скасувати");
            uk.Add("ToolEdit", "✏ Редагувати текст");
            uk.Add("ClickToEdit", "Клікніть по тексту документа, щоб його змінити.");
            uk.Add("NoTextHere", "Тут немає тексту (скан?).");
            loc.Add("uk", uk);

            var cs = new Dictionary<string, string>();
            cs.Add("Title", "GIN-PDF-RW v021 — Prohlížeč a editor PDF");
            cs.Add("Open", "📂 Otevřít (Ctrl+O)");
            cs.Add("Save", "💾 Uložit (Ctrl+S)");
            cs.Add("Print", "🖨️ Tisk (Ctrl+P)");
            cs.Add("ModeReader", "📖 Čtení a formuláře");
            cs.Add("ModeEditor", "✏️ Upravit");
            cs.Add("Fullscreen", "⛶ Celá obrazovka (F11)");
            cs.Add("Restore", "⛶ Do okna (Esc)");
            cs.Add("UpdateBtn", "v021 (aktualizovat)");
            cs.Add("CheckingUpdate", "Kontrola GitHubu...");
            cs.Add("UpdateUpToDate", "Používáte nejnovější verzi (v021)!");
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
            cs.Add("CmR90", "↷ Otočit doprava o 90°");
            cs.Add("CmL90", "↶ Otočit doleva o 90°");
            cs.Add("CmR180", "↻ Otočit doprava o 180°");
            cs.Add("CmL180", "↺ Otočit doleva o 180°");
            cs.Add("CmFlipH", "⇆ Převrátit vodorovně");
            cs.Add("CmFlipV", "⇅ Převrátit svisle");
            cs.Add("CmPrint", "🖨️ Tisk");
            cs.Add("CmClose", "❌ Zavřít");
            cs.Add("SaveAs", "💾 Uložit jako (Ctrl+Shift+S)");
            cs.Add("Undo", "↩ Zpět (Ctrl+Z)");
            cs.Add("Redo", "↪ Vpřed (Ctrl+Y)");
            cs.Add("SaveExit", "✔ Uložit a ukončit");
            cs.Add("Cancel", "✖ Zrušit");
            cs.Add("ToolEdit", "✏ Upravit text");
            cs.Add("ClickToEdit", "Klikněte na existující text a upravte jej.");
            cs.Add("NoTextHere", "Zde není žádný text (sken?).");
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
            Title = "GIN-PDF-RW v021";
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
            btnSave = CreateBtn("💾 Save", delegate { SaveCurrent(); });
            btnPrint = CreateBtn("🖨️ Print", delegate { ExecutePrint(); });
            leftGroup.Children.Add(btnOpen);
            btnSaveAs = CreateBtn("💾 Save As", delegate { SaveAsDialog(); });
            leftGroup.Children.Add(btnSave);
            leftGroup.Children.Add(btnSaveAs);
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

            btnUpdate = CreateBtn("v021 (обновить)", delegate { CheckForManualUpdate(); });
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
            toolEdit = CreateBtn("✏ Edit Text", delegate { SelectTool("EditText"); });
            toolCheck = CreateBtn("☑ Checkmark (✓)", delegate { SelectTool("Checkmark"); });
            toolImage = CreateBtn("🖼 Stamp / Image", delegate { InsertImageDialog(); });
            toolSign = CreateBtn("✍ Draw Sign", delegate { DrawSignDialog(); });
            toolCert = CreateBtn("🔏 X.509 Sign", delegate { ExecuteDigitalSign(); });

            btnUndo = CreateBtn("↩ Undo", delegate { UndoEdit(); });
            btnRedo = CreateBtn("↪ Redo", delegate { RedoEdit(); });
            btnSaveExit = CreateBtn("✔ Save & Exit", delegate { if (SaveCurrent()) Close(); });
            btnCancel = CreateBtn("✖ Cancel", delegate { CancelEditing(); });
            txtToolTip = new TextBlock { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(12, 0, 0, 0), FontStyle = FontStyles.Italic };

            ebStack.Children.Add(btnUndo);
            ebStack.Children.Add(btnRedo);
            ebStack.Children.Add(CreateSep());
            ebStack.Children.Add(toolEdit);
            ebStack.Children.Add(toolText);
            ebStack.Children.Add(toolCheck);
            ebStack.Children.Add(toolImage);
            ebStack.Children.Add(toolSign);
            ebStack.Children.Add(toolCert);
            ebStack.Children.Add(CreateSep());
            ebStack.Children.Add(btnSaveExit);
            ebStack.Children.Add(btnCancel);
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
            pageGrid = new Grid();

            pdfView = new System.Windows.Controls.Image { Stretch = Stretch.Uniform };
            overlayCanvas = new Canvas { Background = System.Windows.Media.Brushes.Transparent, Cursor = System.Windows.Input.Cursors.Arrow };

            overlayCanvas.MouseDown += delegate(object s, MouseButtonEventArgs e) {
                var pos = e.GetPosition(overlayCanvas);
                ImageBox.DeselectAll();
                if (activeTool == "Checkmark") {
                    AddInteractiveField("Check", pos.X, pos.Y, 20, 20, "CheckBox", "", true);
                    redoStack.Clear();
                } else if (activeTool == "Text") {
                    AddInteractiveField("Text", pos.X, pos.Y, 180, 24, "Text", "Sample Text", false);
                    redoStack.Clear();
                } else if (activeTool == "EditText") {
                    EditTextAt(pos);
                }
            };

            // Left click dragging to pan document up/down and sideways smoothly when zoomed in
            docScroll.PreviewMouseLeftButtonDown += delegate(object s, MouseButtonEventArgs e) {
                if (activeTool == "None") {
                    if (!IsChildOfScrollBar(e.OriginalSource as DependencyObject) &&
                        !(e.OriginalSource is TextBox) &&
                        !(e.OriginalSource is Button) &&
                        !(e.OriginalSource is CheckBox) &&
                        !ImageBox.IsInside(e.OriginalSource as DependencyObject)) {
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
            overlayHost = new Viewbox { Stretch = Stretch.Fill, Child = overlayCanvas };
            overlayCanvas.Width = baseDocWidth;
            overlayCanvas.Height = baseDocHeight;
            pageGrid.Children.Add(overlayHost);
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
            verText = new TextBlock { Text = "GIN-PDF-RW v021 [VladiMIR+AI] | UTF-8", FontSize = 11, VerticalAlignment = VerticalAlignment.Center };

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
                bool inText = (e.OriginalSource is TextBox) || (Keyboard.FocusedElement is TextBox);
                if (inText) {
                    bool ctl = (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
                    if (!ctl) return;
                    if (e.Key == Key.Z || e.Key == Key.Y || e.Key == Key.A || e.Key == Key.C || e.Key == Key.V || e.Key == Key.X) return;
                }
                if (e.Key == Key.Delete && ImageBox.Current != null) {
                    UIElement del = ImageBox.Current;
                    ImageBox.DeselectAll();
                    if (overlayCanvas.Children.Contains(del)) { overlayCanvas.Children.Remove(del); redoStack.Add(del); }
                    e.Handled = true;
                    return;
                }
                if (e.Key == Key.Escape && currentMode == "Editor") {
                    if (activeTool != "None") SelectTool(activeTool);
                    ImageBox.DeselectAll();
                    e.Handled = true;
                    return;
                }
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
                    else if (e.Key == Key.S) { if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift) SaveAsDialog(); else SaveCurrent(); e.Handled = true; }
                    else if (e.Key == Key.Z) { UndoEdit(); e.Handled = true; }
                    else if (e.Key == Key.Y) { RedoEdit(); e.Handled = true; }
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
            Button[] btns = new Button[] { btnOpen, btnSave, btnSaveAs, toolEdit, btnUndo, btnRedo, btnSaveExit, btnCancel, btnPrint, btnPrev, btnNext, btnZoomOut, btnZoomIn, btnFit, btnFullscreen, btnThemeDay, btnThemeCloud, btnThemeNight, toolText, toolCheck, toolImage, toolSign, btnUpdate };
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
            if (overlayCanvas != null) { overlayCanvas.Width = baseDocWidth; overlayCanvas.Height = baseDocHeight; }
            if (overlayCanvas != null) foreach (UIElement ue in overlayCanvas.Children) { ImageBox ib = ue as ImageBox; if (ib != null) ib.UpdateHandle(zoom); }
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

            var miRotR90 = new MenuItem { Header = T("CmR90"), FontSize = 16 };
            miRotR90.Click += delegate { rotationAngle = (rotationAngle + 90) % 360; ApplyPageTransforms(); };

            var miRotL90 = new MenuItem { Header = T("CmL90"), FontSize = 16 };
            miRotL90.Click += delegate { rotationAngle = (rotationAngle - 90 + 360) % 360; ApplyPageTransforms(); };

            var miRotR180 = new MenuItem { Header = T("CmR180"), FontSize = 16 };
            miRotR180.Click += delegate { rotationAngle = (rotationAngle + 180) % 360; ApplyPageTransforms(); };

            var miRotL180 = new MenuItem { Header = T("CmL180"), FontSize = 16 };
            miRotL180.Click += delegate { rotationAngle = (rotationAngle - 180 + 360) % 360; ApplyPageTransforms(); };

            var miFlipH = new MenuItem { Header = T("CmFlipH"), FontSize = 16 };
            miFlipH.Click += delegate { isFlipH = !isFlipH; ApplyPageTransforms(); };

            var miFlipV = new MenuItem { Header = T("CmFlipV"), FontSize = 16 };
            miFlipV.Click += delegate { isFlipV = !isFlipV; ApplyPageTransforms(); };

            var miPrint = new MenuItem { Header = T("CmPrint"), FontSize = 16 };
            miPrint.Click += delegate { ExecutePrint(); };

            var miClose = new MenuItem { Header = T("CmClose"), FontSize = 16, FontWeight = FontWeights.Bold };
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
            pageBorder.ContextMenu = cm;
            pageGrid.ContextMenu = cm;
            pdfView.ContextMenu = cm;
            overlayCanvas.ContextMenu = cm;
            rootGrid.ContextMenu = cm;
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
            activeTool = (activeTool == tool) ? "None" : tool;
            if (overlayCanvas != null)
                overlayCanvas.Cursor = activeTool == "None" ? Cursors.Arrow : (activeTool == "EditText" ? Cursors.IBeam : Cursors.Cross);
            bool day = currentTheme == "Day";
            System.Windows.Media.Brush offBg = new SolidColorBrush(day ? System.Windows.Media.Color.FromRgb(230, 234, 242) : System.Windows.Media.Color.FromRgb(49, 50, 68));
            System.Windows.Media.Brush offFg = new SolidColorBrush(day ? System.Windows.Media.Color.FromRgb(24, 24, 37) : System.Windows.Media.Color.FromRgb(205, 214, 244));
            System.Windows.Media.Brush onBg = new SolidColorBrush(System.Windows.Media.Color.FromRgb(137, 180, 250));
            System.Windows.Media.Brush onFg = new SolidColorBrush(System.Windows.Media.Color.FromRgb(17, 17, 27));
            Button[] tools = new Button[] { toolEdit, toolText, toolCheck };
            string[] names = new string[] { "EditText", "Text", "Checkmark" };
            for (int i = 0; i < tools.Length; i++)
            {
                bool on = activeTool == names[i];
                tools[i].Background = on ? onBg : offBg;
                tools[i].Foreground = on ? onFg : offFg;
            }
            txtToolTip.Text = activeTool == "None" ? "" : (activeTool == "EditText" ? T("ClickToEdit") : T("ClickToPlace"));
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
            if (docScroll != null) SetupContextMenu();
            btnModeReader.Content = T("ModeReader");
            btnModeEditor.Content = T("ModeEditor");
            toolEdit.Content = T("ToolEdit");
            toolText.Content = T("ToolText");
            toolCheck.Content = T("ToolCheck");
            toolImage.Content = T("ToolImage");
            toolSign.Content = T("ToolSign");
            toolCert.Content = T("ToolCert");
            btnSaveAs.Content = T("SaveAs");
            btnUndo.Content = T("Undo");
            btnRedo.Content = T("Redo");
            btnSaveExit.Content = T("SaveExit");
            btnCancel.Content = T("Cancel");
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
                        secHeader = "Добро пожаловать в GIN-PDF-RW v021!";
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
                        secHeader = "Ласкаво просимо до GIN-PDF-RW v021!";
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
                        secHeader = "Vítejte v GIN-PDF-RW v021!";
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
                        secHeader = "Welcome to GIN-PDF-RW v021!";
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
                    Width = w,
                    Height = h,
                    IsChecked = isChecked,
                    Cursor = System.Windows.Input.Cursors.Hand,
                    BorderBrush = System.Windows.Media.Brushes.Black,
                    BorderThickness = new Thickness(1.5)
                };
                cb.Click += delegate {
                    statusText.Text = string.Format("Checkbox '{0}' toggled: {1}", name, cb.IsChecked);
                };
                Canvas.SetLeft(cb, x);
                Canvas.SetTop(cb, y);
                overlayCanvas.Children.Add(cb);
            }
            else if (type == "Text")
            {
                var tb = new TextBox {
                    Width = w,
                    Height = h,
                    Text = val,
                    FontSize = 12,
                    Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(40, 0, 120, 215)),
                    BorderBrush = System.Windows.Media.Brushes.Transparent,
                    Padding = new Thickness(2)
                };
                tb.GotFocus += delegate { tb.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(100, 255, 255, 180)); };
                tb.LostFocus += delegate { tb.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(40, 0, 120, 215)); };
                Canvas.SetLeft(tb, x);
                Canvas.SetTop(tb, y);
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
                        var m = System.Text.RegularExpressions.Regex.Match(json, "\"current_version\"\\s*:\\s*\"([^\"]+)\"");
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

                    if (string.Equals(latestVer, "v021", StringComparison.OrdinalIgnoreCase))
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
            pageEdits.Clear();
            textCache.Clear();
            redoStack.Clear();
            isFitMode = true; // Automatically fit page completely in screen!
            hasSwitchedToWindowMode = false;
            RefreshView();
        }

        private bool HasAnyEdits()
        {
            return overlayCanvas.Children.Count > 0 || pageEdits.Count > 0;
        }

        private void StashEdits(int page)
        {
            var l = new List<UIElement>();
            foreach (UIElement ue in overlayCanvas.Children) l.Add(ue);
            if (l.Count > 0) pageEdits[page] = l; else pageEdits.Remove(page);
            overlayCanvas.Children.Clear();
        }

        private void RestoreEdits(int page)
        {
            overlayCanvas.Children.Clear();
            List<UIElement> l;
            if (pageEdits.TryGetValue(page, out l))
            {
                foreach (UIElement ue in l) overlayCanvas.Children.Add(ue);
                pageEdits.Remove(page);
            }
            redoStack.Clear();
        }

        private PdfPageText GetPageText(int pageIndex)
        {
            PdfPageText t;
            if (textCache.TryGetValue(pageIndex, out t)) return t;
            try { t = TextExtract.Extract(docPath, pageIndex); }
            catch (Exception ex)
            {
                if (!textEngineWarned) { textEngineWarned = true; MessageBox.Show("Text engine error: " + ex.Message, "GIN-PDF-RW", MessageBoxButton.OK, MessageBoxImage.Warning); }
                return null;
            }
            textCache[pageIndex] = t;
            return t;
        }

        private void EditTextAt(System.Windows.Point pos)
        {
            if (string.IsNullOrEmpty(docPath) || !File.Exists(docPath)) return;
            PdfPageText pt = GetPageText(currentPage - 1);
            if (pt == null || pt.PageWidth <= 0 || pt.PageHeight <= 0) return;
            double sx = baseDocWidth / pt.PageWidth, sy = baseDocHeight / pt.PageHeight;
            PdfTextLine hit = null;
            foreach (PdfTextLine ln in pt.Lines)
            {
                double padV = ln.H * 0.35 + 2;
                if (pos.X >= ln.X * sx - 3 && pos.X <= (ln.X + ln.W) * sx + 3 &&
                    pos.Y >= (ln.Y - padV) * sy && pos.Y <= (ln.Y + ln.H + padV) * sy) { hit = ln; break; }
            }
            if (hit == null) { statusText.Text = T("NoTextHere"); return; }
            double pad = Math.Max(1.5, hit.H * 0.30);
            var tb = new TextBox {
                Text = hit.Text,
                FontFamily = new System.Windows.Media.FontFamily("Arial"),
                FontSize = Math.Max(6, hit.FontPt * sx),
                Foreground = System.Windows.Media.Brushes.Black,
                Background = System.Windows.Media.Brushes.White,
                BorderBrush = System.Windows.Media.Brushes.Transparent,
                BorderThickness = new Thickness(1),
                Padding = new Thickness(0),
                VerticalContentAlignment = VerticalAlignment.Center,
                MinWidth = hit.W * sx + 6,
                Height = (hit.H + pad * 2) * sy,
                AcceptsReturn = false
            };
            tb.GotFocus += delegate { tb.BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 120, 215)); };
            tb.LostFocus += delegate { tb.BorderBrush = System.Windows.Media.Brushes.Transparent; };
            Canvas.SetLeft(tb, hit.X * sx - 1);
            Canvas.SetTop(tb, (hit.Y - pad) * sy);
            overlayCanvas.Children.Add(tb);
            redoStack.Clear();
            tb.UpdateLayout();
            tb.Focus();
            Keyboard.Focus(tb);
            tb.CaretIndex = tb.Text.Length;
        }

        private void UndoEdit()
        {
            int n = overlayCanvas.Children.Count;
            if (n == 0) return;
            UIElement el = overlayCanvas.Children[n - 1];
            overlayCanvas.Children.RemoveAt(n - 1);
            redoStack.Add(el);
        }

        private void RedoEdit()
        {
            int n = redoStack.Count;
            if (n == 0) return;
            UIElement el = redoStack[n - 1];
            redoStack.RemoveAt(n - 1);
            overlayCanvas.Children.Add(el);
        }

        private void CancelEditing()
        {
            overlayCanvas.Children.Clear();
            pageEdits.Clear();
            redoStack.Clear();
            SetMode("Reader");
        }

        private bool SaveCurrent()
        {
            if (string.IsNullOrEmpty(docPath) || !File.Exists(docPath)) return SaveAsDialog();
            return SaveTo(docPath);
        }

        private bool SaveAsDialog()
        {
            if (string.IsNullOrEmpty(docPath) || !File.Exists(docPath)) return false;
            var sfd = new System.Windows.Forms.SaveFileDialog {
                Filter = "PDF Documents (*.pdf)|*.pdf",
                Title = "Save PDF As",
                FileName = System.IO.Path.GetFileName(docPath)
            };
            if (sfd.ShowDialog() != System.Windows.Forms.DialogResult.OK) return false;
            return SaveTo(sfd.FileName);
        }

        private bool SaveTo(string path)
        {
            try
            {
                string src = System.IO.Path.GetFullPath(docPath);
                string dst = System.IO.Path.GetFullPath(path);
                if (!HasAnyEdits())
                {
                    if (!string.Equals(src, dst, StringComparison.OrdinalIgnoreCase)) File.Copy(src, dst, true);
                }
                else
                {
                    byte[] pdf = BuildFlattenedPdf();
                    if (pdf == null) { MessageBox.Show("Save failed: cannot render pages.", "GIN-PDF-RW", MessageBoxButton.OK, MessageBoxImage.Error); return false; }
                    string tmp = dst + ".tmp";
                    File.WriteAllBytes(tmp, pdf);
                    if (File.Exists(dst)) File.Delete(dst);
                    File.Move(tmp, dst);
                }
                statusText.Text = T("Saved", dst);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        // Renders every page (edits baked into the current page) and writes a PDF made of JPEG page images.
        private byte[] BuildFlattenedPdf()
        {
            ImageBox.DeselectAll();
            Keyboard.ClearFocus();
            int savedPage = currentPage;
            StashEdits(savedPage);
            try { return BuildFlattenedPdfCore(); }
            finally { RestoreEdits(savedPage); ApplyZoomToPage(); }
        }

        private byte[] BuildFlattenedPdfCore()
        {
            var jpgs = new List<byte[]>();
            var wh = new List<int[]>();
            for (int p = 0; p < totalPages; p++)
            {
                byte[] probe = NativePdfRenderer.RenderPage(docPath, p, 0, 0);
                if (probe == null) return null;
                int pw, ph;
                using (var pms = new MemoryStream(probe)) using (var pb = new System.Drawing.Bitmap(pms)) { pw = pb.Width; ph = pb.Height; }
                double k = 2000.0 / pw;
                uint tw = (uint)Math.Round(pw * k), th = (uint)Math.Round(ph * k);
                byte[] png = NativePdfRenderer.RenderPage(docPath, p, tw, th);
                if (png == null) return null;
                using (var ms = new MemoryStream(png))
                using (var bmp = new System.Drawing.Bitmap(ms))
                using (var canvasBmp = new System.Drawing.Bitmap(bmp.Width, bmp.Height))
                {
                    using (var g = System.Drawing.Graphics.FromImage(canvasBmp))
                    {
                        g.Clear(System.Drawing.Color.White);
                        g.DrawImage(bmp, 0, 0, bmp.Width, bmp.Height);
                        RestoreEdits(p + 1);
                        if (overlayCanvas.Children.Count > 0)
                        {
                            overlayCanvas.Width = pw; overlayCanvas.Height = ph;
                            overlayHost.UpdateLayout();
                            var dv = new DrawingVisual();
                            using (var dc = dv.RenderOpen())
                            {
                                dc.DrawRectangle(new VisualBrush(overlayCanvas), null, new System.Windows.Rect(0, 0, bmp.Width, bmp.Height));
                            }
                            var rtb = new RenderTargetBitmap(bmp.Width, bmp.Height, 96, 96, PixelFormats.Pbgra32);
                            rtb.Render(dv);
                            var enc = new PngBitmapEncoder();
                            enc.Frames.Add(BitmapFrame.Create(rtb));
                            using (var oms = new MemoryStream())
                            {
                                enc.Save(oms);
                                oms.Position = 0;
                                using (var ob = new System.Drawing.Bitmap(oms)) g.DrawImage(ob, 0, 0, bmp.Width, bmp.Height);
                            }
                        }
                        StashEdits(p + 1);
                    }
                    var codec = System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders();
                    System.Drawing.Imaging.ImageCodecInfo jpg = null;
                    foreach (var c in codec) if (c.MimeType == "image/jpeg") jpg = c;
                    var ep = new System.Drawing.Imaging.EncoderParameters(1);
                    ep.Param[0] = new System.Drawing.Imaging.EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 92L);
                    using (var jms = new MemoryStream())
                    {
                        canvasBmp.Save(jms, jpg, ep);
                        jpgs.Add(jms.ToArray());
                    }
                    wh.Add(new int[] { canvasBmp.Width, canvasBmp.Height });
                }
            }

            var outMs = new MemoryStream();
            var offsets = new List<long>();
            Action<string> w = delegate(string t) { byte[] b = Encoding.ASCII.GetBytes(t); outMs.Write(b, 0, b.Length); };
            Action<int> beginObj = delegate(int id) { offsets.Add(outMs.Position); w(id + " 0 obj\n"); };
            w("%PDF-1.4\n");
            int n = jpgs.Count;
            beginObj(1); w("<< /Type /Catalog /Pages 2 0 R >>\nendobj\n");
            var kids = new StringBuilder();
            for (int i = 0; i < n; i++) kids.Append((3 + i * 3) + " 0 R ");
            beginObj(2); w("<< /Type /Pages /Count " + n + " /Kids [" + kids + "] >>\nendobj\n");
            for (int i = 0; i < n; i++)
            {
                int pageId = 3 + i * 3, contId = pageId + 1, imgId = pageId + 2;
                double wPt = 595.28, hPt = 595.28 * wh[i][1] / wh[i][0];
                string wS = wPt.ToString("0.##", CultureInfo.InvariantCulture), hS = hPt.ToString("0.##", CultureInfo.InvariantCulture);
                beginObj(pageId);
                w("<< /Type /Page /Parent 2 0 R /MediaBox [0 0 " + wS + " " + hS + "] /Resources << /XObject << /Im0 " + imgId + " 0 R >> >> /Contents " + contId + " 0 R >>\nendobj\n");
                string cs = "q " + wS + " 0 0 " + hS + " 0 0 cm /Im0 Do Q";
                beginObj(contId);
                w("<< /Length " + cs.Length + " >>\nstream\n" + cs + "\nendstream\nendobj\n");
                beginObj(imgId);
                w("<< /Type /XObject /Subtype /Image /Width " + wh[i][0] + " /Height " + wh[i][1] + " /ColorSpace /DeviceRGB /BitsPerComponent 8 /Filter /DCTDecode /Length " + jpgs[i].Length + " >>\nstream\n");
                outMs.Write(jpgs[i], 0, jpgs[i].Length);
                w("\nendstream\nendobj\n");
            }
            long xref = outMs.Position;
            w("xref\n0 " + (offsets.Count + 1) + "\n0000000000 65535 f \n");
            foreach (long o in offsets) w(o.ToString("0000000000") + " 00000 n \n");
            w("trailer\n<< /Size " + (offsets.Count + 1) + " /Root 1 0 R >>\nstartxref\n" + xref + "\n%%EOF\n");
            return outMs.ToArray();
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
            if (string.IsNullOrEmpty(docPath)) return;
            var ofd = new System.Windows.Forms.OpenFileDialog {
                Filter = "Images (*.png;*.jpg;*.jpeg;*.bmp;*.gif)|*.png;*.jpg;*.jpeg;*.bmp;*.gif",
                Title = "Insert Stamp or Image"
            };
            if (ofd.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
            try
            {
                var bi = new BitmapImage();
                bi.BeginInit();
                bi.UriSource = new Uri(ofd.FileName);
                bi.CacheOption = BitmapCacheOption.OnLoad;
                bi.EndInit();
                bi.Freeze();
                double w = Math.Min(bi.Width, baseDocWidth * 0.4);
                var box = new ImageBox(bi, w);
                double scale = baseDocWidth > 0 && pageBorder.Width > 0 ? pageBorder.Width / baseDocWidth : 1;
                double x = Math.Max(0, docScroll.HorizontalOffset / scale) + 40;
                double y = Math.Max(0, docScroll.VerticalOffset / scale) + 40;
                Canvas.SetLeft(box, x);
                Canvas.SetTop(box, y);
                overlayCanvas.Children.Add(box);
                redoStack.Clear();
                box.UpdateHandle(zoom);
                box.Select();
                statusText.Text = "Image placed: drag to move, drag the corner handle to resize (aspect kept), Delete to remove.";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Image Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DrawSignDialog()
        {
            MessageBox.Show("Signature canvas ready! Draw your signature on document.", "GIN-PDF-RW", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void PrevPage()
        {
            if (currentPage > 1) {
                StashEdits(currentPage);
                currentPage--;
                RestoreEdits(currentPage);
                txtPage.Text = T("PageOf", currentPage, totalPages);
                RefreshView();
            }
        }

        private void NextPage()
        {
            if (currentPage < totalPages) {
                StashEdits(currentPage);
                currentPage++;
                RestoreEdits(currentPage);
                txtPage.Text = T("PageOf", currentPage, totalPages);
                RefreshView();
            }
        }
    }

    public class ImageBox : Grid
    {
        public static ImageBox Current;
        private readonly double aspect;
        private readonly System.Windows.Controls.Image img;
        private readonly Border frame;
        private readonly System.Windows.Controls.Primitives.Thumb handle;
        private bool dragging;
        private System.Windows.Point dragStart;
        private double startLeft, startTop;

        public ImageBox(BitmapSource src, double width)
        {
            aspect = src.PixelWidth / (double)src.PixelHeight;
            Width = width;
            Height = width / aspect;
            img = new System.Windows.Controls.Image { Source = src, Stretch = Stretch.Fill, Cursor = Cursors.SizeAll };
            RenderOptions.SetBitmapScalingMode(img, BitmapScalingMode.HighQuality);
            frame = new Border { BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 120, 215)), BorderThickness = new Thickness(1), IsHitTestVisible = false, Visibility = Visibility.Collapsed };
            handle = new System.Windows.Controls.Primitives.Thumb {
                Width = 14, Height = 14,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom,
                Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 120, 215)),
                Cursor = Cursors.SizeNWSE,
                Visibility = Visibility.Collapsed
            };
            Children.Add(img);
            Children.Add(frame);
            Children.Add(handle);

            handle.DragDelta += delegate(object s, System.Windows.Controls.Primitives.DragDeltaEventArgs e) {
                double nw = Math.Max(16, Width + e.HorizontalChange);
                Width = nw;
                Height = nw / aspect;
                e.Handled = true;
            };
            img.MouseLeftButtonDown += delegate(object s, MouseButtonEventArgs e) {
                Select();
                var parent = Parent as UIElement;
                if (parent == null) return;
                dragging = true;
                dragStart = e.GetPosition(parent);
                startLeft = double.IsNaN(Canvas.GetLeft(this)) ? 0 : Canvas.GetLeft(this);
                startTop = double.IsNaN(Canvas.GetTop(this)) ? 0 : Canvas.GetTop(this);
                img.CaptureMouse();
                e.Handled = true;
            };
            img.MouseMove += delegate(object s, MouseEventArgs e) {
                if (!dragging) return;
                var parent = Parent as UIElement;
                if (parent == null) return;
                var p = e.GetPosition(parent);
                Canvas.SetLeft(this, startLeft + p.X - dragStart.X);
                Canvas.SetTop(this, startTop + p.Y - dragStart.Y);
                e.Handled = true;
            };
            img.MouseLeftButtonUp += delegate(object s, MouseButtonEventArgs e) {
                if (dragging) { dragging = false; img.ReleaseMouseCapture(); e.Handled = true; }
            };
        }

        public void UpdateHandle(double zoom)
        {
            double z = zoom <= 0 ? 1 : zoom;
            handle.Width = 14 / z;
            handle.Height = 14 / z;
            handle.Margin = new Thickness(0, 0, -7 / z, -7 / z);
            frame.BorderThickness = new Thickness(1 / z);
        }

        public void Select()
        {
            if (Current != null && Current != this) Current.SetSelected(false);
            Current = this;
            SetSelected(true);
        }

        private void SetSelected(bool on)
        {
            frame.Visibility = on ? Visibility.Visible : Visibility.Collapsed;
            handle.Visibility = on ? Visibility.Visible : Visibility.Collapsed;
        }

        public static void DeselectAll()
        {
            if (Current != null) { Current.SetSelected(false); Current = null; }
        }

        public static bool IsInside(DependencyObject d)
        {
            while (d != null)
            {
                if (d is ImageBox) return true;
                d = (d is Visual || d is System.Windows.Media.Media3D.Visual3D) ? VisualTreeHelper.GetParent(d) : LogicalTreeHelper.GetParent(d);
            }
            return false;
        }
    }
}
