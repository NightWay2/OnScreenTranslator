using OnScreenTranslator.ui;
using System;
using System.Collections.Generic;
using System.Text;

namespace OnScreenTranslator.settings
{
    internal class SettingsManager
    {
        // todo
        private static SettingsManager? _instance;

        public static List<string> LANGUAGES_SOURCE = new List<string>()
        {
            "Auto",
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

        public static List<string> LANGUAGES_TARGET = new List<string>()
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
            // todo test with auto
            mainWindow.ComBoxSourceLang.ItemsSource = LANGUAGES_SOURCE;
            mainWindow.ComBoxSourceLang.SelectedItem = "English";

            mainWindow.ComBoxTargetLang.ItemsSource = LANGUAGES_TARGET;
            mainWindow.ComBoxTargetLang.SelectedItem = "Ukrainian";
        }

        public string GetSourceLanguage()
        {
            return "en";
        }
    }
}
