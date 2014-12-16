using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Drawing.Imaging;
using System.Windows;

namespace FaceEnrollment
{
    public class Util
    {
        public static Bitmap SourceToBitmap(BitmapSource source)
        {
            Bitmap bmp = new Bitmap(
            source.PixelWidth,
            source.PixelHeight,
            System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            BitmapData data = bmp.LockBits(
              new System.Drawing.Rectangle(System.Drawing.Point.Empty, bmp.Size),
              ImageLockMode.WriteOnly,
              System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            source.CopyPixels(
              Int32Rect.Empty,
              data.Scan0,
              data.Height * data.Stride,
              data.Stride);
            bmp.UnlockBits(data);
            return bmp;
        }

        public static Rect TransformFace(Rect faceBox)
        {
            double size = .8;
            double yOffset = 0;
            //double yOffset = .25;

            var x = faceBox.Left - (size / 2) * (faceBox.Right - faceBox.Left);
            var y = faceBox.Top - (size / 2) * (faceBox.Bottom - faceBox.Top) - yOffset * (faceBox.Bottom - faceBox.Top);
            var width = size * 2 * (faceBox.Right - faceBox.Left);
            var height = size * 2 * (faceBox.Bottom - faceBox.Top);

            return new Rect(x, y, width, height);
        }

        public static bool IsValidRect(Rect faceBox, Rect entireImage)
        {
            return entireImage.Contains(faceBox);
        }
    }
}
