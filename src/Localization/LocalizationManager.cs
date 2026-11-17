using System;
using System.Collections.Generic;
using System.Globalization;

namespace GinPdfRw.Localization
{
    public static class LocalizationManager
    {
        public static string CurrentLanguage { get; private set; } = "en";

        private static readonly Dictionary<string, Dictionary<string, string>> Translations = 
            new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

        static LocalizationManager()
        {
            // English (Default)
            Translations["en"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["AppTitle"] = "GIN-PDF-RW - Zen PDF Reader & Editor",
                ["Open"] = "Open (Ctrl+O)",
                ["Save"] = "Save (Ctrl+S)",
                ["Print"] = "Print (Ctrl+P)",
                ["Fullscreen"] = "Fullscreen (F11)",
                ["ExitFullscreen"] = "Exit Fullscreen (Esc/F11)",
                ["ModeReader"] = "Reader & Forms",
                ["ModeEditor"] = "Editor Mode",
                ["AddText"] = "Add Text",
                ["AddCheckmark"] = "Add Checkmark (✓)",
                ["AddImage"] = "Insert Image",
                ["Sign"] = "Sign Document",
                ["SignDraw"] = "Draw Signature",
                ["SignCert"] = "Digital Certificate (X.509)",
                ["ZoomIn"] = "Zoom In (Ctrl++)",
                ["ZoomOut"] = "Zoom Out (Ctrl+-)",
                ["FitWidth"] = "Fit Width",
                ["FitPage"] = "Fit Page",
                ["PrevPage"] = "Previous Page",
                ["NextPage"] = "Next Page",
                ["PageOf"] = "Page {0} of {1}",
                ["Language"] = "Language",
                ["LangEn"] = "English",
                ["LangRu"] = "Русский",
                ["LangCs"] = "Čeština",
                ["Ready"] = "Ready",
                ["DocumentLoaded"] = "Document loaded successfully: {0}",
                ["SavedSuccessfully"] = "Saved successfully: {0}",
                ["ErrorOpening"] = "Failed to open PDF document.",
                ["ErrorSaving"] = "Failed to save PDF document.",
                ["SelectPdfFile"] = "Select PDF Document",
                ["PdfFilter"] = "PDF Documents (*.pdf)|*.pdf|All Files (*.*)|*.*",
                ["SelectImageFile"] = "Select Image (Stamp / Logo)",
                ["ImageFilter"] = "Image Files (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp",
                ["CertSelectTitle"] = "Select Digital Certificate",
                ["NoCertFound"] = "No valid signing certificates found in Windows Certificate Store.",
                ["SignedSuccessfully"] = "Document cryptographically signed by: {0}"
            };

            // Russian
            Translations["ru"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["AppTitle"] = "GIN-PDF-RW - Просмотр и редактор PDF",
                ["Open"] = "Открыть (Ctrl+O)",
                ["Save"] = "Сохранить (Ctrl+S)",
                ["Print"] = "Печать (Ctrl+P)",
                ["Fullscreen"] = "Во весь экран (F11)",
                ["ExitFullscreen"] = "Оконный режим (Esc/F11)",
                ["ModeReader"] = "Чтение и формы",
                ["ModeEditor"] = "Редактировать",
                ["AddText"] = "Добавить текст",
                ["AddCheckmark"] = "Галочка (✓)",
                ["AddImage"] = "Вставить картинку",
                ["Sign"] = "Подписать",
                ["SignDraw"] = "Нарисовать подпись",
                ["SignCert"] = "ЭЦП Сертификат (X.509)",
                ["ZoomIn"] = "Увеличить (Ctrl++)",
                ["ZoomOut"] = "Уменьшить (Ctrl+-)",
                ["FitWidth"] = "По ширине",
                ["FitPage"] = "Вся страница",
                ["PrevPage"] = "Предыдущая страница",
                ["NextPage"] = "Следующая страница",
                ["PageOf"] = "Страница {0} из {1}",
                ["Language"] = "Язык",
                ["LangEn"] = "English",
                ["LangRu"] = "Русский",
                ["LangCs"] = "Čeština",
                ["Ready"] = "Готово",
                ["DocumentLoaded"] = "Документ успешно открыт: {0}",
                ["SavedSuccessfully"] = "Успешно сохранено: {0}",
                ["ErrorOpening"] = "Ошибка при открытии PDF документа.",
                ["ErrorSaving"] = "Ошибка при сохранении PDF документа.",
                ["SelectPdfFile"] = "Выберите PDF документ",
                ["PdfFilter"] = "Документы PDF (*.pdf)|*.pdf|Все файлы (*.*)|*.*",
                ["SelectImageFile"] = "Выберите изображение (штамп / печать)",
                ["ImageFilter"] = "Изображения (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp",
                ["CertSelectTitle"] = "Выберите сертификат ЭЦП",
                ["NoCertFound"] = "В хранилище сертификатов Windows не найдено подходящих сертификатов.",
                ["SignedSuccessfully"] = "Документ успешно подписан сертификатом: {0}"
            };

            // Czech
            Translations["cs"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["AppTitle"] = "GIN-PDF-RW - Prohlížeč a editor PDF",
                ["Open"] = "Otevřít (Ctrl+O)",
                ["Save"] = "Uložit (Ctrl+S)",
                ["Print"] = "Tisk (Ctrl+P)",
                ["Fullscreen"] = "Celá obrazovka (F11)",
                ["ExitFullscreen"] = "Ukončit celou obrazovku (Esc/F11)",
                ["ModeReader"] = "Čtení a formuláře",
                ["ModeEditor"] = "Upravit",
                ["AddText"] = "Přidat text",
                ["AddCheckmark"] = "Zatrhávací políčko (✓)",
                ["AddImage"] = "Vložit obrázek",
                ["Sign"] = "Podepsat",
                ["SignDraw"] = "Nakreslit podpis",
                ["SignCert"] = "Digitální certifikát (X.509)",
                ["ZoomIn"] = "Přiblížit (Ctrl++)",
                ["ZoomOut"] = "Oddálit (Ctrl+-)",
                ["FitWidth"] = "Na šířku",
                ["FitPage"] = "Celá stránka",
                ["PrevPage"] = "Předchozí stránka",
                ["NextPage"] = "Další stránka",
                ["PageOf"] = "Strana {0} z {1}",
                ["Language"] = "Jazyk",
                ["LangEn"] = "English",
                ["LangRu"] = "Русский",
                ["LangCs"] = "Čeština",
                ["Ready"] = "Připraveno",
                ["DocumentLoaded"] = "Dokument byl úspěšně otevřen: {0}",
                ["SavedSuccessfully"] = "Úspěšně uloženo: {0}",
                ["ErrorOpening"] = "Chyba při otevírání PDF dokumentu.",
                ["ErrorSaving"] = "Chyba při ukládání PDF dokumentu.",
                ["SelectPdfFile"] = "Vyberte dokument PDF",
                ["PdfFilter"] = "Dokumenty PDF (*.pdf)|*.pdf|Všechny soubory (*.*)|*.*",
                ["SelectImageFile"] = "Vyberte obrázek (razítko / podpis)",
                ["ImageFilter"] = "Soubory obrázků (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp",
                ["CertSelectTitle"] = "Vyberte digitální certifikát",
                ["NoCertFound"] = "V úložišti certifikátů systému Windows nebyl nalezen žádný platný certifikát.",
                ["SignedSuccessfully"] = "Dokument byl digitálně podepsán certifikátem: {0}"
            };

            DetectDefaultLanguage();
        }

        public static void DetectDefaultLanguage()
        {
            var culture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.ToLowerInvariant();
            if (Translations.ContainsKey(culture))
            {
                CurrentLanguage = culture;
            }
            else
            {
                CurrentLanguage = "en";
            }
        }

        public static void SetLanguage(string langCode)
        {
            if (Translations.ContainsKey(langCode))
            {
                CurrentLanguage = langCode.ToLowerInvariant();
            }
        }

        public static string Get(string key)
        {
            if (Translations.TryGetValue(CurrentLanguage, out var dict) && dict.TryGetValue(key, out var val))
            {
                return val;
            }
            if (Translations["en"].TryGetValue(key, out var fallback))
            {
                return fallback;
            }
            return key;
        }

        public static string Format(string key, params object[] args)
        {
            return string.Format(Get(key), args);
        }
    }
}
