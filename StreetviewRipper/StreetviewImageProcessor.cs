using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
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
    class Vector3
    {
        public Vector3(float _x, float _y, float _z)
        {
            x = _x;
            y = _y;
            z = _z;
        }
        public float x;
        public float y;
        public float z;
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

    class HDRImage
    {
        private int width;
        private int height;
        private HDRPixel[] pixels;

        /* Set the resolution of the HDR image */
        public void SetResolution(int w, int h)
        {
            width = w;
            height = h;
            pixels = new HDRPixel[width * height];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = new HDRPixel(0, 0, 0, 0);
            }
        }

        /* Get the resolution of the HDR image */
        public int Width
        {
            get { return width; }
        }
        public int Height
        {
            get { return height; }
        }

        /* Set a pixel by X,Y position */
        public void SetPixel(int x, int y, HDRPixel p)
        {
            pixels[(y * Width) + x] = p;
        }

        /* Get a pixel by X,Y position */
        public HDRPixel GetPixel(int x, int y)
        {
            return pixels[(y * Width) + x];
        }

        /* Load in a HDR file */
        public void Open(string filename, bool can_reparse = true)
        {
            if (Path.GetExtension(filename) != ".hdr")
            {
                throw new System.FormatException("Trying to load a non-HDR image!");
            }
            if (!File.Exists(filename))
            {
                throw new System.FormatException("Requested to open a file that doesn't exist!");
            }
            BinaryReader InFile = new BinaryReader(File.OpenRead(filename));

            string thisHeaderLine = "";
            while (true)
            {
                byte thisByte = InFile.ReadByte();
                if (thisByte == 0x0A)
                {
                    if (thisHeaderLine.Length >= 6 && thisHeaderLine.Substring(0, 6).ToUpper() == "FORMAT")
                    {
                        if (thisHeaderLine.Substring(7) != "32-bit_rle_rgbe")
                        {
                            throw new System.FormatException("Can only read 32-bit RGBE HDR images!");
                        }
                    }
                    else if (thisHeaderLine.Length >= 2 && thisHeaderLine.Substring(0, 2).ToUpper() == "-Y")
                    {
                        string[] sizeData = thisHeaderLine.Split(' ');
                        int Y = Convert.ToInt32(sizeData[1]);
                        int X = Convert.ToInt32(sizeData[3]);
                        SetResolution(X, Y);
                        break; //Resolution should always be last param!
                    }
                    thisHeaderLine = "";
                }
                else
                {
                    thisHeaderLine += (char)thisByte;
                }
            }
            int headerLen = (int)InFile.BaseStream.Position;

            //Some HDRs use scanline compression - convert it for us, and try again
            if (InFile.BaseStream.Length < (width * height * 4))
            {
                if (!can_reparse) throw new System.FormatException("Failed to load HDR, incorrect format!");
                string HDRConverterPath = AppDomain.CurrentDomain.BaseDirectory + Properties.Resources.Library_HDRConverter;
                File.Copy(filename, HDRConverterPath + "input.hdr", true);
                ProcessStartInfo processInfo = new ProcessStartInfo("cmd.exe", "/c \"" + HDRConverterPath + "run.bat\"");
                processInfo.WorkingDirectory = HDRConverterPath;
                processInfo.CreateNoWindow = true;
                processInfo.UseShellExecute = false;
                Process process = Process.Start(processInfo);
                process.WaitForExit();
                process.Close();
                File.Delete(HDRConverterPath + "input.hdr");
                if (!File.Exists(HDRConverterPath + "output.hdr")) throw new System.FormatException("Failed to convert HDR to correct format!");
                Open(HDRConverterPath + "output.hdr", false);
                File.Delete(HDRConverterPath + "output.hdr");
                return;
            }

            for (int i = 0; i < (InFile.BaseStream.Length - headerLen) / 4; i++)
            {
                HDRPixel newPixel = new HDRPixel();
                newPixel.R = (int)InFile.ReadByte();
                newPixel.G = (int)InFile.ReadByte();
                newPixel.B = (int)InFile.ReadByte();
                newPixel.E = (int)InFile.ReadByte();
                pixels[i] = newPixel;
            }

            InFile.Close();
        }

        /* Save out as a HDR file */
        public void Save(string filename)
        {
            if (File.Exists(filename)) File.Delete(filename);
            BinaryWriter OutFile = new BinaryWriter(File.OpenWrite(filename));
            OutFile.Write("#?RADIANCE".ToCharArray());
            OutFile.Write((byte)0x0A);
            OutFile.Write("FORMAT=32-bit_rle_rgbe".ToCharArray());
            OutFile.Write(new byte[] { 0x0A, 0x0A });
            OutFile.Write(("-Y " + height + " +X " + width).ToCharArray());
            OutFile.Write((byte)0x0A);
            for (int i = 0; i < height * width; i++)
            {
                OutFile.Write((byte)pixels[i].R);
                OutFile.Write((byte)pixels[i].G);
                OutFile.Write((byte)pixels[i].B);
                OutFile.Write((byte)pixels[i].E);
            }
            OutFile.Close();
        }
    }

    class HDRPixel
    {
        public int R;
        public int G;
        public int B;
        public int E;

        public HDRPixel() { }
        public HDRPixel(int _r, int _g, int _b, int _e)
        {
            R = _r;
            G = _g;
            B = _b;
            E = _e;
        }

        public void Set(int _r, int _g, int _b, int _e)
        {
            R = _r;
            G = _g;
            B = _b;
            E = _e;
        }
    }

    class HDRPixelFloat
    {
        private float r;
        private float g;
        private float b;
        private float l;

        public float R
        {
            get { return r; }
        }
        public float G
        {
            get { return g; }
        }
        public float B
        {
            get { return b; }
        }
        public float L
        {
            get { return l; }
        }

        public void FromRGBE(int _r, int _g, int _b, int _e)
        {
            float f = (float)C.math.ldexp(1.0, _e - (int)(128 + 8));
            r = _r * f;
            g = _g * f;
            b = _b * f;
            l = (r * 0.2126f) + (g * 0.7152f) + (b * 0.0722f);
        }

        public List<int> ToRGBE()
        {
            List<int> asRGBE = new List<int>();
            int e = 0;
            float v = (float)(C.math.frexp(r, ref e) * 256.0 / r);
            asRGBE.Add((int)(r * v));
            asRGBE.Add((int)(g * v));
            asRGBE.Add((int)(b * v));
            asRGBE.Add((int)(e + 128));
            return asRGBE;
        }
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

        /* Shift a spherical image by a set number of pixels to the left */
        public Bitmap ShiftImageLeft(Image sphere, int offset)
        {
            Bitmap origSphere = (Bitmap)sphere;
            Bitmap newSphere = new Bitmap(origSphere.Width, origSphere.Height);
            
            for (int x = 0; x < sphere.Width; x++)
            {
                int newX = x - offset;
                if (newX < 0) newX += sphere.Width;
                if (newX >= sphere.Width) newX -= sphere.Width;
                for (int y = 0; y < sphere.Height; y++)
                {
                    newSphere.SetPixel(newX, y, origSphere.GetPixel(x, y));
                }
            }
            return newSphere;
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

        /* Guess the sun's position in the image */
        public Vector2 GuessSunPosition(Image sphere, int groundY)
        {
            Bitmap sphereBMP = (Bitmap)sphere;

            //Work out the best guess X
            List<PixelInfo> avgBrightX = new List<PixelInfo>();
            for (int x = 0; x < sphereBMP.Width; x++)
            {
                float thisAvgBrightness = 0.0f;
                for (int y = 0; y < groundY; y++)
                {
                    Color pixel = sphereBMP.GetPixel(x, y);
                    thisAvgBrightness += pixel.GetBrightness();
                }
                thisAvgBrightness /= groundY;
                avgBrightX.Add(new PixelInfo(thisAvgBrightness, x));
            }
            avgBrightX = avgBrightX.OrderByDescending(o => o.brightness).ToList();
            float avgXPos = 0.0f;
            int maxIterations = sphereBMP.Width / 6;
            if (maxIterations < 1) maxIterations = 1;
            for (int i = 0; i < maxIterations; i++)
            {
                avgXPos += avgBrightX[i].pos;
            }
            avgXPos /= maxIterations;

            //Work out the best guess Y
            List<PixelInfo> avgBrightY = new List<PixelInfo>();
            for (int y = 0; y < groundY; y++)
            {
                float thisAvgBrightness = 0.0f;
                for (int x = 0; x < sphereBMP.Width; x++)
                {
                    Color pixel = sphereBMP.GetPixel(x, y);
                    thisAvgBrightness += pixel.GetBrightness();
                }
                thisAvgBrightness /= sphereBMP.Width;
                avgBrightY.Add(new PixelInfo(thisAvgBrightness, y));
            }
            avgBrightY = avgBrightY.OrderByDescending(o => o.brightness).ToList();
            float avgYPos = 0.0f;
            maxIterations = groundY / 3;
            if (maxIterations < 1) maxIterations = 1;
            for (int i = 0; i < maxIterations; i++)
            {
                avgYPos += avgBrightY[i].pos;
            }
            avgYPos /= maxIterations;

            return new Vector2(avgXPos, avgYPos);
        }
    }
}
