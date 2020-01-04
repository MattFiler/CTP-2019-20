using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StreetviewRipper
{
    class HDRUtilities
    {
        /* Convert a HDR image to fisheye, based on: https://stackoverflow.com/a/26682324/3798962 */
        public HDRImage ToFisheye(HDRImage origImage, int radius)
        {
            HDRImage outImage = new HDRImage();
            outImage.SetResolution(radius * 4, radius * 4);
            int c = radius * 2;

            //First, make every pixel black
            HDRPixel blackPixel = new HDRPixel(0, 0, 0, 0);
            for (int x = 0; x < outImage.Width; x++)
            {
                for (int y = 0; y < outImage.Height; y++)
                {
                    outImage.SetPixel(x, y, blackPixel);
                }
            }

            //Work out the maximum number of calls we're going to make to draw around the circle
            int maxCount = 0;
            {
                double rr = Math.Pow(radius, 2);
                for (int x = c - (int)radius; x <= c + radius; x++)
                {
                    for (int y = c - (int)radius; y <= c + radius; y++)
                    {
                        if (Math.Abs(Math.Pow(x - c, 2) + Math.Pow(y - c, 2) - rr) <= radius)
                        {
                            maxCount++;
                        }
                    }
                }
            }

            //From this, work out the step distance to take between each X and Y value when drawing
            int heightStep = origImage.Height / radius;
            int widthStep = origImage.Width / maxCount;

            //Draw the fisheye
            int h = 0;
            for (int r = 0; r < radius; r++)
            {
                double rr = Math.Pow(r, 2);
                int count = 0;

                for (int x = c - (int)r; x <= c + r; x++)
                {
                    for (int y = c - (int)r; y <= c + r; y++)
                    {
                        if (Math.Abs(Math.Pow(x - c, 2) + Math.Pow(y - c, 2) - rr) <= r)
                        {
                            outImage.SetPixel(x, y, origImage.GetPixel(widthStep * count, h));
                            count++;
                        }
                    }
                }

                h += heightStep; //Step down the Y of the image as we draw outwards
            }

            //Trim the final image
            HDRImage outImageFinal = new HDRImage();
            outImageFinal.SetResolution(outImage.Width / 2, outImage.Height / 2);
            int widthDivision = outImage.Width / 4;
            int heightDivision = outImage.Height / 4;
            int nX = 0;
            int nY = 0;
            for (int x = widthDivision; x < (widthDivision * 3); x++)
            {
                nX++;
                for (int y = heightDivision; y < (heightDivision * 3); y++)
                {
                    outImageFinal.SetPixel(nX, nY, outImage.GetPixel(x, y));
                    nY++;
                }
                nY = 0;
            }
            
            return outImageFinal;
        }

        /* Upscale a HDR image */
        public HDRImage Upscale(HDRImage origImage, float scale)
        {
            Bitmap newimg = new Bitmap((int)(origImage.Width * scale), (int)(origImage.Height * scale));
            Bitmap origimg = new Bitmap(origImage.Width, origImage.Height);

            for (int x = 0; x < origImage.Width; x++)
            {
                for (int y = 0; y < origImage.Height; y++)
                {
                    HDRPixel thisHDRPixel = origImage.GetPixel(x, y);
                    Color thisLDRPixel = Color.FromArgb(255, thisHDRPixel.E, thisHDRPixel.E, thisHDRPixel.E);
                    origimg.SetPixel(x, y, thisLDRPixel);
                }
            }

            using (Graphics g = Graphics.FromImage(newimg))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
                g.DrawImage(origimg, new Rectangle(Point.Empty, newimg.Size));
            }

            HDRImage upscaledImage = new HDRImage();
            upscaledImage.SetResolution(newimg.Width, newimg.Height);

            for (int x = 0; x < newimg.Width; x++)
            {
                for (int y = 0; y < newimg.Height; y++)
                {
                    HDRPixel thisHDRPixel = new HDRPixel();
                    thisHDRPixel.E = newimg.GetPixel(x, y).R;
                    upscaledImage.SetPixel(x, y, thisHDRPixel);
                }
            }

            for (int x = 0; x < origImage.Width; x++)
            {
                for (int y = 0; y < origImage.Height; y++)
                {
                    HDRPixel thisHDRPixel = origImage.GetPixel(x, y);
                    Color thisLDRPixel = Color.FromArgb(255, thisHDRPixel.R, thisHDRPixel.G, thisHDRPixel.B); 
                    origimg.SetPixel(x, y, thisLDRPixel);
                }
            }

            using (Graphics g = Graphics.FromImage(newimg))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
                g.DrawImage(origimg, new Rectangle(Point.Empty, newimg.Size));
            }

            for (int x = 0; x < newimg.Width; x++)
            {
                for (int y = 0; y < newimg.Height; y++)
                {
                    Color thisLDRPixel = newimg.GetPixel(x, y);
                    HDRPixel thisHDRPixel = upscaledImage.GetPixel(x, y);
                    thisHDRPixel.R = thisLDRPixel.R;
                    thisHDRPixel.G = thisLDRPixel.G;
                    thisHDRPixel.B = thisLDRPixel.B;
                    upscaledImage.SetPixel(x, y, thisHDRPixel);
                }
            }

            return upscaledImage;



            HDRImage outImage = new HDRImage();
            outImage.SetResolution((int)(origImage.Width * scale), (int)(origImage.Height * scale));
            
            for (int y = 0; y < outImage.Height; y++)
            {
                for (int x = 0; x < outImage.Width; x++)
                {
                    float gx = ((float)x) / outImage.Width * (origImage.Width - 1);
                    float gy = ((float)y) / outImage.Height * (origImage.Height - 1);
                    int gxi = (int)gx;
                    int gyi = (int)gy;

                    HDRPixel c00 = origImage.GetPixel(gxi, gyi);
                    HDRPixel c10 = origImage.GetPixel(gxi + 1, gyi);
                    HDRPixel c01 = origImage.GetPixel(gxi, gyi + 1);
                    HDRPixel c11 = origImage.GetPixel(gxi + 1, gyi + 1);

                    HDRPixel newPixel = new HDRPixel();
                    newPixel.R = (int)Blerp(c00.R, c10.R, c01.R, c11.R, gx - gxi, gy - gyi);
                    newPixel.G = (int)Blerp(c00.G, c10.G, c01.G, c11.G, gx - gxi, gy - gyi);
                    newPixel.B = (int)Blerp(c00.B, c10.B, c01.B, c11.B, gx - gxi, gy - gyi);
                    newPixel.E = (int)Blerp(c00.E, c10.E, c01.E, c11.E, gx - gxi, gy - gyi);
                    outImage.SetPixel(x, y, newPixel);
                }
            }
            
            return outImage;
        }

        /* Maths functions for upscaling: https://rosettacode.org/wiki/Bilinear_interpolation */
        private float Lerp(float s, float e, float t)
        {
            return s + (e - s) * t;
        }
        private float Blerp(float c00, float c10, float c01, float c11, float tx, float ty)
        {
            return Lerp(Lerp(c00, c10, tx), Lerp(c01, c11, tx), ty);
        }
    }
}
