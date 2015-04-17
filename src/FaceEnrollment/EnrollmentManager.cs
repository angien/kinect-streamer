﻿using System;
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

        public static Window window;
        internal static int currentTrainingId;
        internal static int actualTrainingId;
        internal static event Action<BitmapSource, IEnumerable<Rect>> OnFrameReceived;

        public static event Action<bool> Done;
        public static List<PersonTrainingData> trainingData = new List<PersonTrainingData>();
        public static bool Active
        {
            get
            {
                return isActive;
            }
        }

        public static void Launch(Window window)
        {
            isActive = true;
            initialContent = window.Content;
            window.Content = new Page1();
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
