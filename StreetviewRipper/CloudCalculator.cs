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
        public CloudCalculator(Bitmap origImage, Bitmap classifiedImage)
        {
            originalSkyImage = origImage;
            classifiedSkyImage = classifiedImage;
        }

        public void RunInscatteringFormula()
        {
            //todo: do we really want to do this for every pixel?
            for (int x = 0; x < originalSkyImage.Width; x++) {
                for (int y = 0; y < originalSkyImage.Height; y++)
                {
                    CalculateFormulaForPoint(new Vector2(x, y));
                }
            }
        }

        private void CalculateFormulaForPoint(Vector2 point)
        {
            Vector2 closestPoint = GetClosestColourNeighbour((int)point.x, (int)point.y);

            Color thisColour = originalSkyImage.GetPixel((int)point.x, (int)point.y);
            Color closeColour = originalSkyImage.GetPixel((int)closestPoint.x, (int)closestPoint.y);

            float sigma_s = GetSigmaFromClassified(point);

            float La = GetLuma(thisColour);
            float Lb = GetLuma(closeColour);

            float Lsa = 0.0f; //TODO: what is this?
            float Lsb = 0.0f; //TODO: what is this?

            float da = (float)-Math.Log(Math.Abs((La - Lb) / (Lsa - Lsb))) / sigma_s;
        }
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
        private float GetLuma(Color colour)
        {
            return (0.2126f * colour.R) + (0.7152f * colour.G) + (0.0722f * colour.B);
        }

        private float GetSigmaFromClassified(Vector2 point)
        {
            Color classifiedColour = classifiedSkyImage.GetPixel((int)point.x, (int)point.y);
            if (classifiedColour == Color.Black) return 0.0f;

            //TODO: match colour in LDR image and return sigma value from colour match

            return 0.0f;
        }
    }
}
