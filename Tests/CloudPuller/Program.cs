using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CloudPuller
{
    class BoundingBox
    {
        public BoundingBox(Point _tl, Point _br)
        {
            TopLeft = _tl;
            BottomRight = _br;
        }
        public Point TopLeft;
        public Point BottomRight;
        public bool Contains(Point _point)
        {
            return ((_point.X >= TopLeft.X && _point.X <= BottomRight.X) && (_point.Y >= TopLeft.Y && _point.Y <= BottomRight.Y));
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Directory.CreateDirectory("PulledClouds");
            string[] files = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*_classified_5_extended.png", SearchOption.AllDirectories);
            foreach (string file in files)
            {
                string file_PANOID = Path.GetFileName(file).Substring(0, Path.GetFileName(file).Length - 26);
                Bitmap maskImage = (Bitmap)Image.FromFile(file); 
                Bitmap streetviewImage = (Bitmap)Image.FromFile(file_PANOID + "_shifted_trim.jpg");
                //HDRImage streetviewImageHDR = new HDRImage();
                //streetviewImageHDR.Open(file_PANOID + ".hdr");
                //HDRImage hosekWilkieSky = new HDRImage();
                //hosekWilkieSky.Open(file_PANOID + "_sky.exr");
                //TODO: Load depth values
                //Bitmap inscatteringColours = (Bitmap)Image.FromFile(file_PANOID + "_inscatter_colour_debug.png");

                List<BoundingBox> checkedAreas = new List<BoundingBox>();
                int cloudCount = 0;
                for (int x = 0; x < maskImage.Width; x++)
                {
                    for (int y = 0; y < maskImage.Height; y++)
                    {
                        //If this pixel isn't black, it's a cloud mask
                        Color thisPixel = maskImage.GetPixel(x, y);
                        if (!(thisPixel.R == 0 && thisPixel.G == 0 && thisPixel.B == 0))
                        {
                            //Double check this pixel isn't within a cloud bound we've already exported
                            bool shouldCheck = true;
                            foreach (BoundingBox thisArea in checkedAreas)
                            {
                                if (thisArea.Contains(new Point(x, y)))
                                {
                                    shouldCheck = false;
                                    break;
                                }
                            }
                            if (!shouldCheck) continue;

                            //Work out the bounds of the cloud mask this pixel is within
                            List<Point> linkedContents = ThisRegion(maskImage, x, y);
                            Point boundsTopLeft = GetMin(linkedContents);
                            Point boundsBottomRight = GetMax(linkedContents);
                            checkedAreas.Add(new BoundingBox(boundsTopLeft, boundsBottomRight));

                            //Pull the mask's bounds out from the original images
                            Point maskDims = new Point(boundsBottomRight.X - boundsTopLeft.X, boundsBottomRight.Y - boundsTopLeft.Y);
                            if (maskDims.X == 0 || maskDims.Y == 0) continue;
                            if (maskDims.X <= 40 || maskDims.Y <= 40) continue; //Images below this size are typically crap noise
                            PullRegionLDR(maskImage, boundsTopLeft, maskDims).Save("PulledClouds/" + file_PANOID + "_CLOUD_" + cloudCount + ".CLOUD_MASK.png", ImageFormat.Png);
                            PullRegionLDR(streetviewImage, boundsTopLeft, maskDims).Save("PulledClouds/" + file_PANOID + "_CLOUD_" + cloudCount + ".STREETVIEW_LDR.png", ImageFormat.Png);
                            //PullRegionHDR(streetviewImageHDR, boundsTopLeft, maskDims).Save("PulledClouds/" + file_PANOID + "_CLOUD_" + cloudCount + ".STREETVIEW_HDR.hdr");
                            //PullRegionHDR(hosekWilkieSky, boundsTopLeft, maskDims).Save("PulledClouds/" + file_PANOID + "_CLOUD_" + cloudCount + ".SKY_MODEL.hdr");
                            //TODO: depth values
                            //PullRegionLDR(inscatteringColours, boundsTopLeft, maskDims).Save("PulledClouds/" + file_PANOID + "_CLOUD_" + cloudCount + ".INSCATTER_COLOUR.png", ImageFormat.Png);

                            cloudCount++;
                        }
                    }
                }
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
        static HDRImage PullRegionHDR(HDRImage originalImage, Point topLeft, Point widthAndHeight)
        {
            HDRImage toReturn = new HDRImage();
            toReturn.SetResolution(widthAndHeight.X, widthAndHeight.Y);
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
        static List<Point> ThisRegion(Bitmap bitmap, int x, int y)
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

            List<Point> toReturn = new List<Point>();
            if (floodFrom != floodTo)
            {
                check.AddLast(new Point(x, y));
                toReturn.Add(new Point(x, y));
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
                                if (!toReturn.Contains(next)) toReturn.Add(next);
                                bits[next.X + next.Y * data.Stride / 4] = floodTo;
                            }
                        }
                    }
                }
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
