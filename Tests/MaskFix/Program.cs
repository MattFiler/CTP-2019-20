using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NormaliseAllImageSizes
{
    class FloodResult
    {
        public List<Point> pointlist = new List<Point>();
        public bool shouldoutput = true;
    }

    class ValidCloudSquare
    {
        public ValidCloudSquare(Point _tl, Point _br, Bitmap _m, bool _b)
        {
            TopLeft = _tl;
            BottomRight = _br;
            StreetviewImg = _m;
            ShouldKeep = _b;
        }
        public Point TopLeft;
        public Point BottomRight;
        public bool Contains(Point _point)
        {
            return ((_point.X >= TopLeft.X && _point.X <= BottomRight.X) && (_point.Y >= TopLeft.Y && _point.Y <= BottomRight.Y));
        }
        public Bitmap StreetviewImg;
        public bool ShouldKeep = false;
    }

    class Program
    {
        static void Main(string[] args)
        {
            Directory.CreateDirectory("SECOND_FIX");

            string[] files = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*_cloudmask.png", SearchOption.TopDirectoryOnly);
            Point biggestSize = new Point(0, 0);
            foreach (string file in files)
            {
                Console.WriteLine("Processing: " + Path.GetFileName(file));
                Bitmap cloudMaskV1 = (Bitmap)Image.FromFile(file);
                Bitmap streetviewImage = (Bitmap)Image.FromFile(file.Substring(0, file.Length - 14) + ".jpg");

                string streetviewID = Path.GetFileName(file).Substring(0, Path.GetFileName(file).Length - 14);

                //Cut out the clouds from all our data, based on our cloud mask
                List<ValidCloudSquare> validCloudRegions = new List<ValidCloudSquare>();
                for (int x = 0; x < cloudMaskV1.Width; x++)
                {
                    for (int y = 0; y < cloudMaskV1.Height; y++)
                    {
                        //If this pixel isn't black, it's a cloud mask
                        Color thisPixel = cloudMaskV1.GetPixel(x, y);
                        if (!(thisPixel.R == 0 && thisPixel.G == 0 && thisPixel.B == 0))
                        {
                            //Double check this pixel isn't within a cloud bound we've already done
                            bool shouldCheck = true;
                            foreach (ValidCloudSquare thisArea in validCloudRegions)
                            {
                                if (thisArea.Contains(new Point(x, y)))
                                {
                                    shouldCheck = false;
                                    break;
                                }
                            }
                            if (!shouldCheck) continue;

                            //Work out the bounds of the cloud mask this pixel is within
                            FloodResult regionResult = ThisRegion(cloudMaskV1, x, y);
                            List<Point> linkedContents = regionResult.pointlist;
                            Point boundsTopLeft = GetMin(linkedContents);
                            Point boundsBottomRight = GetMax(linkedContents);

                            //Work out the mask dimensions, and pull the section from our Streeview image
                            Point maskDims = new Point(boundsBottomRight.X - boundsTopLeft.X, boundsBottomRight.Y - boundsTopLeft.Y);
                            if (maskDims.X == 0 || maskDims.Y == 0) regionResult.shouldoutput = false;
                            if (maskDims.X <= 40 || maskDims.Y <= 40) regionResult.shouldoutput = false;
                            validCloudRegions.Add(
                                new ValidCloudSquare(
                                    boundsTopLeft,
                                    boundsBottomRight,
                                    (regionResult.shouldoutput) ? PullRegionLDR(streetviewImage, boundsTopLeft, maskDims) : null,
                                    regionResult.shouldoutput
                                )
                            );
                        }
                    }
                }

                //Evaluate the cut-out clouds
                foreach (ValidCloudSquare thisRegion in validCloudRegions)
                {
                    if (!thisRegion.ShouldKeep) continue;

                    Point imgDims = new Point(0, 0);
                    float lowestBrightness = int.MaxValue;
                    float avgBrightness = 0.0f;
                    float avgRed = 0.0f;
                    float avgGreen = 0.0f;
                    float avgBlue = 0.0f;

                    for (int x = 0; x < thisRegion.StreetviewImg.Width; x++)
                    {
                        for (int y = 0; y < thisRegion.StreetviewImg.Height; y++)
                        {
                            Color thisPixel = thisRegion.StreetviewImg.GetPixel(x, y);
                            if (thisPixel.GetBrightness() < lowestBrightness) lowestBrightness = thisPixel.GetBrightness();
                            avgBrightness += thisPixel.GetBrightness();
                            avgRed += thisPixel.R;
                            avgGreen += thisPixel.G;
                            avgBlue += thisPixel.B;
                        }
                    }
                    imgDims.X = thisRegion.StreetviewImg.Width; imgDims.Y = thisRegion.StreetviewImg.Height;
                    avgBrightness /= (thisRegion.StreetviewImg.Width * thisRegion.StreetviewImg.Height);
                    avgRed /= (thisRegion.StreetviewImg.Width * thisRegion.StreetviewImg.Height);
                    avgGreen /= (thisRegion.StreetviewImg.Width * thisRegion.StreetviewImg.Height);
                    avgBlue /= (thisRegion.StreetviewImg.Width * thisRegion.StreetviewImg.Height);

                    thisRegion.ShouldKeep = (avgBrightness > 0.5 && /*pixelsWithZeroBr < ((imgDims.X * imgDims.Y) / 6) &&*/ lowestBrightness > 0.2 && ((avgBlue >= avgGreen) && (avgBlue >= avgRed)));
                }

                //Using the clouds we've determined are good enough, keep these bits of the mask and remove the others
                Bitmap cloudMaskV2 = new Bitmap(cloudMaskV1.Width, cloudMaskV1.Height);
                for (int x = 0; x < cloudMaskV2.Width; x++)
                {
                    for (int y = 0; y < cloudMaskV2.Height; y++)
                    {
                        cloudMaskV2.SetPixel(x, y, Color.Black);
                    }
                }
                foreach (ValidCloudSquare thisRegion in validCloudRegions)
                {
                    if (!thisRegion.ShouldKeep) continue;

                    for (int x = 0; x < thisRegion.StreetviewImg.Width; x++)
                    {
                        for (int y = 0; y < thisRegion.StreetviewImg.Height; y++)
                        {
                            cloudMaskV2.SetPixel(thisRegion.TopLeft.X + x, thisRegion.TopLeft.Y + y, cloudMaskV1.GetPixel(thisRegion.TopLeft.X + x, thisRegion.TopLeft.Y + y));
                        }
                    }
                }
                streetviewImage.Save("SECOND_FIX/" + streetviewID + ".jpg");
                cloudMaskV2.Save("SECOND_FIX/" + streetviewID + ".png");
            }
        }

        static Bitmap PullRegionLDR(Bitmap originalImage, Point topLeft, Point widthAndHeight)
        {
            Bitmap toReturn = new Bitmap(widthAndHeight.X, widthAndHeight.Y);
            for (int x = 0; x < widthAndHeight.X; x++)
            {
                for (int y = 0; y < widthAndHeight.Y; y++)
                {
                    toReturn.SetPixel(x, y, originalImage.GetPixel(topLeft.X + x, topLeft.Y + y));
                }
            }
            return toReturn;
        }

        /* thanks in part: https://stackoverflow.com/a/14897412 */
        static FloodResult ThisRegion(Bitmap bitmap, int x, int y)
        {
            BitmapData data = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            int[] bits = new int[data.Stride / 4 * data.Height];
            Marshal.Copy(data.Scan0, bits, 0, bits.Length);

            LinkedList<Point> check = new LinkedList<Point>();
            int floodTo = Color.Black.ToArgb();
            int floodFrom = bits[x + y * data.Stride / 4];
            bits[x + y * data.Stride / 4] = floodTo;

            FloodResult toReturn = new FloodResult();
            if (floodFrom != floodTo)
            {
                check.AddLast(new Point(x, y));
                toReturn.pointlist.Add(new Point(x, y));

                Stopwatch st = new Stopwatch();
                st.Start();
                while (check.Count > 0)
                {
                    if (st.Elapsed.Seconds >= 1) //Don't pull anything that's taking too long - going for quantity here!
                    {
                        toReturn.shouldoutput = false;
                        break;
                    }

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
                                if (!toReturn.pointlist.Contains(next)) toReturn.pointlist.Add(next);
                                bits[next.X + next.Y * data.Stride / 4] = floodTo;
                            }
                        }
                    }
                }
                st.Stop();
            }

            bitmap.UnlockBits(data);
            return toReturn;
        }

        static Point GetMin(List<Point> points)
        {
            Point topLeft = new Point(int.MaxValue, int.MaxValue);
            foreach (Point thisPoint in points)
            {
                if (topLeft.X > thisPoint.X)
                {
                    topLeft.X = thisPoint.X;
                }
                if (topLeft.Y > thisPoint.Y)
                {
                    topLeft.Y = thisPoint.Y;
                }
            }
            if (topLeft.X == int.MaxValue || topLeft.Y == int.MaxValue) return new Point(0, 0);
            return topLeft;
        }
        static Point GetMax(List<Point> points)
        {
            Point bottomRight = new Point(-int.MaxValue, -int.MaxValue);
            foreach (Point thisPoint in points)
            {
                if (bottomRight.X < thisPoint.X)
                {
                    bottomRight.X = thisPoint.X;
                }
                if (bottomRight.Y < thisPoint.Y)
                {
                    bottomRight.Y = thisPoint.Y;
                }
            }
            if (bottomRight.X == -int.MaxValue || bottomRight.Y == -int.MaxValue) return new Point(0, 0);
            return bottomRight;
        }
    }
}
