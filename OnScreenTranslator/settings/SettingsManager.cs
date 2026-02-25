using OnScreenTranslator.ui;

namespace OnScreenTranslator.settings
{
    internal class SettingsManager
    {
        private static SettingsManager? _instance;
        private string _localization;

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
            mainWindow.ComBoxSourceLang.SelectedValue = "en";
            mainWindow.ComBoxTargetLang.SelectedValue = "uk";

            // todo mb add sorting for diff locals
            LocalizationManager.GetInstance().SetLanguage("uk");
            mainWindow.ComBoxLocalization.SelectedValue = "uk";
        }

        public string GetLocalizationLanguage()
        {
            return "en";
        }
    }
}
