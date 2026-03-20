using OnScreenTranslator.adapters.translators;

namespace OnScreenTranslator.models
{
    public class SettingsModel
    {
        public string Localization { get; set; } = "en";
        public string SourceLanguage { get; set; } = "en";
        public string TargetLanguage { get; set; } = "uk";
        public int OverlayFontSize { get; set; } = 12;
        public int OverlayTransparency { get; set; } = 80;
        public bool OverlayAllowSelectingText { get; set; } = false;
        public string OverlayTheme { get; set; } = "dark";
        public int TranslationInterval { get; set; } = 10;
        public string TranslationWorkMode { get; set; } = "translation";
        public string TranslationOcrMode { get; set; } = "block";
        public string TranslationUpdateMode { get; set; } = "onetime";
        public string Translator { get; set; } = Translators.GoogleFreeTranslator.ToString();
        public string LibreTranslatorEndpoint { get; set; } = "http://localhost:5000";
        public string LibreTranslatorApikey { get; set; } = "";
    }
}
