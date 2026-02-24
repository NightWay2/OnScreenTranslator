using OnScreenTranslator.adapters.ocrs;
using OnScreenTranslator.adapters.translators;
using OnScreenTranslator.services;
using OnScreenTranslator.settings;
using OnScreenTranslator.win32;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;

namespace OnScreenTranslator.ui
{
    // todo mb hide prpogram in tray
    // todo mb add scale option (100%, 125%, 150%)

    // todo add hint text in buttons on hover (long hover)
    // todo add guide how to use + hotkeys

    // todo mb save location of overlay

    // todo add settings:
    // add translation_interval_ms to settings (can`t be 0 or less)
    // add _countdownTime to settings (can`t be less than 0)
    // add overlay transparency in lock mode (0 to 100 %)
    // add overlay font size (mb 12 and other)
    // add overlay font color
    // mb overlay color (locked and unlocked mode)
    // add check box to enable selecting text in overlay (only in unlocked mode)
    // todo save file (prev and below):
    // mb add overlay last overlay
    // langs and translator
    // localization

    // todo add button restore to defaults and aplly settings !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

    // add custom size of text in overlay, custom color of text in overlay, custom alpha of overlay window

    // mb add posibility to translate only one time, and repeatedly

    // docker compose: add only supported languages
    public partial class MainWindow : Window
    {
        private OverlayWindow? _overlayWindow;
        private Rect? _selectedScreenArea;
        private bool _isSelectingArea = false;

        private OcrService? _ocrService = new OcrService(new OneOcrAdapter());
        private TranslationService? _translationService;

        // Translation related vars
        private CancellationTokenSource? _translationCts;
        private bool _isTranslationRunning = false;
        private const int TRANSLATION_INTERVAL_MS = 10000; // min ~1000
        private string _previousText = "";
        private string _previousTranslatedText = "";
        private string _previousSourceLang = "";
        private string _previousTargetLang = "";
        private DispatcherTimer? _countdownTimer;
        private int _secondsRemaining;
        private bool _isHotkeyCall = false;
        private int _countdownTime = 3;

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
        }

        private void CreateOverlayWindow(object sender, RoutedEventArgs e)
        {
            if (_overlayWindow != null)
                return;

            _overlayWindow = new OverlayWindow();
            _overlayWindow.Closed += OverlayWindow_Closed;
            if (_previousTranslatedText != "")
                _overlayWindow.TxtOverlay.Text = _previousTranslatedText;
            else
                _overlayWindow.TxtOverlay.Visibility = Visibility.Hidden;
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
            bool wasTranslationRunning = _isTranslationRunning;

            try
            {
                if (_isTranslationRunning)
                {
                    TlgBtnStartStopTranslation.IsChecked = false;
                    StopTranslationLoop();
                    ResetCountdown();
                }
                AreaSelectionWindow selector = new AreaSelectionWindow();
                if (selector.ShowDialog() == true)
                {
                    _selectedScreenArea = selector.SelectedArea;
                    TlgBtnStartStopTranslation.IsEnabled = true;
                }
                selector.Close();
            }
            finally
            {
                _isSelectingArea = false;
                if (wasTranslationRunning)
                {
                    TlgBtnStartStopTranslation.IsChecked = true;
                    ResetCountdown();
                    StartTranslationLoop();
                }
            }
        }

        /*
         * Translation logic
         */
        private void StartStopTranslation(object sender, RoutedEventArgs e)
        {
            if (_isHotkeyCall) return;

            if (TlgBtnStartStopTranslation.IsChecked == true)
            {
                //StartTranslationLoop();
                StartCountdown(_countdownTime);
            }
            else
            {
                StopTranslationLoop();
                ResetCountdown();
            }
        }

        private void StartCountdown(int seconds)
        {
            ResetCountdown();

            _secondsRemaining = seconds;
            TlgBtnStartStopTranslation.IsEnabled = false;

            _countdownTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _countdownTimer.Tick += (s, e) =>
            {
                _secondsRemaining--;
                if (_secondsRemaining > 0)
                {
                    TlgBtnStartStopTranslation.Content = _secondsRemaining.ToString();
                }
                else
                {
                    ResetCountdown();
                    StartTranslationLoop();
                }
            };

            TlgBtnStartStopTranslation.Content = _secondsRemaining.ToString();
            _countdownTimer.Start();
        }

        private void ResetCountdown()
        {
            _countdownTimer?.Stop();
            _countdownTimer = null;

            TlgBtnStartStopTranslation.IsEnabled = _selectedScreenArea != null;
            TlgBtnStartStopTranslation.ClearValue(ContentProperty);
        }

