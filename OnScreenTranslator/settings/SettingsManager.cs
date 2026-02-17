using OnScreenTranslator.resources;
using OnScreenTranslator.ui;

namespace OnScreenTranslator.settings
{
    internal class SettingsManager
    {
        private static List<string> LANGUAGES_SOURCE = new List<string>()
        {
            "Auto",
        };

        private static List<string> LANGUAGES_TARGET = new List<string>()
        {
            "Arabic",
            "Chinese (simplified)",
            "Chinese (traditional)",
            "Czech",
            "Danish",
            "Dutch",
            "English",
            "Finnish",
            "French",
            "German",
            "Hindi",
            "Indonesian",
            "Italian",
            "Japanese",
            "Korean",
            "Persian",
            "Polish",
            "Portuguese",
            "Romanian",
            "Russian",
            "Spanish",
            "Thai",
            "Turkish",
            "Ukrainian",
            "Vietnamese"
        };

        private static SettingsManager? _instance;
        private string localization;

        private SettingsManager() { }

        public static SettingsManager GetInstance()
        { 
            if (_instance != null)
                return _instance;
            return _instance = new SettingsManager(); 
        }

        // todo all stuff like reading conf file and set def params
        public void Init(MainWindow mainWindow)
        {
            LANGUAGES_SOURCE.AddRange(LANGUAGES_TARGET);
            mainWindow.ComBoxSourceLang.ItemsSource = LANGUAGES_SOURCE;
            mainWindow.ComBoxSourceLang.SelectedItem = "English"; // todo add through localization

            mainWindow.ComBoxTargetLang.ItemsSource = LANGUAGES_TARGET;
            mainWindow.ComBoxTargetLang.SelectedItem = "Ukrainian";

            LocalizationManager.GetInstance().SetLanguage("en");
        }

        public string GetLocalizationLanguage()
        {
            return "en";
        }
    }
}
