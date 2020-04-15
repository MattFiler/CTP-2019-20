using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using StreetviewRipper;

namespace RedBlueCloudScanTest
{
    class Vec2
    {
        public Vec2 (float _x, float _y)
        {
            x = _x;
            y = _y;
        }
        public float x = 0.0f;
        public float y = 0.0f;
    }

    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            HDRImage skyImg = new HDRImage();
            skyImg.Open("../../DUMMY DATA/SKULUDwlJQa9o9yZot82ew_upscaled_trim.hdr");
            Bitmap classifiedImg = new Bitmap(skyImg.Width, skyImg.Height);

            //First, work out an average red blue division value
            float avgRBDiv = 0.0f;
            for (int x = 0; x < skyImg.Width; x++)
            {
                for (int y = 0; y < skyImg.Height; y++)
                {
                    HDRPixelFloat thisSkyPixel = skyImg.GetPixel(x, y).AsFloat();
                    avgRBDiv += thisSkyPixel.R / thisSkyPixel.B;
                }
            }
            avgRBDiv /= (skyImg.Width * skyImg.Height);

            //Categorise pixels based on the average divison (+ a bit extra)
            for (int x = 0; x < skyImg.Width; x++)
            {
                for (int y = 0; y < skyImg.Height; y++)
                {
                    HDRPixelFloat thisSkyPixel = skyImg.GetPixel(x, y).AsFloat();
                    float redBlueDiv = thisSkyPixel.R / thisSkyPixel.B;

                    if (redBlueDiv > (avgRBDiv + (avgRBDiv / 6.5f)))
                    {
                        classifiedImg.SetPixel(x, y, Color.White);
                    }
                    else
                    {
                        classifiedImg.SetPixel(x, y, Color.Black);
                    }
                }
            }

            //Remove all categorised pixels close to the sun position
            int start_x = classifiedImg.Width / 4;
            int start_y = 588; //588 is hard-coded for the test img, actually get this value from StreetviewImageProcessor
            FloodFill(classifiedImg, start_x, start_y, Color.Black);

            classifiedImg.Save("test.png");

            Application.Exit();
        }

        /* Fill a region of colour in a bitmap (thanks: https://stackoverflow.com/a/14897412) */
        private void FloodFill(Bitmap bitmap, int x, int y, Color color)
        {
            BitmapData data = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            int[] bits = new int[data.Stride / 4 * data.Height];
            Marshal.Copy(data.Scan0, bits, 0, bits.Length);

            LinkedList<Point> check = new LinkedList<Point>();
            int floodTo = color.ToArgb();
            int floodFrom = bits[x + y * data.Stride / 4];
            bits[x + y * data.Stride / 4] = floodTo;

            if (floodFrom != floodTo)
            {
                check.AddLast(new Point(x, y));
                while (check.Count > 0)
                {
                    Point cur = check.First.Value;
                    check.RemoveFirst();

                    foreach (Point off in new Point[] {
                new Point(0, -1), new Point(0, 1),
                new Point(-1, 0), new Point(1, 0)})
                    {
                        Point next = new Point(cur.X + off.X, cur.Y + off.Y);
                        if (next.X >= 0 && next.Y >= 0 &&
                            next.X < data.Width &&
                            next.Y < data.Height)
                        {
                            if (bits[next.X + next.Y * data.Stride / 4] == floodFrom)
                            {
                                check.AddLast(next);
                                bits[next.X + next.Y * data.Stride / 4] = floodTo;
                            }
                        }
                    }
                }
            }

            Marshal.Copy(bits, 0, data.Scan0, bits.Length);
            bitmap.UnlockBits(data);
        }
    }
}
