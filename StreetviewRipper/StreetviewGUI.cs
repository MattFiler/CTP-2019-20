using ImageMagick;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
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
        bool canEnableProcessing = true;
        StraightLineBias selectedBias = StraightLineBias.MIDDLE;
        List<string> downloadedIDs = new List<string>();

        public StreetviewGUI()
        {
            InitializeComponent();
            imageQuality.SelectedIndex = selectedQuality;
            straightBias.SelectedIndex = (int)selectedBias;
        }

        /* Start downloading */
        private void downloadStreetview_Click(object sender, EventArgs e)
        {
            //Setup UI
            stopThreadedDownload.Enabled = true;
            downloadStreetview.Enabled = false;
            processImages.Enabled = false;
            straightBias.Enabled = false;
            imageQuality.Enabled = false;
            streetviewURL.Enabled = false;

            //Download all in list
            downloadCount = 0;
            UpdateDownloadCountText(downloadCount);
            selectedQuality = imageQuality.SelectedIndex;
            shouldStop = false;
            selectedBias = (StraightLineBias)straightBias.SelectedIndex;
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
            try
            {
                foreach (string id in ids)
                {
                    if (id != "")
                    {
                        JArray neighbours = DownloadStreetview(id);
                        DownloadNeighbours(neighbours);
                    }
                }
            }
            catch { }

            //Downloads are done, re-enable UI
            stoppingText.Visible = false;
            downloadStreetview.Enabled = true;
            straightBias.Enabled = true;
            if (canEnableProcessing) processImages.Enabled = true;
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
            UpdateDownloadStatusText("downloading LDR image...");

            //First up, here are all the files we'll be creating/dealing with
            string File_InitialLDR = Properties.Resources.Output_Images + id + ".jpg";
            string File_ShiftedLDR = Properties.Resources.Output_Images + id + "_shifted.jpg";
            string File_DownscaledLDR = Properties.Resources.Output_Images + id + "_downscaled.jpg";
            string File_Metadata = Properties.Resources.Output_Images + id + ".json";
            string File_SkyHDR = Properties.Resources.Output_Images + id + "_sky.exr";
            string File_SkyLDR = Properties.Resources.Output_Images + id + "_sky.png";
            string File_SkyExtracted = Properties.Resources.Output_Images + id + "_removedsky.png";
            string File_ConvertedHDR = Properties.Resources.Output_Images + id + ".hdr";
            string File_TrimmedHDR = Properties.Resources.Output_Images + id + "_upscaled_trim.hdr";
            string File_ClassifiedHDR = Properties.Resources.Output_Images + id + "_classified.hdr";

            string File_PBRTOutput = Properties.Resources.Library_PBRT + id + ".exr";
            string File_LDR2HDRInput = Properties.Resources.Library_LDR2HDR + "streetview.jpg";
            string File_LDR2HDROutput = Properties.Resources.Library_LDR2HDR + "streetview.hdr";
            string File_HDRUpscalerInputLDR = Properties.Resources.Library_HDRUpscaler + "input.jpg";
            string File_HDRUpscalerInputHDR = Properties.Resources.Library_HDRUpscaler + "input.hdr";
            string File_HDRUpscalerOutput = Properties.Resources.Library_HDRUpscaler + "output.hdr";
            string File_ToFisheyeInput = Properties.Resources.Library_IM + "infile.hdr";
            string File_ToFisheyeOutput = Properties.Resources.Library_IM + "outfile.hdr";
            string File_ClassifierInput = Properties.Resources.Library_Classifier + "Input_Output_Files/" + id + ".hdr";
            string File_ClassifierOutput = Properties.Resources.Library_Classifier + "Input_Output_Files/" + id + "_classified.hdr";

            //And here are all the library executables we'll be using
            string Library_PBRT = AppDomain.CurrentDomain.BaseDirectory + Properties.Resources.Library_PBRT + "imgtool.exe";
            string Library_EXR2LDR = AppDomain.CurrentDomain.BaseDirectory + Properties.Resources.Library_EXR2LDR + "exr2ldr.exe";
            string Library_LDR2HDR = AppDomain.CurrentDomain.BaseDirectory + Properties.Resources.Library_LDR2HDR + "run.bat";
            string Library_Classifier = AppDomain.CurrentDomain.BaseDirectory + Properties.Resources.Library_Classifier + "Classify.exe";
            string Library_HDRUpscaler = AppDomain.CurrentDomain.BaseDirectory + Properties.Resources.Library_HDRUpscaler /*+ "hdr_upscaler.m"*/;
            string Library_ToFisheye = AppDomain.CurrentDomain.BaseDirectory + Properties.Resources.Library_IM + "tools/convert.exe";

            //Create any directories we'll need
            if (!Directory.Exists(Properties.Resources.Output_Images)) Directory.CreateDirectory(Properties.Resources.Output_Images);
            
            //Get metadata
            downloadedIDs.Add(id);
            JToken thisMeta = GetMetadata(id);
            if (thisMeta["error"].Value<string>() != "") return null;
            int tileWidth = thisMeta["tile_size"][0].Value<int>();
            int tileHeight = thisMeta["tile_size"][1].Value<int>();

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
                return thisMeta["neighbours"].Value<JArray>();
            }

            //Calculate metadata
            UpdateDownloadStatusText("calculating metadata...");
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
            UpdateDownloadStatusText("adjusting LDR image...");
            int shiftDist = (int)sunPos.x - (streetviewImage.Width / 4);
            streetviewImage = processor.ShiftImageLeft(streetviewImage, shiftDist);
            streetviewImage.Save(File_ShiftedLDR, System.Drawing.Imaging.ImageFormat.Jpeg);

            //Trim the ground from the adjusted image
            UpdateDownloadStatusText("cropping LDR image...");
            Bitmap streetviewImageTrim = new Bitmap(thisMeta["compiled_sizes"][selectedQuality][0].Value<int>(), groundY);
            for (int x = 0; x < streetviewImageTrim.Width; x++)
            {
                for (int y = 0; y < streetviewImageTrim.Height; y++)
                {
                    streetviewImageTrim.SetPixel(x, y, streetviewImage.GetPixel(x, y));
                }
            }

            //Create Hosek-Wilkie sky model for background
            UpdateDownloadStatusText("calculating sky model...");
            if (File.Exists(File_PBRTOutput)) File.Delete(File_PBRTOutput);

            float groundAlbedo = 0.5f; //TODO: set this between 0-1
            float sunElevation = (sunPos.y / groundY) * 90;
            float skyTurbidity = 3.0f; //TODO: set this between 1.7-10

            ProcessStartInfo processInfo = new ProcessStartInfo(Library_PBRT, "makesky --albedo " + groundAlbedo + " --elevation " + sunElevation + " --outfile " + id + ".exr --turbidity " + skyTurbidity + " --resolution " + (int)(thisMeta["compiled_sizes"][selectedQuality][0].Value<int>() / 2));
            processInfo.WorkingDirectory = GetPathWithoutFilename(Library_PBRT);
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            Process process = Process.Start(processInfo);
            process.WaitForExit();
            process.Close();

            if (File.Exists(File_PBRTOutput)) File.Copy(File_PBRTOutput, File_SkyHDR, true);
            if (File.Exists(File_PBRTOutput)) File.Delete(File_PBRTOutput);

            //Convert sky model to HDR from LDR
            UpdateDownloadStatusText("converting sky model...");
            processInfo = new ProcessStartInfo(Library_EXR2LDR, Path.GetFileName(File_SkyHDR) + " " + Path.GetFileName(File_SkyLDR));
            processInfo.WorkingDirectory = GetPathWithoutFilename(File_SkyHDR);
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            process = Process.Start(processInfo);
            process.WaitForExit();
            process.Close();

            //Cut the sky model out of the original LDR image
            UpdateDownloadStatusText("removing sky model from LDR...");
            Bitmap skyModel = (Bitmap)Image.FromFile(File_SkyLDR);
            Bitmap streetviewNoSky = new Bitmap(streetviewImageTrim.Width, streetviewImageTrim.Height);
            for (int x = 0; x < streetviewImageTrim.Width; x++)
            {
                for (int y = 0; y < streetviewImageTrim.Height; y++)
                {
                    Color skyPixel = skyModel.GetPixel(x, y);
                    Color originalPixel = streetviewImageTrim.GetPixel(x, y);

                    float newR = (float)(originalPixel.R - skyPixel.R) / 255.0f;
                    if (newR < 0) newR = 0;
                    float newG = (float)(originalPixel.G - skyPixel.G) / 255.0f;
                    if (newG < 0) newG = 0;
                    float newB = (float)(originalPixel.B - skyPixel.B) / 255.0f;
                    if (newB < 0) newB = 0;

                    float newA = (newR * 0.2126f) + (newG * 0.7152f) + (newB * 0.0722f);
                    int newAFinal = ((int)(newA * 255) - 255) * -1;
                    if (newAFinal < 0) newAFinal = 0;
                    if (newAFinal > 255) newAFinal = 255;
                    //int grayScale = (int)((originalPixel.R * .3) + (originalPixel.G * .59) + (originalPixel.B * .11));
                    Color newColor = Color.FromArgb(newAFinal, originalPixel.R, originalPixel.G, originalPixel.B);

                    streetviewNoSky.SetPixel(x, y, newColor);
                }
            }
            streetviewNoSky.Save(File_SkyExtracted, System.Drawing.Imaging.ImageFormat.Png);

            //Convert to HDR image
            UpdateDownloadStatusText("converting to HDR...");
            streetviewImage.Save(File_LDR2HDRInput, System.Drawing.Imaging.ImageFormat.Jpeg);
            if (File.Exists(File_LDR2HDROutput)) File.Delete(File_LDR2HDROutput);

            processInfo = new ProcessStartInfo("cmd.exe", "/c \"" + Library_LDR2HDR + "\"");
            processInfo.WorkingDirectory = GetPathWithoutFilename(Library_LDR2HDR);
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            process = Process.Start(processInfo);
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
            UpdateDownloadStatusText("upscaling HDR...");
            if (File.Exists(File_HDRUpscalerInputHDR)) File.Delete(File_HDRUpscalerInputHDR);
            File.Copy(File_ConvertedHDR, File_HDRUpscalerInputHDR);
            streetviewImage.Save(File_HDRUpscalerInputLDR, System.Drawing.Imaging.ImageFormat.Jpeg);

            MLApp.MLApp matlab = new MLApp.MLApp();
            matlab.Execute(@"cd '" + Library_HDRUpscaler + "'");
            object result = null;
            matlab.Feval("hdr_upscaler", 0, out result);
            matlab.Quit();

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
            UpdateDownloadStatusText("reading HDR...");
            HDRImage hdrImage = new HDRImage();
            hdrImage.Open(File_ConvertedHDR);

            //Re-write the HDR image without the ground
            UpdateDownloadStatusText("cropping HDR...");
            HDRImage hdrCropped = new HDRImage();
            hdrCropped.SetResolution(thisMeta["compiled_sizes"][selectedQuality][0].Value<int>(), groundY);
            for (int x = 0; x < hdrCropped.Width; x++)
            {
                for (int y = 0; y < hdrCropped.Height; y++)
                {
                    hdrCropped.SetPixel(x, y, hdrImage.GetPixel(x, y));
                }
            }
            hdrCropped.Save(File_TrimmedHDR);

            //Re-write the upscaled & cropped HDR image as a fisheye ready for classifying
            UpdateDownloadStatusText("converting to fisheye HDR...");
            File.Copy(File_TrimmedHDR, File_ToFisheyeInput, true);
            
            string FisheyeFolder = GetPathWithoutFilename(Library_ToFisheye);
            FisheyeFolder = FisheyeFolder.Substring(0, FisheyeFolder.Length - 6); //Remove "tools/"
            File.WriteAllText(FisheyeFolder + "run.bat", 
                //TODO implement the maths to calculate these magic numbers in script (take from pano2fisheye)
                "\"tools/convert.exe\" -quiet infile.hdr +repage -roll +" + (hdrCropped.Width/2) + "+0 -rotate 180 temp.mpc" + Environment.NewLine +
                "\"tools/convert.exe\" -size " + hdrCropped.Height + "x" + hdrCropped.Height + " xc: temp.mpc -virtual-pixel background -background black -monitor -fx \"xd=(i-779); yd=(j-779); rd=hypot(xd,yd); theta=atan2(yd,xd); phiang=asin(2*rd/1561); xs=3327+theta*1059.34; ys=1561-phiang*993.763; (rd>779)?black:v.p{xs,ys}\" +monitor -rotate -90 outfile.hdr");

            processInfo = new ProcessStartInfo("cmd.exe", "/c \"" + FisheyeFolder + "run.bat\"");
            processInfo.WorkingDirectory = FisheyeFolder;
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            process = Process.Start(processInfo);
            process.WaitForExit();
            process.Close();

            File.Delete(File_ToFisheyeInput);
            if (File.Exists(FisheyeFolder + "temp.mpc")) File.Delete(FisheyeFolder + "temp.mpc");
            if (File.Exists(FisheyeFolder + "temp.cache")) File.Delete(FisheyeFolder + "temp.cache");

            //If we didn't get a fisheye back, we probably don't have Bash or ImageMagick installed
            if (!File.Exists(File_ToFisheyeOutput))
            {
                MessageBox.Show("Could not distort to fisheye.\nCheck Bash/ImageMagick is installed!", "Conversion error.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                shouldStop = true;
                downloadCount++;
                UpdateDownloadCountText(downloadCount);
                UpdateDownloadStatusText("failed on fisheye conversion stage!");
                return null;
            }
            
            /*
            UpdateDownloadStatusText("converting to fisheye...");
            using (MagickImage image = new MagickImage(streetviewImage))
            {
                using (IMagickImage backgroundImg = image.Clone())
                {
                    backgroundImg.VirtualPixelMethod = VirtualPixelMethod.Background;
                    backgroundImg.BackgroundColor = Color.Black;
                    backgroundImg.Resize(4096, 4096);
                    backgroundImg.Fx("xd=(i-2047); yd=(j-2047); rd=hypot(xd,yd); theta=atan2(yd,xd); phiang=asin(2*rd/4096); xs=4095+theta*1303.8; ys=4096-phiang*2607.59; (rd>2047)?white:v.p{xs,ys}");
                    backgroundImg.Rotate(90);
                    backgroundImg.Write("test.png");
                }
            }
            return null;
            */

            //Classify the processed image
            UpdateDownloadStatusText("classifying cloud formations...");
            if (File.Exists(File_ClassifierInput)) File.Delete(File_ClassifierInput);
            if (File.Exists(File_ClassifierOutput)) File.Delete(File_ClassifierOutput);
            File.Move(File_ToFisheyeOutput, File_ClassifierInput);

            processInfo = new ProcessStartInfo(Library_Classifier, "5 400 100 0 " + id + " " + id + "_classified");
            processInfo.WorkingDirectory = GetPathWithoutFilename(Library_Classifier);
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            process = Process.Start(processInfo);
            process.WaitForExit();
            process.Close();

            if (File.Exists(File_ClassifiedHDR)) File.Delete(File_ClassifiedHDR);
            if (File.Exists(File_ClassifierOutput)) File.Move(File_ClassifierOutput, File_ClassifiedHDR);
            if (File.Exists(File_ClassifierInput)) File.Delete(File_ClassifierInput);

            //If we didn't get anything back from the classifier, we have a larger issue (memory?)
            if (!File.Exists(File_ClassifiedHDR))
            {
                MessageBox.Show("Failed to classify!", "Classifier error.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                shouldStop = true;
                downloadCount++;
                UpdateDownloadCountText(downloadCount);
                UpdateDownloadStatusText("failed on classifier stage!");
                return null;
            }

            //Pull classified clouds from the image & save them
            UpdateDownloadStatusText("extracting classified clouds...");
            HDRImage hdrClassified = new HDRImage();
            hdrClassified.Open(File_ClassifiedHDR);

            HDRUtilities hdrUtils = new HDRUtilities();
            Bitmap cutoutStratocumulus = hdrUtils.PullCloudType(hdrClassified, streetviewImageTrim, HDRUtilities.CloudTypes.STRATOCUMULUS);
            Bitmap cutoutCumulus = hdrUtils.PullCloudType(hdrClassified, streetviewImageTrim, HDRUtilities.CloudTypes.CUMULUS);
            Bitmap cutoutCirrus = hdrUtils.PullCloudType(hdrClassified, streetviewImageTrim, HDRUtilities.CloudTypes.CIRRUS);
            Bitmap cutoutClearSky = hdrUtils.PullCloudType(hdrClassified, streetviewImageTrim, HDRUtilities.CloudTypes.CLEAR_SKY);

            cutoutStratocumulus.Save(Properties.Resources.Output_Images + id + "_classified_stratocumulus.png", System.Drawing.Imaging.ImageFormat.Png);
            cutoutCumulus.Save(Properties.Resources.Output_Images + id + "_classified_cumulus.png", System.Drawing.Imaging.ImageFormat.Png);
            cutoutCirrus.Save(Properties.Resources.Output_Images + id + "_classified_cirrus.png", System.Drawing.Imaging.ImageFormat.Png);
            cutoutClearSky.Save(Properties.Resources.Output_Images + id + "_classified_clearsky.png", System.Drawing.Imaging.ImageFormat.Png);

            /*
            //Convert HDR values to regular float values
            UpdateDownloadStatusText("converting HDR output...");
            if (File.Exists("HDR2Float/streetview.hdr")) File.Delete("HDR2Float/streetview.hdr");
            File.Copy(Properties.Resources.Output_Images + id + ".hdr", "HDR2Float/streetview.hdr");

            processInfo = new ProcessStartInfo(AppDomain.CurrentDomain.BaseDirectory + "/HDR2Float/HDR2Float.exe", "");
            processInfo.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory + "/HDR2Float/";
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            process = Process.Start(processInfo);
            process.WaitForExit();
            process.Close();

            if (File.Exists(Properties.Resources.Output_Images + id + ".bin")) File.Delete(Properties.Resources.Output_Images + id + ".bin");
            if (File.Exists("HDR2Float/streetview.bin")) File.Copy("HDR2Float/streetview.bin", Properties.Resources.Output_Images + id + ".bin");
            if (File.Exists("HDR2Float/streetview.bin")) File.Delete("HDR2Float/streetview.bin");
            if (File.Exists("HDR2Float/streetview.hdr")) File.Delete("HDR2Float/streetview.hdr");

            //Read in the converted float values from the HDR
            BinaryReader binReader = new BinaryReader(File.OpenRead(Properties.Resources.Output_Images + id + ".bin"));
            List<HDRPixelAsFloat> parsedPixels = new List<HDRPixelAsFloat>();
            for (int i = 0; i < binReader.BaseStream.Length / sizeof(float) / 3; i++)
            {
                HDRPixelAsFloat newPixel = new HDRPixelAsFloat();
                newPixel.R = binReader.ReadSingle();
                newPixel.G = binReader.ReadSingle();
                newPixel.B = binReader.ReadSingle();
                parsedPixels.Add(newPixel);
            }
            binReader.Close();
            */

            //Done!
            downloadCount++;
            UpdateDownloadCountText(downloadCount);
            UpdateDownloadStatusText("finished!");
            return thisMeta["neighbours"].Value<JArray>();
        }

        /* On load, validate our setup, and disable image processing options if external tools are unavailable. */
        private void StreetviewGUI_Load(object sender, EventArgs e)
        {
            Control.CheckForIllegalCrossThreadCalls = false; //We always do this safely.

            if (!Directory.Exists(Properties.Resources.Library_PBRT) ||
                !Directory.Exists(Properties.Resources.Library_LDR2HDR) ||
                !Directory.Exists(Properties.Resources.Library_EXR2LDR) ||
                !Directory.Exists(Properties.Resources.Library_Classifier))
            {
                processImages.Checked = false;
                processImages.Enabled = false;
                straightBias.Enabled = false;
                canEnableProcessing = false;
            }
        }

        /* Change available options on check changed */
        private void processImages_CheckedChanged(object sender, EventArgs e)
        {
            straightBias.Enabled = processImages.Checked;
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
