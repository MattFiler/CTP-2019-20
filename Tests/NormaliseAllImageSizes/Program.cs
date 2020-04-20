using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NormaliseAllImageSizes
{
    class Program
    {
        static void Main(string[] args)
        {
            Directory.CreateDirectory("NormalisedSizes");

            string[] files = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.STREETVIEW_LDR.png", SearchOption.AllDirectories);
            Point biggestSize = new Point(0, 0);
            foreach (string file in files)
            {
                Bitmap thisImg = (Bitmap)Image.FromFile(file);
                if (thisImg.Width > biggestSize.X) biggestSize.X = thisImg.Width;
                if (thisImg.Height > biggestSize.Y) biggestSize.Y = thisImg.Height;
            }
            foreach (string file in files)
            { 
                string file_PANOID = Path.GetFileName(file).Split(new string[] { "_CLOUD_" }, StringSplitOptions.None)[0];
                string file_CLOUDID = Path.GetFileName(file).Split(new string[] { "_CLOUD_" }, StringSplitOptions.None)[1].Split('.')[0];

                Bitmap thisImg = (Bitmap)Image.FromFile(file_PANOID + "_CLOUD_" + file_CLOUDID + ".STREETVIEW_LDR.png");
                Bitmap thisImgMask = (Bitmap)Image.FromFile(file_PANOID + "_CLOUD_" + file_CLOUDID + ".CLOUD_MASK.png");

                Bitmap thisImgResize = new Bitmap(biggestSize.X, biggestSize.Y);
                for (int x = 0; x < thisImgResize.Width; x++)
                {
                    for (int y = 0; y < thisImgResize.Height; y++)
                    {
                        if (x < thisImg.Width && y < thisImg.Height)
                        {
                            thisImgResize.SetPixel(x, y, thisImg.GetPixel(x, y));
                        }
                        else
                        {
                            thisImgResize.SetPixel(x, y, Color.Black);
                        }
                    }
                }
                Bitmap thisImgMaskResize = new Bitmap(biggestSize.X, biggestSize.Y);
                for (int x = 0; x < thisImgMaskResize.Width; x++)
                {
                    for (int y = 0; y < thisImgMaskResize.Height; y++)
                    {
                        if (x < thisImgMask.Width && y < thisImgMask.Height)
                        {
                            thisImgMaskResize.SetPixel(x, y, thisImgMask.GetPixel(x, y));
                        }
                        else
                        {
                            thisImgMaskResize.SetPixel(x, y, Color.Black);
                        }
                    }
                }

                thisImgResize.Save("NormalisedSizes/" + file_PANOID + "_CLOUD_" + file_CLOUDID + ".STREETVIEW_LDR.png", System.Drawing.Imaging.ImageFormat.Png);
                thisImgMaskResize.Save("NormalisedSizes/" + file_PANOID + "_CLOUD_" + file_CLOUDID + ".CLOUD_MASK.png", System.Drawing.Imaging.ImageFormat.Png);
            }
        }
    }
}
