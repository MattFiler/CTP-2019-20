using RedBlueExtended.StreetviewRipper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RedBlueExtended
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            Thread t = new Thread(() => ThreadedClassifier());
            t.Start();
        }

        private void ThreadedClassifier()
        {
            string[] arguments = Environment.GetCommandLineArgs();
            string[] files = Directory.GetFiles(arguments[1], "*.*", SearchOption.AllDirectories);
            if (files.Count() == 0) return;
            progressBar1.Step = 100 / files.Count();
            foreach (string file in files)
            {
                Bitmap streetviewImage = (Bitmap)Image.FromFile(file);

                StreetviewImageProcessor processor = new StreetviewImageProcessor();
                int selectedQuality = 4;
                StraightLineBias selectedBias = StraightLineBias.MIDDLE;
                List<GroundInfo> groundPositions = processor.GuessGroundPositions(streetviewImage, (selectedQuality * 5) + 15, true, (StraightLineBias)selectedBias);
                int groundY = (int)groundPositions[0].position.y; //We only have [0] and [1] when using straight line cutting, both have the same Y
                Vector2 sunPos = processor.GuessSunPosition(streetviewImage, groundY);
                streetviewImage = processor.ShiftImageLeft(streetviewImage, (int)sunPos.x - (streetviewImage.Width / 4));

                Bitmap classifierOverlay = new Bitmap(streetviewImage.Width, groundY);
                float avgRBDiv = 0.0f;
                for (int x = 0; x < classifierOverlay.Width; x++)
                {
                    for (int y = 0; y < classifierOverlay.Height; y++)
                    {
                        Color thisSkyPixel = streetviewImage.GetPixel(x, y);
                        if (thisSkyPixel.B == 0) continue;
                        avgRBDiv += thisSkyPixel.R / thisSkyPixel.B;
                    }
                }
                avgRBDiv /= (classifierOverlay.Width * classifierOverlay.Height);
                for (int x = 0; x < classifierOverlay.Width; x++)
                {
                    for (int y = 0; y < classifierOverlay.Height; y++)
                    {
                        Color thisSkyPixel = streetviewImage.GetPixel(x, y);
                        float redBlueDiv = 0.0f;
                        if (thisSkyPixel.B != 0) redBlueDiv = thisSkyPixel.R / thisSkyPixel.B;

                        if (redBlueDiv > (avgRBDiv + (avgRBDiv / 6.5f)))
                        {
                            classifierOverlay.SetPixel(x, y, Color.FromArgb(255, 0, 255)); //STRATOCUMULUS
                        }
                        else
                        {
                            classifierOverlay.SetPixel(x, y, Color.Black);
                        }
                    }
                }
                FloodFill(classifierOverlay, classifierOverlay.Width / 4, (int)sunPos.y, Color.Black);
                classifierOverlay.Save(file + ".classified.png", ImageFormat.Png);

                Bitmap streetviewImageTrim = new Bitmap(streetviewImage.Width, groundY);
                for (int x = 0; x < streetviewImageTrim.Width; x++)
                {
                    for (int y = 0; y < streetviewImageTrim.Height; y++)
                    {
                        streetviewImageTrim.SetPixel(x, y, streetviewImage.GetPixel(x, y));
                    }
                }
                streetviewImageTrim.Save(file + ".sky.jpg", ImageFormat.Jpeg);

                progressBar1.PerformStep();
            }
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
