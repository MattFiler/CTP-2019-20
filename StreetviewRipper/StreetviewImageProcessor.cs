using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StreetviewRipper
{
    class Vector2
    {
        public Vector2(float _x, float _y)
        {
            x = _x;
            y = _y;
        }
        public float x;
        public float y;
    }

    class GroundInfo
    {
        public Vector2 position;
        public int block_width;
    }

    class PixelInfo
    {
        public float brightness;
        public int pos;
    }

    class StreetviewImageProcessor
    {
        /* Try and remove the ground from the image */
        public Image CutOutSky(Image sphere, int res, int acc)
        {
            //Find positions to use
            List<GroundInfo> positions = new List<GroundInfo>();
            int posOffset = 0;
            if (res > sphere.Width) res = sphere.Width;
            for (int i = 0; i < res; i++)
            {
                GroundInfo thisGround = new GroundInfo();
                thisGround.position = GuessGroundPos(posOffset, (Bitmap)sphere, acc);
                thisGround.block_width = sphere.Width / res;
                positions.Add(thisGround);
                posOffset += thisGround.block_width;
            }
            positions = positions.OrderBy(o => o.position.x).ToList();

            //Rebuild sphere
            Bitmap origSphere = (Bitmap)sphere;
            Bitmap newSphere = new Bitmap(origSphere.Width, origSphere.Height);
            int thisIndex = 0;
            int nextIndex = 1;
            int xSince = 0;
            for (int x = 0; x < sphere.Width; x++)
            {
                xSince++;
                int thisY = (int)positions[thisIndex].position.y - (int)(((positions[thisIndex].position.y - positions[nextIndex].position.y) / positions[thisIndex].block_width) * xSince);
                for (int y = 0; y < thisY; y++)
                {
                    Color thisPixel = origSphere.GetPixel(x, y);
                    newSphere.SetPixel(x, y, thisPixel);
                }
                if (thisIndex + 1 < positions.Count && x > positions[thisIndex + 1].position.x)
                {
                    thisIndex++;
                    nextIndex++;
                    xSince = 0;
                    if (nextIndex >= positions.Count) nextIndex = 0;
                }
            }
            return newSphere;
        }
        private Vector2 GuessGroundPos(int x, Bitmap sphere, int acc)
        {
            List<PixelInfo> p = new List<PixelInfo>();
            for (int y = 0; y < sphere.Height - sphere.Height/5; y++)
            {
                PixelInfo pi = new PixelInfo();
                Color pixel = sphere.GetPixel(x, y);
                pi.brightness = pixel.GetBrightness();
                pi.pos = y;
                p.Add(pi);
            }
            p = p.OrderBy(o => o.brightness).ToList();
            if (p.Count == 0) return new Vector2(0, 0);
            int guessY = 0;
            if (acc > sphere.Height) acc = sphere.Height;
            for (int i = 0; i < sphere.Height / acc; i++)
            {
                guessY += p[i].pos;
            }
            guessY /= sphere.Height / acc;
            return new Vector2(x, guessY);
        }

        /* Guess the X position of the sun in the image */
        public int GetSunXPos(Image sphere)
        {
            int guess1 = GuessSunPos(sphere.Height / 4, (Bitmap)sphere);
            int guess2 = GuessSunPos(sphere.Height / 3, (Bitmap)sphere);
            if (guess2 - guess1 > sphere.Width / 2 || guess1 - guess2 > sphere.Width / 2) return guess1;
            return (guess1 + guess2) / 2;
        }
        private int GuessSunPos(int y, Bitmap sphere)
        {
            List<PixelInfo> p = new List<PixelInfo>();
            for (int x = 0; x < sphere.Width; x++)
            {
                PixelInfo pi = new PixelInfo();
                Color pixel = sphere.GetPixel(x, y);
                pi.brightness = pixel.GetBrightness();
                pi.pos = x;
                p.Add(pi);
            }
            p = p.OrderByDescending(o => o.brightness).ToList();
            if (p.Count == 0) return 0;
            int guessX = 0;
            for (int i = 0; i < sphere.Width / 70; i++)
            {
                guessX += p[i].pos;
            }
            guessX /= sphere.Width / 70;
            return guessX;
        }
    }
}
