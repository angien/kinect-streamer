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

using System.Diagnostics;

namespace FaceEnrollment
{
    /// <summary>
    /// Interaction logic for Page4.xaml
    /// </summary>
    public partial class Page4 : Page
    {
        private BitmapSource lastFrame;
        private List<Rect> lastFaceBoxes;
        private List<BitmapSource> lastFrames;
        private DrawingGroup drawingGroup = new DrawingGroup();
        private static int i;
        private static DateTime otherTime;

        public Page4()
        {
            InitializeComponent();
            EnrollmentManager.OnFrameReceived += ReceiveFrame;
            liveImage.Source = new DrawingImage(drawingGroup);
            lastFaceBoxes = new List<Rect>();
            lastFrames = new List<BitmapSource>();
            i = 0;
        }

        private void ReceiveFrame(BitmapSource frame, IEnumerable<Rect> faceBoxes)
        {
            Rect faceBox = Rect.Empty;
            if (faceBoxes.Count() > 0)
            {
                //Debug.WriteLine("faceboxes COUNT: " + faceBoxes.Count());
                Rect bounds = new Rect(new System.Windows.Size(frame.Width, frame.Height));
                IEnumerable<Rect> filteredFaceBoxes = faceBoxes.Select((box) => Util.TransformFace(box));
                filteredFaceBoxes = filteredFaceBoxes.Where((box) => Util.IsValidRect(box, bounds));
                faceBox = filteredFaceBoxes.FirstOrDefault();
                foreach (Rect rect in filteredFaceBoxes)
                {
                    if (rect.Width > faceBox.Width)
                    {
                        faceBox = rect;
                    }
                }
            }
               
                if (!faceBox.Equals(Rect.Empty))
                {/*
                    DateTime now = DateTime.UtcNow;
                    TimeSpan difference = now.Subtract(otherTime); // could also write `now - otherTime`
                    if (difference.TotalSeconds > 1)
                    {
                        // Debug.WriteLine("facebox: " + faceBox);
                        otherTime = now;*/
                        //Debug.WriteLine("SOMETHING IS HAPPENING HERE" + lastFaceBoxes.Count());
                        ((List<Rect>)lastFaceBoxes).Insert(i, faceBox);
                        ((List<BitmapSource>)lastFrames).Insert(i, frame);
                        i = (i + 1) % 10;
                    //}
                }

                DrawingVisual drawingVisual = new DrawingVisual();
                using (DrawingContext drawingContext = drawingGroup.Open())
                {
                    drawingContext.DrawImage(frame, new Rect(new System.Windows.Size(frame.Width, frame.Height)));
                    var brush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red);
                    var pen = new System.Windows.Media.Pen(brush, 5);
                    drawingContext.DrawRectangle(null, pen, faceBox);
                }

                lastFrame = frame;
            
            
        }

        private void Snap_Click(object sender, RoutedEventArgs e)
        {


            if (lastFaceBoxes.Count() < 10)
            {
                output.Content = "No faces found";
            }
            else
            {
                for (int j = 0; j < 10; j++ )
                {

                    Debug.WriteLine("SSNAPCLICK" + lastFaceBoxes.Count());
                    Rect faceBox = lastFaceBoxes[j];
                    PersonTrainingData person = EnrollmentManager.trainingData[EnrollmentManager.currentTrainingId];
                    Debug.WriteLine("lastframe" + lastFrames[j]);
                    Bitmap image = Util.SourceToBitmap(lastFrames[j]);
                    person.trainingImages.Add(image);
                    person.faceBoxes.Add(faceBox);
                    output.Content = "Success";

                    FaceRecognition.FaceRecognizerBridge.Preview(image, faceBox);
                    MemoryStream ms = new MemoryStream();
                    BitmapImage bi = new BitmapImage();
                    byte[] bytArray = File.ReadAllBytes(@"C:/Test/preview.jpg");
                    ms.Write(bytArray, 0, bytArray.Length); ms.Position = 0;
                    bi.BeginInit();
                    bi.StreamSource = ms;
                    bi.EndInit();
                    snapshotImage.Source = bi;
                }
               
            }
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            EnrollmentManager.currentTrainingId++;
            EnrollmentManager.actualTrainingId++;
            EnrollmentManager.OnFrameReceived -= ReceiveFrame;
            EnrollmentManager.window.Content = new Page3();
        }

        private void Done_Click(object sender, RoutedEventArgs e)
        {
            EnrollmentManager.OnFrameReceived -= ReceiveFrame;
            EnrollmentManager.Finish(false);
        }
    }
}
