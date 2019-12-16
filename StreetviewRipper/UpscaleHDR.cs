using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StreetviewRipper
{
    class UpscaleHDR
    {
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

        /* Maths functions: https://rosettacode.org/wiki/Bilinear_interpolation */
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
