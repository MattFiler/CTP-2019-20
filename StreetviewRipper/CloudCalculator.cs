using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreetviewRipper
{
    class CloudCalculator
    {
        private Bitmap originalSkyImage;
        private Bitmap classifiedSkyImage;
        private Bitmap backgroundSkyImage;
        public CloudCalculator(Bitmap origImage, Bitmap classifiedImage, Bitmap hosekWilkieImage)
        {
            originalSkyImage = origImage;
            classifiedSkyImage = classifiedImage;
            backgroundSkyImage = hosekWilkieImage;
        }

        /* Run the calculations to calculate inscattering data */
        public void RunInscatteringFormula()
        {
            //todo: do we really want to do this for every pixel?
            Bitmap outputDebug = new Bitmap(originalSkyImage.Width, originalSkyImage.Height);
            for (int x = 0; x < originalSkyImage.Width; x++) {
                for (int y = 0; y < originalSkyImage.Height; y++)
                {
                    outputDebug.SetPixel(x, y, Color.FromArgb((int)CalculateForPoint(new Vector2(x, y)) * 255, 0, 0, 0));
                }
            }
            outputDebug.Save("InscatteringCalcDebug.png");
        }

        /* Calculate light scattering value at point */
        private double CalculateForPoint(Vector2 point)
        {
            Vector2 closestPoint = GetClosestColourNeighbour((int)point.x, (int)point.y);
            
            double sigma_s = GetSigmaFromClassified(point);

            Color thisColour = originalSkyImage.GetPixel((int)point.x, (int)point.y);
            Color closeColour = originalSkyImage.GetPixel((int)closestPoint.x, (int)closestPoint.y);
            double La = GetLuma(thisColour);
            double Lb = GetLuma(closeColour);

            Color thisBG = backgroundSkyImage.GetPixel((int)point.x, (int)point.y);
            Color closeBG = backgroundSkyImage.GetPixel((int)closestPoint.x, (int)closestPoint.y);
            double Lsa = GetLuma(thisBG);
            double Lsb = GetLuma(closeBG);

            Vector3 da = new Vector3(
                (float)CalculateDaForColour(thisColour.R, closeColour.R, thisBG.R, closeBG.R, sigma_s),
                (float)CalculateDaForColour(thisColour.G, closeColour.G, thisBG.G, closeBG.G, sigma_s),
                (float)CalculateDaForColour(thisColour.B, closeColour.B, thisBG.B, closeBG.B, sigma_s)
            );
            Vector3 Lia = new Vector3(
                (float)CalculateLiaForColour(thisColour.R, thisBG.R, da.x, sigma_s),
                (float)CalculateLiaForColour(thisColour.G, thisBG.G, da.y, sigma_s),
                (float)CalculateLiaForColour(thisColour.B, thisBG.B, da.z, sigma_s)
            );
            double Lia = La - (Lsa * Math.Exp(-da * sigma_s));

            return Lia;
        }
        private double CalculateDaForColour(double La, double Lb, double Lsa, double Lsb, double sigma_s)
        {
            return -Math.Log(Math.Abs((La - Lb) / (Lsa - Lsb))) / sigma_s;
        }
        private double CalculateLiaForColour(double La, double Lsa, double da, double sigma_s)
        {
            return La - (Lsa * Math.Exp(-da * sigma_s));
        }

        /* Get the closest colour value within a radius of a pixel */
        private Vector2 GetClosestColourNeighbour(int initialX, int initialY, int radius = 5)
        {
            Color targetColour = originalSkyImage.GetPixel(initialX, initialY);
            int closestColourMatch = int.MaxValue;
            Vector2 closestMatchPos = new Vector2(0, 0);
            for (int x = initialX - radius; x < initialX + radius; x++)
            {
                for (int y = initialY - radius; y < initialY + radius; y++)
                {
                    if (x == initialX && y == initialY) continue;
                    if (x < 0 || y < 0) continue;
                    if (x > originalSkyImage.Width || y > originalSkyImage.Height) continue;

                    Color thisColour = originalSkyImage.GetPixel(x, y);
                    if (ColourDiff(targetColour, thisColour) < closestColourMatch)
                    {
                        closestMatchPos.x = x;
                        closestMatchPos.y = y;
                    }
                }
            }
            return closestMatchPos;
        }

        /* Get the difference in colour values between two given colours */
        private int ColourDiff(Color colour1, Color colour2)
        {
            int r = colour1.R - colour2.R;
            if (r < 0) r *= -1;
            int g = colour1.G - colour2.G;
            if (g < 0) g *= -1;
            int b = colour1.B - colour2.B;
            if (b < 0) b *= -1;
            return r + g + b;
        }

        /* Get the luma value for a given colour */
        private double GetLuma(Color colour)
        {
            return (0.2126 * colour.R) + (0.7152 * colour.G) + (0.0722 * colour.B);
        }

        /* Calculate the sigma value from a given point using the de-warped classified image */
        private double GetSigmaFromClassified(Vector2 point)
        {
            Color classifiedColour = classifiedSkyImage.GetPixel((int)point.x, (int)point.y);

            //NULL
            if (classifiedColour == Color.Black) return 0.0;
            //STRATOCUMULUS
            else if (classifiedColour.R == 255 && classifiedColour.G == 0 && classifiedColour.B == 255) return 0.1222340 + 0.0000000844671;
            //CUMULUS
            else if (classifiedColour.R == 255 && classifiedColour.G == 0 && classifiedColour.B == 0) return 0.0814896 + 0.000000110804;
            //CIRRUS
            else if (classifiedColour.R == 0 && classifiedColour.G == 255 && classifiedColour.B == 0) return 0.1661800 + 0.000000001;
            //CLEAR_SKY
            else return 0.0; 
        }
    }
}
