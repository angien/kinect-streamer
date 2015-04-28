using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
    public partial class TrainingPage : Page
    {
        private BitmapSource lastFrame;
        private List<Rect> lastFaceBoxes; // the rectangles of the faces
        private List<BitmapSource> lastFrames;
        private DrawingGroup drawingGroup = new DrawingGroup();
        private static int i;
        private static int j;
        private static int NUMBER_TO_TRAIN = 20;
        private static DateTime otherTime;

        public TrainingPage()
        {
            InitializeComponent();
            //trainDataText.Visibility = System.Windows.Visibility.Hidden;
            EnrollmentManager.OnFrameReceived += ReceiveFrame;
            liveImage.Source = new DrawingImage(drawingGroup);
            lastFaceBoxes = new List<Rect>();
            lastFrames = new List<BitmapSource>();
            i = 0;
            j = 0;
        }

        private void ReceiveFrame(BitmapSource frame, IEnumerable<Rect> faceBoxes)
        {


            Rect faceBox = Rect.Empty;

            PersonTrainingData person = EnrollmentManager.personToTrain;
            

            if (faceBoxes.Count() == 1) // people in the shot, only want one right now
            {
                //Debug.WriteLine("faceboxes COUNT: " + faceBoxes.Count());
                Rect bounds = new Rect(new System.Windows.Size(frame.Width, frame.Height));
                IEnumerable<Rect> filteredFaceBoxes = faceBoxes.Select((box) => Util.TransformFace(box));
                filteredFaceBoxes = filteredFaceBoxes.Where((box) => Util.IsValidRect(box, bounds));
                faceBox = filteredFaceBoxes.FirstOrDefault();

           


                    Bitmap image = Util.SourceToBitmap(frame);
    

                    i++;
         
                    if ((i % 4) == 0)
                    {

                        FaceRecognition.FaceRecognizerBridge.Preview(image, faceBox);
                        MemoryStream ms = new MemoryStream();
                        BitmapImage bi = new BitmapImage();
                        byte[] bytArray = File.ReadAllBytes(EnrollmentManager.filepath+"preview.jpg");
                        ms.Write(bytArray, 0, bytArray.Length); ms.Position = 0;
                        bi.BeginInit();
                        bi.StreamSource = ms;
                        bi.EndInit();
                        snapshotImage.Source = bi;

                        if (person.trainingImages.Count() == NUMBER_TO_TRAIN)
                        {
                            EnrollmentManager.OnFrameReceived -= ReceiveFrame;
                            EnrollmentManager.Finish(false);
                        }
                        person.trainingImages.Insert(j, image);
                        person.faceBoxes.Insert(j, faceBox);
                        j++;

                    }

                    if (i == NUMBER_TO_TRAIN)
                    {
                    
                        i = 0;
                        //EnrollmentManager.OnFrameReceived -= ReceiveFrame;
                        //EnrollmentManager.Finish(false);
                    }
     
               // }
            }
            else if (faceBoxes.Count() > 1) {
                Debug.WriteLine("Too many people in the shot: " + faceBoxes.Count());
            }
               
            // drawing the red boxes
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
        

        private void Done_Click(object sender, RoutedEventArgs e)
        {


            EnrollmentManager.OnFrameReceived -= ReceiveFrame;
            EnrollmentManager.Finish(false);
        }
    }
}
