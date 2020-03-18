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

            //TODO IMPLEMENT FORMULA HERE ON VALUES
            //float da = 
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
        private int ColourDiff(Color color1, Color color2)
        {
            int r = color1.R - color2.R;
            if (r < 0) r *= -1;
            int g = color1.G - color2.G;
            if (g < 0) g *= -1;
            int b = color1.B - color2.B;
            if (b < 0) b *= -1;
            return r + g + b;
        }
    }
}
