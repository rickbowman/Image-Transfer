using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Image_Transfer
{
    public partial class MainWindow : Window
    {
        //used to focus mspaint
        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        //used to move the mouse cursor around programatically
        [DllImport("user32.dll")]
        public static extern long SetCursorPos(int x, int y);

        //used to simulate mouse clicks
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);
        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;

        //only used to display in the stack panel
        public BitmapImage imagePreview { get; set; }
        //used for analysis
        public Bitmap image { get; set; }
        public List<System.Drawing.Point> WhitePixels;
        public List<System.Drawing.Point> BlackPixels;
        public PointPicker picker;

        public MainWindow()
        {
            InitializeComponent();
            picker = new PointPicker();
            picker.Show();
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            //open a file dialog to upload an image
            OpenFileDialog op = new OpenFileDialog();
            op.Title = "Select a picture";
            op.Filter = "All supported graphics|*.jpg;*.jpeg;*.png|" +
              "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
              "Portable Network Graphic (*.png)|*.png";
            if (op.ShowDialog() == true)
            {
                //save the selected image and display it in the UI stack panel
                imagePreview = new BitmapImage(new Uri(op.FileName));
                image = new Bitmap(op.FileName);
                ImageBrush ib = new ImageBrush();
                ib.ImageSource = imagePreview;
                splPreview.Background = ib;
            }
            lblImageSize.Content = String.Empty;
            lblWhitePixels.Content = String.Empty;
            lblBlackPixels.Content = String.Empty;
            lblDrawingTime.Content = String.Empty;
        }

        private void btnAnalyze_Click(object sender, RoutedEventArgs e)
        {
            if (image != null)
            {
                //initialize black/white point lists
                WhitePixels = new List<System.Drawing.Point>();
                BlackPixels = new List<System.Drawing.Point>();
                //loop through the uploaded images points and check what color it is, add to appropriate collection
                for (int x = 0; x < image.Width; x++)
                {
                    for (int y = 0; y < image.Height; y++)
                    {
                        System.Drawing.Point tempPoint = new System.Drawing.Point(x, y);
                        System.Drawing.Color clr = image.GetPixel(x, y);
                        if (clr.Name == "ffffffff")
                            WhitePixels.Add(tempPoint);
                        else
                            BlackPixels.Add(tempPoint);
                    }
                }
                //display image properties on user interface
                lblImageSize.Content = String.Format("{0}w x {1}h", (int)imagePreview.Width, (int)imagePreview.Height);
                lblWhitePixels.Content = WhitePixels.Count;
                lblBlackPixels.Content = BlackPixels.Count;
                lblDrawingTime.Content = String.Empty;
            }
        }

        private void btnDraw_Click(object sender, RoutedEventArgs e)
        {
            if (!picker.StartingPoint.IsEmpty)
            {
                FocusPaint();
                Stopwatch sw = new Stopwatch();
                sw.Start();
                //loop through all the points in the blackpixel collection
                foreach (System.Drawing.Point point in BlackPixels)
                {
                    //move the mouse cursor
                    SetCursorPos((picker.StartingPoint.X + point.X), (picker.StartingPoint.Y + point.Y));
                    //simulate left mouse down and mouse up
                    mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, (uint)(picker.StartingPoint.X + point.X), (uint)(picker.StartingPoint.Y + point.Y), 0, 0);
                    //pause for 75 milliseconds
                    Thread.Sleep(75);
                }
                sw.Stop();
                //display how long it took to draw the uploaded image on the user interface
                lblDrawingTime.Content = String.Format("{0}h {1}m {2}s", sw.Elapsed.Hours, sw.Elapsed.Minutes, sw.Elapsed.Seconds);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            picker.Close();
        }

        //check to see if mspaint is running
        public static IntPtr WinGetHandle()
        {
            foreach (Process pList in Process.GetProcesses())
                if (pList.ProcessName == "mspaint")
                    return pList.MainWindowHandle;

            return IntPtr.Zero;
        }

        //if paint is running focus it
        private bool FocusPaint()
        {
            IntPtr iPtrHwnd;
            iPtrHwnd = WinGetHandle();

            if (iPtrHwnd == IntPtr.Zero)
            {
                MessageBox.Show("Paint doesn't appear to be running.", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;
            }

            SetForegroundWindow(iPtrHwnd);
            return true;
        }
    }
}
