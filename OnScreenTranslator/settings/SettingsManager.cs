using OnScreenTranslator.ui;

namespace OnScreenTranslator.settings
{
    internal class SettingsManager
    {
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
            mainWindow.ComBoxSourceLang.SelectedValue = "en"; // default Eng index
            mainWindow.ComBoxTargetLang.SelectedValue = "uk"; // default Ukr index

            LocalizationManager.GetInstance().SetLanguage("uk");
            mainWindow.ComBoxLocalization.SelectedValue = "uk"; // default ukr index
        }

        public string GetLocalizationLanguage()
        {
            return "en";
        }
    }
}
