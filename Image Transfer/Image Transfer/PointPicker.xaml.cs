using System;
using System.Windows;
using System.Windows.Input;

namespace Image_Transfer
{
    public partial class PointPicker : Window
    {
        public System.Drawing.Point StartingPoint;
        private string MousePosition { get { return String.Format("    Mouse Position: {0}, {1}\n", System.Windows.Forms.Control.MousePosition.X, System.Windows.Forms.Control.MousePosition.Y); } }
        private string StartingPosition { get { return String.Format("    Starting Point: {0}, {1}", StartingPoint.X, StartingPoint.Y); } }

        public PointPicker()
        {
            InitializeComponent();
            lblMousePosition.Content = MousePosition;
        }

        //get the mouse cursor's current x and y coordinates while moving the mouse around
        private void GetCursorPosition(object sender, MouseEventArgs e)
        {
            if (StartingPoint.IsEmpty)
                lblMousePosition.Content = MousePosition;
            else
                lblMousePosition.Content = MousePosition + StartingPosition;
        }

        //when the user clicks set the starting position to the point they clicked on
        private void SetStartingPoint(object sender, MouseButtonEventArgs e)
        {
            StartingPoint = new System.Drawing.Point();
            StartingPoint.X = System.Windows.Forms.Control.MousePosition.X;
            StartingPoint.Y = System.Windows.Forms.Control.MousePosition.Y;
            lblMousePosition.Content = MousePosition + StartingPosition;
        }
    }
}
