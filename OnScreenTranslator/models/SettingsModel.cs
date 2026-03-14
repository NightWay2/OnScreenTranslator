namespace OnScreenTranslator.models
{
    public class SettingsModel
    {
        // todo add more settings
        public string Localization { get; set; } = "en";
        public string SourceLanguage { get; set; } = "en";
        public string TargetLanguage { get; set; } = "uk";
        public int OverlayFontSize { get; set; } = 12;
        public int OverlayTransparency { get; set; } = 80;
        public bool OverlayAllowSelectingText { get; set; } = false;
        public string OverlayTheme { get; set; } = "dark";
        public int TranslationInterval { get; set; } = 10;
    }
}
