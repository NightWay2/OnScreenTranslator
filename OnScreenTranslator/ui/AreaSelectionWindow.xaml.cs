using OnScreenTranslator.win32;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace OnScreenTranslator.ui
{
    public partial class AreaSelectionWindow : Window
    {
        private Point _startPoint;
        private bool _isSelecting;
        public Rect SelectedArea { get; private set; }

        public AreaSelectionWindow()
        {
            InitializeComponent();

            InitLogic();
        }

        private void InitLogic()
        {
            Rect screen = NativeMethods.GetCurrentMonitorBounds();

            Left = screen.Left;
            Top = screen.Top;
            Width = screen.Width;
            Height = screen.Height;

            Loaded += (_, __) =>
            {
                Activate();
                Focus();
                Keyboard.Focus(this);
            };

            MouseLeftButtonDown += OnMouseDown;
            MouseMove += OnMouseMove;
            MouseLeftButtonUp += OnMouseUp;
            KeyDown += OnKeyDown;
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(this);
            _isSelecting = true;

            SelectionRect.Visibility = Visibility.Visible;
            Canvas.SetLeft(SelectionRect, _startPoint.X);
            Canvas.SetTop(SelectionRect, _startPoint.Y);
            SelectionRect.Width = 0;
            SelectionRect.Height = 0;
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (!_isSelecting)
                return;

            Point current = e.GetPosition(this);

            double x = Math.Min(current.X, _startPoint.X);
            double y = Math.Min(current.Y, _startPoint.Y);
            double w = Math.Abs(current.X - _startPoint.X);
            double h = Math.Abs(current.Y - _startPoint.Y);

            Canvas.SetLeft(SelectionRect, x);
            Canvas.SetTop(SelectionRect, y);
            SelectionRect.Width = w;
            SelectionRect.Height = h;
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            _isSelecting = false;

            Point p1 = PointToScreen(_startPoint);
            Point p2 = PointToScreen(e.GetPosition(this));

            double x = Math.Min(p1.X, p2.X);
            double y = Math.Min(p1.Y, p2.Y);
            double w = Math.Abs(p1.X - p2.X);
            double h = Math.Abs(p1.Y - p2.Y);

            SelectedArea = new Rect(x, y, w, h);

            DialogResult = true;
            Close();
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                DialogResult = false;
                Close();
            }
        }
    }
}
