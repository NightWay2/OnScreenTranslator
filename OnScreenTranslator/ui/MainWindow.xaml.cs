using OnScreenTranslator.adapters.ocrs;
using OnScreenTranslator.adapters.translators;
using OnScreenTranslator.services;
using OnScreenTranslator.win32;
using System.Drawing;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;

namespace OnScreenTranslator.ui
{
    // todo hide prpogram in tray
    public partial class MainWindow : Window
    {
        private OverlayWindow? _overlayWindow;
        private Rect? _selectedScreenArea;

        private OcrService? _ocrService;
        private TranslationService? _translationService;

        private bool _isSelectingArea = false;

        // Translation related vars
        private CancellationTokenSource? _translationCts;
        private const int TRANSLATION_INTERVAL_MS = 5000;
        private string _previousText = ""; // todo : control this var, if we destroy overlay and return it without visible text it prevent translation

        // Hotkey IDs and Keys
        private const int SCREEN_CAPTURE_HOTKEY_ID = 1;
        private const int START_STOP_TRANSLATION_HOTKEY_ID = 2;
        private const int CREATE_DESTROY_OVERLAY_HOTKEY_ID = 3;
        private const int LOCK_UNLOCK_OVERLAY_HOTKEY_ID = 4;
        private const Key SCREEN_CAPTURE_KEY = Key.Q;
        private const Key START_STOP_TRANSLATION_KEY = Key.T;
        private const Key CREATE_DESTROY_OVERLAY_KEY = Key.O;
        private const Key LOCK_UNLOCK_OVERLAY_KEY = Key.L;
        public const uint MOD_ALT = 0x0001;
        public const uint MOD_CONTROL = 0x0002;
        public const uint MOD_SHIFT = 0x0004;
        public const uint MOD_WIN = 0x0008;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void CreateOverlayWindow(object sender, RoutedEventArgs e)
        {
            if (_overlayWindow != null)
                return;

            _overlayWindow = new OverlayWindow();
            _overlayWindow.Closed += OverlayWindow_Closed;
            _overlayWindow.Show();

            TlgBtnOverlayLockUnlock.IsEnabled = true;
        }

        private void DestroyOverlayWindow(object sender, RoutedEventArgs e)
        {
            if (_overlayWindow != null)
            {
                _overlayWindow.Closed -= OverlayWindow_Closed;
                _overlayWindow.Close();
                _overlayWindow = null;
            }

            TlgBtnOverlayLockUnlock.IsEnabled = false;
            TlgBtnOverlayLockUnlock.IsChecked = false;
        }

        private void LockOverlayWindow(object sender, RoutedEventArgs e)
        {
            _overlayWindow?.EnableLockMode();
        }

        private void UnlockOverlayWindow(object sender, RoutedEventArgs e)
        {
            _overlayWindow?.DisableLockMode();
        }

        private void SelectAreaOnScreen(object sender, RoutedEventArgs e)
        {
            _isSelectingArea = true;

            try
            {
                AreaSelectionWindow selector = new AreaSelectionWindow();
                if (selector.ShowDialog() == true)
                {
                    _selectedScreenArea = selector.SelectedArea;
                }
            }
            finally
            {
                _isSelectingArea = false;
            }
        }

        /*
         * Translation logic
         */
        private void StartStopTranslation(object sender, RoutedEventArgs e) // todo mb add timer before start
        {
            if (TlgBtnStartStopTranslation.IsChecked == true)
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
            if (!_selectedScreenArea.HasValue)
            {
                // mb change here
                MessageBox.Show("Area on screen isn`t selected.");
                TlgBtnStartStopTranslation.IsChecked = false;
                return;
            }

            _translationCts = new CancellationTokenSource();
            var token = _translationCts.Token;

            // зробити метод, який буде викликатися на початку роботи програми в якому будуть
            // ініціалізовуватися необхідні сервіси
            // (поки ініціалізовуються тут)
            _ocrService = new OcrService(new OneOcrAdapter());
            _translationService = new TranslationService(
                TranslatorFactory.GetTranslator(
                    Translators.LibreTranslator,
                    "http://localhost:5000"
                )
            );

            Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    if (_overlayWindow == null) continue;

                    try
                    {
                        Bitmap image;

                        bool intersects = _overlayWindow.Dispatcher.Invoke(() =>
                            _overlayWindow.IntersectsScreenArea(_selectedScreenArea.Value)
                        );

                        if (intersects)
                        {
                            _overlayWindow.Dispatcher.Invoke(() => _overlayWindow.Hide());
                            image = ScreenCaptureService.GetImage(_selectedScreenArea.Value);
                            _overlayWindow.Dispatcher.Invoke(() => _overlayWindow.Show());
                        }
                        else
                        {
                            image = ScreenCaptureService.GetImage(_selectedScreenArea.Value);
                        }

                        // optional
                        // image = ImagePreprocessor.Process(image);

                        string text = _ocrService.GetTextFromImage(image);

                        if (!string.IsNullOrEmpty(text) && !_previousText.Equals(text))
                        {
                            _previousText = text;

                            // change to custom langs
                            /*string translated = await _translationService.TranslateAsync(
                                text, "en", "uk"
                            );*/
                            string translated = text;

                            Dispatcher.Invoke(() => _overlayWindow?.TxtOverlay.Text = translated);
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
                            TlgBtnStartStopTranslation.IsChecked = false;
                        });
                        break;
                    }

