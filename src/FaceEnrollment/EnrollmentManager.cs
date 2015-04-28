using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace FaceEnrollment
{
    public static class EnrollmentManager
    {
        private static object initialContent;
        private static bool isActive = false;
        private static bool kinectNum = false; // false means that this is kinect number 1


        public static Window window;
        internal static event Action<BitmapSource, IEnumerable<Rect>> OnFrameReceived;

        public static bool doUpdate = false;
        public static event Action<bool> Done;
        public static PersonTrainingData personToTrain;

       // public const string filepath = @"C:\Test\";
        public const string filepath = @"\\NARENDRAN-PC\Users\Narendran\Documents\eyehome\Test\";

        public static bool Active
        {
            get
            {
                return isActive;
            }
        }

        public static bool KinectNum2
        {
            get
            {
                return kinectNum;
            }
        }

        public static void Launch(Window window)
        {
            isActive = true;
            initialContent = window.Content;
            window.Content = new WelcomePage();
            EnrollmentManager.window = window;
        }

        public static void UpdateFrame(BitmapSource frame, IEnumerable<Rect> faceBoxes)
        {
            if (OnFrameReceived != null)
            {
                OnFrameReceived(frame, faceBoxes);
            }
        }

        public static void Relaunch()
        {
            isActive = true;
        }

        public static void isKinect2()
        {
            kinectNum = true;
        }

        internal static void Finish(bool loadDB)
        {
            isActive = false;
            window.Content = initialContent;
            if (Done != null)
            {
                Done(loadDB);
            }
        }
    }
}
