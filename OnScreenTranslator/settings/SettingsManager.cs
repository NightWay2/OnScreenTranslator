using OnScreenTranslator.adapters.translators;
using OnScreenTranslator.models;
using OnScreenTranslator.services;
using OnScreenTranslator.ui;
using System.IO;
using System.Text.Json;

namespace OnScreenTranslator.settings
{
    internal class SettingsManager
    {
        private static SettingsManager? _instance;
        private static SettingsModel _settings = new SettingsModel();

        private static string _filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");

        private static List<string> _allowedLangs = new List<string>
        {
                "ar", "zh", "zt", "cs", "da", "nl", "en", "fi", "fr",
                "de", "hi", "id", "it", "ja", "ko", "fa", "pl", "pt",
                "ro", "ru", "es", "th", "tr", "uk", "vi",
        };

        private SettingsManager() { }

        public static SettingsManager GetInstance()
        { 
            if (_instance != null)
                return _instance;
            return _instance = new SettingsManager(); 
        }

        public void Init(MainWindow mainWindow)
        {
            LoadSettings(mainWindow);
        }

        public void LoadSettings(MainWindow mainWindow)
        {
            if (File.Exists(_filePath))
            {
                try
                {
                    string jsonString = File.ReadAllText(_filePath);

                    _settings = JsonSerializer.Deserialize<SettingsModel>(jsonString) ?? new SettingsModel();

                    CheckParams();
                }
                catch
                {
                    _settings = new SettingsModel();
                }
            }
            else
            {
                _settings = new SettingsModel();
            }

            SetInitialParams(mainWindow);
        }

        public void SaveSettings(MainWindow mainWindow)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };

            // all other fields should be writed when we use button aplly settings !!!!!!!!!!!!!!!!!!

            _settings.Localization = mainWindow.ComBoxLocalization.SelectedValue.ToString();
            _settings.SourceLanguage = mainWindow.ComBoxSourceLang.SelectedValue.ToString();
            _settings.TargetLanguage = mainWindow.ComBoxTargetLang.SelectedValue.ToString();

            string jsonString = JsonSerializer.Serialize(_settings, options);

