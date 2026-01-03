namespace OnScreenTranslator
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        static App()
        {
            AppContext.SetSwitch(
                "Switch.System.Windows.DpiAwareness.EnablePerMonitorV2",
                true);

            AppContext.SetSwitch(
                "Switch.System.Windows.Forms.DoNotScaleForDpiChanges",
                true);
        }
    }

}