                    await Task.Delay(TRANSLATION_INTERVAL_MS, token);
                }
            }, token);
        }

        private void StopTranslationLoop()
        {
            _translationCts?.Cancel();
            _translationCts = null;
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
            _overlayWindow = null;

            TlgBtnOverlayCreateDestroy.IsChecked = false;

            TlgBtnOverlayLockUnlock.IsEnabled = false;
            TlgBtnOverlayLockUnlock.IsChecked = false;
        }

        // todo add posoibility of changing hotkeys
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            /*
             * Binding hotkeys
             */
            var hwnd = new WindowInteropHelper(this).Handle;

            RegisterHotKey(hwnd, SCREEN_CAPTURE_HOTKEY_ID, MOD_ALT, Key.Q,
                "Screen Capture Hotkey is not registered");
            RegisterHotKey(hwnd, START_STOP_TRANSLATION_HOTKEY_ID, MOD_ALT, Key.T,
                "Start/Stop Translation Hotkey is not registered");
            RegisterHotKey(hwnd, CREATE_DESTROY_OVERLAY_HOTKEY_ID, MOD_ALT, Key.O,
                "Create/Destroy Overlay Hotkey is not registered");
            RegisterHotKey(hwnd, LOCK_UNLOCK_OVERLAY_HOTKEY_ID, MOD_ALT, Key.L,
                "Lock/Unlock Hotkey is not registered");

            HwndSource.FromHwnd(hwnd).AddHook(WndProc);
        }

        private void RegisterHotKey(nint hwnd, int id, uint fsModifiers, Key key, string errMessage)
        {
            if (!NativeMethods.RegisterHotKey(hwnd, id, fsModifiers, (uint)KeyInterop.VirtualKeyFromKey(key)))
            {
                MessageBox.Show(errMessage, "Hotkey error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
                        if (!_isSelectingArea)
                            Dispatcher.BeginInvoke(new Action(() => SelectAreaOnScreen(this, null)));
                        break;

                    case START_STOP_TRANSLATION_HOTKEY_ID:
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            if (TlgBtnStartStopTranslation.IsChecked == false)
                                TlgBtnStartStopTranslation.IsChecked = true;
                            else 
                                TlgBtnStartStopTranslation.IsChecked = false;

                            StartStopTranslation(this, null);
                        }));

                        break;

                    case CREATE_DESTROY_OVERLAY_HOTKEY_ID:
                        if (_overlayWindow != null)
                        {
                            Dispatcher.BeginInvoke(new Action(() =>
                            {
                                DestroyOverlayWindow(this, null);
                                TlgBtnOverlayCreateDestroy.IsChecked = false;
                            }));
                        }
                        else
                        {
                            Dispatcher.BeginInvoke(new Action(() => 
                            { 
                                CreateOverlayWindow(this, null);
                                TlgBtnOverlayCreateDestroy.IsChecked = true;
                            }));
                        }

                        break;

                    case LOCK_UNLOCK_OVERLAY_HOTKEY_ID:
                        if (_overlayWindow != null)
                        {
                            if (TlgBtnOverlayLockUnlock.IsChecked == true)
                            {
                                Dispatcher.BeginInvoke(new Action(() =>
                                {
                                    UnlockOverlayWindow(this, null);
                                    TlgBtnOverlayLockUnlock.IsChecked = false;
                                }));
                            }
                            else
                            {
                                Dispatcher.BeginInvoke(new Action(() =>
                                {
                                    LockOverlayWindow(this, null);
                                    TlgBtnOverlayLockUnlock.IsChecked = true;
                                }));
                            }
                        }

                        break;
                }

                handled = true;
            }

            return IntPtr.Zero;
        }

        protected override void OnClosed(EventArgs e)
        {
            _overlayWindow?.Close();

            /*
             * Unbinding hotkeys
             */
            var hwnd = new WindowInteropHelper(this).Handle;
            NativeMethods.UnregisterHotKey(hwnd, SCREEN_CAPTURE_HOTKEY_ID);
            NativeMethods.UnregisterHotKey(hwnd, START_STOP_TRANSLATION_HOTKEY_ID);
            NativeMethods.UnregisterHotKey(hwnd, CREATE_DESTROY_OVERLAY_HOTKEY_ID);
            NativeMethods.UnregisterHotKey(hwnd, LOCK_UNLOCK_OVERLAY_HOTKEY_ID);
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