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

        // Buttons
        public string CreateOverlay => Strings.CreateOverlay;
        public string DestroyOverlay => Strings.DestroyOverlay;
        public string LockOverlay => Strings.LockOverlay;
        public string UnlockOverlay => Strings.UnlockOverlay;
        public string SelectArea => Strings.SelectArea;
        public string Start => Strings.Start;
        public string Stop => Strings.Stop;
        public string Guide => Strings.Guide;
        public string TranslatorSettings => Strings.TranslatorSettings;
        public string RestoreSettings => Strings.RestoreSettings;
        public string ApplySettings => Strings.ApplySettings;
        public string Yes => Strings.Yes;
        public string Cancel => Strings.Cancel;
        public string Save => Strings.Save;

        // Languages
        public string Language_Auto => Strings.Language_Auto;
        public string Language_Arabic => Strings.Language_Arabic;
        public string Language_Chinese_sim => Strings.Language_Chinese_sim;
        public string Language_Chinese_tra => Strings.Language_Chinese_tra;
        public string Language_Czech => Strings.Language_Czech;
        public string Language_Danish => Strings.Language_Danish;
        public string Language_Dutch => Strings.Language_Dutch;
        public string Language_English => Strings.Language_English;
        public string Language_Finnish => Strings.Language_Finnish;
        public string Language_French => Strings.Language_French;
        public string Language_German => Strings.Language_German;
        public string Language_Hindi => Strings.Language_Hindi;
        public string Language_Indonesian => Strings.Language_Indonesian;
        public string Language_Italian => Strings.Language_Italian;
        public string Language_Japanese => Strings.Language_Japanese;
        public string Language_Korean => Strings.Language_Korean;
        public string Language_Persian => Strings.Language_Persian;
        public string Language_Polish => Strings.Language_Polish;
        public string Language_Portuguese => Strings.Language_Portuguese;
        public string Language_Romanian => Strings.Language_Romanian;
        public string Language_Russian => Strings.Language_Russian;
        public string Language_Spanish => Strings.Language_Spanish;
        public string Language_Thai => Strings.Language_Thai;
        public string Language_Turkish => Strings.Language_Turkish;
        public string Language_Ukrainian => Strings.Language_Ukrainian;
        public string Language_Vietnamese => Strings.Language_Vietnamese;

        // Labels
        public string LocalizationLabel => Strings.LocalizationLabel;
        public string SourceLangLabel => Strings.SourceLangLabel;
        public string TargetLangLabel => Strings.TargetLangLabel;
        public string SettingsLabel => Strings.SettingsLabel;
        public string OverlaySettingsLabel => Strings.OverlaySettingsLabel;
        public string ApplyConfirmation => Strings.ApplyConfirmation;
        public string ConfirmationWindow => Strings.ConfirmationWindow;
        public string FontSize => Strings.FontSize;
        public string Transparency => Strings.Transparency;
        public string AllowTextSelecting => Strings.AllowTextSelecting;
        public string SelectedTheme => Strings.SelectedTheme;
        public string Dark => Strings.Dark;
        public string Light => Strings.Light;
        public string TranslationInterval => Strings.TranslationInterval;
        public string TranslationSettingsLabel => Strings.TranslationSettingsLabel;
        public string TranslatorEngine => Strings.TranslatorEngine;
        public string EndpointURL => Strings.EndpointURL;
        public string ApiKey => Strings.ApiKey;
        public string TranslationStatusOn => Strings.TranslationStatusOn;
        public string TranslationStatusOff => Strings.TranslationStatusOff;
    }
}
