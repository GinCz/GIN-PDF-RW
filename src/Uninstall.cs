using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Security.Principal;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;

[assembly: AssemblyTitle("GIN-PDF-RW_v014_Uninstall")]
[assembly: AssemblyProduct("GIN-PDF-RW_v014_Uninstall")]
[assembly: AssemblyDescription("GIN-PDF-RW_v014_Uninstall")]
[assembly: AssemblyCompany("VladiMIR+AI")]
[assembly: AssemblyCopyright("VladiMIR+AI")]
[assembly: AssemblyFileVersion("0.014.0.0")]
[assembly: AssemblyVersion("0.014.0.0")]

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
        private string currentLang = "en";
        private readonly Dictionary<string, Dictionary<string, string>> loc = 
            new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

        public UninstallWindow()
        {
            InitLocalization();
            DetectLanguage();
            BuildUi();
        }

        private void InitLocalization()
        {
            var en = new Dictionary<string, string>();
            en["Title"] = "Uninstall GIN-PDF-RW v014";
            en["Header"] = "Uninstall GIN-PDF-RW v014?";
            en["Desc"] = "Are you sure you want to completely remove GIN-PDF-RW and all its components from this computer?";
            en["Cancel"] = "Cancel";
            en["Uninstall"] = "Uninstall";
            en["Success"] = "GIN-PDF-RW has been successfully uninstalled from your computer.";
            en["AdminReq"] = "Administrator privileges are required to uninstall.";
            en["Error"] = "Uninstall error: ";
            loc["en"] = en;

            var ru = new Dictionary<string, string>();
            ru["Title"] = "Удаление GIN-PDF-RW v014";
            ru["Header"] = "Удалить GIN-PDF-RW v014?";
            ru["Desc"] = "Вы уверены, что хотите полностью удалить GIN-PDF-RW и все его компоненты с этого компьютера?";
            ru["Cancel"] = "Отмена";
            ru["Uninstall"] = "Удалить";
            ru["Success"] = "GIN-PDF-RW был успешно удален с вашего компьютера.";
            ru["AdminReq"] = "Для удаления программы требуются права администратора.";
            ru["Error"] = "Ошибка при удалении: ";
            loc["ru"] = ru;

            var uk = new Dictionary<string, string>();
            uk["Title"] = "Видалення GIN-PDF-RW v014";
            uk["Header"] = "Видалити GIN-PDF-RW v014?";
            uk["Desc"] = "Ви впевнені, що хочете повністю видалити GIN-PDF-RW та всі його компоненти з цього комп'ютера?";
            uk["Cancel"] = "Скасувати";
            uk["Uninstall"] = "Видалити";
            uk["Success"] = "GIN-PDF-RW було успішно видалено з вашого комп'ютера.";
            uk["AdminReq"] = "Для видалення програми потрібні права адміністратора.";
            uk["Error"] = "Помилка при видаленні: ";
            loc["uk"] = uk;

            var cs = new Dictionary<string, string>();
            cs["Title"] = "Odinstalace GIN-PDF-RW v014";
            cs["Header"] = "Odinstalovat GIN-PDF-RW v014?";
            cs["Desc"] = "Opravdu chcete zcela odebrat GIN-PDF-RW a všechny jeho součásti z tohoto počítače?";
            cs["Cancel"] = "Zrušit";
            cs["Uninstall"] = "Odinstalovat";
            cs["Success"] = "GIN-PDF-RW byl úspěšně odinstalován z vašeho počítače.";
            cs["AdminReq"] = "K odinstalaci jsou vyžadována oprávnění správce.";
            cs["Error"] = "Chyba při odinstalaci: ";
            loc["cs"] = cs;

            var de = new Dictionary<string, string>();
            de["Title"] = "GIN-PDF-RW v014 Deinstallation";
            de["Header"] = "GIN-PDF-RW v014 deinstallieren?";
            de["Desc"] = "Möchten Sie GIN-PDF-RW und alle Komponenten wirklich vollständig von diesem Computer entfernen?";
            de["Cancel"] = "Abbrechen";
            de["Uninstall"] = "Deinstallieren";
            de["Success"] = "GIN-PDF-RW wurde erfolgreich von Ihrem Computer deinstalliert.";
            de["AdminReq"] = "Administratorrechte sind erforderlich.";
            de["Error"] = "Fehler bei der Deinstallation: ";
            loc["de"] = de;

            var fr = new Dictionary<string, string>();
            fr["Title"] = "Désinstallation de GIN-PDF-RW v014";
            fr["Header"] = "Désinstaller GIN-PDF-RW v014 ?";
            fr["Desc"] = "Voulez-vous vraiment supprimer complètement GIN-PDF-RW de cet ordinateur ?";
            fr["Cancel"] = "Annuler";
            fr["Uninstall"] = "Désinstaller";
            fr["Success"] = "GIN-PDF-RW a été désinstallé avec succès de votre ordinateur.";
            fr["AdminReq"] = "Les privilèges d'administrateur sont requis.";
            fr["Error"] = "Erreur de désinstallation : ";
            loc["fr"] = fr;

            var es = new Dictionary<string, string>();
            es["Title"] = "Desinstalación de GIN-PDF-RW v014";
            es["Header"] = "¿Desinstalar GIN-PDF-RW v014?";
            es["Desc"] = "¿Está seguro de que desea eliminar completamente GIN-PDF-RW de este equipo?";
            es["Cancel"] = "Cancelar";
            es["Uninstall"] = "Desinstalar";
            es["Success"] = "GIN-PDF-RW se ha desinstalado correctamente de su equipo.";
            es["AdminReq"] = "Se requieren privilegios de administrador.";
            es["Error"] = "Error de desinstalación: ";
            loc["es"] = es;

            var it = new Dictionary<string, string>();
            it["Title"] = "Disinstallazione GIN-PDF-RW v014";
            it["Header"] = "Disinstallare GIN-PDF-RW v014?";
            it["Desc"] = "Sei sicuro di voler rimuovere completamente GIN-PDF-RW da questo computer?";
            it["Cancel"] = "Annulla";
            it["Uninstall"] = "Disinstalla";
            it["Success"] = "GIN-PDF-RW è stato disinstallato con successo dal tuo computer.";
            it["AdminReq"] = "Sono richiesti i privilegi di amministratore.";
            it["Error"] = "Errore durante la disinstallazione: ";
            loc["it"] = it;

            var pl = new Dictionary<string, string>();
            pl["Title"] = "Odinstalowanie GIN-PDF-RW v014";
            pl["Header"] = "Odinstalować GIN-PDF-RW v014?";
            pl["Desc"] = "Czy na pewno chcesz całkowicie usunąć GIN-PDF-RW z tego komputera?";
            pl["Cancel"] = "Anuluj";
            pl["Uninstall"] = "Odinstaluj";
            pl["Success"] = "Program GIN-PDF-RW został pomyślnie odinstalowany.";
            pl["AdminReq"] = "Wymagane są uprawnienia administratora.";
            pl["Error"] = "Błąd odinstalowania: ";
            loc["pl"] = pl;

            var tr = new Dictionary<string, string>();
            tr["Title"] = "GIN-PDF-RW v014 Kaldır";
            tr["Header"] = "GIN-PDF-RW v014 Kaldırılsın mı?";
            tr["Desc"] = "GIN-PDF-RW uygulamasını bu bilgisayardan tamamen kaldırmak istediğinizden emin misiniz?";
            tr["Cancel"] = "İptal";
            tr["Uninstall"] = "Kaldır";
            tr["Success"] = "GIN-PDF-RW bilgisayarınızdan başarıyla kaldırıldı.";
            tr["AdminReq"] = "Yönetici ayrıcalıkları gereklidir.";
            tr["Error"] = "Kaldırma hatası: ";
            loc["tr"] = tr;

            var zh = new Dictionary<string, string>();
            zh["Title"] = "卸载 GIN-PDF-RW v014";
            zh["Header"] = "卸载 GIN-PDF-RW v014？";
            zh["Desc"] = "您确定要从这台电脑中彻底删除 GIN-PDF-RW 及其所有组件吗？";
            zh["Cancel"] = "取消";
            zh["Uninstall"] = "卸载";
            zh["Success"] = "GIN-PDF-RW 已成功从此电脑卸载。";
            zh["AdminReq"] = "需要管理员权限。";
            zh["Error"] = "卸载错误：";
            loc["zh"] = zh;

            var ja = new Dictionary<string, string>();
            ja["Title"] = "GIN-PDF-RW v014 のアンインストール";
            ja["Header"] = "GIN-PDF-RW v014 をアンインストールしますか？";
            ja["Desc"] = "このコンピューターから GIN-PDF-RW を完全に削除してもよろしいですか？";
            ja["Cancel"] = "キャンセル";
            ja["Uninstall"] = "アンインストール";
            ja["Success"] = "GIN-PDF-RW はコンピューターから正常にアンインストールされました。";
            ja["AdminReq"] = "管理者権限が必要です。";
            ja["Error"] = "アンインストール エラー: ";
            loc["ja"] = ja;
        }

        private void DetectLanguage()
        {
            // 1. Registry HKCU\Software\GIN-PDF-RW\Language
            try
            {
                using (var k = Registry.CurrentUser.OpenSubKey(@"Software\GIN-PDF-RW"))
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

            // 2. Local directory language.cfg
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
            string sysLang = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.ToLowerInvariant();
            if (loc.ContainsKey(sysLang)) currentLang = sysLang;
            else currentLang = "en";
        }

        private string T(string key)
        {
            if (!loc.ContainsKey(currentLang)) currentLang = "en";
            if (!loc[currentLang].ContainsKey(key)) return loc["en"][key];
            return loc[currentLang][key];
        }

        private void BuildUi()
        {
            Title = T("Title");
            Width = 500;
            Height = 240;
            MinWidth = 500;
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
                Text = T("Header"),
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(243, 139, 168))
            };
            Grid.SetRow(title, 0);
            mainGrid.Children.Add(title);

            var desc = new TextBlock {
                Text = T("Desc"),
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
                Content = T("Cancel"),
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
                Content = T("Uninstall"),
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
                    MessageBox.Show(T("AdminReq"), T("Title"), MessageBoxButton.OK, MessageBoxImage.Warning);
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
                try { Registry.CurrentUser.DeleteSubKeyTree(@"Software\GIN-PDF-RW", false); } catch {}

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

                // Remove AppData configuration
                try
                {
                    string appDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "GIN-PDF-RW");
                    if (Directory.Exists(appDataDir)) Directory.Delete(appDataDir, true);
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

                MessageBox.Show(T("Success"), T("Title"), MessageBoxButton.OK, MessageBoxImage.Information);
                Process.Start(psiCmd);
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(T("Error") + ex.Message, T("Title"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