        // todo mb add default param to check if we call method using hotkey to create notification
        private void StartTranslationLoop()
        {
            // check if translation is running
            if (_isTranslationRunning)
                return;

            // if translation is not running set lock
            _isTranslationRunning = true;

            // cancellation token to cancel translation using button
            _translationCts = new CancellationTokenSource();
            var token = _translationCts.Token;

            // todo add possibility to change translator !!!!!!!!!!!!!!!!!
            _translationService = new TranslationService(
                TranslatorFactory.GetTranslator(
                    Translators.LibreTranslator,
                    "http://localhost:5000"
                )
            );

            // languages that will be used in translator
            string source = ComBoxSourceLang.SelectedValue.ToString();
            string target = ComBoxTargetLang.SelectedValue.ToString();

            Task.Run(async () =>
            {
                try
                {
                    while (!token.IsCancellationRequested)
                    {
                        // if overlay is hidden do not spend resources to translate anything
                        if (_overlayWindow == null)
                        {
                            await Task.Delay(400);
                            continue;
                        }

                        Bitmap image;

                        // check if overlay lays over text area we want to translate
                        bool? intersects = _overlayWindow?.Dispatcher.Invoke(() =>
                            _overlayWindow?.IntersectsScreenArea(_selectedScreenArea.Value)
                        );

                        if (intersects == true)
                        {
                            // hide overlay to not capture it
                            _overlayWindow?.Dispatcher.Invoke(() => _overlayWindow?.Hide());
                            image = ScreenCaptureService.GetImage(_selectedScreenArea.Value);
                            _overlayWindow?.Dispatcher.Invoke(() => _overlayWindow?.Show());
                        }
                        else
                        {
                            image = ScreenCaptureService.GetImage(_selectedScreenArea.Value);
                        }

                        // getting text from image
                        string text = _ocrService.GetTextFromImage(image);
                        image.Dispose();

                        // check if text has to be translated
                        if (!string.IsNullOrEmpty(text) && (!_previousText.Equals(text) ||
                            !_previousSourceLang.Equals(source) || !_previousTargetLang.Equals(target)))
                        {
                            // update previous data
                            _previousText = text;
                            _previousSourceLang = source;
                            _previousTargetLang = target;

                            // translate text if source and target languages are different
                            string translated = source == target ? text : await _translationService.TranslateAsync(
                                text, source, target // todo mb add api
                            );
                            _previousTranslatedText = translated;

                            // update text in overlay
                            Dispatcher.Invoke(() =>
                            {
                                _overlayWindow?.TxtOverlay.Visibility = Visibility.Visible;
                                _overlayWindow?.TxtOverlay.Text = translated;
                            });
                        }

                        await Task.Delay(TRANSLATION_INTERVAL_MS, token);
                    }
                }
                catch (OperationCanceledException)
                {
                    // exit translation loop if stop button was pressed
                    return;
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show(ex.Message, "Translation error", MessageBoxButton.OK, MessageBoxImage.Error);
                    });
                    return;
                }
                finally
                {
                    // release lock
                    _isTranslationRunning = false;
                    Dispatcher.Invoke(() => TlgBtnStartStopTranslation.IsChecked = false);
                }
            }, token);
        }

        private void StopTranslationLoop()
        {
            _translationCts?.Cancel();
            _translationCts = null;
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

        private void LanguageIsChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TlgBtnStartStopTranslation == null) return;

            // todo call setting manager and set new params
            if (TlgBtnStartStopTranslation.IsChecked == true)
            {
                TlgBtnStartStopTranslation.IsChecked = false;
                StopTranslationLoop();
            }
            ResetCountdown();
        }

        // todo mb add sorting for diff langs
        private void LocalizationChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComBoxLocalization.SelectedItem is ComboBoxItem item)
            {
                string langCode = item.Tag.ToString();
                LocalizationManager.GetInstance().SetLanguage(langCode);
            }
        }

        private void SwitchLanguages(object sender, RoutedEventArgs e)
        {
            if (ComBoxSourceLang.SelectedValue.ToString() == "auto") return;

            string temp = ComBoxSourceLang.SelectedValue.ToString();
            ComBoxSourceLang.SelectedValue = ComBoxTargetLang.SelectedValue;
            ComBoxTargetLang.SelectedValue = temp;

            // todo call setting manager and set new params
            if (TlgBtnStartStopTranslation.IsChecked == true)
            {
                TlgBtnStartStopTranslation.IsChecked = false;
                StopTranslationLoop();
            }
            ResetCountdown();
        }

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
        // todo add notifications when hotkey is pressed
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
                            if (!_isSelectingArea && TlgBtnStartStopTranslation.IsEnabled == true)
                            {
                                _isHotkeyCall = true;

                                if (TlgBtnStartStopTranslation.IsChecked == false)
                                {
                                    TlgBtnStartStopTranslation.IsChecked = true;
                                    ResetCountdown();
                                    StartTranslationLoop();
                                }
                                else
                                {
                                    TlgBtnStartStopTranslation.IsChecked = false;
                                    StopTranslationLoop();
                                    ResetCountdown();
                                }

                                _isHotkeyCall = false;
                            }
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
}