            File.WriteAllText(_filePath, jsonString);
        }

        private void CheckParams()
        {
            if (!_allowedLangs.Contains(_settings.SourceLanguage) && _settings.SourceLanguage != "auto")
            {
                _settings.SourceLanguage = "en";
            }

            if (!_allowedLangs.Contains(_settings.TargetLanguage))
            {
                _settings.TargetLanguage = "uk";
            }

            if (_settings.Localization != "en" && _settings.Localization != "uk")
            {
                _settings.Localization = "en";
            }

            if (_settings.OverlayFontSize < 8 || _settings.OverlayFontSize > 40) // mb change
            {
                _settings.OverlayFontSize = 12;
            }

            if (_settings.OverlayTransparency < 0 || _settings.OverlayTransparency > 100)
            {
                _settings.OverlayTransparency = 80;
            }

            if (_settings.OverlayAllowSelectingText != false && _settings.OverlayAllowSelectingText != true)
            {
                _settings.OverlayAllowSelectingText = false;
            }

            if (_settings.OverlayTheme != "dark" && _settings.OverlayTheme != "light")
            {
                _settings.OverlayTheme = "dark";
            }

            if (_settings.TranslationInterval < 1 || _settings.TranslationInterval > 60) 
            {
                _settings.TranslationInterval = 10;
            }

            if (_settings.LibreTranslatorEndpoint == string.Empty || 
                _settings.LibreTranslatorEndpoint == null)
            {
                _settings.LibreTranslatorEndpoint = "http://localhost:5000";
            }

            if (_settings.TranslationWorkMode != "translation" && _settings.TranslationWorkMode!= "ocr")
            {
                _settings.TranslationWorkMode = "translation";
            }

            if (_settings.TranslationOcrMode != "block" && _settings.TranslationOcrMode != "row")
            {
                _settings.TranslationOcrMode = "block";
            }

            if (_settings.TranslationUpdateMode != "onetime" && _settings.TranslationUpdateMode != "onetimewithwaiting"
                && _settings.TranslationUpdateMode != "repeatedly")
            {
                _settings.TranslationUpdateMode = "onetime";
            }
        }

        private void SetInitialParams(MainWindow mainWindow)
        {
            LocalizationManager.GetInstance().SetLanguage(_settings.Localization);
            mainWindow.ComBoxLocalization.SelectedValue = _settings.Localization;
            mainWindow.ComBoxSourceLang.SelectedValue = _settings.SourceLanguage;
            mainWindow.ComBoxTargetLang.SelectedValue = _settings.TargetLanguage;

            mainWindow.TxtOverlayFontSize.Text = _settings.OverlayFontSize.ToString();
            mainWindow.SliderOverlayTransparency.Value = _settings.OverlayTransparency;
            mainWindow.CheckBoxOverlayAllowCopy.IsChecked = _settings.OverlayAllowSelectingText;
            mainWindow.ComBoxOverlayTheme.SelectedValue = _settings.OverlayTheme;

            mainWindow.TxtTranslationInterval.Text = _settings.TranslationInterval.ToString();
            mainWindow.ComBoxTranslationWorkMode.SelectedValue = _settings.TranslationWorkMode;
            mainWindow.ComBoxTranslationOcrMode.SelectedValue = _settings.TranslationOcrMode;
            mainWindow.ComBoxTranslationUpdateMode.SelectedValue = _settings.TranslationUpdateMode;
        }

        public void ApplySettings(MainWindow mainWindow) 
        {
            _settings.OverlayFontSize = int.Parse(mainWindow.TxtOverlayFontSize.Text); // careful, need to be only special int
            _settings.OverlayTransparency = (int) mainWindow.SliderOverlayTransparency.Value;
            _settings.OverlayAllowSelectingText = mainWindow.CheckBoxOverlayAllowCopy.IsChecked.Value;
            _settings.OverlayTheme = mainWindow.ComBoxOverlayTheme.SelectedValue.ToString();

            _settings.TranslationInterval = int.Parse(mainWindow.TxtTranslationInterval.Text);
            _settings.TranslationWorkMode = mainWindow.ComBoxTranslationWorkMode.SelectedValue.ToString();
            _settings.TranslationOcrMode = mainWindow.ComBoxTranslationOcrMode.SelectedValue.ToString();
            _settings.TranslationUpdateMode = mainWindow.ComBoxTranslationUpdateMode.SelectedValue.ToString();

            SaveSettings(mainWindow);
        }

        public void RestoreSettings(MainWindow mainWindow)
        {
            SettingsModel newSettings = new SettingsModel() // add new params if smth added !!!!!!!!!!!!!!
            {
                Localization = _settings.Localization,
                SourceLanguage = _settings.SourceLanguage,
                TargetLanguage = _settings.TargetLanguage,
                Translator = _settings.Translator,
                LibreTranslatorEndpoint = _settings.LibreTranslatorEndpoint,
                LibreTranslatorApikey = _settings.LibreTranslatorApikey,
            };

            _settings = newSettings;

            mainWindow.TxtOverlayFontSize.Text = _settings.OverlayFontSize.ToString();
            mainWindow.SliderOverlayTransparency.Value = _settings.OverlayTransparency;
            mainWindow.CheckBoxOverlayAllowCopy.IsChecked = _settings.OverlayAllowSelectingText;
            mainWindow.ComBoxOverlayTheme.SelectedValue = _settings.OverlayTheme;

            mainWindow.TxtTranslationInterval.Text = _settings.TranslationInterval.ToString();
            mainWindow.ComBoxTranslationWorkMode.SelectedValue = _settings.TranslationWorkMode;
            mainWindow.ComBoxTranslationOcrMode.SelectedValue = _settings.TranslationOcrMode;
            mainWindow.ComBoxTranslationUpdateMode.SelectedValue = _settings.TranslationUpdateMode;

            SaveSettings(mainWindow);
        }

        public void ApplyTranslatorSettings(TranslatorSettingsWindow tsw)
        {
            _settings.Translator = tsw.ComBoxTranslator.SelectedValue.ToString();
            
            if (_settings.Translator == "LibreTranslator")
            {
                _settings.LibreTranslatorEndpoint = tsw.TxtEndpoint.Text;
                _settings.LibreTranslatorApikey = tsw.TxtApiKey.Text;

                if (_settings.LibreTranslatorEndpoint == string.Empty)
                    _settings.LibreTranslatorEndpoint = "http://localhost:5000";
            }
        }

        // IF NEW TRANSLATORS ARE ADDED, THEY SHOULD BE ADDED HERE AS WELL
        public TranslationService GetTranslationService(out string apikey)
        {
            switch (_settings.Translator)
            {
                case "LibreTranslator":
                    apikey = _settings.LibreTranslatorApikey;
                    return new TranslationService(TranslatorFactory.GetTranslator(
                        Translators.LibreTranslator, _settings.LibreTranslatorEndpoint
                    ));
                default:
                    apikey = "";
                    return new TranslationService(TranslatorFactory.GetTranslator(Translators.GoogleFreeTranslator));
            }
            ;
        }

        public int GetOverlayFontSize()
        {
            return _settings.OverlayFontSize;
        }

        public int GetOverlayTransparency()
        {
            return _settings.OverlayTransparency;
        }

        public bool GetOverlayIsTextSelectingAllowed()
        {
            return _settings.OverlayAllowSelectingText;
        }

        public string GetOverlaySelectedTheme()
        {
            return _settings.OverlayTheme;
        }

        public int GetTranslationInterval()
        {
            return _settings.TranslationInterval;
        }

        public string GetTranslator()
        {
            return _settings.Translator;
        }

        public string GetLibreTranslatorEndpoint()
        {
            return _settings.LibreTranslatorEndpoint;
        }

        public string GetLibreTranslatorApikey()
        {
            return _settings.LibreTranslatorApikey;
        }

        public string GetTranslationWorkMode()
        {
            return _settings.TranslationWorkMode;
        }

        public string GetTranslationOcrMode()
        {
            return _settings.TranslationOcrMode;
        }

        public string GetTranslationUpdateMode()
        {
            return _settings.TranslationUpdateMode;
        }
    }
}
