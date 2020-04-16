using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudPuller
{
    /* A representation of a HDR image */
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
        public void Open(string filename, bool can_reparse_hdr = true, bool can_reparse_exr = true)
        {
            //Check sanity of input
            if (!(Path.GetExtension(filename) == ".hdr" || Path.GetExtension(filename) == ".exr"))
            {
                throw new System.FormatException("Trying to load a non-HDR image!");
            }
            if (!File.Exists(filename))
            {
                throw new System.FormatException("Requested to open a file that doesn't exist!");
            }

            //If given an EXR, we need it in HDR format
            if (Path.GetExtension(filename) == ".exr")
            {
                if (!can_reparse_exr) throw new System.FormatException("Failed to load HDR, incorrect format!");
                string HDRConverterPath = @"D:\Github Repos\CTP-2019-20\Libraries\EXR2HDR\";
                File.Copy(filename, HDRConverterPath + "input.exr", true);
                ProcessStartInfo processInfo = new ProcessStartInfo("cmd.exe", "/c \"" + HDRConverterPath + "run.bat\"");
                processInfo.WorkingDirectory = HDRConverterPath;
                processInfo.CreateNoWindow = true;
                processInfo.UseShellExecute = false;
                Process process = Process.Start(processInfo);
                process.WaitForExit();
                process.Close();
                File.Delete(HDRConverterPath + "input.exr");
                if (!File.Exists(HDRConverterPath + "output.hdr")) throw new System.FormatException("Failed to convert EXR to correct format!");
                Open(HDRConverterPath + "output.hdr", true, false);
                File.Delete(HDRConverterPath + "output.hdr");
                return;
            }

            BinaryReader InFile = new BinaryReader(File.OpenRead(filename));

            //Read the header
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
                if (!can_reparse_hdr) throw new System.FormatException("Failed to load HDR, incorrect format!");
                string HDRConverterPath = @"D:\Github Repos\CTP-2019-20\Libraries\HDRConverter\";
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
                InFile.Close();
                return;
            }

            //Read RGBE data
            for (int i = 0; i < (InFile.BaseStream.Length - headerLen) / 4; i++)
            {
                HDRPixel newPixel = new HDRPixel();
                byte[] values = InFile.ReadBytes(4);
                newPixel.R = (int)values[0];
                newPixel.G = (int)values[1];
                newPixel.B = (int)values[2];
                newPixel.E = (int)values[3];
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

    /* An integer RGBE representation of a pixel in a HDR image */
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

        public HDRPixelFloat AsFloat()
        {
            HDRPixelFloat asFloat = new HDRPixelFloat();
            asFloat.FromRGBE(R, G, B, E);
            return asFloat;
        }
    }

    /* An RGB floating point representation of a pixel in a HDR image */
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
}
