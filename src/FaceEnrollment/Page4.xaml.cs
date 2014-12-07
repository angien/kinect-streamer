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
using System.Drawing.Imaging;

namespace FaceEnrollment
{
    /// <summary>
    /// Interaction logic for Page4.xaml
    /// </summary>
    public partial class Page4 : Page
    {
        private BitmapSource lastFrame;
        private IEnumerable<Rect> lastFaceBoxes;
        private DrawingGroup drawingGroup = new DrawingGroup();

        public Page4()
        {
            InitializeComponent();
            EnrollmentManager.OnFrameReceived += ReceiveFrame;
            liveImage.Source = new DrawingImage(drawingGroup);
        }

        private void ReceiveFrame(BitmapSource frame, IEnumerable<Rect> faceBoxes)
        {
            DrawingVisual drawingVisual = new DrawingVisual();
            using (DrawingContext drawingContext = drawingGroup.Open())
            {
                drawingContext.DrawImage(frame, new Rect(new System.Windows.Size(frame.Width, frame.Height)));
                var brush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red);
                var pen = new System.Windows.Media.Pen(brush, 5);
                foreach (Rect faceBox in faceBoxes)
                {
                    drawingContext.DrawRectangle(null, pen, TransformFace(faceBox));
                }
                drawingContext.DrawRectangle(null, pen, new Rect(0, 0, 100, 100));
            }

            lastFrame = frame;
            lastFaceBoxes = faceBoxes;
        }

        private void Snap_Click(object sender, RoutedEventArgs e)
        {
            if (lastFaceBoxes.Count() < 1)
            {
                output.Content = "No faces found";
            }
            else if (lastFaceBoxes.Count() > 1)
            {
                output.Content = "Too many faces found: " + lastFaceBoxes.Count().ToString();
            }
            else
            {
                snapshotImage.Source = lastFrame.CloneCurrentValue();
                PersonTrainingData person = EnrollmentManager.trainingData[EnrollmentManager.currentTrainingId];
                person.trainingImages.Add(Util.SourceToBitmap(lastFrame));
                Rect faceBox = TransformFace(lastFaceBoxes.First());
                person.faceBoxes.Add(faceBox);
                output.Content = "Success";
            }
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

        private Rect TransformFace(Rect faceBox)
        {
            return new Rect(faceBox.Left - 0.5 * (faceBox.Right - faceBox.Left),
                    faceBox.Top - 1 * (faceBox.Bottom - faceBox.Top),
                    2 * (faceBox.Right - faceBox.Left),
                    2.5 * (faceBox.Bottom - faceBox.Top));
        }
    }
}
