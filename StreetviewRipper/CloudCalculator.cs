using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreetviewRipper
{
    class CalculatedInscatter
    {
        public double da;
        public Vector3 Lia;
    }

    class InscatteringResult
    {
        public Bitmap CloudDepthLocationDebug;
        public List<string> CloudDepthValueDebug;
        public Bitmap CloudInscatteringColourDebug;
    }

    class CloudCalculator
    {
        private HDRImage originalSkyImage;
        private Bitmap classifiedSkyImage;
        private HDRImage backgroundSkyImage;
        public CloudCalculator(HDRImage origImage, Bitmap classifiedImage, HDRImage hosekWilkieImage)
        {
            if (!(((origImage.Width == classifiedImage.Width) && (classifiedImage.Width == hosekWilkieImage.Width)) &&
                ((origImage.Height == classifiedImage.Height) && (classifiedImage.Height == hosekWilkieImage.Height))))
            {
                throw new System.FormatException("Image sizes do not match as expected!");
            }

            originalSkyImage = origImage;
            classifiedSkyImage = classifiedImage;
            backgroundSkyImage = hosekWilkieImage;
        }

        /* Run the calculations to calculate inscattering data */
        public InscatteringResult RunInscatteringFormula()
        {
            InscatteringResult toReturn = new InscatteringResult();
            toReturn.CloudDepthLocationDebug = new Bitmap(originalSkyImage.Width, originalSkyImage.Height);
            toReturn.CloudInscatteringColourDebug = new Bitmap(originalSkyImage.Width, originalSkyImage.Height);
            toReturn.CloudDepthValueDebug = new List<string>();

            for (int x = 0; x < originalSkyImage.Width; x++)
            {
                for (int y = 0; y < originalSkyImage.Height; y++)
                {
                    CalculatedInscatter returnedVal = CalculateForPoint(new Vector2(x, y));

                    //Depth value debug output
                    if (returnedVal.da != 0)
                    {
                        toReturn.CloudDepthLocationDebug.SetPixel(x, y, Color.White);
                        toReturn.CloudDepthValueDebug.Add("Value for da at (" + x + ", " + y + "): " + returnedVal.da);
                    }
                    else
                    {
                        toReturn.CloudDepthLocationDebug.SetPixel(x, y, Color.Black);
                    }

                    //Inscattering colour debug output
                    int final_r = (int)(255.0f * returnedVal.Lia.x);
                    if (final_r > 255) final_r = 255;
                    int final_g = (int)(255.0f * returnedVal.Lia.y);
                    if (final_g > 255) final_g = 255;
                    int final_b = (int)(255.0f * returnedVal.Lia.z);
                    if (final_b > 255) final_b = 255;
                    toReturn.CloudInscatteringColourDebug.SetPixel(x, y, Color.FromArgb(/*(int)(returnedVal.da * 255),*/ final_r, final_g, final_b));
                }
            }

            return toReturn;
        }

        /* Calculate light scattering value at point */
        private CalculatedInscatter CalculateForPoint(Vector2 point)
        {
            Vector2 closestPoint = GetClosestColourNeighbour((int)point.x, (int)point.y);
            double sigma_s = GetSigmaFromClassified(point);

            HDRPixelFloat thisColour = originalSkyImage.GetPixel((int)point.x, (int)point.y).AsFloat();
            HDRPixelFloat closeColour = originalSkyImage.GetPixel((int)closestPoint.x, (int)closestPoint.y).AsFloat();
            HDRPixelFloat thisBG = backgroundSkyImage.GetPixel((int)point.x, (int)point.y).AsFloat();
            HDRPixelFloat closeBG = backgroundSkyImage.GetPixel((int)closestPoint.x, (int)closestPoint.y).AsFloat();

            CalculatedInscatter toReturn = new CalculatedInscatter();

            double da_r = CalculateDaForColour(thisColour.R, closeColour.R, thisBG.R, closeBG.R, sigma_s);
            double da_g = CalculateDaForColour(thisColour.G, closeColour.G, thisBG.G, closeBG.G, sigma_s);
            double da_b = CalculateDaForColour(thisColour.B, closeColour.B, thisBG.B, closeBG.B, sigma_s);

            toReturn.da = (da_r + da_g + da_b) / 3;

            float Lia_r = (float)CalculateLiaForColour(thisColour.R, thisBG.R, toReturn.da, sigma_s);
            float Lia_g = (float)CalculateLiaForColour(thisColour.G, thisBG.G, toReturn.da, sigma_s);
            float Lia_b = (float)CalculateLiaForColour(thisColour.B, thisBG.B, toReturn.da, sigma_s);

            toReturn.Lia = new Vector3(Lia_r, Lia_g, Lia_b);

            return toReturn;
        }
        private double CalculateDaForColour(double La, double Lb, double Lsa, double Lsb, double sigma_s)
        {
            double toReturn = 0.0;
            try
            {
                toReturn = -Math.Log(Math.Abs((La - Lb) / (Lsa - Lsb))) / sigma_s;
            }
            catch { return 0.0f; }
            if (double.IsNaN(toReturn)) return 0.0f;
            if (double.IsInfinity(toReturn)) return 0.0f;
            return toReturn * -1;
        }
        private double CalculateLiaForColour(double La, double Lsa, double da, double sigma_s)
        {
            double toReturn = La - (Lsa * Math.Exp(-da * sigma_s));
            if (toReturn < 0) toReturn = 0;
            //if (toReturn > 255) toReturn = 255;
            return toReturn;
        }

        /* Get the closest colour value within a radius of a pixel */
        private Vector2 GetClosestColourNeighbour(int initialX, int initialY, int radius = 5)
        {
            HDRPixelFloat targetColour = originalSkyImage.GetPixel(initialX, initialY).AsFloat();
            float closestColourMatch = int.MaxValue;
            Vector2 closestMatchPos = new Vector2(0, 0);
            for (int x = initialX - radius; x < initialX + radius; x++)
            {
                for (int y = initialY - radius; y < initialY + radius; y++)
                {
                    if (x == initialX && y == initialY) continue;
                    if (x < 0 || y < 0) continue;
                    if (x >= originalSkyImage.Width || y >= originalSkyImage.Height) continue;

                    HDRPixelFloat thisColour = originalSkyImage.GetPixel(x, y).AsFloat();
                    float thisColourDiff = ColourDiff(targetColour, thisColour);
                    if (thisColourDiff < closestColourMatch)
                    {
                        closestMatchPos.x = x;
                        closestMatchPos.y = y;
                        closestColourMatch = thisColourDiff;
                    }
                }
            }
            return closestMatchPos;
        }

        /* Get the difference in colour values between two given colours */
        private float ColourDiff(HDRPixelFloat colour1, HDRPixelFloat colour2)
        {
            float r = colour1.R - colour2.R;
            if (r < 0) r *= -1;
            float g = colour1.G - colour2.G;
            if (g < 0) g *= -1;
            float b = colour1.B - colour2.B;
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
            else if (classifiedColour.R == 255 && classifiedColour.G == 0 && classifiedColour.B == 255)
            {
                return 0.1222340 + 0.0000000844671;
            }
            //CUMULUS
            else if (classifiedColour.R == 255 && classifiedColour.G == 0 && classifiedColour.B == 0)
            {
                return 0.0814896 + 0.000000110804;
            }
            //CIRRUS
            else if (classifiedColour.R == 0 && classifiedColour.G == 255 && classifiedColour.B == 0)
            {
                return 0.1661800 + 0.000000001;
            }
            //CLEAR_SKY
            else return 0.0;
        }
    }
}
