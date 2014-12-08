//------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.FaceBasics
{
    using System;
    using System.Threading;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Media.Media3D;
    using Microsoft.Kinect;
    using Microsoft.Kinect.Face;
    using HeyYou.EyeTracking;
    using TextToSpeech;

    using System.Windows.Shapes;
    using System.Windows.Controls;
    using FaceEnrollment;
    using FaceRecognition;
    using System.Drawing;

    using Point = System.Windows.Point;
    using PointF = Microsoft.Kinect.PointF;
    using Brush = System.Windows.Media.Brush;
    using Brushes = System.Windows.Media.Brushes;
    using Pen = System.Windows.Media.Pen;

    /// <summary>
    /// Interaction logic for MainWindow
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        //set every new color frame. where the person is looking
        private double gazeX, gazeY;

        private FaceRecognizerBridge faceRecognizer = new FaceRecognizerBridge();
        private Dictionary<int, string> labelToName = new Dictionary<int, string>();
        private FaceRecognitionResult[] faceToResult;

        //stores the buttons in an array
        private Button[] buttons = new Button[3];

        private SpeechOutput speaker;
        private bool isConversationScreen = false;
        private bool isVideoFeed = true;

        /// <summary>
        /// Thickness of face bounding box and face points
        /// </summary>
        private const double DrawFaceShapeThickness = 8;

        /// <summary>
        /// Font size of face property text 
        /// </summary>
        private const double DrawTextFontSize = 30;

        /// <summary>
        /// Radius of face point circle
        /// </summary>
        private const double FacePointRadius = 1.0;

        /// <summary>
        /// Text layout offset in X axis
        /// </summary>
        private const float TextLayoutOffsetX = -0.1f;

        /// <summary>
        /// Text layout offset in Y axis
        /// </summary>
        private const float TextLayoutOffsetY = -0.15f;

        /// <summary>
        /// Face rotation display angle increment in degrees
        /// </summary>
        private const double FaceRotationIncrementInDegrees = 5.0;

        /// <summary>
        /// Formatted text to indicate that there are no bodies/faces tracked in the FOV
        /// </summary>
        /*private FormattedText textFaceNotTracked = new FormattedText(
                        "No bodies or faces are tracked ...",
                        CultureInfo.GetCultureInfo("en-us"),
                        FlowDirection.LeftToRight,
                        new Typeface("Georgia"),
                        DrawTextFontSize,
                        Brushes.White);
        */

        /// <summary>
        /// Text layout for the no face tracked message
        /// </summary>
        private Point textLayoutFaceNotTracked = new Point(3.0, 3.0);

        /// <summary>
        /// Drawing group for body rendering output
        /// </summary>
        private DrawingGroup drawingGroup;
        private DrawingGroup gazeDrawingGroup;

        /// <summary>
        /// Drawing image that we will display
        /// </summary>
        private DrawingImage imageSource;
        private DrawingImage gazeSource;


        /// <summary>
        /// Active Kinect sensor
        /// </summary>
        private KinectSensor kinectSensor = null;

        /// <summary>
        /// Coordinate mapper to map one type of point to another
        /// </summary>
        private CoordinateMapper coordinateMapper = null;

        /// <summary>
        /// Reader for body frames
        /// </summary>
        private BodyFrameReader bodyFrameReader = null;

        /// <summary>
        /// Array to store bodies
        /// </summary>
        private Body[] bodies = null;

        /// <summary>
        /// Number of bodies tracked
        /// </summary>
        private int bodyCount;

        /// <summary>
        /// Face frame sources
        /// </summary>
        private FaceFrameSource[] faceFrameSources = null;

        /// <summary>
        /// Face frame readers
        /// </summary>
        private FaceFrameReader[] faceFrameReaders = null;

        /// <summary>
        /// Storage for face frame results
        /// </summary>
        private FaceFrameResult[] faceFrameResults = null;

        private ColorFrameReader colorFrameReader = null;
        private WriteableBitmap colorBitmap = null;

        /// <summary>
        /// Width of display (color space)
        /// </summary>
        private int displayWidth;

        /// <summary>
        /// Height of display (color space)
        /// </summary>
        private int displayHeight;

        /// <summary>
        /// Display rectangle
        /// </summary>
        private Rect displayRect;

        /// <summary>
        /// List of brushes for each face tracked
        /// </summary>
        private List<Brush> faceBrush;

        /// <summary>
        /// Current status text to display
        /// </summary>
        private string statusText = null;

        private EyeTrackingWindow eyeTracker;
        static public List<List<String>> profiles;
        

        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            //profiles object
            profiles = new List<List<String>>();
            //creates the database
            ReadWriteCsv.CsvFileReader read = new ReadWriteCsv.CsvFileReader("dbcsv.csv");
            int count = 0;
            while (true)
            {
                ReadWriteCsv.CsvRow row = new ReadWriteCsv.CsvRow();
                if (!read.ReadRow(row))
                {
                    break;
                }

                profiles.Add(new List<String>());
                foreach (String value in row)
                {
                    profiles[count].Add(value);
                }
                count++;
            } //end creation of the database

            this.speaker = new SpeechOutput();

            System.Diagnostics.Debug.Write("MainWindow\n\n");
            // one sensor is currently supported
            this.kinectSensor = KinectSensor.GetDefault();

            // get the coordinate mapper
            this.coordinateMapper = this.kinectSensor.CoordinateMapper;

            // get the color frame details
            FrameDescription frameDescription = this.kinectSensor.ColorFrameSource.FrameDescription;
            FrameDescription colorFrameDescription = this.kinectSensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Bgra);

            

            // set the display specifics
            this.displayWidth = frameDescription.Width;
            this.displayHeight = frameDescription.Height;
            this.displayRect = new Rect(0.0, 0.0, this.displayWidth, this.displayHeight);

            //transform the resulting image to so the view lines up with yours
            /*Matrix myMat2 = new Matrix(-1, 0, 0, 1, 0, 0);
            MatrixTransform matMirrorImage2 = new MatrixTransform(myMat2);
            this.drawingGroup.Transform = matMirrorImage2;
             * */

            this.colorBitmap = new WriteableBitmap(colorFrameDescription.Width, colorFrameDescription.Height, 96.0, 96.0, PixelFormats.Bgr32, null);

            // open the reader for the body frames
            this.bodyFrameReader = this.kinectSensor.BodyFrameSource.OpenReader();
            this.colorFrameReader = this.kinectSensor.ColorFrameSource.OpenReader();

            // wire handler for body frame arrival and color
            this.bodyFrameReader.FrameArrived += this.Reader_BodyFrameArrived;
            this.colorFrameReader.FrameArrived += this.Reader_ColorFrameArrived;

            // set the maximum number of bodies that would be tracked by Kinect
            this.bodyCount = this.kinectSensor.BodyFrameSource.BodyCount;

            // allocate storage to store body objects
            this.bodies = new Body[this.bodyCount];

            // specify the required face frame results
 /*           FaceFrameFeatures faceFrameFeatures =
                FaceFrameFeatures.BoundingBoxInColorSpace
                | FaceFrameFeatures.PointsInColorSpace
                | FaceFrameFeatures.RotationOrientation
                | FaceFrameFeatures.FaceEngagement
                | FaceFrameFeatures.Glasses
                | FaceFrameFeatures.Happy
                | FaceFrameFeatures.LeftEyeClosed
                | FaceFrameFeatures.RightEyeClosed
                | FaceFrameFeatures.LookingAway
                | FaceFrameFeatures.MouthMoved
                | FaceFrameFeatures.MouthOpen;
*/
            FaceFrameFeatures faceFrameFeatures = FaceFrameFeatures.BoundingBoxInColorSpace
                | FaceFrameFeatures.PointsInColorSpace;


            // create a face frame source + reader to track each face in the FOV
            this.faceFrameSources = new FaceFrameSource[this.bodyCount];
            this.faceFrameReaders = new FaceFrameReader[this.bodyCount];
            for (int i = 0; i < this.bodyCount; i++)
            {
                // create the face frame source with the required face frame features and an initial tracking Id of 0
                this.faceFrameSources[i] = new FaceFrameSource(this.kinectSensor, 0, faceFrameFeatures);

                // open the corresponding reader
                this.faceFrameReaders[i] = this.faceFrameSources[i].OpenReader();
            }

            // allocate storage to store face frame results for each face in the FOV
            this.faceFrameResults = new FaceFrameResult[this.bodyCount];
            this.faceToResult = new FaceRecognitionResult[this.bodyCount];

            // populate face result colors - one for each face index
            this.faceBrush = new List<Brush>()
            {
                Brushes.White, 
                Brushes.Orange,
                Brushes.Green,
                Brushes.Red,
                Brushes.LightBlue,
                Brushes.Yellow
            };

            // set IsAvailableChanged event notifier
            this.kinectSensor.IsAvailableChanged += this.Sensor_IsAvailableChanged;

            // open the sensor
            this.kinectSensor.Open();

            // set the status text
            this.StatusText = this.kinectSensor.IsAvailable ? Properties.Resources.RunningStatusText
                                                            : Properties.Resources.NoSensorStatusText;

            // Create the drawing group we'll use for drawing the video feed
            this.drawingGroup = new DrawingGroup();
            this.gazeDrawingGroup = new DrawingGroup();
            //DrawingImage drawingVideoFrame = new DrawingImage();

            /*
                        DrawingGroup myDrawingGroup = new DrawingGroup();
                        DrawingImage myDrawingImage = new DrawingImage();
                        myDrawingGroup.Children.Add(myDrawingImage);

                        this.drawingGroup.Children.Add(myDrawingImage);
                        */

            //transform the resulting image to so the view lines up with yours
            Matrix myMat = new Matrix(-1, 0, 0, 1, 0, 0);
            MatrixTransform matMirrorImage = new MatrixTransform(myMat);
            this.drawingGroup.Transform = matMirrorImage;


            // Create an image source that we can use in our image control
            this.imageSource = new DrawingImage(this.drawingGroup);
            this.gazeSource = new DrawingImage(this.gazeDrawingGroup);

            // use the window object as the view model in this simple example
            this.DataContext = this;

            // initialize the components (controls) of the window
            this.InitializeComponent();

            System.Diagnostics.Debug.Write("Is this looping?\n\n");

            this.eyeTracker = new EyeTrackingWindow();
            this.eyeTracker.Show();

            conversationScreen.Visibility = System.Windows.Visibility.Collapsed;

            //populate the buttons array with the buttons
            buttons[0] = Phrase1;
            buttons[1] = Phrase2;
            buttons[2] = Phrase3;
            
            
        }

        /// <summary>
        /// INotifyPropertyChangedPropertyChanged event to allow window controls to bind to changeable data
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets the bitmap to display
        /// </summary>
        public ImageSource ImageSource
        {
            get
            {
                //return this.colorBitmap;
                return this.imageSource;
            }
        }

        public ImageSource GazeSource
        {
            get
            {
                //return this.colorBitmap;
                return this.gazeSource;
            }
        }

        /// <summary>
        /// Gets or sets the current status text to display
        /// </summary>
        public string StatusText
        {
            get
            {
                return this.statusText;
            }

            set
            {
                if (this.statusText != value)
                {
                    this.statusText = value;

                    // notify any bound elements that the text has changed
                    if (this.PropertyChanged != null)
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs("StatusText"));
                    }
                }
            }
        }

        /// <summary>
        /// Converts rotation quaternion to Euler angles 
        /// And then maps them to a specified range of values to control the refresh rate
        /// </summary>
        /// <param name="rotQuaternion">face rotation quaternion</param>
        /// <param name="pitch">rotation about the X-axis</param>
        /// <param name="yaw">rotation about the Y-axis</param>
        /// <param name="roll">rotation about the Z-axis</param>
        private static void ExtractFaceRotationInDegrees(Vector4 rotQuaternion, out int pitch, out int yaw, out int roll)
        {
            double x = rotQuaternion.X;
            double y = rotQuaternion.Y;
            double z = rotQuaternion.Z;
            double w = rotQuaternion.W;

            // convert face rotation quaternion to Euler angles in degrees
            double yawD, pitchD, rollD;
            pitchD = Math.Atan2(2 * ((y * z) + (w * x)), (w * w) - (x * x) - (y * y) + (z * z)) / Math.PI * 180.0;
            yawD = Math.Asin(2 * ((w * y) - (x * z))) / Math.PI * 180.0;
            rollD = Math.Atan2(2 * ((x * y) + (w * z)), (w * w) + (x * x) - (y * y) - (z * z)) / Math.PI * 180.0;

            // clamp the values to a multiple of the specified increment to control the refresh rate
            double increment = FaceRotationIncrementInDegrees;
            pitch = (int)(Math.Floor((pitchD + ((increment / 2.0) * (pitchD > 0 ? 1.0 : -1.0))) / increment) * increment);
            yaw = (int)(Math.Floor((yawD + ((increment / 2.0) * (yawD > 0 ? 1.0 : -1.0))) / increment) * increment);
            roll = (int)(Math.Floor((rollD + ((increment / 2.0) * (rollD > 0 ? 1.0 : -1.0))) / increment) * increment);
        }

        /// <summary>
        /// Execute start up tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            EnrollmentManager.Launch(this);
            EnrollmentManager.Done += EnrollmentManager_Done;

            for (int i = 0; i < this.bodyCount; i++)
            {
                if (this.faceFrameReaders[i] != null)
                {
                    // wire handler for face frame arrival
                    this.faceFrameReaders[i].FrameArrived += this.Reader_FaceFrameArrived;
                }
            }

            if (this.bodyFrameReader != null)
            {
                // wire handler for body frame arrival
                this.bodyFrameReader.FrameArrived += this.Reader_BodyFrameArrived;
            }
        }

        void EnrollmentManager_Done()
        {
            List<Bitmap> images = new List<Bitmap>();
            List<int> ids = new List<int>();
            List<Rect> faceCrops = new List<Rect>();
            foreach (PersonTrainingData data in EnrollmentManager.trainingData)
            {
                images.AddRange(data.trainingImages);
                faceCrops.AddRange(data.faceBoxes);
                foreach (var image in data.trainingImages)
                {
                    ids.Add(data.trainingId);
                }
                labelToName[data.trainingId] = data.name;
            }

            if (images.Count() > 1)
            {
                faceRecognizer.Train(images.ToArray(), ids.ToArray(), faceCrops.ToArray());
            }
        }

        /// <summary>
        /// Execute shutdown tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            for (int i = 0; i < this.bodyCount; i++)
            {
                if (this.faceFrameReaders[i] != null)
                {
                    // FaceFrameReader is IDisposable
                    this.faceFrameReaders[i].Dispose();
                    this.faceFrameReaders[i] = null;
                }

                if (this.faceFrameSources[i] != null)
                {
                    // FaceFrameSource is IDisposable
                    this.faceFrameSources[i].Dispose();
                    this.faceFrameSources[i] = null;
                }
            }

            if (this.bodyFrameReader != null)
            {
                // BodyFrameReader is IDisposable
                this.bodyFrameReader.Dispose();
                this.bodyFrameReader = null;
            }

            if (this.kinectSensor != null)
            {
                this.kinectSensor.Close();
                this.kinectSensor = null;
            }
        }

        /// <summary>
        /// Handles the face frame data arriving from the sensor
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void Reader_FaceFrameArrived(object sender, FaceFrameArrivedEventArgs e)
        {
            if (isVideoFeed)
            {
                //System.Diagnostics.Debug.Write("Face Frame Arived\n");

                using (FaceFrame faceFrame = e.FrameReference.AcquireFrame())
                {
                    if (faceFrame != null)
                    {
                        // get the index of the face source from the face source array
                        int index = this.GetFaceSourceIndex(faceFrame.FaceFrameSource);

                        // check if this face frame has valid face frame results
                        if (this.ValidateFaceBoxAndPoints(faceFrame.FaceFrameResult))
                        {
                            // store this face frame result to draw later
                            this.faceFrameResults[index] = faceFrame.FaceFrameResult;
                        }
                        else
                        {
                            // indicates that the latest face frame result from this reader is invalid
                            this.faceFrameResults[index] = null;
                        }
                    }
                }
            }//end if videofeed
        }

        /// <summary>
        /// Returns the index of the face frame source
        /// </summary>
        /// <param name="faceFrameSource">the face frame source</param>
        /// <returns>the index of the face source in the face source array</returns>
        private int GetFaceSourceIndex(FaceFrameSource faceFrameSource)
        {
            int index = -1;

            for (int i = 0; i < this.bodyCount; i++)
            {
                if (this.faceFrameSources[i] == faceFrameSource)
                {
                    index = i;
                    break;
                }
            }

            return index;
        }

        //TODO
        private void Reader_ColorFrameArrived(object sender, ColorFrameArrivedEventArgs e)
        {
            //color frame dictates the frequency that gaze point is updated
            this.gazeX = eyeTracker.GetX();
            this.gazeY = eyeTracker.GetY();


            if (isConversationScreen)
            {
                //handle the events for the conversation screen
                using (DrawingContext dc = this.gazeDrawingGroup.Open())
                {
                    //dc.DrawImage(colorBitmap, this.displayRect);

                    Brush myBrush = new SolidColorBrush(Colors.Red);
                    Pen drawingPen = new Pen(myBrush, 10);

                    Brush rectBrush = new SolidColorBrush(Colors.Green);
                    Pen rectPen = new Pen(rectBrush, .01);

                    //create a rectangle to draw the ellipse on
                    Rect falseRect = displayRect;
                    falseRect.Width = 0;
                    falseRect.Height = 0;
                    dc.DrawRectangle(rectBrush, rectPen, falseRect);

                    //dc.DrawImage(new WriteableBitmap(1920, 1080, 96.0, 96.0, PixelFormats.Bgr32, BitmapPalette), this.displayRect);
                    dc.DrawEllipse(myBrush, drawingPen, new Point(gazeX, gazeY), 2, 2);

                    this.gazeDrawingGroup.ClipGeometry = new RectangleGeometry(this.displayRect);

                    System.Diagnostics.Debug.WriteLine("Phrase1 actual height, width: {0}, {1}", Phrase1.ActualHeight, Phrase1.ActualWidth);
                    System.Diagnostics.Debug.WriteLine("Phrase1 height, width: {0}, {1}", Phrase1.Height, Phrase1.Width);
                    System.Diagnostics.Debug.WriteLine("Phrase1 left, top: {0}, {1}", Canvas.GetLeft(Phrase1), Canvas.GetTop(Phrase1));

                    System.Diagnostics.Debug.WriteLine("Phrase1 content: {0}\n", Phrase1.Content);


                    //determine if gaze is on any button, and if a double blink occurs
                    handleButtons(buttons, gazeX, gazeY);

                }
            }

            //generic audio debug messages
            if (eyeTracker.getDoubleBlink())
            {
                speaker.OutputToAudio("double blink");
                eyeTracker.resetDoubleBlink();
            }
            if (eyeTracker.getLongBlink())
            {
                speaker.OutputToAudio("long blink");
                eyeTracker.resetLongBlink();
                setProfile(0);
                toggleScreens();
                

            }

            if (isVideoFeed)
            {
                // ColorFrame is IDisposable
                using (ColorFrame colorFrame = e.FrameReference.AcquireFrame())
                {


                    if (colorFrame != null)
                    {

                        FrameDescription colorFrameDescription = colorFrame.FrameDescription;

                        using (KinectBuffer colorBuffer = colorFrame.LockRawImageBuffer())
                        {
                            this.colorBitmap.Lock();

                            // verify data and write the new color frame data to the display bitmap
                            if ((colorFrameDescription.Width == this.colorBitmap.PixelWidth) && (colorFrameDescription.Height == this.colorBitmap.PixelHeight))
                            {
                                //System.Diagnostics.Debug.Write("Inside If\n");
                                colorFrame.CopyConvertedFrameDataToIntPtr(
                                    this.colorBitmap.BackBuffer,
                                    (uint)(colorFrameDescription.Width * colorFrameDescription.Height * 4),
                                    ColorImageFormat.Bgra);

                                this.colorBitmap.AddDirtyRect(new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight));


                                /*
                                TransformedBitmap myTransform = new TransformedBitmap();
                                myTransform.BeginInit();

                                myTransform.Source = this.colorBitmap;

                                //transform the resulting image to so the view lines up with yours
                                Matrix myMat = new Matrix(-1, 0, 0, 1, 0, 0);
                                MatrixTransform matMirrorImage = new MatrixTransform(myMat);
                                //this.drawingGroup.Transform = matMirrorImage;
                                myTransform.Transform = matMirrorImage;

                                myTransform.EndInit();
                                 * */

                            }

                            this.colorBitmap.Unlock();
                        }
                    }
                }
            }//end if videofeed
        }

        /// <summary>
        /// Handles the body frame data arriving from the sensor
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void Reader_BodyFrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            if (isVideoFeed) {
                using (var bodyFrame = e.FrameReference.AcquireFrame())
                {
                    if (bodyFrame != null)
                    {
                        // update body data
                        bodyFrame.GetAndRefreshBodyData(this.bodies);
                        
                        if (EnrollmentManager.Active)
                        {
                            // iterate through each face source
                            for (int i = 0; i < this.bodyCount; i++)
                            {
                                // check if a valid face is tracked in this face source
                                if (!this.faceFrameSources[i].IsTrackingIdValid)
                                {
                                    // check if the corresponding body is tracked 
                                    if (this.bodies[i].IsTracked)
                                    {
                                        // update the face frame source to track this body
                                        this.faceFrameSources[i].TrackingId = this.bodies[i].TrackingId;
                                    }
                                }
                            }

                            IEnumerable<RectI> faceRectis = this.faceFrameResults
                                .Where(faceResult => faceResult != null)
                                .Select((faceResult) => faceResult.FaceBoundingBoxInColorSpace);
                            IEnumerable<Rect> faceRects =
                                faceRectis.Select((rect) => new Rect(new Point(rect.Left, rect.Top), new Point(rect.Right, rect.Bottom)));
                            EnrollmentManager.UpdateFrame(colorBitmap, faceRects);
                            
                        }
                        else
                        {
                            DrawTheWholeFrame();
                        }
                    }
                }
            }//end if isVideoFeed
        }

        int counter = 0; // delete this

        /// <summary>
        /// This method draws everything. Really. (called when body frame arrives)
        /// </summary>
        private void DrawTheWholeFrame()
        {
            using (DrawingContext dc = this.drawingGroup.Open())
            {
                //TODO. draws the color frame into the display rect
                dc.DrawImage(colorBitmap, this.displayRect);
                // draw the dark background
                //dc.DrawRectangle(Brushes.Black, null, this.displayRect);

                bool drawFaceResult = false;

                // iterate through each face source
                for (int i = 0; i < this.bodyCount; i++)
                {
                    // check if a valid face is tracked in this face source
                    if (this.faceFrameSources[i].IsTrackingIdValid)
                    {
                        // check if we have valid face frame results
                        if (this.faceFrameResults[i] != null)
                        {
                            // draw face frame results
                            this.DrawFaceFrameResults(i, this.faceFrameResults[i], dc);

                            RectI recti = faceFrameResults[i].FaceBoundingBoxInColorSpace;
                            Rect rect = new Rect(recti.Left, recti.Top, recti.Right - recti.Left, recti.Bottom - recti.Top);

                            if (!EnrollmentManager.Active && faceToResult[i] != null)
                            {
                                FaceRecognitionResult result = faceToResult[i];
                                string name = labelToName[result.label];
                                string text = name + '\n' + result.confidence.ToString();
                                FormattedText ftext = new FormattedText(text, CultureInfo.CurrentCulture,
                                    System.Windows.FlowDirection.LeftToRight, new Typeface("Georgia"), 20,
                                    new SolidColorBrush(Colors.Yellow));
                                dc.DrawText(ftext, new Point(rect.X + rect.Width / 2, rect.Bottom));
                            }

                            if (counter % 100 == 0 && !EnrollmentManager.Active)
                            {
                                Bitmap colorBitmapBuffer = Util.SourceToBitmap(colorBitmap);
                                int index = i;
                                Thread thread = new Thread(new ThreadStart(() =>
                                {
                                    FaceRecognitionResult faceResult = faceRecognizer.Predict(colorBitmapBuffer, rect);
                                    faceToResult[index] = faceResult;
                                }));
                                thread.Start();

                                if (!drawFaceResult)
                                {
                                    drawFaceResult = true;
                                }
                            }
                            counter++;
                        }
                    }
                    else
                    {
                        // check if the corresponding body is tracked 
                        if (this.bodies[i].IsTracked)
                        {
                            // update the face frame source to track this body
                            this.faceFrameSources[i].TrackingId = this.bodies[i].TrackingId;
                        }
                    }
                }

                if (!drawFaceResult)
                {
                    // if no faces were drawn then this indicates one of the following:
                    // a body was not tracked 
                    // a body was tracked but the corresponding face was not tracked
                    // a body and the corresponding face was tracked though the face box or the face points were not valid
                    //dc.DrawText(
                    //    this.textFaceNotTracked,
                    //    this.textLayoutFaceNotTracked);
                }


                Brush myBrush = new SolidColorBrush(Colors.Red);
                Pen drawingPen = new Pen(myBrush, 10);

                //account for mirrored image. draw gaze point on screen
                dc.DrawEllipse(myBrush, drawingPen, new Point(colorBitmap.Width - gazeX, gazeY), 2, 2);



                /*
                String gazeMessage = string.Format("({0}, {1})", eyeTracker.GetX(), eyeTracker.GetY());
                FormattedText gazeCenter = new FormattedText(
                            gazeMessage,
                            CultureInfo.GetCultureInfo("en-us"),
                            FlowDirection.LeftToRight,
                            new Typeface("Georgia"),
                            DrawTextFontSize,
                            Brushes.Red);
                
                //line the point up with the comma in the message
                Point center = new Point(eyeTracker.GetX() - 150, eyeTracker.GetY() - 20);
                dc.DrawText(gazeCenter, center);
                */
                

                this.drawingGroup.ClipGeometry = new RectangleGeometry(this.displayRect);


            }
        }

        /// <summary>
        /// Draws face frame results
        /// </summary>
        /// <param name="faceIndex">the index of the face frame corresponding to a specific body in the FOV</param>
        /// <param name="faceResult">container of all face frame results</param>
        /// <param name="drawingContext">drawing context to render to</param>
        private void DrawFaceFrameResults(int faceIndex, FaceFrameResult faceResult, DrawingContext drawingContext)
        {
            // choose the brush based on the face index
            Brush drawingBrush = this.faceBrush[0];
            if (faceIndex < this.bodyCount)
            {
                drawingBrush = this.faceBrush[faceIndex];
            }


            // draw the face bounding box
            var faceBoxSource = faceResult.FaceBoundingBoxInColorSpace;
            Rect faceBox = new Rect(faceBoxSource.Left - 0.5 * (faceBoxSource.Right - faceBoxSource.Left), faceBoxSource.Top - 0.5 * (faceBoxSource.Bottom - faceBoxSource.Top),
                                    2 * (faceBoxSource.Right - faceBoxSource.Left), 4 * (faceBoxSource.Bottom - faceBoxSource.Top));
            double faceBoxThickness = DrawFaceShapeThickness;

            //determine if user is looking at the person. if so, bolden the user's face box
            if (faceBox.Contains(colorBitmap.Width - gazeX, gazeY))
            {
                faceBoxThickness *= 4;
                if (eyeTracker.getDoubleBlink())
                {
                    eyeTracker.ResetBlinkCount();

                    speaker.OutputToAudio("Hey you"); //TODO say person's name
                    eyeTracker.resetDoubleBlink();

                    setProfile(1); //sets profile to person's number in database
                    toggleScreens();

                }
            }
            else
            {
                //eyeTracker.ResetBlinkCount();
            }



            //draw the rectangle around the person's face
            Pen drawingPen = new Pen(drawingBrush, faceBoxThickness);
            drawingContext.DrawRectangle(null, drawingPen, faceBox);

            //TODO. determine and print out center of box. this will be used in the eyeTribe app to check if looking at face.
            //int faceCenterX = faceBoxSource.Left + (faceBoxSource.Right - faceBoxSource.Left) / 2;
            //int faceCenterY = faceBoxSource.Top + (faceBoxSource.Bottom - faceBoxSource.Top) / 2;

            //System.Diagnostics.Debug.WriteLine("face center: ({0}, {1})", faceCenterX, faceCenterY);
            /*
            String faceMessage = string.Format("Face center: ({0}, {1})", faceCenterX, faceCenterY);
            FormattedText faceCenter = new FormattedText(
                        faceMessage,
                        CultureInfo.GetCultureInfo("en-us"),
                        FlowDirection.LeftToRight,
                        new Typeface("Georgia"),
                        DrawTextFontSize,
                        drawingBrush);

            Point center = new Point(faceCenterX, faceCenterY);
            //drawingContext.DrawText(faceCenter, center);
            */
            /*
            if (faceResult.FacePointsInColorSpace != null)
            {
                // draw each face point
                foreach (PointF pointF in faceResult.FacePointsInColorSpace.Values)
                {
                    drawingContext.DrawEllipse(null, drawingPen, new Point(pointF.X, pointF.Y), FacePointRadius, FacePointRadius);
                }
            }

            string faceText = string.Empty;

            // extract each face property information and store it in faceText
            if (faceResult.FaceProperties != null)
            {
                foreach (var item in faceResult.FaceProperties)
                {
                    faceText += item.Key.ToString() + " : ";

                    // consider a "maybe" as a "no" to restrict 
                    // the detection result refresh rate
                    if (item.Value == DetectionResult.Maybe)
                    {
                        faceText += DetectionResult.No + "\n";
                    }
                    else
                    {
                        faceText += item.Value.ToString() + "\n";
                    }                    
                }
            }

            // extract face rotation in degrees as Euler angles
            if (faceResult.FaceRotationQuaternion != null)
            {
                int pitch, yaw, roll;
                ExtractFaceRotationInDegrees(faceResult.FaceRotationQuaternion, out pitch, out yaw, out roll);
                faceText += "FaceYaw : " + yaw + "\n" +
                            "FacePitch : " + pitch + "\n" +
                            "FacenRoll : " + roll + "\n";
            }

            // render the face property and face rotation information
            Point faceTextLayout;
            if (this.GetFaceTextPositionInColorSpace(faceIndex, out faceTextLayout))
            {
                drawingContext.DrawText(
                        new FormattedText(
                            faceText,
                            CultureInfo.GetCultureInfo("en-us"),
                            FlowDirection.LeftToRight,
                            new Typeface("Georgia"),
                            DrawTextFontSize,
                            drawingBrush),
                        faceTextLayout);
            }
             * */
        }

        /// <summary>
        /// Computes the face result text position by adding an offset to the corresponding 
        /// body's head joint in camera space and then by projecting it to screen space
        /// </summary>
        /// <param name="faceIndex">the index of the face frame corresponding to a specific body in the FOV</param>
        /// <param name="faceTextLayout">the text layout position in screen space</param>
        /// <returns>success or failure</returns>
        private bool GetFaceTextPositionInColorSpace(int faceIndex, out Point faceTextLayout)
        {
            faceTextLayout = new Point();
            bool isLayoutValid = false;

            Body body = this.bodies[faceIndex];
            if (body.IsTracked)
            {
                var headJoint = body.Joints[JointType.Head].Position;

                CameraSpacePoint textPoint = new CameraSpacePoint()
                {
                    X = headJoint.X + TextLayoutOffsetX,
                    Y = headJoint.Y + TextLayoutOffsetY,
                    Z = headJoint.Z
                };

                ColorSpacePoint textPointInColor = this.coordinateMapper.MapCameraPointToColorSpace(textPoint);

                faceTextLayout.X = textPointInColor.X;
                faceTextLayout.Y = textPointInColor.Y;
                isLayoutValid = true;
            }

            return isLayoutValid;
        }

        /// <summary>
        /// Validates face bounding box and face points to be within screen space
        /// </summary>
        /// <param name="faceResult">the face frame result containing face box and points</param>
        /// <returns>success or failure</returns>
        private bool ValidateFaceBoxAndPoints(FaceFrameResult faceResult)
        {
            bool isFaceValid = faceResult != null;

            if (isFaceValid)
            {
                var faceBox = faceResult.FaceBoundingBoxInColorSpace;
                if (faceBox != null)
                {
                    // check if we have a valid rectangle within the bounds of the screen space
                    isFaceValid = (faceBox.Right - faceBox.Left) > 0 &&
                                  (faceBox.Bottom - faceBox.Top) > 0 &&
                                  faceBox.Right <= this.displayWidth &&
                                  faceBox.Bottom <= this.displayHeight;

                    if (isFaceValid)
                    {
                        var facePoints = faceResult.FacePointsInColorSpace;
                        if (facePoints != null)
                        {
                            foreach (PointF pointF in facePoints.Values)
                            {
                                // check if we have a valid face point within the bounds of the screen space
                                bool isFacePointValid = pointF.X > 0.0f &&
                                                        pointF.Y > 0.0f &&
                                                        pointF.X < this.displayWidth &&
                                                        pointF.Y < this.displayHeight;

                                if (!isFacePointValid)
                                {
                                    isFaceValid = false;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            return isFaceValid;
        }

        /// <summary>
        /// Handles the event which the sensor becomes unavailable (E.g. paused, closed, unplugged).
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void Sensor_IsAvailableChanged(object sender, IsAvailableChangedEventArgs e)
        {
            if (this.kinectSensor != null)
            {
                // on failure, set the status text
                this.StatusText = this.kinectSensor.IsAvailable ? Properties.Resources.RunningStatusText
                                                                : Properties.Resources.SensorNotAvailableStatusText;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
        private void setProfile (int i)
        {
            Phrase1.Content = profiles[i][2];
            Phrase2.Content = profiles[i][3];
            Phrase3.Content = profiles[i][4];
            String imageloc = "profileImages" + "\\" + "profile" + profiles[i][0] + ".jpg";
            //profilePic.Source = new BitmapImage(new Uri(imageloc));
        }
        private void toggleScreens()
        {

            if (isVideoFeed)
            {
                videoFeed.Visibility = System.Windows.Visibility.Collapsed;
                conversationScreen.Visibility = System.Windows.Visibility.Visible;
                isVideoFeed = false;
                isConversationScreen = true;

                //bodyFrameReader.IsPaused = true;

            }
            else if (isConversationScreen)
            {
                conversationScreen.Visibility = System.Windows.Visibility.Collapsed;
                videoFeed.Visibility = System.Windows.Visibility.Visible;
                isConversationScreen = false;
                isVideoFeed = true;

                //bodyFrameReader.IsPaused = false;
            }
        }

        private bool buttonContains(Button button, double x, double y) {

            Rect temp = new Rect(Canvas.GetLeft(button), Canvas.GetTop(button), button.ActualWidth, button.ActualHeight); 
         
            if (temp.Contains(x, y)) {
                return true;
            }
            else {
                return false;
            }


        }

        private void handleButtons(Button[] buttons, double x, double y) {

            for (int i = 0; i < buttons.Length; i++) {

                if (buttonContains(buttons[i], x, y))
                {
                    buttons[i].Opacity = .3;

                    if (eyeTracker.getDoubleBlink()) {
                        speaker.OutputToAudio(buttons[i].Content.ToString());
                        eyeTracker.resetDoubleBlink();
                    }

                    //stop checking to see if gaze is in any other button. can only be on one button at a time
                    break;
                }
                else {
                    buttons[i].Opacity = 1;
                }
            }
        }
    }
}
