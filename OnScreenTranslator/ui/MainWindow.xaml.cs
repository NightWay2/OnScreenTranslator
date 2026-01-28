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

        private CancellationTokenSource? translationCts;

        private int TRANSLATION_INTERVAL_MS = 10000;
        private string PREVIOUS_TEXT = "";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void CreateOverlayWindow(object sender, RoutedEventArgs e)
        {
            if (overlayWindow != null)
                return;

            overlayWindow = new OverlayWindow();
            overlayWindow.Closed += OverlayWindow_Closed;
            overlayWindow.Show();

            TlgBtnOverlayLock.IsEnabled = true;
        }

        private void DestroyOverlayWindow(object sender, RoutedEventArgs e)
        {
            if (overlayWindow != null)
            {
                overlayWindow.Closed -= OverlayWindow_Closed;
                overlayWindow.Close();
                overlayWindow = null;
            }

            TlgBtnOverlayLock.IsEnabled = false;
            TlgBtnOverlayLock.IsChecked = false;
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
                }
            }
            finally
            {
                isSelectingArea = false;
            }
        }

        /*
         * Translation logic
         */
        private void StartTranslation(object sender, RoutedEventArgs e) // todo mb add timer before start
        {
            if (BtnStartTranslation.IsChecked == true)
            {
                StartTranslationLoop();
            }
            else
            {
                StopTranslationLoop();
            }
        }

        // todo
        private void StartTranslationLoop()
        {
            if (!selectedScreenArea.HasValue)
            {
                // mb change here
                MessageBox.Show("Area on screen isn`t selected.");
                BtnStartTranslation.IsChecked = false;
                return;
            }

            translationCts = new CancellationTokenSource();
            var token = translationCts.Token;

            // зробити метод, який буде викликатися на початку роботи програми в якому будуть
            // ініціалізовуватися необхідні сервіси
            // (поки ініціалізовуються тут)
            ocrService = new OcrService(new TesseractOcrAdapter());
            translationService = new TranslationService(
                TranslatorFactory.GetTranslator(
                    Translators.LibreTranslator,
                    "http://localhost:5000"
                )
            );

            Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    if (overlayWindow == null) continue;

                    try
                    {
                        Bitmap image;

                        bool intersects = overlayWindow.Dispatcher.Invoke(() =>
                            overlayWindow.IntersectsScreenArea(selectedScreenArea.Value)
                        );

                        if (intersects)
                        {
                            overlayWindow.Dispatcher.Invoke(() => overlayWindow.Hide());
                            image = ScreenCaptureService.GetImage(selectedScreenArea.Value);
                            overlayWindow.Dispatcher.Invoke(() => overlayWindow.Show());
                        }
                        else
                        {
                            image = ScreenCaptureService.GetImage(selectedScreenArea.Value);
                        }

                        // optional
                        // image = ImagePreprocessor.Process(image);

                        string text = ocrService.GetTextFromImage(image);

                        if (!string.IsNullOrEmpty(text) && !PREVIOUS_TEXT.Equals(text))
                        {
                            PREVIOUS_TEXT = text;

                            // change to custom langs
                            string translated = await translationService.TranslateAsync(
                                text, "en", "uk"
                            );

                            Dispatcher.Invoke(() => overlayWindow?.TxtOverlay.Text = translated);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            MessageBox.Show(
                                ex.Message,
                                "Translation error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error
                            );
                            BtnStartTranslation.IsChecked = false;
                        });
                        break;
                    }

                    await Task.Delay(TRANSLATION_INTERVAL_MS, token);
                }
            }, token);
        }

        private void StopTranslationLoop()
        {
            translationCts?.Cancel();
            translationCts = null;
        }

        /*
         * Method for setting params using settingsManager
         */
        private void LoadSettings()
        {

        }

        /*
         * METHODS THAT PROVIDE NON-MAIN LOGIC
         */
        private void OverlayWindow_Closed(object? sender, EventArgs e)
        {
            overlayWindow = null;

            TlgBtnOverlaySwitch.IsChecked = false;

            TlgBtnOverlayLock.IsEnabled = false;
            TlgBtnOverlayLock.IsChecked = false;
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