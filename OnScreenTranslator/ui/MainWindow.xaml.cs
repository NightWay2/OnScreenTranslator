using OnScreenTranslator.adapters.ocrs;
using OnScreenTranslator.adapters.translators;
using OnScreenTranslator.services;
using OnScreenTranslator.win32;
using System.Drawing;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace OnScreenTranslator.ui
{
    public partial class MainWindow : Window
    {
        private OverlayWindow? overlayWindow;
        private Rect? selectedScreenArea;

        private OcrService? ocrService;
        private TranslationService? translationService;

        private bool isSelectingArea = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Create overlay window if it not exist, add listener to overlay window
        /// </summary>
        private void CreateOverlayWindow(object sender, RoutedEventArgs e)
        {
            if (overlayWindow != null)
                return;

            overlayWindow = new OverlayWindow();
            overlayWindow.Closed += OverlayWindow_Closed;
            overlayWindow.Show();

            tlgBtnOverlayLock.IsEnabled = true;
        }

        /// <summary>
        /// Destroy overlay window if toggle button is disable
        /// </summary>
        private void DestroyOverlayWindow(object sender, RoutedEventArgs e)
        {
            if (overlayWindow != null)
            {
                overlayWindow.Closed -= OverlayWindow_Closed;
                overlayWindow.Close();
                overlayWindow = null;
            }

            tlgBtnOverlayLock.IsEnabled = false;
            tlgBtnOverlayLock.IsChecked = false;
        }

        private void LockOverlayWindow(object sender, RoutedEventArgs e)
        {
            overlayWindow?.EnableLockMode();
        }

        private void UnlockOverlayWindow(object sender, RoutedEventArgs e)
        {
            overlayWindow?.DisableLockMode();
        }

        private void SelectAreaOnScreen(object sender, RoutedEventArgs e)
        {
            isSelectingArea = true;

            try
            {
                AreaSelectionWindow selector = new AreaSelectionWindow();
                if (selector.ShowDialog() == true)
                {
                    selectedScreenArea = selector.SelectedArea;

                    // Debug code :: ToDo clear
                    /*var source = PresentationSource.FromVisual(this);
                    double dpiX = source.CompositionTarget.TransformToDevice.M11;
                    double dpiY = source.CompositionTarget.TransformToDevice.M22;

                    MessageBox.Show($"X={selectedScreenArea.Value.X}, " +
                        $"Y={selectedScreenArea.Value.Y}, w={selectedScreenArea.Value.Width}, " +
                        $"h={selectedScreenArea.Value.Height}, " +
                        $"x={dpiX}, y={dpiY}");*/
                    // Debug code :: end
                }
            }
            finally
            {
                isSelectingArea = false;
            }
        }

        private async void StartTranslation(object sender, RoutedEventArgs e)
        {
            
            if (!selectedScreenArea.HasValue)
            {
                // think about something better
                MessageBox.Show("Area on screen isn`t selected.");
                return;
            }

            // change as need
            // hide overlay while doing screenshot

            // maybe something different, not hiding and showing
            overlayWindow?.Hide();
            Bitmap bmp = ScreenCaptureService.GetImage(selectedScreenArea.Value);
            overlayWindow?.Show();

            // Create new Ocr with settings and not here
            ocrService = new OcrService(new TesseractOcrAdapter());

            try
            {
                translationService = new TranslationService(TranslatorFactory
                    .GetTranslator(Translators.MicrosoftFreeTranslator, "http://localhost:5000"));

                string translatedText = await Task.Run(async () =>
                {
                    string textToTranslate = ocrService.GetTextFromImage(bmp);
                    return await translationService.TranslateAsync(textToTranslate, "en", "uk");
                });

                overlayWindow?.TxtOverlay.Text = translatedText;
            }
            catch (Exception ex)
            {
                btnStartTranslation.IsChecked = false;

                MessageBox.Show(
                   ex.Message,
                   "Translation error",
                   MessageBoxButton.OK,
                   MessageBoxImage.Error
               );
            }
            finally {} // mb
        }

        /*
         * METHODS THAT PROVIDE NON-MAIN LOGIC
         */
        private void OverlayWindow_Closed(object? sender, EventArgs e)
        {
            overlayWindow = null;

            tlgBtnOverlaySwitch.IsChecked = false;

            tlgBtnOverlayLock.IsEnabled = false;
            tlgBtnOverlayLock.IsChecked = false;
        }

        
        // Hotkey IDs
        private const int SCREEN_CAPTURE_HOTKEY_ID = 1;

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            /*
             * Binding hotkeys
             */
            var hwnd = new WindowInteropHelper(this).Handle;

            if (!NativeMethods.RegisterHotKey(hwnd, SCREEN_CAPTURE_HOTKEY_ID, NativeMethods.MOD_CONTROL,
                    (uint)KeyInterop.VirtualKeyFromKey(Key.Oem5)))
            {
                MessageBox.Show("Screen Capture Hotkey wasn`t registered", "Hotkey error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }

            HwndSource.FromHwnd(hwnd).AddHook(WndProc);
        }

        /*
         * Hotkey handler
         */
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == NativeMethods.WM_HOTKEY)
            {
                var hotkeyId = wParam.ToInt32();

                switch (hotkeyId)
                {
                    case SCREEN_CAPTURE_HOTKEY_ID:
                        if (!isSelectingArea)
                            Dispatcher.BeginInvoke(new Action(() => SelectAreaOnScreen(this, null)));
                        break;
                }

                handled = true;
            }

            return IntPtr.Zero;
        }

        protected override void OnClosed(EventArgs e)
        {
            overlayWindow?.Close();

            /*
             * Unbinding hotkeys
             */
            var hwnd = new WindowInteropHelper(this).Handle;
            NativeMethods.UnregisterHotKey(hwnd, SCREEN_CAPTURE_HOTKEY_ID);
            base.OnClosed(e);
        }
    }

    // add Language class with supported langs

    // think about multithreading

    // mb hide overlay window while doing screenshot & delete screenshot?
    // add setting of selecting monitor ?

    // Add settings logic with all necessary checks
    // Fix OverlayWindow transparency, add textblock or text field to provide text on, add clicking through window
    // Think about binding and how it can be used in this project
    // Think about different hot keys
    // mb animations
    // download tesseract +
    // mb rename all methods with more appropriate 
    // mb add button restore to defaults 
    // mb tray icon

    // add user entered seconds for delay before each translation
    // add custom size of text in overlay, custom color of text in overlay, custom alpha of overlay window

    // add other free translators
    // check if overlay lies in selected area, to not hide it
    // add posibility to translate only one time, and repeatedly

    // docker compose: add only supported languages
}