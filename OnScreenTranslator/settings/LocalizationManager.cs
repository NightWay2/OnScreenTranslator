using OnScreenTranslator.resources;
using System.ComponentModel;
using System.Globalization;

namespace OnScreenTranslator.settings
{
    public class LocalizationManager : INotifyPropertyChanged
    {
        private static LocalizationManager? _instance;

        private LocalizationManager() { }

        public static LocalizationManager GetInstance()
        {
            if (_instance != null)
                return _instance;
            return _instance = new LocalizationManager();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public void SetLanguage(string langCode)
        {
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(langCode);
            Thread.CurrentThread.CurrentCulture = new CultureInfo(langCode);

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        }

        public string CreateOverlay => Strings.CreateOverlay;
        public string DestroyOverlay => Strings.DestroyOverlay;
        public string LockOverlay => Strings.LockOverlay;
        public string UnlockOverlay => Strings.UnlockOverlay;
    }
}
