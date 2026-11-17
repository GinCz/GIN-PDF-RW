using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Principal;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;

[assembly: AssemblyTitle("GIN-PDF-RW Uninstall")]
[assembly: AssemblyProduct("GIN-PDF-RW_v005_Uninstall")]
[assembly: AssemblyCompany("VladiMIR+AI")]
[assembly: AssemblyCopyright("VladiMIR+AI")]
[assembly: AssemblyFileVersion("0.005.0.0")]
[assembly: AssemblyVersion("0.005.0.0")]

namespace GinPdfRwUninstall
{
    public class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            var app = new Application();
            var window = new UninstallWindow();
            app.Run(window);
        }
    }

    public class UninstallWindow : Window
    {
        public UninstallWindow()
        {
            Title = "GIN-PDF-RW Uninstall";
            Width = 480;
            Height = 240;
            MinWidth = 480;
            MinHeight = 240;
            ResizeMode = ResizeMode.NoResize;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Background = new SolidColorBrush(Color.FromRgb(24, 24, 37));
            Foreground = new SolidColorBrush(Color.FromRgb(205, 214, 244));
            FontFamily = new FontFamily("Segoe UI, sans-serif");

            var mainGrid = new Grid { Margin = new Thickness(24) };
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var title = new TextBlock {
                Text = "Uninstall GIN-PDF-RW v005?",
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(243, 139, 168))
            };
            Grid.SetRow(title, 0);
            mainGrid.Children.Add(title);

            var desc = new TextBlock {
                Text = "Are you sure you want to completely remove GIN-PDF-RW and all its components from this computer?",
                TextWrapping = TextWrapping.Wrap,
                FontSize = 13,
                Foreground = new SolidColorBrush(Color.FromRgb(186, 194, 222)),
                Margin = new Thickness(0, 10, 0, 0)
            };
            Grid.SetRow(desc, 1);
            mainGrid.Children.Add(desc);

            var btnStack = new StackPanel {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right
            };

            var btnCancel = new Button {
                Content = "Cancel",
                Width = 100,
                Height = 34,
                Background = new SolidColorBrush(Color.FromRgb(49, 50, 68)),
                Foreground = new SolidColorBrush(Color.FromRgb(205, 214, 244)),
                FontWeight = FontWeights.SemiBold,
                BorderThickness = new Thickness(0),
                Cursor = System.Windows.Input.Cursors.Hand,
                Margin = new Thickness(0, 0, 10, 0)
            };
            btnCancel.Click += delegate { Close(); };

            var btnUninstall = new Button {
                Content = "Uninstall",
                Width = 120,
                Height = 34,
                Background = new SolidColorBrush(Color.FromRgb(243, 139, 168)),
                Foreground = new SolidColorBrush(Color.FromRgb(17, 17, 27)),
                FontWeight = FontWeights.Bold,
                BorderThickness = new Thickness(0),
                Cursor = System.Windows.Input.Cursors.Hand
            };
            btnUninstall.Click += delegate { DoUninstall(); };

            btnStack.Children.Add(btnCancel);
            btnStack.Children.Add(btnUninstall);
            Grid.SetRow(btnStack, 2);
            mainGrid.Children.Add(btnStack);

            Content = mainGrid;
        }

        private static bool IsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private void DoUninstall()
        {
            if (!IsAdministrator())
            {
                try
                {
                    var psi = new ProcessStartInfo {
                        FileName = Process.GetCurrentProcess().MainModule.FileName,
                        Verb = "runas",
                        UseShellExecute = true
                    };
                    Process.Start(psi);
                    Close();
                    return;
                }
                catch
                {
                    MessageBox.Show("Administrator privileges are required to uninstall.", "Permission Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            try
            {
                // 1. Remove Shortcuts
                string[] desktopDirs = new string[] {
                    @"D:\MEGA\DOCS\desktop",
                    Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory)
                };

                foreach (var d in desktopDirs)
                {
                    if (Directory.Exists(d))
                    {
                        string lnk = Path.Combine(d, "GIN-PDF-RW.lnk");
                        if (File.Exists(lnk)) {
                            try { File.Delete(lnk); } catch {}
                        }
                    }
                }

                // 2. Remove Registry entries
                try { Registry.CurrentUser.DeleteSubKeyTree(@"Software\Classes\GIN-PDF-RW.Document", false); } catch {}
                try { Registry.CurrentUser.DeleteSubKeyTree(@"Software\Microsoft\Windows\CurrentVersion\Uninstall\GIN-PDF-RW", false); } catch {}

                // Reset .pdf if pointing to GIN-PDF-RW
                try
                {
                    using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Classes\.pdf", true))
                    {
                        if (key != null && (string)key.GetValue("") == "GIN-PDF-RW.Document")
                        {
                            key.SetValue("", "");
                        }
                    }
                }
                catch {}

                // 3. Self-deletion script for C:\Program Files\GIN-PDF-RW
                string targetDir = @"C:\Program Files\GIN-PDF-RW";

                string batchCmd = string.Format(
                    "/c ping 127.0.0.1 -n 2 > nul & rmdir /s /q \"{0}\"", targetDir);

                var psiCmd = new ProcessStartInfo {
                    FileName = "cmd.exe",
                    Arguments = batchCmd,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    UseShellExecute = true
                };

                MessageBox.Show("GIN-PDF-RW has been successfully uninstalled from your computer.", "GIN-PDF-RW", MessageBoxButton.OK, MessageBoxImage.Information);
                Process.Start(psiCmd);
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Uninstall error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
