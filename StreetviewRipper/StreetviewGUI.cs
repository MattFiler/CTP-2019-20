using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace StreetviewRipper
{
    public partial class StreetviewGUI : Form
    {
        int downloadCount = 0;
        int selectedQuality = 4;
        bool shouldStop = false;
        StraightLineBias selectedBias = StraightLineBias.MIDDLE;
        List<string> downloadedIDs = new List<string>();

        int sinceLastDownload = 0;
        int neighboursToSkip = 25;

        public StreetviewGUI()
        {
            InitializeComponent();
            imageQuality.SelectedIndex = selectedQuality;
            straightBias.SelectedIndex = (int)selectedBias;
            neighbourSkip.Value = neighboursToSkip;
        }

        /* Start downloading */
        private void downloadStreetview_Click(object sender, EventArgs e)
        {
            //Setup UI
            stopThreadedDownload.Enabled = true;
            downloadStreetview.Enabled = false;
            processImages.Enabled = false;
            doRecursion.Enabled = false;
            neighbourSkip.Enabled = false;
            straightBias.Enabled = false;
            imageQuality.Enabled = false;
            streetviewURL.Enabled = false;

            //Download all in list
            downloadCount = 0;
            UpdateDownloadCountText(downloadCount);
            selectedQuality = imageQuality.SelectedIndex;
            shouldStop = false;
            selectedBias = (StraightLineBias)straightBias.SelectedIndex;
            neighboursToSkip = (int)neighbourSkip.Value;
            sinceLastDownload = neighboursToSkip + 1;
            downloadedIDs.Clear();
            List<string> streetviewIDs = new List<string>();
            foreach (string thisURL in streetviewURL.Lines)
            {
                try
                {
                    //Get the streetview ID from string and download sphere if one is found
                    streetviewIDs.Add((thisURL.Split(new string[] { "!1s" }, StringSplitOptions.None)[1].Split(new string[] { "!2e" }, StringSplitOptions.None)[0]).Replace("%2F", "/"));
                }
                catch { }
            }
            Thread t = new Thread(() => StartDownloading(streetviewIDs));
            t.Start();
        }
        private void StartDownloading(List<string> ids)
        {
            //try
            //{
                foreach (string id in ids)
                {
                    if (id != "")
                    {
                        JArray neighbours = DownloadStreetview(id);
                        if (neighbours != null) DownloadNeighbours(neighbours);
                    }
                }
            //}
            //catch { }

            //Downloads are done, re-enable UI
            stoppingText.Visible = false;
            downloadStreetview.Enabled = true;
            stopThreadedDownload.Enabled = false;
            straightBias.Enabled = true;
            processImages.Enabled = true;
            doRecursion.Enabled = true;
            neighbourSkip.Enabled = true;
            imageQuality.Enabled = true;
            streetviewURL.Enabled = true;
            streetviewURL.Text = "";
        }

        /* Stop downloading */
        private void stopThreadedDownload_Click(object sender, EventArgs e)
        {
            shouldStop = true;
            stopThreadedDownload.Enabled = false;
            stoppingText.Visible = true;
        }

        /* Update download text (multithread support) */
        public void UpdateDownloadCountText(int value)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<int>(UpdateDownloadCountText), new object[] { value });
                return;
            }
            downloadTracker.Text = "Total: " + value;
        }
        public void UpdateDownloadStatusText(string value)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(UpdateDownloadStatusText), new object[] { value });
                return;
            }
            statusText.Text = "Currently: " + value;
        }

        /* Get metadata for a Streetview ID */
        private JToken GetMetadata(string id)
        {
            var request = WebRequest.Create("http://streetview.mattfiler.co.uk?panoid=" + id);
            using (var response = request.GetResponse())
            using (var reader = new System.IO.StreamReader(response.GetResponseStream(), ASCIIEncoding.ASCII))
            {
                return JToken.Parse(reader.ReadToEnd());
            }
        }

        /* Recurse into neighbours and download them, for a specified count */
        private void DownloadNeighbours(JArray neighbourInfos)
        {
            if (shouldStop) return;
            foreach (JObject thisNeighbour in neighbourInfos)
            {
                string thisID = thisNeighbour["id"].Value<string>();
                if (!downloadedIDs.Contains(thisID))
                {
                    JArray neighbours = DownloadStreetview(thisID);
                    if (neighbours == null) continue;
                    DownloadNeighbours(neighbours);
                }
            }
        }

        /* Download a complete sphere from Streetview at a globally defined quality */
        private JArray DownloadStreetview(string id)
        {
            UpdateDownloadStatusText("finished!");
            if (shouldStop) return null;
            UpdateDownloadStatusText("downloading streetview image LDR...");

            float LARGE_DEPTH_VALUE = 40.0f; //This is used as a default for zero depth values in a cloud

            //First up, here are all the files we'll be creating/dealing with
            string File_InitialLDR = Properties.Resources.Output_Images + id + ".jpg";
            string File_ShiftedLDRTrim = Properties.Resources.Output_Images + id + "_trim.jpg";
            string File_Metadata = Properties.Resources.Output_Images + id + ".json";
            string File_SkyHDR = Properties.Resources.Output_Images + id + "_sky.exr";
            string File_SkyHDRTrim = Properties.Resources.Output_Images + id + "_sky_trim.hdr";
            string File_ConvertedHDR = Properties.Resources.Output_Images + id + ".hdr";
            string File_TrimmedHDR = Properties.Resources.Output_Images + id + "_trim.hdr";
            string File_ClassifiedExtended = Properties.Resources.Output_Images + id + "_cloudmask.png";

            string File_CloudMapBinary = Properties.Resources.Output_Images + id + "_cloudmap.bin";
            string File_DepthValueBinary = Properties.Resources.Output_Images + id + "_depth.bin";

            string File_PBRTOutput = Properties.Resources.Library_PBRT + id + ".exr";
            string File_LDR2HDRInput = Properties.Resources.Library_LDR2HDR + "streetview.jpg";
            string File_LDR2HDROutput = Properties.Resources.Library_LDR2HDR + "streetview.hdr";
            string File_HDRUpscalerInputLDR = Properties.Resources.Library_HDRUpscaler + "input.jpg";
            string File_HDRUpscalerInputHDR = Properties.Resources.Library_HDRUpscaler + "input.hdr";
            string File_HDRUpscalerOutput = Properties.Resources.Library_HDRUpscaler + "output.hdr";
            string File_HDR2EXRInput = Properties.Resources.Library_HDR2EXR + "input.hdr";
            string File_HDR2EXROutput = Properties.Resources.Library_HDR2EXR + "output.exr";
            string File_ToFisheyeInput = Properties.Resources.Library_IM + "infile.hdr";
            string File_ToFisheyeOutput = Properties.Resources.Library_IM + "outfile.hdr";

            //And here are all the library executables we'll be using
            string Library_PBRT = AppDomain.CurrentDomain.BaseDirectory + Properties.Resources.Library_PBRT + "imgtool.exe";
            string Library_EXR2LDR = AppDomain.CurrentDomain.BaseDirectory + Properties.Resources.Library_EXR2LDR + "exr2ldr.exe";
            string Library_LDR2HDR = AppDomain.CurrentDomain.BaseDirectory + Properties.Resources.Library_LDR2HDR + "run.bat";
            string Library_Classifier = AppDomain.CurrentDomain.BaseDirectory + Properties.Resources.Library_Classifier + "Classify.exe";
            string Library_HDRUpscaler = AppDomain.CurrentDomain.BaseDirectory + Properties.Resources.Library_HDRUpscaler /*+ "hdr_upscaler.m"*/;
            string Library_HDRUpscaler_M = AppDomain.CurrentDomain.BaseDirectory + Properties.Resources.Library_HDRUpscaler + "hdr_upscaler.m";
            string Library_ToFisheye = AppDomain.CurrentDomain.BaseDirectory + Properties.Resources.Library_IM + "tools/convert.exe";
            string Library_HDR2EXR = AppDomain.CurrentDomain.BaseDirectory + Properties.Resources.Library_HDR2EXR /*+ "hdr2exr.py"*/;

            //Create any directories we'll need
            if (!Directory.Exists(Properties.Resources.Output_Images)) Directory.CreateDirectory(Properties.Resources.Output_Images);
            
            //Get metadata
            downloadedIDs.Add(id);
            JToken thisMeta = GetMetadata(id);
            if (thisMeta["error"].Value<string>() != "") return null;
            int tileWidth = thisMeta["tile_size"][0].Value<int>();
            int tileHeight = thisMeta["tile_size"][1].Value<int>();

            //Should we continue to process, or skip this? (we skip some neighbours for processing to get a wider sample)
            if (sinceLastDownload < neighboursToSkip)
            {
                UpdateDownloadStatusText("skipped!");
                sinceLastDownload++;
                if (doRecursion.Checked) return thisMeta["neighbours"].Value<JArray>();
                else return null;
            }
            sinceLastDownload = 1;

            //Load every tile
            int xOffset = 0;
            int yOffset = 0;
            bool stop = false;
            List<StreetviewTile> streetviewTiles = new List<StreetviewTile>();
            for (int y = 0; y < int.MaxValue; y++)
            {
                for (int x = 0; x < int.MaxValue; x++)
                {
                    StreetviewTile newTile = new StreetviewTile();

                    WebRequest request = WebRequest.Create(thisMeta["tile_url"].Value<string>().Replace("*X*", x.ToString()).Replace("*Y*", y.ToString()).Replace("*Z*", selectedQuality.ToString()));
                    try
                    {
                        using (WebResponse response = request.GetResponse())
                        using (Stream stream = response.GetResponseStream())
                        {
                            newTile.image = Bitmap.FromStream(stream);
                        }
                    }
                    catch
                    {
                        if (x == 0) stop = true;
                        break;
                    }
                    newTile.x = xOffset;
                    newTile.y = yOffset;

                    streetviewTiles.Add(newTile);
                    xOffset += tileWidth;
                }
                if (stop) break;
                yOffset += tileWidth;
                xOffset = 0;
            }

            //Cap zoom to the max available for this sphere (UGC varies)
            if (selectedQuality >= thisMeta["compiled_sizes"].Value<JArray>().Count) selectedQuality = thisMeta["compiled_sizes"].Value<JArray>().Count - 1;

            //Compile all image tiles to one whole image
            Bitmap streetviewImage = new Bitmap(thisMeta["compiled_sizes"][selectedQuality][0].Value<int>(), thisMeta["compiled_sizes"][selectedQuality][1].Value<int>());
            Graphics streetviewRenderer = Graphics.FromImage(streetviewImage);
            for (int i = 0; i < streetviewTiles.Count; i++)
            {
                streetviewRenderer.DrawImage(streetviewTiles[i].image, streetviewTiles[i].x, streetviewTiles[i].y, tileWidth, tileHeight);
            }
            streetviewRenderer.Dispose();
            streetviewImage.Save(File_InitialLDR, System.Drawing.Imaging.ImageFormat.Jpeg);

            //Only continue if the user chose to process images
            if (!processImages.Checked)
            {
                UpdateDownloadStatusText("finished!");
                downloadCount++;
                UpdateDownloadCountText(downloadCount);
                if (doRecursion.Checked) return thisMeta["neighbours"].Value<JArray>();
                else return null;
            }

            //Calculate metadata
            UpdateDownloadStatusText("calculating streetview metadata...");
            StreetviewImageProcessor processor = new StreetviewImageProcessor();
            List<GroundInfo> groundPositions = processor.GuessGroundPositions(streetviewImage, (selectedQuality * 5) + 15, true, (StraightLineBias)selectedBias);
            int groundY = (int)groundPositions[0].position.y; //We only have [0] and [1] when using straight line cutting, both have the same Y
            Vector2 sunPos = processor.GuessSunPosition(streetviewImage, groundY);

            //Write some of the metadata locally
            JToken localMeta = JToken.Parse("{}");
            if (thisMeta["road"].Value<string>() == null) localMeta["location"] = new JArray { "Unknown", "Unknown" };
            else localMeta["location"] = new JArray { thisMeta["road"].Value<string>(), thisMeta["region"].Value<string>() };
            localMeta["coordinates"] = new JArray { thisMeta["coordinates"][0].Value<double>(), thisMeta["coordinates"][1].Value<double>() };
            localMeta["date"] = new JArray { thisMeta["date"][1].Value<int>(), thisMeta["date"][0].Value<int>() };
            localMeta["resolution"] = new JArray { thisMeta["compiled_sizes"][selectedQuality][0].Value<int>(), thisMeta["compiled_sizes"][selectedQuality][1].Value<int>() };
            localMeta["history"] = thisMeta["history"];
            if (thisMeta["is_ugc"].Value<bool>()) localMeta["creator"] = thisMeta["creator"];
            else localMeta["creator"] = "Google";
            localMeta["ground_y"] = groundY;
            localMeta["sun"] = new JArray { (int)sunPos.x, (int)sunPos.y };
            File.WriteAllText(File_Metadata, localMeta.ToString(Formatting.Indented));

            //Shift the image to match Hosek-Wilkie sun position
            UpdateDownloadStatusText("adjusting streetview image LDR...");
            int shiftDist = (int)sunPos.x - (streetviewImage.Width / 4);
            streetviewImage = processor.ShiftImageLeft(streetviewImage, shiftDist);

            //Trim the ground from the adjusted image
            UpdateDownloadStatusText("cropping streetview image LDR...");
            Bitmap streetviewImageTrim = new Bitmap(thisMeta["compiled_sizes"][selectedQuality][0].Value<int>(), groundY);
            for (int x = 0; x < streetviewImageTrim.Width; x++)
            {
                for (int y = 0; y < streetviewImageTrim.Height; y++)
                {
                    streetviewImageTrim.SetPixel(x, y, streetviewImage.GetPixel(x, y));
                }
            }
            streetviewImageTrim.Save(File_ShiftedLDRTrim, System.Drawing.Imaging.ImageFormat.Jpeg);
            
            //Convert to HDR image
            UpdateDownloadStatusText("converting streetview to HDR...");
            streetviewImage.Save(File_LDR2HDRInput, System.Drawing.Imaging.ImageFormat.Jpeg);
            if (File.Exists(File_LDR2HDROutput)) File.Delete(File_LDR2HDROutput);

            ProcessStartInfo processInfo = new ProcessStartInfo("cmd.exe", "/c \"" + Library_LDR2HDR + "\"");
            processInfo.WorkingDirectory = GetPathWithoutFilename(Library_LDR2HDR);
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            Process process = Process.Start(processInfo);
            process.WaitForExit();
            process.Close();

            if (File.Exists(File_ConvertedHDR)) File.Delete(File_ConvertedHDR);
            if (File.Exists(File_LDR2HDROutput)) File.Move(File_LDR2HDROutput, File_ConvertedHDR);
            if (File.Exists(File_LDR2HDRInput)) File.Delete(File_LDR2HDRInput);

            //If we didn't get a HDR image back, the Python environment probably isn't installed properly
            if (!File.Exists(File_ConvertedHDR))
            {
                MessageBox.Show("Could not convert to HDR.\nCheck Conda environment!", "Conversion error.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                shouldStop = true;
                downloadCount++;
                UpdateDownloadCountText(downloadCount);
                UpdateDownloadStatusText("failed on HDR generation stage!");
                return null;
            }

            //Upscale the HDR output from LDR2HDR
            UpdateDownloadStatusText("upscaling streetview HDR...");
            if (File.Exists(File_HDRUpscalerInputHDR)) File.Delete(File_HDRUpscalerInputHDR);
            File.Copy(File_ConvertedHDR, File_HDRUpscalerInputHDR);
            streetviewImage.Save(File_HDRUpscalerInputLDR, System.Drawing.Imaging.ImageFormat.Jpeg);
            
            processInfo = new ProcessStartInfo(@"E:\Program Files\MATLAB\bin\matlab.exe", "-wait -r \"cd '" + Library_HDRUpscaler + "'; try, run ('" + Library_HDRUpscaler_M + "'); end; quit\"");
            processInfo.WorkingDirectory = @"E:\Program Files\MATLAB\bin";
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            process = Process.Start(processInfo);
            process.WaitForExit();
            process.Close();

            File.Delete(File_HDRUpscalerInputHDR);
            File.Delete(File_HDRUpscalerInputLDR);

            //If we didn't get the upscaled version back, MATLAB might not be installed
            if (!File.Exists(File_HDRUpscalerOutput))
            {
                MessageBox.Show("Could not upscale HDR.\nCheck MATLAB environment!", "Upscale error.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                shouldStop = true;
                downloadCount++;
                UpdateDownloadCountText(downloadCount);
                UpdateDownloadStatusText("failed on HDR upscale stage!");
                return null;
            }
            File.Delete(File_ConvertedHDR);
            File.Move(File_HDRUpscalerOutput, File_ConvertedHDR);

            //Read in HDR values
            UpdateDownloadStatusText("reading streetview HDR...");
            HDRImage hdrImage = new HDRImage();
            hdrImage.Open(File_ConvertedHDR);

            //Re-write the HDR image without the ground
            UpdateDownloadStatusText("cropping streetview HDR...");
            HDRImage hdrCropped = new HDRImage();
            hdrCropped.SetResolution(streetviewImageTrim.Width, streetviewImageTrim.Height);
            for (int x = 0; x < hdrCropped.Width; x++)
            {
                for (int y = 0; y < hdrCropped.Height; y++)
                {
                    hdrCropped.SetPixel(x, y, hdrImage.GetPixel(x, y));
                }
            }
            hdrCropped.Save(File_TrimmedHDR);
            
            //Work out our average blue value to match with Hosek Wilkie
            UpdateDownloadStatusText("calculating average HDR blue...");
            float avgBlue = 0.0f;
            for (int x = 0; x < hdrCropped.Width; x++)
            {
                for (int y = 0; y < hdrCropped.Height; y++)
                {
                    avgBlue += hdrCropped.GetPixel(x, y).AsFloat().B;
                }
            }
            avgBlue /= (hdrCropped.Width * hdrCropped.Height);

            //Create a bunch of Hosek Wilkie sky models, and we'll pick the closest to that blue value
            float turbidity02 = CreateAndEvaluateHosekWilkie(id, thisMeta["compiled_sizes"][selectedQuality][0].Value<int>(), sunPos.y, groundY, 2.0f, 0.5f);
            float turbidity04 = CreateAndEvaluateHosekWilkie(id, thisMeta["compiled_sizes"][selectedQuality][0].Value<int>(), sunPos.y, groundY, 4.0f, 0.5f);
            float turbidity06 = CreateAndEvaluateHosekWilkie(id, thisMeta["compiled_sizes"][selectedQuality][0].Value<int>(), sunPos.y, groundY, 6.0f, 0.5f);
            float turbidity08 = CreateAndEvaluateHosekWilkie(id, thisMeta["compiled_sizes"][selectedQuality][0].Value<int>(), sunPos.y, groundY, 8.0f, 0.5f);
            float turbidity10 = CreateAndEvaluateHosekWilkie(id, thisMeta["compiled_sizes"][selectedQuality][0].Value<int>(), sunPos.y, groundY, 10.0f, 0.5f);

            //Pick the closest
            UpdateDownloadStatusText("picking closest sky model...");
            float diff_turb02 = turbidity02 - avgBlue;
            if (diff_turb02 < 0) diff_turb02 *= -1;
            float diff_turb04 = turbidity04 - avgBlue;
            if (diff_turb04 < 0) diff_turb04 *= -1;
            float diff_turb06 = turbidity06 - avgBlue;
            if (diff_turb06 < 0) diff_turb06 *= -1;
            float diff_turb08 = turbidity08 - avgBlue;
            if (diff_turb08 < 0) diff_turb08 *= -1;
            float diff_turb10 = turbidity10 - avgBlue;
            if (diff_turb10 < 0) diff_turb10 *= -1;
            
            if (diff_turb02 < diff_turb04 &&
                diff_turb02 < diff_turb06 &&
                diff_turb02 < diff_turb08 &&
                diff_turb02 < diff_turb10)
            {
                File.Copy(File_SkyHDR.Substring(0, File_SkyHDR.Length - 4) + "_2_0.5.exr", File_SkyHDR, true);
            }
            else if (diff_turb04 < diff_turb02 &&
                     diff_turb04 < diff_turb06 &&
                     diff_turb04 < diff_turb08 &&
                     diff_turb04 < diff_turb10)
            {
                File.Copy(File_SkyHDR.Substring(0, File_SkyHDR.Length - 4) + "_4_0.5.exr", File_SkyHDR, true);
            }
            else if (diff_turb06 < diff_turb02 &&
                     diff_turb06 < diff_turb04 &&
                     diff_turb06 < diff_turb08 &&
                     diff_turb06 < diff_turb10)
            {
                File.Copy(File_SkyHDR.Substring(0, File_SkyHDR.Length - 4) + "_6_0.5.exr", File_SkyHDR, true);
            }
            else if (diff_turb08 < diff_turb02 &&
                     diff_turb08 < diff_turb04 &&
                     diff_turb08 < diff_turb06 &&
                     diff_turb08 < diff_turb10)
            {
                File.Copy(File_SkyHDR.Substring(0, File_SkyHDR.Length - 4) + "_8_0.5.exr", File_SkyHDR, true);
            }
            else
            {
                File.Copy(File_SkyHDR.Substring(0, File_SkyHDR.Length - 4) + "_10_0.5.exr", File_SkyHDR, true);
            }
            File.Delete(File_SkyHDR.Substring(0, File_SkyHDR.Length - 4) + "_2_0.5.exr");
            File.Delete(File_SkyHDR.Substring(0, File_SkyHDR.Length - 4) + "_4_0.5.exr");
            File.Delete(File_SkyHDR.Substring(0, File_SkyHDR.Length - 4) + "_6_0.5.exr");
            File.Delete(File_SkyHDR.Substring(0, File_SkyHDR.Length - 4) + "_8_0.5.exr");
            File.Delete(File_SkyHDR.Substring(0, File_SkyHDR.Length - 4) + "_10_0.5.exr");

            //Trim sky model to match sky image height
            UpdateDownloadStatusText("cropping sky model...");
            HDRImage skyModelHDR = new HDRImage();
            skyModelHDR.Open(File_SkyHDR);
            HDRImage skyModelHDRTrim = new HDRImage();
            skyModelHDRTrim.SetResolution(skyModelHDR.Width, groundY);
            for (int x = 0; x < skyModelHDRTrim.Width; x++)
            {
                for (int y = 0; y < skyModelHDRTrim.Height; y++)
                {
                    skyModelHDRTrim.SetPixel(x, y, skyModelHDR.GetPixel(x, y));
                }
            }
            skyModelHDRTrim.Save(File_SkyHDRTrim);

            //Try and guess stratocumulus clouds based on red/blue division
            UpdateDownloadStatusText("calculating cloud mask...");
            Bitmap cloudMaskV1 = new Bitmap(streetviewImageTrim.Width, streetviewImageTrim.Height);
            avgBlue = 0.0f; float avgRed = 0.0f; float avgGreen = 0.0f; float avgBright = 0.0f; float avgRBDiv = 0.0f;
            int divMod = 0;
            for (int x = 0; x < cloudMaskV1.Width; x++)
            {
                for (int y = 0; y < cloudMaskV1.Height; y++)
                {
                    Color thisSkyPixel = streetviewImage.GetPixel(x, y);
                    avgBlue += thisSkyPixel.B;
                    avgRed += thisSkyPixel.R;
                    avgGreen += thisSkyPixel.G;
                    avgBright += thisSkyPixel.GetBrightness();
                    if (thisSkyPixel.B == 0) continue;
                    avgRBDiv += (float)thisSkyPixel.R / (float)thisSkyPixel.B;
                    divMod++;
                }
            }
            avgBlue /= (float)(cloudMaskV1.Width * cloudMaskV1.Height);
            avgRed /= (float)(cloudMaskV1.Width * cloudMaskV1.Height);
            avgGreen /= (float)(cloudMaskV1.Width * cloudMaskV1.Height);
            avgBright /= (float)(cloudMaskV1.Width * cloudMaskV1.Height);
            avgRBDiv /= (float)(divMod);
            for (int x = 0; x < cloudMaskV1.Width; x++)
            {
                for (int y = 0; y < cloudMaskV1.Height; y++)
                {
                    Color thisSkyPixel = streetviewImage.GetPixel(x, y);
                    float redBlueDiv = 0.0f;
                    if (thisSkyPixel.B != 0) redBlueDiv = (float)thisSkyPixel.R / (float)thisSkyPixel.B;

                    bool check1 = thisSkyPixel.R > avgRed && thisSkyPixel.G > avgGreen && thisSkyPixel.B > avgBlue;
                    bool check2 = (redBlueDiv > (avgRBDiv + (avgRBDiv / 6.5f)));
                    bool check3 = thisSkyPixel.B > thisSkyPixel.G && thisSkyPixel.B > thisSkyPixel.R;

                    if (check2 || (check1 && !check3))
                    {
                        cloudMaskV1.SetPixel(x, y, Color.White);
                    }
                    else
                    {
                        cloudMaskV1.SetPixel(x, y, Color.Black);
                    }
                }
            }
            FloodFill(cloudMaskV1, cloudMaskV1.Width / 4, (int)sunPos.y, Color.Black);

            //Cut out the clouds from all our data, based on our cloud mask
            UpdateDownloadStatusText("checking cloud mask...");
            List<ValidCloudSquare> validCloudRegions = new List<ValidCloudSquare>();
            for (int x = 0; x < cloudMaskV1.Width; x++)
            {
                for (int y = 0; y < cloudMaskV1.Height; y++)
                {
                    //If this pixel isn't black, it's a cloud mask
                    Color thisPixel = cloudMaskV1.GetPixel(x, y);
                    if (!(thisPixel.R == 0 && thisPixel.G == 0 && thisPixel.B == 0))
                    {
                        //Double check this pixel isn't within a cloud bound we've already done
                        bool shouldCheck = true;
                        foreach (ValidCloudSquare thisArea in validCloudRegions)
                        {
                            if (thisArea.Contains(new Point(x, y)))
                            {
                                shouldCheck = false;
                                break;
                            }
                        }
                        if (!shouldCheck) continue;

                        //Work out the bounds of the cloud mask this pixel is within
                        FloodResult regionResult = ThisRegion(cloudMaskV1, x, y);
                        List<Point> linkedContents = regionResult.pointlist;
                        Point boundsTopLeft = GetMin(linkedContents);
                        Point boundsBottomRight = GetMax(linkedContents);
                        
                        //Work out the mask dimensions, and pull the section from our Streeview image
                        Point maskDims = new Point(boundsBottomRight.X - boundsTopLeft.X, boundsBottomRight.Y - boundsTopLeft.Y);
                        if (maskDims.X == 0 || maskDims.Y == 0) regionResult.shouldoutput = false;
                        if (maskDims.X <= 40 || maskDims.Y <= 40) regionResult.shouldoutput = false;
                        validCloudRegions.Add(
                            new ValidCloudSquare(
                                boundsTopLeft, 
                                boundsBottomRight, 
                                (regionResult.shouldoutput) ? PullRegionLDR(streetviewImage, boundsTopLeft, maskDims) : null, 
                                regionResult.shouldoutput
                            )
                        );
                    }
                }
            }

            //Evaluate the cut-out clouds
            UpdateDownloadStatusText("double-checking cloud mask...");
            foreach (ValidCloudSquare thisRegion in validCloudRegions)
            {
                if (!thisRegion.ShouldKeep) continue;

                Point imgDims = new Point(0, 0);
                float lowestBrightness = int.MaxValue;
                float avgBrightness = 0.0f;
                avgRed = 0.0f;
                avgGreen = 0.0f;
                avgBlue = 0.0f;

                for (int x = 0; x < thisRegion.StreetviewImg.Width; x++)
                {
                    for (int y = 0; y < thisRegion.StreetviewImg.Height; y++)
                    {
                        Color thisPixel = thisRegion.StreetviewImg.GetPixel(x, y);
                        if (thisPixel.GetBrightness() < lowestBrightness) lowestBrightness = thisPixel.GetBrightness();
                        avgBrightness += thisPixel.GetBrightness();
                        avgRed += thisPixel.R;
                        avgGreen += thisPixel.G;
                        avgBlue += thisPixel.B;
                    }
                }
                imgDims.X = thisRegion.StreetviewImg.Width; imgDims.Y = thisRegion.StreetviewImg.Height;
                avgBrightness /= (thisRegion.StreetviewImg.Width * thisRegion.StreetviewImg.Height);
                avgRed /= (thisRegion.StreetviewImg.Width * thisRegion.StreetviewImg.Height);
                avgGreen /= (thisRegion.StreetviewImg.Width * thisRegion.StreetviewImg.Height);
                avgBlue /= (thisRegion.StreetviewImg.Width * thisRegion.StreetviewImg.Height);

                thisRegion.ShouldKeep = (avgBrightness > 0.5 && /*pixelsWithZeroBr < ((imgDims.X * imgDims.Y) / 6) &&*/ lowestBrightness > 0.2 && ((avgBlue >= avgGreen) && (avgBlue >= avgRed)));
            }
            
            //Using the clouds we've determined are good enough, keep these bits of the mask and remove the others
            UpdateDownloadStatusText("refining cloud mask...");
            Bitmap cloudMaskV2 = new Bitmap(cloudMaskV1.Width, cloudMaskV1.Height);
            for (int x = 0; x < cloudMaskV2.Width; x++)
            {
                for (int y = 0; y < cloudMaskV2.Height; y++)
                {
                    cloudMaskV2.SetPixel(x, y, Color.Black);
                }
            }
            foreach (ValidCloudSquare thisRegion in validCloudRegions)
            {
                if (!thisRegion.ShouldKeep) continue;
                
                for (int x = 0; x < thisRegion.StreetviewImg.Width; x++)
                {
                    for (int y = 0; y < thisRegion.StreetviewImg.Height; y++)
                    {
                        cloudMaskV2.SetPixel(thisRegion.TopLeft.X + x, thisRegion.TopLeft.Y + y, cloudMaskV1.GetPixel(thisRegion.TopLeft.X + x, thisRegion.TopLeft.Y + y));
                    }
                }
            }
            cloudMaskV2.Save(File_ClassifiedExtended, System.Drawing.Imaging.ImageFormat.Png);
            
            //Apply the extra classification ontop of the original classifier output
            UpdateDownloadStatusText("saving cloud mask bin...");
            List<byte> binMapForUs = new List<byte>();
            BinaryWriter outputBinMap = new BinaryWriter(File.OpenWrite(File_CloudMapBinary));
            outputBinMap.BaseStream.SetLength(0);
            outputBinMap.Write(cloudMaskV2.Width);
            outputBinMap.Write(cloudMaskV2.Height);
            for (int x = 0; x < cloudMaskV2.Width; x++)
            {
                for (int y = 0; y < cloudMaskV2.Height; y++)
                {
                    Color thisColour = cloudMaskV2.GetPixel(x, y);
                    if (thisColour.R == 0 && thisColour.G == 0 && thisColour.B == 0)
                    {
                        outputBinMap.Write((byte)0);
                        binMapForUs.Add((byte)0);
                    }
                    else
                    {
                        outputBinMap.Write((byte)1);
                        binMapForUs.Add((byte)1);
                    }
                }
            }
            outputBinMap.Close();

            //Perform the inscattering equation on the de-fisheyed LDR
            UpdateDownloadStatusText("calculating inscattering and depth...");
            CloudCalculator inscatteringCalc = new CloudCalculator(hdrCropped, cloudMaskV2, skyModelHDRTrim); 
            InscatteringResult inscatterResult = inscatteringCalc.RunInscatteringFormula();

            //Output debug results from inscattering
            inscatterResult.CloudDepthLocationDebug.Save(Properties.Resources.Output_Images + id + "_inscatter_depth_debug.png", System.Drawing.Imaging.ImageFormat.Png);
            inscatterResult.CloudInscatteringColourDebug.Save(Properties.Resources.Output_Images + id + "_inscatter_colour_debug.png", System.Drawing.Imaging.ImageFormat.Png);
            File.WriteAllLines(Properties.Resources.Output_Images + id + "_inscatter_depth_debug.txt", inscatterResult.CloudDepthValueDebug);
            if (!(cloudMaskV2.Width == inscatterResult.CloudDepthLocationDebug.Width && cloudMaskV2.Height == inscatterResult.CloudDepthLocationDebug.Height))
            {
                //Don't think this should ever happen
                MessageBox.Show("Failed to calculate inscattering!", "Image size error.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                shouldStop = true;
                downloadCount++;
                UpdateDownloadCountText(downloadCount);
                UpdateDownloadStatusText("failed on inscatter final stage!");
                return null;
            }
            BinaryWriter outputDepthBin = new BinaryWriter(File.OpenWrite(File_DepthValueBinary));
            outputDepthBin.BaseStream.SetLength(0);
            outputDepthBin.Write(inscatterResult.CloudDepthLocationDebug.Width);
            outputDepthBin.Write(inscatterResult.CloudDepthLocationDebug.Height);
            for (int i = 0; i < inscatterResult.CloudDepthValueDebugActual.Count; i++)
            {
                float thisVal = inscatterResult.CloudDepthValueDebugActual[i];
                if (thisVal == 0 && binMapForUs[i] == (byte)1)
                {
                    thisVal = LARGE_DEPTH_VALUE; //We're in a cloud - it can't be zero
                }
                outputDepthBin.Write(thisVal);
            }
            outputDepthBin.Close();

            //Done!
            downloadCount++;
            UpdateDownloadCountText(downloadCount);
            UpdateDownloadStatusText("finished!");
            if (doRecursion.Checked) return thisMeta["neighbours"].Value<JArray>();
            else return null;
        }
        
        /* Produce a Hosek Wilkie sky model (TURBIDITY CAN BE 1.7-10, ALBEDO CAN BE 0-1) and return average blue value */
        private float CreateAndEvaluateHosekWilkie(string id, int qualityX, float sunPosY, float groundY, float skyTurbidity, float groundAlbedo = 0.5f)
        {
            string Library_PBRT = AppDomain.CurrentDomain.BaseDirectory + Properties.Resources.Library_PBRT + "imgtool.exe";
            string File_PBRTOutput = Properties.Resources.Library_PBRT + id + ".exr";
            string File_SkyHDR = Properties.Resources.Output_Images + id + "_sky_" + skyTurbidity + "_" + groundAlbedo + ".exr";
            float sunElevation = (sunPosY / groundY) * 90;

            UpdateDownloadStatusText("calculating sky model (T:" + skyTurbidity + ", A:" + groundAlbedo + ")...");
            if (File.Exists(File_PBRTOutput)) File.Delete(File_PBRTOutput);

            ProcessStartInfo processInfo = new ProcessStartInfo(Library_PBRT, "makesky --albedo " + groundAlbedo + " --elevation " + sunElevation + " --outfile " + id + ".exr --turbidity " + skyTurbidity + " --resolution " + (int)(qualityX / 2));
            processInfo.WorkingDirectory = GetPathWithoutFilename(Library_PBRT);
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            Process process = Process.Start(processInfo);
            process.WaitForExit();
            process.Close();

            if (!File.Exists(File_PBRTOutput)) return 0.0f;
            File.Copy(File_PBRTOutput, File_SkyHDR, true);
            File.Delete(File_PBRTOutput);

            //We evaluate here so the garbage collector knows the image is out of scope :)
            UpdateDownloadStatusText("evaluating sky model (T:" + skyTurbidity + ", A:" + groundAlbedo + ")...");
            HDRImage hosekWilkie = new HDRImage();
            hosekWilkie.Open(File_SkyHDR);
            float avgBlue = 0.0f;
            for (int x = 0; x < hosekWilkie.Width; x++)
            {
                for (int y = 0; y < groundY; y++) //Don't use Hosek Wilkie height here, since we don't work below the horizon
                {
                    avgBlue += hosekWilkie.GetPixel(x, y).AsFloat().B;
                }
            }
            avgBlue /= (hosekWilkie.Width * hosekWilkie.Height);
            return avgBlue;
        }

        /* Helper functions for dewarping fisheye in LDR or HDR (ref:https://github.com/crongjie/FisheyeToPanorama) */
        private Bitmap DewarpFisheyeLDR(Bitmap fisheye)
        {
            int width = fisheye.Width * 2;
            int height = fisheye.Height / 2;

            Bitmap result_image = new Bitmap(width, height);
            for (int w = 0; w < width; ++w)
            {
                for (int h = 0; h < height; ++h)
                {
                    double radius = height - h;
                    double theta = Math.PI * 2 / width * w * -1;

                    int x = Convert.ToInt32(radius * Math.Cos(theta) + height);
                    int y = Convert.ToInt32(height - radius * Math.Sin(theta));
                    if (x >= 0 && x < fisheye.Width && y >= 0 && y < fisheye.Height)
                    {
                        result_image.SetPixel(width - w - 1, height - h - 1, fisheye.GetPixel(x, y));
                    }
                }
            }
            return result_image;
        }
        private HDRImage DewarpFisheyeHDR(HDRImage fisheye)
        {
            int width = fisheye.Width * 2;
            int height = fisheye.Height / 2;

            HDRImage result_image = new HDRImage();
            result_image.SetResolution(width, height);
            for (int w = 0; w < width; ++w)
            {
                for (int h = 0; h < height; ++h)
                {
                    double radius = height - h;
                    double theta = Math.PI * 2 / width * w * -1;

                    int x = Convert.ToInt32(radius * Math.Cos(theta) + height);
                    int y = Convert.ToInt32(height - radius * Math.Sin(theta));
                    if (x >= 0 && x < fisheye.Width && y >= 0 && y < fisheye.Height)
                    {
                        result_image.SetPixel(width - w - 1, height - h - 1, fisheye.GetPixel(x, y));
                    }
                }
            }
            return result_image;
        }

        static Bitmap PullRegionLDR(Bitmap originalImage, Point topLeft, Point widthAndHeight)
        {
            Bitmap toReturn = new Bitmap(widthAndHeight.X, widthAndHeight.Y);
            for (int x = 0; x < widthAndHeight.X; x++)
            {
                for (int y = 0; y < widthAndHeight.Y; y++)
                {
                    toReturn.SetPixel(x, y, originalImage.GetPixel(topLeft.X + x, topLeft.Y + y));
                }
            }
            return toReturn;
        }
        static HDRImage PullRegionHDR(HDRImage originalImage, Point topLeft, Point widthAndHeight)
        {
            HDRImage toReturn = new HDRImage();
            toReturn.SetResolution(widthAndHeight.X, widthAndHeight.Y);
            for (int x = 0; x < widthAndHeight.X; x++)
            {
                for (int y = 0; y < widthAndHeight.Y; y++)
                {
                    toReturn.SetPixel(x, y, originalImage.GetPixel(topLeft.X + x, topLeft.Y + y));
                }
            }
            return toReturn;
        }
        static List<byte> PullRegionDEPTHBIN(List<byte> originalImage, Point topLeft, Point widthAndHeight, Point imageDims)
        {
            List<byte> toReturn = new List<byte>();
            for (int x = 0; x < widthAndHeight.X; x++)
            {
                for (int y = 0; y < widthAndHeight.Y; y++)
                {
                    toReturn.Add(originalImage[((topLeft.Y + y) * imageDims.X) + (topLeft.X + x)]);
                }
            }
            return toReturn;
        }

        /* thanks in part: https://stackoverflow.com/a/14897412 */
        static FloodResult ThisRegion(Bitmap bitmap, int x, int y)
        {
            BitmapData data = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            int[] bits = new int[data.Stride / 4 * data.Height];
            Marshal.Copy(data.Scan0, bits, 0, bits.Length);

            LinkedList<Point> check = new LinkedList<Point>();
            int floodTo = Color.Black.ToArgb();
            int floodFrom = bits[x + y * data.Stride / 4];
            bits[x + y * data.Stride / 4] = floodTo;

            FloodResult toReturn = new FloodResult();
            if (floodFrom != floodTo)
            {
                check.AddLast(new Point(x, y));
                toReturn.pointlist.Add(new Point(x, y));

                Stopwatch st = new Stopwatch();
                st.Start();
                while (check.Count > 0)
                {
                    if (st.Elapsed.Seconds >= 1) //Don't pull anything that's taking too long - going for quantity here!
                    {
                        toReturn.shouldoutput = false;
                        break;
                    }

                    Point cur = check.First.Value;
                    check.RemoveFirst();

                    foreach (Point off in new Point[] {
                    new Point(0, -1), new Point(0, 1),
                    new Point(-1, 0), new Point(1, 0)})
                    {
                        Point next = new Point(cur.X + off.X, cur.Y + off.Y);
                        if (next.X >= 0 && next.Y >= 0 &&
                            next.X < data.Width &&
                            next.Y < data.Height)
                        {
                            if (bits[next.X + next.Y * data.Stride / 4] == floodFrom)
                            {
                                check.AddLast(next);
                                if (!toReturn.pointlist.Contains(next)) toReturn.pointlist.Add(next);
                                bits[next.X + next.Y * data.Stride / 4] = floodTo;
                            }
                        }
                    }
                }
                st.Stop();
            }

            bitmap.UnlockBits(data);
            return toReturn;
        }

        static Point GetMin(List<Point> points)
        {
            Point topLeft = new Point(int.MaxValue, int.MaxValue);
            foreach (Point thisPoint in points)
            {
                if (topLeft.X > thisPoint.X)
                {
                    topLeft.X = thisPoint.X;
                }
                if (topLeft.Y > thisPoint.Y)
                {
                    topLeft.Y = thisPoint.Y;
                }
            }
            if (topLeft.X == int.MaxValue || topLeft.Y == int.MaxValue) return new Point(0, 0);
            return topLeft;
        }
        static Point GetMax(List<Point> points)
        {
            Point bottomRight = new Point(-int.MaxValue, -int.MaxValue);
            foreach (Point thisPoint in points)
            {
                if (bottomRight.X < thisPoint.X)
                {
                    bottomRight.X = thisPoint.X;
                }
                if (bottomRight.Y < thisPoint.Y)
                {
                    bottomRight.Y = thisPoint.Y;
                }
            }
            if (bottomRight.X == -int.MaxValue || bottomRight.Y == -int.MaxValue) return new Point(0, 0);
            return bottomRight;
        }

        /* Fill a region of colour in a bitmap (thanks: https://stackoverflow.com/a/14897412) */
        private void FloodFill(Bitmap bitmap, int x, int y, Color color)
        {
            BitmapData data = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            int[] bits = new int[data.Stride / 4 * data.Height];
            Marshal.Copy(data.Scan0, bits, 0, bits.Length);

            LinkedList<Point> check = new LinkedList<Point>();
            int floodTo = color.ToArgb();
            int floodFrom = bits[x + y * data.Stride / 4];
            bits[x + y * data.Stride / 4] = floodTo;

            if (floodFrom != floodTo)
            {
                check.AddLast(new Point(x, y));
                while (check.Count > 0)
                {
                    Point cur = check.First.Value;
                    check.RemoveFirst();

                    foreach (Point off in new Point[] {
                new Point(0, -1), new Point(0, 1),
                new Point(-1, 0), new Point(1, 0)})
                    {
                        Point next = new Point(cur.X + off.X, cur.Y + off.Y);
                        if (next.X >= 0 && next.Y >= 0 &&
                            next.X < data.Width &&
                            next.Y < data.Height)
                        {
                            if (bits[next.X + next.Y * data.Stride / 4] == floodFrom)
                            {
                                check.AddLast(next);
                                bits[next.X + next.Y * data.Stride / 4] = floodTo;
                            }
                        }
                    }
                }
            }

            Marshal.Copy(bits, 0, data.Scan0, bits.Length);
            bitmap.UnlockBits(data);
        }

        /* Work out what cloud value this pixel is closest to */
        private Color RoundClassifierColourValue(Color pixel)
        {
            int stratocumulus_diff = LDRPixelColourDiff(Color.FromArgb(255, 0, 255), pixel);
            int cumulus_diff = LDRPixelColourDiff(Color.FromArgb(255, 0, 0), pixel);
            int cirrus_diff = LDRPixelColourDiff(Color.FromArgb(0, 255, 0), pixel);
            int clearsky_diff = LDRPixelColourDiff(Color.FromArgb(0, 0, 255), pixel);
            int null_diff = LDRPixelColourDiff(Color.FromArgb(0, 0, 0), pixel);

            //STRATOCUMULUS
            if (stratocumulus_diff < cumulus_diff &&
                stratocumulus_diff < cirrus_diff &&
                stratocumulus_diff < clearsky_diff &&
                stratocumulus_diff < null_diff)
            {
                return Color.FromArgb(255, 0, 255);
            }

            //CUMULUS
            if (cumulus_diff < stratocumulus_diff &&
                cumulus_diff < cirrus_diff &&
                cumulus_diff < clearsky_diff &&
                cumulus_diff < null_diff)
            {
                return Color.FromArgb(255, 0, 0);
            }

            //CIRRUS
            if (cirrus_diff < stratocumulus_diff &&
                cirrus_diff < cumulus_diff &&
                cirrus_diff < clearsky_diff &&
                cirrus_diff < null_diff)
            {
                return Color.FromArgb(0, 255, 0);
            }

            //CLEAR_SKY
            if (clearsky_diff < stratocumulus_diff &&
                clearsky_diff < cumulus_diff &&
                clearsky_diff < cirrus_diff &&
                clearsky_diff < null_diff)
            {
                return Color.FromArgb(0, 0, 255);
            }

            //NULL
            return Color.Black;
        }
        private int LDRPixelColourDiff(Color colour1, Color colour2)
        {
            int r = colour1.R - colour2.R;
            if (r < 0) r *= -1;
            int g = colour1.G - colour2.G;
            if (g < 0) g *= -1;
            int b = colour1.B - colour2.B;
            if (b < 0) b *= -1;
            return r + g + b;
        }

        /* Resize an LDR bitmap (thanks: https://stackoverflow.com/a/24199315) */
        private Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        /* On load, validate our setup, and disable image processing options if external tools are unavailable. */
        private void StreetviewGUI_Load(object sender, EventArgs e)
        {
            Control.CheckForIllegalCrossThreadCalls = false; //We always do this safely.

            /*
            if (!Directory.Exists(Properties.Resources.Library_PBRT) ||
                !Directory.Exists(Properties.Resources.Library_LDR2HDR) ||
                !Directory.Exists(Properties.Resources.Library_EXR2LDR) ||
                !Directory.Exists(Properties.Resources.Library_Classifier))
            {
                processImages.Checked = false;
                processImages.Enabled = false;
                straightBias.Enabled = false;
                neighbourSkip.Enabled = false;
                cutCloudsOut.Enabled = false;
            }
            */
        }

        /* Change available options on check changed */
        private void processImages_CheckedChanged(object sender, EventArgs e)
        {
            straightBias.Enabled = processImages.Checked;
            neighbourSkip.Enabled = (processImages.Checked && doRecursion.Checked);
        }
        private void doRecursion_CheckedChanged(object sender, EventArgs e)
        {
            neighbourSkip.Enabled = (processImages.Checked && doRecursion.Checked);
        }

        /* Get a file path without filename from string */
        private string GetPathWithoutFilename(string fullPath)
        {
            try
            {
                return fullPath.Substring(0, fullPath.Length - Path.GetFileName(fullPath).Length);
            }
            catch
            {
                throw new FormatException("Couldn't get path from given filename.");
            }
        }
    }
}
