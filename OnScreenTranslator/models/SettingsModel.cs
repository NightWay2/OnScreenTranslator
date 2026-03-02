namespace OnScreenTranslator.models
{
    public class SettingsModel
    {
        public string Localization { get; set; } = "en";
        public string SourceLang { get; set; } = "en";
        public string TargetLanguage { get; set; } = "ua";
        public int OverlayFontSize { get; set; } = 12;
        public int OverlayTransparency { get; set; } = 80;
        public bool OverlayAllowSelectingText { get; set; } = false;
        public string OverlayTheme { get; set; } = "dark";
    }
}
