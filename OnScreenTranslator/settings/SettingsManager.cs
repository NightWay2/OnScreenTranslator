using OnScreenTranslator.models;
using OnScreenTranslator.ui;
using System.IO;
using System.Text.Json;

namespace OnScreenTranslator.settings
{
    internal class SettingsManager
    {
        private static SettingsManager? _instance;
        private static SettingsModel _settings = new SettingsModel();

        private SettingsManager() { }

        public static SettingsManager GetInstance()
        { 
            if (_instance != null)
                return _instance;
            return _instance = new SettingsManager(); 
        }

        // todo
        public void Init(MainWindow mainWindow)
        {
            LoadSettings(mainWindow);
        }

        // todo (check all setting, if they validated)
        public void LoadSettings(MainWindow mainWindow)
        {
            //sUpdateAllFields(mainWindow);
            // if (fail ) {
            // set def localization
            // mb set def source and target langs
            // call LoadDefauktSettings

            mainWindow.ComBoxSourceLang.SelectedValue = _settings.SourceLang = "en";
            mainWindow.ComBoxTargetLang.SelectedValue = _settings.TargetLanguage = "uk";

            // todo mb add sorting for diff locals
            LocalizationManager.GetInstance().SetLanguage("uk");
            mainWindow.ComBoxLocalization.SelectedValue = _settings.Localization = "uk";
        }

        // todo mb
        public void SaveSettings(MainWindow mainWindow)
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");

            var options = new JsonSerializerOptions { WriteIndented = true };

            // all other fields should be writed somewhere else

            _settings.Localization = mainWindow.ComBoxLocalization.SelectedValue.ToString();
            _settings.SourceLang = mainWindow.ComBoxSourceLang.SelectedValue.ToString();
            _settings.TargetLanguage = mainWindow.ComBoxTargetLang.SelectedValue.ToString();

            string jsonString = JsonSerializer.Serialize(_settings, options);

            File.WriteAllText(filePath, jsonString);
        }

        // todo (Can be used by button Set by default)
        public void LoadSettingsByDefault(MainWindow mainWindow)
        {

        }

        // mb
        //private void ApplySettings() { }

        // todo change and use
        public string GetSourceLanguage()
        {
            return "en";
        }

        // todo change and use
        public string GetTargetLanguage()
        {
            return "ua";
        }

        public int GetOverlayFontSize()
        {
            return 14;
        }

        public int GetOverlayTransparency()
        {
            return 99;
        }

        public bool GetOverlayIsTextSelectingAllowed()
        {
            return false;
        }

        public string GetOverlaySelectedTheme()
        {
            return "dark";
        }
    }
}
