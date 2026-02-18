using OnScreenTranslator.win32;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace OnScreenTranslator.ui
{
    public partial class OverlayWindow : Window
    {
        /*
         * WinAPI constants
         */
        private const int GWL_EXSTYLE = -20;
        private const long WS_EX_TRANSPARENT = 0x00000020L;
        private const long WS_EX_LAYERED = 0x00080000L;
        private const long WS_EX_TOOLWINDOW = 0x00000080L;

        private IntPtr _hwnd;

        private bool _isLocked = false;

        private int _textSize = 12;
        private int _transparencyPercentage = 20;

        public OverlayWindow()
        {
            InitializeComponent();

            InitLogic();
        }

        private void InitLogic()
        {
            Rect screen = NativeMethods.GetCurrentMonitorBounds();

            Left = screen.Left;
            Top = screen.Top;

            Loaded += OverlayWindow_Loaded;
            MouseLeftButtonDown += (s, e) => DragMove();

            TxtOverlay.FontSize = _textSize;
        }

        private void OverlayWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _hwnd = new WindowInteropHelper(this).Handle;

            SetUnlockedVisuals();
        }

        public void EnableLockMode()
        {
            if (_isLocked) return;

            long ex = NativeMethods.GetWindowLongPtr64(_hwnd, GWL_EXSTYLE).ToInt64();
            ex |= WS_EX_LAYERED | WS_EX_TRANSPARENT | WS_EX_TOOLWINDOW;
            NativeMethods.SetWindowLongPtr64(_hwnd, GWL_EXSTYLE, new IntPtr(ex));

            RootGrid.IsHitTestVisible = false;

            byte transparency = (byte) (_transparencyPercentage * 255 / 100);

            MainBorder.Background = new SolidColorBrush(Color.FromArgb(transparency, 0, 0, 0));
            ResizeMode = ResizeMode.NoResize;

            BtnOverlayClose.Visibility = Visibility.Hidden;

            _isLocked = true;
        }

        public void DisableLockMode()
        {
            if (!_isLocked) return;

            long ex = NativeMethods.GetWindowLongPtr64(_hwnd, GWL_EXSTYLE).ToInt64();
            ex &= ~WS_EX_TRANSPARENT;
            NativeMethods.SetWindowLongPtr64(_hwnd, GWL_EXSTYLE, new IntPtr(ex));

            RootGrid.IsHitTestVisible = true;

            MainBorder.Background = new SolidColorBrush(Color.FromArgb(170, 0, 0, 0));
            ResizeMode = ResizeMode.CanResizeWithGrip;

            BtnOverlayClose.Visibility = Visibility.Visible;

            _isLocked = false;
        }

        private void SetUnlockedVisuals()
        {
            RootGrid.IsHitTestVisible = true;
            MainBorder.Background = new SolidColorBrush(Color.FromArgb(170, 0, 0, 0));

            long ex = NativeMethods.GetWindowLongPtr64(_hwnd, GWL_EXSTYLE).ToInt64();
            ex &= ~WS_EX_TRANSPARENT;
            NativeMethods.SetWindowLongPtr64(_hwnd, GWL_EXSTYLE, new IntPtr(ex));
            _isLocked = false;
        }

        public bool IntersectsScreenArea(Rect screenArea)
        {
            PresentationSource source = PresentationSource.FromVisual(this);
            if (source?.CompositionTarget == null)
                return false;

            Matrix transformToDevice = source.CompositionTarget.TransformToDevice;

            double dpiX = transformToDevice.M11;
            double dpiY = transformToDevice.M22;

            Rect dipRect = RestoreBounds;

            Rect pixelRect = new Rect(
                dipRect.X * dpiX,
                dipRect.Y * dpiY,
                dipRect.Width * dpiX,
                dipRect.Height * dpiY
            );

            return pixelRect.IntersectsWith(screenArea);
        }

        private void CloseOverlay(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
