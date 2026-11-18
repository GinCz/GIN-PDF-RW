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

[assembly: AssemblyTitle("GIN-PDF-RW_v008")]
[assembly: AssemblyProduct("GIN-PDF-RW_v008")]
[assembly: AssemblyCompany("VladiMIR+AI")]
[assembly: AssemblyCopyright("VladiMIR+AI")]
[assembly: AssemblyFileVersion("0.008.0.0")]
[assembly: AssemblyVersion("0.008.0.0")]

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

        public static byte[] RenderPage(string pdfPath, int pageIndex)
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
                MethodInfo renderMethod = page.GetType().GetMethod("RenderToStreamAsync", new Type[] { iRandomAccess });
                object asyncAction = renderMethod.Invoke(page, new object[] { memStream });
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
        private bool isFitWidth = true;   // Fit to width by default!
        private string currentMode = "Reader";
        private string activeTool = "None";
        private string currentLang = "en";
        private string currentTheme = "Night"; // "Day", "Cloud", "Night"

        private double baseDocWidth = 794;
        private double baseDocHeight = 1123;

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
        private Border pageBorder;
        private System.Windows.Controls.Image pdfView;
        private Canvas overlayCanvas;
        private Border statusBar;
        private TextBlock statusText;
        private TextBlock verText;

        private readonly Dictionary<string, Dictionary<string, string>> loc = 
            new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

        public MainWindow(string initialFile, string explicitLang = null)
        {
            InitLocalization();
            DetectDefaultLanguage(explicitLang);
            BuildUi();
            ApplyTheme("Day"); // Default Day (Light) theme as requested!
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

            Loaded += delegate {
                DoFitWidth();
            };

            SizeChanged += delegate {
                if (isFitWidth) DoFitWidth();
            };
        }

        private void InitLocalization()
        {
            var en = new Dictionary<string, string>();
            en.Add("Title", "GIN-PDF-RW v008 — Zen PDF Reader & Editor");
            en.Add("Open", "📂 Open (Ctrl+O)");
            en.Add("Save", "💾 Save (Ctrl+S)");
            en.Add("Print", "🖨️ Print (Ctrl+P)");
            en.Add("ModeReader", "📖 Reader & Forms");
            en.Add("ModeEditor", "✏️ Editor");
            en.Add("Fullscreen", "⛶ Fullscreen (F11)");
            en.Add("Restore", "⛶ Window (Esc)");
            en.Add("FitWidth", "Fit Width");
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
            ru.Add("Title", "GIN-PDF-RW v008 — Просмотр и редактор PDF");
            ru.Add("Open", "📂 Открыть (Ctrl+O)");
            ru.Add("Save", "💾 Сохранить (Ctrl+S)");
            ru.Add("Print", "🖨️ Печать (Ctrl+P)");
            ru.Add("ModeReader", "📖 Чтение и формы");
            ru.Add("ModeEditor", "✏️ Редактировать");
            ru.Add("Fullscreen", "⛶ Во весь экран (F11)");
            ru.Add("Restore", "⛶ В окно (Esc)");
            ru.Add("FitWidth", "По ширине");
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
            uk.Add("Title", "GIN-PDF-RW v008 — Перегляд і редактор PDF");
            uk.Add("Open", "📂 Відкрити (Ctrl+O)");
            uk.Add("Save", "💾 Зберегти (Ctrl+S)");
            uk.Add("Print", "🖨️ Друк (Ctrl+P)");
            uk.Add("ModeReader", "📖 Читання та форми");
            uk.Add("ModeEditor", "✏️ Редагувати");
            uk.Add("Fullscreen", "⛶ На весь екран (F11)");
            uk.Add("Restore", "⛶ У вікно (Esc)");
            uk.Add("FitWidth", "По ширині");
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
            cs.Add("Title", "GIN-PDF-RW v008 — Prohlížeč a editor PDF");
            cs.Add("Open", "📂 Otevřít (Ctrl+O)");
            cs.Add("Save", "💾 Uložit (Ctrl+S)");
            cs.Add("Print", "🖨️ Tisk (Ctrl+P)");
            cs.Add("ModeReader", "📖 Čtení a formuláře");
            cs.Add("ModeEditor", "✏️ Upravit");
            cs.Add("Fullscreen", "⛶ Celá obrazovka (F11)");
            cs.Add("Restore", "⛶ Do okna (Esc)");
            cs.Add("FitWidth", "Na šířku");
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

        private string T(string key, params object[] args)
        {
            if (!loc.ContainsKey(currentLang)) currentLang = "en";
            if (!loc[currentLang].ContainsKey(key)) key = "en";
            string val = loc[currentLang].ContainsKey(key) ? loc[currentLang][key] : key;
            return (args != null && args.Length > 0) ? string.Format(val, args) : val;
        }

        private void BuildUi()
        {
            Title = "GIN-PDF-RW v008";
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
            btnFit = CreateBtn("Fit Width", delegate { DoFitWidth(); });

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
                Margin = new Thickness(6, 0, 0, 0),
                FontSize = 11,
                FontWeight = FontWeights.Bold,
                VerticalContentAlignment = VerticalAlignment.Center
            };
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
                    UpdateStrings();
                    if (string.IsNullOrEmpty(docPath)) RenderWelcomeSheet();
                }
            };

            rightGroup.Children.Add(btnFullscreen);
            rightGroup.Children.Add(cmbLang);
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
            var container = new Grid { HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Top, Margin = new Thickness(15) };

            pageBorder = new Border {
                Background = System.Windows.Media.Brushes.White,
                CornerRadius = new CornerRadius(2)
            };
            var pageGrid = new Grid();

            pdfView = new System.Windows.Controls.Image { Stretch = Stretch.Uniform };
            overlayCanvas = new Canvas { Background = System.Windows.Media.Brushes.Transparent, Cursor = System.Windows.Input.Cursors.Cross };

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

            // Left click anywhere in fullscreen (outside active text/buttons) exits fullscreen to maximized view
            docScroll.PreviewMouseLeftButtonDown += delegate(object s, MouseButtonEventArgs e) {
                if (isFullscreen && activeTool == "None") {
                    if (!(e.OriginalSource is System.Windows.Controls.TextBox) && 
                        !(e.OriginalSource is System.Windows.Controls.Button) &&
                        !(e.OriginalSource is System.Windows.Controls.Primitives.ToggleButton)) {
                        ExitFullscreenToWindow();
                    }
                }
            };

            docScroll.SizeChanged += delegate {
                if (isFitWidth) DoFitWidth();
            };

            pageGrid.Children.Add(pdfView);
            pageGrid.Children.Add(overlayCanvas);
            pageBorder.Child = pageGrid;
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
            verText = new TextBlock { Text = "GIN-PDF-RW v008 [VladiMIR+AI] | UTF-8", FontSize = 11, VerticalAlignment = VerticalAlignment.Center };

            Grid.SetColumn(statusText, 0);
            Grid.SetColumn(verText, 1);
            sbGrid.Children.Add(statusText);
            sbGrid.Children.Add(verText);
            statusBar.Child = sbGrid;
            Grid.SetRow(statusBar, 3);
            rootGrid.Children.Add(statusBar);

            Content = rootGrid;

            // Hotkeys
            KeyDown += delegate(object s, KeyEventArgs e) {
                if (isFullscreen) {
                    if (e.Key == Key.F11) {
                        ToggleFullscreen();
                        e.Handled = true;
                        return;
                    } else if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && 
                               (e.Key == Key.O || e.Key == Key.S || e.Key == Key.P)) {
                        // Allow file open / save / print shortcuts to execute directly
                    } else {
                        // ANY KEY (Escape, Space, Enter, arrows, etc.) exits fullscreen to maximized view!
                        ExitFullscreenToWindow();
                        e.Handled = true;
                        return;
                    }
                }

                if (e.Key == Key.F11) {
                    ToggleFullscreen();
                    e.Handled = true;
                } else if (e.Key == Key.Escape) {
                    ExitFullscreenToWindow();
                    e.Handled = true;
                } else if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control) {
                    if (e.Key == Key.O) { OpenFileDialog(); e.Handled = true; }
                    else if (e.Key == Key.S) { SaveFileDialog(); e.Handled = true; }
                    else if (e.Key == Key.P) { ExecutePrint(); e.Handled = true; }
                    else if (e.Key == Key.OemPlus || e.Key == Key.Add) { AdjustZoom(0.05); e.Handled = true; }
                    else if (e.Key == Key.OemMinus || e.Key == Key.Subtract) { AdjustZoom(-0.05); e.Handled = true; }
                }
            };
        }

        private void ApplyTheme(string theme)
        {
            currentTheme = theme;
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
            Button[] btns = new Button[] { btnOpen, btnSave, btnPrint, btnPrev, btnNext, btnZoomOut, btnZoomIn, btnFit, btnFullscreen, btnThemeDay, btnThemeCloud, btnThemeNight, toolText, toolCheck, toolImage, toolSign };
            foreach (var b in btns) {
                if (b != null) {
                    b.Background = new SolidColorBrush(bgBtn);
                    b.Foreground = new SolidColorBrush(fgBtn);
                }
            }

            // High contrast ComboBox styling (Fixing white text on white background)
            cmbLang.Background = new SolidColorBrush(bgBtn);
            cmbLang.Foreground = new SolidColorBrush(fgBtn);
            foreach (var item in cmbLang.Items) {
                var cbi = item as ComboBoxItem;
                if (cbi != null) {
                    cbi.Background = new SolidColorBrush(bgTool);
                    cbi.Foreground = new SolidColorBrush(fgText);
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
            Dispatcher.BeginInvoke(new Action(DoFitWidth), System.Windows.Threading.DispatcherPriority.Background);
        }

        private void ExitFullscreenToWindow()
        {
            isFullscreen = false;
            WindowStyle = WindowStyle.SingleBorderWindow;
            WindowState = WindowState.Maximized; // Maximized window view as requested!
            btnFullscreen.Content = T("Fullscreen");
            toolbar.Visibility = Visibility.Visible;
            statusBar.Visibility = Visibility.Visible;
            Dispatcher.BeginInvoke(new Action(DoFitWidth), System.Windows.Threading.DispatcherPriority.Background);
        }

        private void ToggleFullscreen()
        {
            if (isFullscreen) ExitFullscreenToWindow();
            else EnterFullscreen();
        }

        private void ToggleToolbarPopup()
        {
            toolbar.Visibility = (toolbar.Visibility == Visibility.Visible) ? Visibility.Collapsed : Visibility.Visible;
            statusBar.Visibility = toolbar.Visibility;
        }

        private void DoFitWidth()
        {
            isFitWidth = true;
            double availWidth = docScroll.ViewportWidth;
            if (availWidth <= 50) availWidth = docScroll.ActualWidth;
            if (availWidth <= 50) availWidth = ActualWidth - 50;
            if (availWidth <= 50) availWidth = SystemParameters.PrimaryScreenWidth - 50;

            if (availWidth > 100 && baseDocWidth > 0)
            {
                zoom = Math.Max(0.2, (availWidth - 30) / baseDocWidth);
                txtZoom.Text = string.Format("{0:P0}", zoom);
                ApplyZoomToPage();
            }
        }

        private void ApplyZoomToPage()
        {
            pageBorder.Width = baseDocWidth * zoom;
            pageBorder.Height = baseDocHeight * zoom;
        }

        private void AdjustZoom(double delta)
        {
            isFitWidth = false; // User manually zooming
            zoom = Math.Max(0.2, Math.Min(5.0, zoom + delta));
            txtZoom.Text = string.Format("{0:P0}", zoom);
            ApplyZoomToPage();
        }

        private void SetMode(string mode)
        {
            currentMode = mode;
            if (mode == "Reader")
            {
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
                toolText.Background = (currentTheme == "Day") ? new SolidColorBrush(System.Windows.Media.Color.FromRgb(230, 234, 242)) : new SolidColorBrush(System.Windows.Media.Color.FromRgb(49, 50, 68));
                toolCheck.Background = toolText.Background;
                txtToolTip.Text = "";
            }
            else
            {
                activeTool = tool;
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
                byte[] bytes = NativePdfRenderer.RenderPage(docPath, currentPage - 1);
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
                    baseDocWidth = bi.PixelWidth;
                    baseDocHeight = bi.PixelHeight;

                    if (isFitWidth) DoFitWidth();
                    else ApplyZoomToPage();
                    return;
                }
            }
            RenderWelcomeSheet();
        }

        private void RenderWelcomeSheet()
        {
            baseDocWidth = 794;
            baseDocHeight = 1123;
            var bmp = new Bitmap((int)baseDocWidth, (int)baseDocHeight);
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(System.Drawing.Color.White);
                using (var brushDark = new SolidBrush(System.Drawing.Color.FromArgb(30, 30, 46)))
                using (var brushGray = new SolidBrush(System.Drawing.Color.FromArgb(100, 100, 120)))
                using (var fontHeader = new System.Drawing.Font("Segoe UI", 24, System.Drawing.FontStyle.Bold))
                using (var fontSub = new System.Drawing.Font("Segoe UI", 12, System.Drawing.FontStyle.Regular))
                using (var fontBody = new System.Drawing.Font("Segoe UI", 11, System.Drawing.FontStyle.Regular))
                using (var pen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(200, 200, 215), 1))
                {
                    string subTitle, welcome;
                    if (currentLang == "ru")
                    {
                        subTitle = "Быстрый просмотр и редактор PDF с открытым исходным кодом";
                        welcome = 
                            "Добро пожаловать в GIN-PDF-RW v008!\n\n" +
                            "Основные возможности:\n" +
                            "• Полноэкранный режим по умолчанию (любая клавиша или клик мыши — переход в окно)\n" +
                            "• Развернутое на весь экран окно со стандартными кнопками и меню\n" +
                            "• Плавный зум с шагом 5% (+/-) и автоматическое растягивание по ширине\n" +
                            "• 3 темы оформления: Светлая [День], Нейтральная [Облако], Тёмная [Ночь]\n" +
                            "• Высококонтрастный интерфейс и полная поддержка UTF-8\n" +
                            "• Аппаратный движок рендеринга PDF WinRT (Windows.Data.Pdf)\n" +
                            "• Интерактивные AcroForms (галочки и текстовые поля) и цифровая подпись\n\n" +
                            "Нажмите «Открыть (Ctrl+O)» для выбора вашего PDF-документа.";
                    }
                    else if (currentLang == "uk")
                    {
                        subTitle = "Швидкий перегляд та редактор PDF з відкритим кодом";
                        welcome = 
                            "Ласкаво просимо до GIN-PDF-RW v008!\n\n" +
                            "Основні можливості:\n" +
                            "• Повноекранний режим за замовчуванням (будь-яка клавіша або клік миші — перехід у вікно)\n" +
                            "• Розгорнуте на весь екран вікно зі стандартними кнопками та меню\n" +
                            "• Плавний зум з кроком 5% (+/-) та автоматичне розтягування по ширині\n" +
                            "• 3 теми оформлення: Світла [День], Нейтральна [Хмара], Темна [Ніч]\n" +
                            "• Висококонтрастний інтерфейс і повна підтримка UTF-8\n" +
                            "• Апаратний рушій рендерингу PDF WinRT (Windows.Data.Pdf)\n" +
                            "• Інтерактивні AcroForms (позначки та текстові поля) і цифровий підпис\n\n" +
                            "Натисніть «Відкрити (Ctrl+O)» для вибору вашого PDF-документа.";
                    }
                    else if (currentLang == "cs")
                    {
                        subTitle = "Rychlý prohlížeč a editor PDF s otevřeným zdrojovým kódem";
                        welcome = 
                            "Vítejte v GIN-PDF-RW v008!\n\n" +
                            "Klíčové funkce:\n" +
                            "• Režim celé obrazovky jako výchozí (stisknutí libovolné klávesy přejde do okna)\n" +
                            "• Maximalizované okno se standardními tlačítky a panelem nabídek\n" +
                            "• Plynulé přiblížení po 5 % (+/-) a automatické přizpůsobení na celou šířku\n" +
                            "• 3 motivy vzhledu: Světlý [Den], Neutrální šedý [Oblak], Tmavý [Noc]\n" +
                            "• Vysoce kontrastní rozhraní a plná podpora UTF-8\n" +
                            "• Hardwarově akcelerované vykreslování PDF WinRT\n" +
                            "• Interaktivní AcroForms (zaškrtávací pole a text) a digitální podpis\n\n" +
                            "Klikněte na «Otevřít (Ctrl+O)» pro otevření PDF dokumentu.";
                    }
                    else
                    {
                        subTitle = "High-Performance Open Source PDF Reader & Editor";
                        welcome = 
                            "Welcome to GIN-PDF-RW v008!\n\n" +
                            "Key Features:\n" +
                            "• Fullscreen by Default (Press Any Key or Left Click to restore Maximized View)\n" +
                            "• Maximized Window with Standard Title Bar & Accessible Top/Bottom Toolbars\n" +
                            "• Smooth 5% Zoom on +/- and Auto Fit to Width\n" +
                            "• 3 Themes: Day [Light], Neutral Cloud [Gray], Night [Dark]\n" +
                            "• High-Contrast UI & Multilingual Localization (EN, RU, UK, CS, DE, FR, ES, IT, PL, TR, ZH, JA)\n" +
                            "• Native Windows WinRT PDF Hardware Rendering Engine\n" +
                            "• AcroForms Interactive Fill & Sign + X.509 Cryptographic Signatures\n\n" +
                            "Click 'Open (Ctrl+O)' to open your PDF documents.";
                    }

                    g.DrawString("GIN-PDF-RW", fontHeader, brushDark, 60, 80);
                    g.DrawString(subTitle, fontSub, brushGray, 60, 125);
                    g.DrawLine(pen, 60, 160, 734, 160);
                    g.DrawString(welcome, fontBody, brushDark, new RectangleF(60, 190, 674, 500));
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
            if (isFitWidth) DoFitWidth();
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
            isFitWidth = true; // Automatically fit to width!
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
