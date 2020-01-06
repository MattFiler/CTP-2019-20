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
        /* Convert a HDR image to fisheye */
        public HDRImage ToFisheye(HDRImage origImage, int radius)
        {
            HDRImage outImage = new HDRImage();
            outImage.SetResolution(radius * 4, radius * 4);
            int c = radius * 2;

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

        /* Pull clouds from an LDR image, by the HDR classifier's data */
        public enum CloudTypes
        {
            NONE,         //Purple colour code
            CLOUD_TYPE_1, //Red colour code (need to work out what it actually thinks these are)
            CLOUD_TYPE_2, //Green colour code (need to work out what it actually thinks these are)
            CLOUD_TYPE_3  //Blue colour code (need to work out what it actually thinks these are)
        }
        public Bitmap PullCloudType(HDRImage classifiedImage, Bitmap origImage, CloudTypes cloudType)
        {
            //Ignore E for the cloud type colours
            HDRPixel colourToMatch = new HDRPixel(128, 0, 128, 129);
            switch (cloudType)
            {
                case CloudTypes.CLOUD_TYPE_1:
                    colourToMatch = new HDRPixel(128, 0, 0, 129);
                    break;
                case CloudTypes.CLOUD_TYPE_2:
                    colourToMatch = new HDRPixel(0, 128, 0, 129);
                    break;
                case CloudTypes.CLOUD_TYPE_3:
                    colourToMatch = new HDRPixel(0, 0, 128, 129);
                    break;
            }

            //Pull all pixels from original image that match our cloud type in classified image
            Bitmap classifiedTrim = new Bitmap(origImage.Width, origImage.Height);
            for (int x = 0; x < origImage.Width; x++)
            {
                for (int y = 0; y < origImage.Height; y++)
                {
                    HDRPixel thisPixel = classifiedImage.GetPixel(x, y);
                    if (thisPixel != null && 
                        thisPixel.R == colourToMatch.R &&
                        thisPixel.G == colourToMatch.G &&
                        thisPixel.B == colourToMatch.B &&
                        thisPixel.E == colourToMatch.E)
                    {
                        classifiedTrim.SetPixel(x, y, origImage.GetPixel(x, y));
                    }
                }
            }
            return classifiedTrim;
        }

        /* Upscale a HDR image */
        public HDRImage Upscale(HDRImage origImage, float scale)
        {
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
