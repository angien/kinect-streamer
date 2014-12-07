using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FaceEnrollment
{
    /// <summary>
    /// Interaction logic for Page4.xaml
    /// </summary>
    public partial class Page4 : Page
    {
        private BitmapSource lastFrame;
        public Page4()
        {
            InitializeComponent();
            EnrollmentManager.OnFrameReceived += ReceiveFrame;
        }

        private void ReceiveFrame(BitmapSource frame, IEnumerable<Rect> faceBoxes)
        {
            liveImage.Source = frame;
            lastFrame = frame;
        }

        private void Snap_Click(object sender, RoutedEventArgs e)
        {
            snapshotImage.Source = lastFrame.CloneCurrentValue();
            EnrollmentManager.trainingData[EnrollmentManager.currentTrainingId].trainingImages.Add(BitmapFromSource(lastFrame));
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            EnrollmentManager.currentTrainingId++;
            EnrollmentManager.OnFrameReceived -= ReceiveFrame;
            EnrollmentManager.window.Content = new Page3();
        }

        private void Done_Click(object sender, RoutedEventArgs e)
        {
            EnrollmentManager.OnFrameReceived -= ReceiveFrame;
            EnrollmentManager.Finish();
        }

        private static Bitmap BitmapFromSource(BitmapSource bitmapsource)
        {
            Bitmap bitmap;
            using (var outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapsource));
                enc.Save(outStream);
                bitmap = new Bitmap(outStream);
            }

            return bitmap;
        }

        public static BitmapSource ConvertBitmap(Bitmap source)
        {
            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                          source.GetHbitmap(),
                          IntPtr.Zero,
                          Int32Rect.Empty,
                          BitmapSizeOptions.FromEmptyOptions());
        }

    }
}
