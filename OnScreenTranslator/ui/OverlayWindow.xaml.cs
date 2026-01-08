using OnScreenTranslator.win32;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace OnScreenTranslator.ui
{
    public partial class OverlayWindow : Window
    {
        // WinAPI constants
        private const int GWL_EXSTYLE = -20;
        private const long WS_EX_TRANSPARENT = 0x00000020L;
        private const long WS_EX_LAYERED = 0x00080000L;
        private const long WS_EX_TOOLWINDOW = 0x00000080L;

        private IntPtr hwnd;

        private bool isLocked = false;

        private int textSize = 12;
        private int transparencyPercentage = 20;

        public OverlayWindow()
        {
            InitializeComponent();

            Loaded += OverlayWindow_Loaded;
            MouseLeftButtonDown += (s, e) => DragMove();

            TxtOverlay.FontSize = textSize;
        }

        private void OverlayWindow_Loaded(object sender, RoutedEventArgs e)
        {
            hwnd = new WindowInteropHelper(this).Handle;

            SetUnlockedVisuals();
        }

        public void EnableLockMode()
        {
            if (isLocked) return;

            long ex = NativeMethods.GetWindowLongPtr64(hwnd, GWL_EXSTYLE).ToInt64();
            ex |= WS_EX_LAYERED | WS_EX_TRANSPARENT | WS_EX_TOOLWINDOW;
            NativeMethods.SetWindowLongPtr64(hwnd, GWL_EXSTYLE, new IntPtr(ex));

            RootGrid.IsHitTestVisible = false;

            byte transparency = (byte) (transparencyPercentage * 255 / 100);

            MainBorder.Background = new SolidColorBrush(Color.FromArgb(transparency, 0, 0, 0));
            ResizeMode = ResizeMode.NoResize;

            isLocked = true;
        }

        public void DisableLockMode()
        {
            if (!isLocked) return;

            long ex = NativeMethods.GetWindowLongPtr64(hwnd, GWL_EXSTYLE).ToInt64();
            ex &= ~WS_EX_TRANSPARENT;
            NativeMethods.SetWindowLongPtr64(hwnd, GWL_EXSTYLE, new IntPtr(ex));

            RootGrid.IsHitTestVisible = true;

            MainBorder.Background = new SolidColorBrush(Color.FromArgb(170, 0, 0, 0));
            ResizeMode = ResizeMode.CanResizeWithGrip;

            isLocked = false;
        }

        private void SetUnlockedVisuals()
        {
            RootGrid.IsHitTestVisible = true;
            MainBorder.Background = new SolidColorBrush(Color.FromArgb(170, 0, 0, 0));

            long ex = NativeMethods.GetWindowLongPtr64(hwnd, GWL_EXSTYLE).ToInt64();
            ex &= ~WS_EX_TRANSPARENT;
            NativeMethods.SetWindowLongPtr64(hwnd, GWL_EXSTYLE, new IntPtr(ex));
            isLocked = false;
        }
    }
}
