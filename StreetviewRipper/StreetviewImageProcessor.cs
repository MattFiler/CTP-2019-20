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
        public bool discredit = false;
    }

    class PixelInfo
    {
        public PixelInfo(float _b, int _p)
        {
            brightness = _b;
            pos = _p;
        }
        public float brightness;
        public int pos;
    }

    class PrevBest
    {
        public PrevBest(float _d, int _p)
        {
            diff = _d;
            pos = _p;
        }
        public float diff;
        public int pos;
    }

    enum StraightLineBias
    {
        TOP,
        MIDDLE,
        BOTTOM
    }

    //TODO: Implement BackgroundWorker to multithread this process.
    class StreetviewImageProcessor
    {
        /* Trim the image by the given ground positions */
        public Image CutOutSky(Image sphere, List<GroundInfo> positions)
        {
            //Work out the height of the final image 
            float finalY = 0;
            for (int i = 0; i < positions.Count; i++)
            {
                if (positions[i].position.y >= finalY) finalY = positions[i].position.y;
            }

            //Rebuild sphere
            Bitmap origSphere = (Bitmap)sphere;
            Bitmap newSphere = new Bitmap(origSphere.Width, (int)finalY + 1);
            int thisIndex = 0;
            int nextIndex = 1;
            int xSince = 0;
            for (int x = 0; x < sphere.Width; x++)
            {
                xSince++;
                int thisY = (int)positions[thisIndex].position.y - (int)(((positions[thisIndex].position.y - positions[nextIndex].position.y) / positions[thisIndex].block_width) * xSince);
                if (thisY > finalY + 1) thisY = (int)finalY; //worrying
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
                    if (nextIndex >= positions.Count) nextIndex--;
                }
            }
            return newSphere;
        }

        /* Try and guess ground positions across an image */
        public List<GroundInfo> GuessGroundPositions(Image sphere, int acc, bool straight, StraightLineBias bias)
        {
            //Work out the classifier between ground and sky
            float skyClassifier = TakeAverageBrightness(sphere.Height / 6, (Bitmap)sphere);
            float groundClassifier = TakeAverageBrightness(sphere.Height - (sphere.Height / 6), (Bitmap)sphere);
            float diffClassifier = skyClassifier - groundClassifier;

            //First pass of ground position guess, check every column
            List<GroundInfo> positions = new List<GroundInfo>();
            int posOffset = 0;
            for (int i = 0; i < sphere.Width; i++)
            {
                GroundInfo thisGround = new GroundInfo();
                thisGround.position = GuessGroundPositionForX(posOffset, (Bitmap)sphere, diffClassifier);
                thisGround.block_width = 1;
                positions.Add(thisGround);
                posOffset += thisGround.block_width;
            }
            positions = positions.OrderBy(o => o.position.x).ToList();

            //Discredit any outliers on first pass
            List<GroundInfo> positions_new = new List<GroundInfo>();
            int prevDiscredit = 0;
            int checkRadius = 1;
            for (int i = checkRadius; i < positions.Count - checkRadius; i++)
            {
                if (positions_new.Count != 0)
                {
                    float prevDif = positions[i].position.y - positions[i - checkRadius].position.y;
                    if (prevDif < 0) prevDif *= -1;
                    float nextDif = positions[i].position.y - positions[i + checkRadius].position.y;
                    if (nextDif < 0) nextDif *= -1;
                    if (prevDif >= 50 || nextDif >= 50) //todo dont use a set value here, do it by sphere height
                    {
                        prevDiscredit++;
                        continue;
                    }
                    positions_new[positions_new.Count - 1].block_width += prevDiscredit;
                }
                positions_new.Add(positions[i]);
                prevDiscredit = 0;
            }
            positions = positions_new;

            //Second/third pass, get average of blocks from first pass, remove any outliers again, recalculate average
            positions_new = new List<GroundInfo>();
            for (int i = 1; i < positions.Count / acc; i++)
            {
                int startPos = acc * (i - 1);
                int endPos = acc * (i);
                List<float> yPos = new List<float>();
                float avgY = 0.0f;
                float avgX = 0.0f;
                for (int x = startPos; x < endPos; x++)
                {
                    yPos.Add(positions[x].position.y);
                    avgY += positions[x].position.y;
                    avgX += positions[x].position.x;
                }
                avgY /= acc;
                avgX /= acc;
                float avgY2 = 0.0f;
                int avgCount = 0;
                for (int x = 0; x < yPos.Count; x++)
                {
                    if (yPos[x] >= avgY)
                    {
                        avgY2 += yPos[x];
                        avgCount++;
                    }
                }
                avgY2 /= avgCount;
                GroundInfo newGndInf = new GroundInfo();
                newGndInf.block_width = acc;
                newGndInf.position = new Vector2(avgX, avgY2);
                positions_new.Add(newGndInf);
            }
            positions = positions_new;

            //If requested to trim as a straight line, take an average of all points, or pick top/bottom
            if (straight)
            {
                float avgY = 0.0f;
                switch (bias)
                {
                    case StraightLineBias.TOP:
                        avgY = float.MaxValue;
                        for (int i = 0; i < positions.Count; i++)
                        {
                            if (avgY >= positions[i].position.y) avgY = positions[i].position.y;
                        }
                        break;
                    case StraightLineBias.MIDDLE:
                        for (int i = 0; i < positions.Count; i++)
                        {
                            avgY += positions[i].position.y;
                        }
                        avgY /= positions.Count;
                        break;
                    case StraightLineBias.BOTTOM:
                        for (int i = 0; i < positions.Count; i++)
                        {
                            if (avgY <= positions[i].position.y) avgY = positions[i].position.y;
                        }
                        break;
                }
                positions.Clear();
                GroundInfo avgPos = new GroundInfo();
                avgPos.block_width = sphere.Width / 2;
                avgPos.position = new Vector2(0.0f, avgY);
                positions.Add(avgPos);
                avgPos.position = new Vector2(sphere.Width / 2, avgY);
                positions.Add(avgPos);
            }
            return positions;
        }

        /* Guess the ground position on the Y for a given X */
        int firstY = -1;
        private Vector2 GuessGroundPositionForX(int x, Bitmap sphere, float classifier)
        {
            List<float> pDiff = new List<float>();
            for (int y = 1; y < sphere.Height - sphere.Height / 5; y++)
            {
                pDiff.Add(sphere.GetPixel(x, y - 1).GetBrightness() - sphere.GetPixel(x, y).GetBrightness());
            }
            List<PrevBest> prevBest = new List<PrevBest>();
            int bestY = 0;
            float lastYd = float.MaxValue;
            for (int i = 0; i < pDiff.Count; i++)
            {
                float nD = pDiff[i] - classifier;
                if (nD < 0) nD *= -1;
                if (nD <= lastYd)
                {
                    prevBest.Add(new PrevBest(lastYd, bestY)); //store best guesses at ground by intelligent brightness diff check
                    bestY = i;
                    lastYd = nD;
                }
            }
            List<PrevBest> prevBestTrim = new List<PrevBest>();
            for (int i = 0; i < prevBest.Count; i++) 
            {
                if (prevBest[i].pos > sphere.Height / 4) //remove some guesses as we were finding our feet
                {
                    prevBestTrim.Add(prevBest[i]);
                }
            }
            if (prevBestTrim.Count != 0) prevBest = prevBestTrim;
            prevBest = prevBest.OrderBy(o => o.pos).ToList();
            prevBest.Reverse();
            if (x == 0)
            {
                for (int i = 0; i < prevBest.Count; i++)
                {
                    if (prevBest[i].pos <= sphere.Height / 1.8f) //first guess is dumb
                    {
                        firstY = prevBest[i].pos;
                        return new Vector2(x, prevBest[i].pos);
                    }
                }
                firstY = bestY;
                return new Vector2(x, bestY);
            }
            else
            {
                int bestI = 0;
                float lastId = float.MaxValue;
                for (int i = 0; i < prevBest.Count; i++)
                {
                    float yD = prevBest[i].pos - firstY;
                    if (yD < 0) yD *= -1;
                    if (yD <= lastId) //second guess onwards is based off of neighbour
                    {
                        bestI = i;
                        lastId = yD;
                    }
                }
                firstY = prevBest[bestI].pos;
                return new Vector2(x, prevBest[bestI].pos);
            }
        }

        /* Take the average brightness across each X pixel on a given Y */
        private float TakeAverageBrightness(int y, Bitmap sphere)
        {
            float avg = 0.0f;
            for (int x = 0; x < sphere.Width; x++)
            {
                avg += sphere.GetPixel(x, y).GetBrightness();
            }
            return avg / sphere.Width;
        }

        /* Guess the sun's position in the image (pass an image with cropped ground) */
        public Vector2 GuessSunPosition(Image sphere)
        {
            Bitmap sphereBMP = (Bitmap)sphere;

            List<PixelInfo> avgBrightX = new List<PixelInfo>();
            for (int x = 0; x < sphereBMP.Width; x++)
            {
                float thisAvgBrightness = 0.0f;
                for (int y = 0; y < sphereBMP.Height; y++)
                {
                    Color pixel = sphereBMP.GetPixel(x, y);
                    thisAvgBrightness += pixel.GetBrightness();
                }
                thisAvgBrightness /= sphereBMP.Height;
                avgBrightX.Add(new PixelInfo(thisAvgBrightness, x));
            }
            avgBrightX = avgBrightX.OrderByDescending(o => o.brightness).ToList();

            List<PixelInfo> avgBrightY = new List<PixelInfo>();
            for (int y = 0; y < sphereBMP.Height; y++)
            {
                float thisAvgBrightness = 0.0f;
                for (int x = 0; x < sphereBMP.Width; x++)
                {
                    Color pixel = sphereBMP.GetPixel(x, y);
                    thisAvgBrightness += pixel.GetBrightness();
                }
                thisAvgBrightness /= sphereBMP.Height;
                avgBrightY.Add(new PixelInfo(thisAvgBrightness, y));
            }
            avgBrightY = avgBrightY.OrderByDescending(o => o.brightness).ToList();

            return new Vector2(avgBrightX[0].pos, avgBrightY[0].pos);
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
                PixelInfo pi = new PixelInfo(0.0f, 0);
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
