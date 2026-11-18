using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Principal;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;

[assembly: AssemblyTitle("GIN-PDF-RW_v008_Setup")]
[assembly: AssemblyProduct("GIN-PDF-RW_v008_Setup")]
[assembly: AssemblyCompany("VladiMIR+AI")]
[assembly: AssemblyCopyright("VladiMIR+AI")]
[assembly: AssemblyFileVersion("0.008.0.0")]
[assembly: AssemblyVersion("0.008.0.0")]

namespace GinPdfRwSetup
{
    public class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            string lang = "en";
            bool auto = false;

            for (int i = 0; i < args.Length; i++)
            {
                if ((args[i] == "--lang" || args[i] == "-lang") && i + 1 < args.Length)
                {
                    lang = args[i + 1].ToLowerInvariant();
                    i++;
                }
                else if (args[i] == "--auto" || args[i] == "-auto")
                {
                    auto = true;
                }
            }

            var app = new Application();
            var window = new SetupWindow(lang, auto);
            app.Run(window);
        }
    }

    public class SetupWindow : Window
    {
        private RadioButton[] langRadios;
        private readonly string initialLang;
        private readonly bool autoInstall;

        private readonly string[] langNames = new string[] {
            "English (Default)",
            "Українська (Ukrainian)",
            "Русский (Russian)",
            "Čeština (Czech)",
            "Deutsch (German)",
            "Français (French)",
            "Español (Spanish)",
            "Italiano (Italian)",
            "Polski (Polish)",
            "Türkçe (Turkish)",
            "中文 (Chinese)",
            "日本語 (Japanese)"
        };
        private readonly string[] langCodes = new string[] {
            "en", "uk", "ru", "cs", "de", "fr", "es", "it", "pl", "tr", "zh", "ja"
        };

        public SetupWindow(string lang, bool auto)
        {
            this.initialLang = string.IsNullOrEmpty(lang) ? "en" : lang.ToLowerInvariant();
            this.autoInstall = auto;
            BuildUi();

            if (autoInstall)
            {
                Loaded += delegate { DoInstall(); };
            }
        }

        private void BuildUi()
        {
            Title = "GIN-PDF-RW v008 Setup";
            Width = 580;
            Height = 540;
            MinWidth = 580;
            MinHeight = 540;
            ResizeMode = ResizeMode.NoResize;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Background = new SolidColorBrush(Color.FromRgb(24, 24, 37));
            Foreground = new SolidColorBrush(Color.FromRgb(205, 214, 244));
            FontFamily = new FontFamily("Segoe UI, sans-serif");

            var mainGrid = new Grid { Margin = new Thickness(24) };
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // 1. Header in English
            var headerStack = new StackPanel { Margin = new Thickness(0, 0, 0, 16) };
            var txtTitle = new TextBlock {
                Text = "Install GIN-PDF-RW v008?",
                FontSize = 22,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(137, 180, 250))
            };
            var txtQuestion = new TextBlock {
                Text = "Do you want to install GIN-PDF-RW on this computer?",
                FontSize = 13,
                Foreground = new SolidColorBrush(Color.FromRgb(186, 194, 222)),
                Margin = new Thickness(0, 4, 0, 0)
            };
            headerStack.Children.Add(txtTitle);
            headerStack.Children.Add(txtQuestion);
            Grid.SetRow(headerStack, 0);
            mainGrid.Children.Add(headerStack);

            // Separator
            var sep = new Border {
                Height = 1,
                Background = new SolidColorBrush(Color.FromRgb(49, 50, 68)),
                Margin = new Thickness(0, 0, 0, 16)
            };
            Grid.SetRow(sep, 1);
            mainGrid.Children.Add(sep);

            // 2. Language Selection Grid
            var contentStack = new StackPanel();
            var lblSelect = new TextBlock {
                Text = "Select your preferred application language:",
                FontSize = 12,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(166, 173, 200)),
                Margin = new Thickness(0, 0, 0, 12)
            };
            contentStack.Children.Add(lblSelect);

            var radioGrid = new UniformGrid { Columns = 2, Margin = new Thickness(6, 0, 0, 16) };
            langRadios = new RadioButton[langNames.Length];

            for (int i = 0; i < langNames.Length; i++)
            {
                bool isTarget = string.Equals(langCodes[i], initialLang, StringComparison.OrdinalIgnoreCase);
                var rb = new RadioButton {
                    Content = langNames[i],
                    Tag = langCodes[i],
                    Foreground = new SolidColorBrush(Color.FromRgb(205, 214, 244)),
                    FontSize = 12,
                    Margin = new Thickness(0, 4, 0, 4),
                    Cursor = Cursors.Hand,
                    IsChecked = isTarget
                };
                langRadios[i] = rb;
                radioGrid.Children.Add(rb);
            }
            contentStack.Children.Add(radioGrid);

            // Target folder notice
            var txtPath = new TextBlock {
                Text = "Target folder: C:\\Program Files\\GIN-PDF-RW\nDefault reader for .PDF documents",
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(108, 112, 134)),
                Margin = new Thickness(0, 8, 0, 0)
            };
            contentStack.Children.Add(txtPath);

            Grid.SetRow(contentStack, 2);
            mainGrid.Children.Add(contentStack);

            // 3. Two Action Buttons: Cancel and Install
            var btnGrid = new StackPanel {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 16, 0, 0)
            };

            var btnCancel = new Button {
                Content = "Cancel",
                Width = 110,
                Height = 36,
                Background = new SolidColorBrush(Color.FromRgb(49, 50, 68)),
                Foreground = new SolidColorBrush(Color.FromRgb(205, 214, 244)),
                FontWeight = FontWeights.SemiBold,
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand,
                Margin = new Thickness(0, 0, 12, 0)
            };
            btnCancel.Click += delegate { Close(); };

            var btnInstall = new Button {
                Content = "Install",
                Width = 130,
                Height = 36,
                Background = new SolidColorBrush(Color.FromRgb(137, 180, 250)),
                Foreground = new SolidColorBrush(Color.FromRgb(17, 17, 27)),
                FontWeight = FontWeights.Bold,
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand
            };
            btnInstall.Click += delegate { DoInstall(); };

            btnGrid.Children.Add(btnCancel);
            btnGrid.Children.Add(btnInstall);
            Grid.SetRow(btnGrid, 3);
            mainGrid.Children.Add(btnGrid);

            Content = mainGrid;
        }

        private static bool IsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private void DoInstall()
        {
            string selectedLang = "en";
            foreach (var rb in langRadios)
            {
                if (rb.IsChecked == true && rb.Tag != null)
                {
                    selectedLang = rb.Tag.ToString();
                    break;
                }
            }

            if (!IsAdministrator())
            {
                try
                {
                    var psi = new ProcessStartInfo {
                        FileName = Process.GetCurrentProcess().MainModule.FileName,
                        Arguments = "--lang " + selectedLang + " --auto",
                        Verb = "runas",
                        UseShellExecute = true
                    };
                    Process.Start(psi);
                    Close();
                    return;
                }
                catch
                {
                    MessageBox.Show("Administrator privileges are required to install into Program Files.", "Permission Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            try
            {
                string targetDir = @"C:\Program Files\GIN-PDF-RW";
                if (!Directory.Exists(targetDir))
                {
                    Directory.CreateDirectory(targetDir);
                }

                string targetExe = Path.Combine(targetDir, "GIN-PDF-RW.exe");
                string targetUninstall = Path.Combine(targetDir, "Uninstall.exe");

                // 1. Extract embedded GIN-PDF-RW.exe
                ExtractResource("GIN-PDF-RW.exe", targetExe);

                // 2. Extract embedded Uninstall.exe
                ExtractResource("Uninstall.exe", targetUninstall);

                // 3. Extract embedded app.ico
                ExtractResource("app.ico", Path.Combine(targetDir, "app.ico"));

                // 4. Save chosen language to install dir, appdata, and registry
                File.WriteAllText(Path.Combine(targetDir, "language.cfg"), selectedLang);
                try {
                    string appData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "GIN-PDF-RW");
                    if (!Directory.Exists(appData)) Directory.CreateDirectory(appData);
                    File.WriteAllText(Path.Combine(appData, "language.cfg"), selectedLang);
                } catch {}
                try {
                    using (var k = Registry.CurrentUser.CreateSubKey(@"Software\GIN-PDF-RW")) {
                        if (k != null) k.SetValue("Language", selectedLang);
                    }
                } catch {}

                // 5. Create Desktop Shortcuts
                CreateShortcuts(targetExe, targetDir);

                // 6. Associate .PDF
                AssociatePdf(targetExe);

                // 7. Register Uninstall in Windows Programs & Features
                RegisterUninstall(targetUninstall, targetDir);

                string successMsg;
                if (selectedLang == "ru") successMsg = "GIN-PDF-RW v008 успешно установлен на ваш компьютер!\nПолноэкранный режим и режим по ширине активированы.";
                else if (selectedLang == "uk") successMsg = "GIN-PDF-RW v008 успішно встановлено на ваш комп'ютер!\nПовноекранний режим та режим по ширині активовано.";
                else if (selectedLang == "cs") successMsg = "GIN-PDF-RW v008 byl úspěšně nainstalován na váš počítač!\nRežim celé obrazovky a na šířku byl aktivován.";
                else successMsg = "GIN-PDF-RW v008 has been successfully installed!\nDefault fullscreen & Fit-Width enabled.";

                MessageBox.Show(successMsg, "GIN-PDF-RW Setup", MessageBoxButton.OK, MessageBoxImage.Information);

                // Launch installed app with language passed directly
                Process.Start(targetExe, "-lang " + selectedLang);
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Installation error: " + ex.Message, "Setup Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExtractResource(string resName, string outPath)
        {
            var asm = Assembly.GetExecutingAssembly();
            Stream stream = asm.GetManifestResourceStream(resName);
            if (stream == null)
            {
                foreach (string name in asm.GetManifestResourceNames())
                {
                    if (string.Equals(name, resName, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(name.Replace('_', '-'), resName.Replace('_', '-'), StringComparison.OrdinalIgnoreCase))
                    {
                        stream = asm.GetManifestResourceStream(name);
                        break;
                    }
                }
            }

            if (stream != null)
            {
                using (stream)
                using (FileStream fs = new FileStream(outPath, FileMode.Create, FileAccess.Write))
                {
                    byte[] buffer = new byte[81920];
                    int read;
                    while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        fs.Write(buffer, 0, read);
                    }
                }
            }
            else
            {
                string localPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, resName);
                if (File.Exists(localPath))
                {
                    File.Copy(localPath, outPath, true);
                }
                else
                {
                    throw new FileNotFoundException("Failed to locate embedded resource: " + resName);
                }
            }
        }

        private void CreateShortcuts(string targetExe, string workDir)
        {
            string[] desktopDirs = new string[] {
                @"D:\MEGA\DOCS\desktop",
                Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
                Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory)
            };

            foreach (var d in desktopDirs)
            {
                if (Directory.Exists(d))
                {
                    try
                    {
                        string linkPath = Path.Combine(d, "GIN-PDF-RW.lnk");
                        CreateShortcutFile(linkPath, targetExe, workDir, "GIN-PDF-RW v008 by VladiMIR+AI");
                    }
                    catch { }
                }
            }
        }

        private void CreateShortcutFile(string shortcutPath, string targetPath, string workDir, string description)
        {
            Type shellType = Type.GetTypeFromProgID("WScript.Shell");
            dynamic shell = Activator.CreateInstance(shellType);
            dynamic shortcut = shell.CreateShortcut(shortcutPath);
            shortcut.TargetPath = targetPath;
            shortcut.WorkingDirectory = workDir;
            shortcut.Description = description;
            shortcut.IconLocation = targetPath + ",0";
            shortcut.Save();
        }

        private void AssociatePdf(string exePath)
        {
            try
            {
                using (var key = Registry.CurrentUser.CreateSubKey(@"Software\Classes\.pdf"))
                {
                    if (key != null) key.SetValue("", "GIN-PDF-RW.Document");
                }
                using (var key = Registry.CurrentUser.CreateSubKey(@"Software\Classes\GIN-PDF-RW.Document\shell\open\command"))
                {
                    if (key != null) key.SetValue("", "\"" + exePath + "\" \"%1\"");
                }
            }
            catch { }
        }

        private void RegisterUninstall(string uninstallExe, string installDir)
        {
            try
            {
                using (var key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall\GIN-PDF-RW"))
                {
                    if (key != null)
                    {
                        key.SetValue("DisplayName", "GIN-PDF-RW v008 [VladiMIR+AI]");
                        key.SetValue("DisplayVersion", "v008");
                        key.SetValue("Publisher", "VladiMIR+AI");
                        key.SetValue("DisplayIcon", Path.Combine(installDir, "GIN-PDF-RW.exe") + ",0");
                        key.SetValue("UninstallString", "\"" + uninstallExe + "\"");
                        key.SetValue("InstallLocation", installDir);
                    }
                }
            }
            catch { }
        }
    }
}
