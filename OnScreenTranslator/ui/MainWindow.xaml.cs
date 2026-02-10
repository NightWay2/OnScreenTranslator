using OnScreenTranslator.adapters.ocrs;
using OnScreenTranslator.adapters.translators;
using OnScreenTranslator.services;
using OnScreenTranslator.settings;
using OnScreenTranslator.win32;
using System.Drawing;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;

namespace OnScreenTranslator.ui
{
    // todo hide prpogram in tray
    // todo add hide button (mb custom buttons for exit and hide)
    // todo add scale option (100%, 125%, 150%)
    // todo add localization
    // add posibility use in ocr mode
    // mb add possibility to copy text in unlocked mode
    public partial class MainWindow : Window
    {
        private OverlayWindow? _overlayWindow;
        private Rect? _selectedScreenArea;
        private bool _isSelectingArea = false;

        private OcrService? _ocrService;
        private TranslationService? _translationService;

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

            SettingsManager.GetInstance().Init(this);

            //MessageBox.Show(ComBoxSourceLang.SelectedItem.ToString());
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
                    TlgBtnStartStopTranslation.IsEnabled = true;
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

            // todo
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

            string source = ComBoxSourceLang.SelectedItem.ToString();
            string target = ComBoxTargetLang.SelectedItem.ToString();

            Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    if (_overlayWindow == null) continue;

                    try
                    {
                        Bitmap image;

                        bool? intersects = _overlayWindow?.Dispatcher.Invoke(() =>
                            _overlayWindow?.IntersectsScreenArea(_selectedScreenArea.Value)
                        );

                        if (intersects.HasValue == true)
                        {
                            /*_overlayWindow?.Dispatcher.Invoke(() => _overlayWindow?.Hide());
                            image = ScreenCaptureService.GetImage(_selectedScreenArea.Value);
                            _overlayWindow?.Dispatcher.Invoke(() => _overlayWindow?.Show());*/

                            _overlayWindow?.Dispatcher.Invoke(() => _overlayWindow?.Opacity = 0);
                            image = ScreenCaptureService.GetImage(_selectedScreenArea.Value);
                            _overlayWindow?.Dispatcher.Invoke(() => _overlayWindow?.Opacity = 1);
                        }
                        else
                        {
                            image = ScreenCaptureService.GetImage(_selectedScreenArea.Value);
                        }

                        // optional
                        // image = ImagePreprocessor.Process(image);

                        string text = _ocrService.GetTextFromImage(image);

                        if (source.Equals(target))
                        {
                            _previousText = text;
                            _overlayWindow?.Dispatcher.Invoke(() => _overlayWindow?.TxtOverlay.Text = text);
                        }

                        if (!string.IsNullOrEmpty(text) && !_previousText.Equals(text))
                        {
                            _previousText = text;

                            string translated = await _translationService.TranslateAsync(
                                text, source, target
                            );

                            _overlayWindow?.Dispatcher.Invoke(() => _overlayWindow?.TxtOverlay.Text = translated);
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

        private void LanguageIsChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // call setting manager and set new params
            if (TlgBtnStartStopTranslation.IsChecked == true)
            {
                TlgBtnStartStopTranslation.IsChecked = false;
                StopTranslationLoop();
            }
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

            RegisterHotKey(hwnd, SCREEN_CAPTURE_HOTKEY_ID, MOD_ALT, SCREEN_CAPTURE_KEY,
                "Screen Capture Hotkey is not registered");
            RegisterHotKey(hwnd, START_STOP_TRANSLATION_HOTKEY_ID, MOD_ALT, START_STOP_TRANSLATION_KEY,
                "Start/Stop Translation Hotkey is not registered");
            RegisterHotKey(hwnd, CREATE_DESTROY_OVERLAY_HOTKEY_ID, MOD_ALT, CREATE_DESTROY_OVERLAY_KEY,
                "Create/Destroy Overlay Hotkey is not registered");
            RegisterHotKey(hwnd, LOCK_UNLOCK_OVERLAY_HOTKEY_ID, MOD_ALT, LOCK_UNLOCK_OVERLAY_KEY,
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

            // todo call setings manager to rewrite config file with new data

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