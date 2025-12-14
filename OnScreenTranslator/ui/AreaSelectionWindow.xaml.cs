using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace OnScreenTranslator.ui
{
    public partial class AreaSelectionWindow : Window
    {
        private Point startPoint;
        private bool isSelecting;
        public Rect SelectedArea { get; private set; }

        public AreaSelectionWindow()
        {
            InitializeComponent();

            MouseLeftButtonDown += OnMouseDown;
            MouseMove += OnMouseMove;
            MouseLeftButtonUp += OnMouseUp;
            KeyDown += OnKeyDown;
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            startPoint = e.GetPosition(this);
            isSelecting = true;

            SelectionRect.Visibility = Visibility.Visible;
            Canvas.SetLeft(SelectionRect, startPoint.X);
            Canvas.SetTop(SelectionRect, startPoint.Y);
            SelectionRect.Width = 0;
            SelectionRect.Height = 0;
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (!isSelecting)
                return;

            Point current = e.GetPosition(this);

            double x = Math.Min(current.X, startPoint.X);
            double y = Math.Min(current.Y, startPoint.Y);
            double w = Math.Abs(current.X - startPoint.X);
            double h = Math.Abs(current.Y - startPoint.Y);

            Canvas.SetLeft(SelectionRect, x);
            Canvas.SetTop(SelectionRect, y);
            SelectionRect.Width = w;
            SelectionRect.Height = h;
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            isSelecting = false;

            double x = Canvas.GetLeft(SelectionRect);
            double y = Canvas.GetTop(SelectionRect);

            SelectedArea = new Rect(
                x,
                y,
                SelectionRect.Width,
                SelectionRect.Height
            );

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
