﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceEnrollment
{
    public class PersonTrainingData
    {
        public string name;
        public List<Bitmap> trainingImages = new List<Bitmap>();
        public List<System.Windows.Rect> faceBoxes = new List<System.Windows.Rect>();
        public int trainingId;
    }
}
