using System;
using System.Collections.Generic;
using System.Text;

namespace OnScreenTranslator.settings
{
    internal class SettingsManager
    {
        // todo
        private static SettingsManager? _instance;

        private SettingsManager() { }

        public static SettingsManager GetInstance()
        { 
            if (_instance != null)
                return _instance;
            return _instance = new SettingsManager(); 
        }

        public string GetSourceLanguage()
        {
            return "en";
        }
    }
}
