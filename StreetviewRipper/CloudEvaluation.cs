using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreetviewRipper
{
    /* Evaluate the cut out clouds to match them into best/ok/worst */
    class CloudEvaluation
    {
        static public void Evaluate()
        {
            string baseDir = Properties.Resources.Output_Images + "PulledClouds/";

            Directory.CreateDirectory(baseDir + "BestMatch");
            Directory.CreateDirectory(baseDir + "AlrightMatch");
            Directory.CreateDirectory(baseDir + "BadMatch");

            string[] files = Directory.GetFiles(baseDir, "*.STREETVIEW_LDR.png", SearchOption.TopDirectoryOnly);
            foreach (string file in files)
            {
                string file_PANOID = Path.GetFileName(file).Split(new string[] { "_CLOUD_" }, StringSplitOptions.None)[0];
                string file_CLOUDID = Path.GetFileName(file).Split(new string[] { "_CLOUD_" }, StringSplitOptions.None)[1].Split('.')[0];

                Point imgDims = new Point(0, 0);
                float lowestBrightness = int.MaxValue;
                float avgBrightness = 0.0f;
                float avgRed = 0.0f;
                float avgGreen = 0.0f;
                float avgBlue = 0.0f;
                using (Bitmap cloudImg = (Bitmap)Image.FromFile(file))
                {
                    for (int x = 0; x < cloudImg.Width; x++)
                    {
                        for (int y = 0; y < cloudImg.Height; y++)
                        {
                            Color thisPixel = cloudImg.GetPixel(x, y);
                            if (thisPixel.GetBrightness() < lowestBrightness) lowestBrightness = thisPixel.GetBrightness();
                            avgBrightness += thisPixel.GetBrightness();
                            avgRed += thisPixel.R;
                            avgGreen += thisPixel.G;
                            avgBlue += thisPixel.B;
                        }
                    }
                    imgDims.X = cloudImg.Width; imgDims.Y = cloudImg.Height;
                    avgBrightness /= (cloudImg.Width * cloudImg.Height);
                    avgRed /= (cloudImg.Width * cloudImg.Height);
                    avgGreen /= (cloudImg.Width * cloudImg.Height);
                    avgBlue /= (cloudImg.Width * cloudImg.Height);
                }

                if (avgBrightness > 0.5 && /*pixelsWithZeroBr < ((imgDims.X * imgDims.Y) / 6) &&*/ lowestBrightness > 0.2 && ((avgBlue >= avgGreen) && (avgBlue >= avgRed)))
                {
                    MoveAllFiles(file_PANOID, file_CLOUDID, baseDir + "BestMatch");
                }
                else if (avgBrightness > 0.5 && /*pixelsWithZeroBr < ((imgDims.X * imgDims.Y) / 6)*/ lowestBrightness > 0.2)
                {
                    MoveAllFiles(file_PANOID, file_CLOUDID, baseDir + "AlrightMatch");
                }
                else
                {
                    MoveAllFiles(file_PANOID, file_CLOUDID, baseDir + "BadMatch");
                }
            }
        }

        static private void MoveAllFiles(string panoID, string cloudID, string newFolderName)
        {
            string baseDir = Properties.Resources.Output_Images + "PulledClouds/";
            string thisPrefix = panoID + "_CLOUD_" + cloudID;
            File.Move(baseDir + thisPrefix + ".STREETVIEW_LDR.png", newFolderName + "/" + thisPrefix + ".STREETVIEW_LDR.png");
            File.Move(baseDir + thisPrefix + ".STREETVIEW_HDR.hdr", newFolderName + "/" + thisPrefix + ".STREETVIEW_HDR.hdr");
            File.Move(baseDir + thisPrefix + ".SKY_MODEL.hdr", newFolderName + "/" + thisPrefix + ".SKY_MODEL.hdr");
            File.Move(baseDir + thisPrefix + ".METADATA.bin", newFolderName + "/" + thisPrefix + ".METADATA.bin");
            File.Move(baseDir + thisPrefix + ".INSCATTER_COLOUR.png", newFolderName + "/" + thisPrefix + ".INSCATTER_COLOUR.png");
            File.Move(baseDir + thisPrefix + ".DEPTH_LOCATIONS.png", newFolderName + "/" + thisPrefix + ".DEPTH_LOCATIONS.png");
            File.Move(baseDir + thisPrefix + ".DEPTH.bin", newFolderName + "/" + thisPrefix + ".DEPTH.bin");
            File.Move(baseDir + thisPrefix + ".CLOUD_MASK.png", newFolderName + "/" + thisPrefix + ".CLOUD_MASK.png");
        }
    }
}
