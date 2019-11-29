using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace StreetviewRipper
{
    public partial class StreetviewGUI : Form
    {
        StreetviewImageProcessor processor = new StreetviewImageProcessor();
        bool isUGC = false; //User Generated Content uses a different URL
        int downloadCount = 0;
        List<string> downloadedIDs = new List<string>();

        public StreetviewGUI()
        {
            InitializeComponent();
            imageQuality.SelectedIndex = 4;
            straightBias.SelectedIndex = 1;
        }

        private void downloadStreetview_Click(object sender, EventArgs e)
        {
            //Setup UI
            downloadStreetview.Enabled = false;
            Cursor.Current = Cursors.WaitCursor;
            downloadProgress.Value = 0;
            downloadProgress.Maximum = streetviewURL.Lines.Length;

            //Check the user means to recurse!
            if (recurseNeighbours.Checked)
                if (MessageBox.Show("Recursion selected! Are you sure?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;

            //Download all in list
            downloadCount = 0;
            downloadedIDs.Clear();
            string streetviewID = "";
            foreach (string thisURL in streetviewURL.Lines)
            {
                try
                {
                    //Get the streetview ID from string and download sphere if one is found
                    streetviewID = (thisURL.Split(new string[] { "!1s" }, StringSplitOptions.None)[1].Split(new string[] { "!2e" }, StringSplitOptions.None)[0]).Replace("%2F", "/");
                    if (streetviewID != "")
                    {
                        JArray neighbours = DownloadStreetview(streetviewID);
                        if (recurseNeighbours.Checked) DownloadNeighbours(neighbours);
                    }
                }
                catch { }
                downloadProgress.PerformStep();
            }
            
            //Finished
            streetviewURL.Text = "";
            downloadStreetview.Enabled = true;
            Cursor.Current = Cursors.Default;
            downloadProgress.Value = downloadProgress.Maximum;
            MessageBox.Show("Downloaded " + downloadCount + " Streetview sphere(s) from " + downloadProgress.Maximum + " URL(s)!", "Complete!", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            foreach (JObject thisNeighbour in neighbourInfos)
            {
                string thisID = thisNeighbour["id"].Value<string>();
                if (!downloadedIDs.Contains(thisID))
                {
                    JArray neighbours = DownloadStreetview(thisID);
                    DownloadNeighbours(neighbours);
                }
            }
        }

        /* Download a complete sphere from Streetview at a globally defined quality */
        private JArray DownloadStreetview(string id)
        {
            //Get metadata
            JToken thisMeta = GetMetadata(id);
            if (thisMeta["error"].Value<string>() != "") return null;
            int tileWidth = thisMeta["tile_size"][0].Value<int>();
            int tileHeight = thisMeta["tile_size"][1].Value<int>();

            //Load every tile
            int xOffset = 0;
            int yOffset = 0;
            bool stop = false;
            isUGC = false;
            List<StreetviewTile> streetviewTiles = new List<StreetviewTile>();
            for (int y = 0; y < int.MaxValue; y++)
            {
                for (int x = 0; x < int.MaxValue; x++)
                {
                    StreetviewTile newTile = new StreetviewTile();

                    WebRequest request = WebRequest.Create(thisMeta["tile_url"].Value<string>().Replace("*X*", x.ToString()).Replace("*Y*", y.ToString()).Replace("*Z*", imageQuality.SelectedIndex.ToString()));
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
            if (imageQuality.SelectedIndex >= thisMeta["compiled_sizes"].Value<JArray>().Count) imageQuality.SelectedIndex = thisMeta["compiled_sizes"].Value<JArray>().Count - 1;

            //Compile all image tiles to one whole image
            Bitmap streetviewImage = new Bitmap(thisMeta["compiled_sizes"][imageQuality.SelectedIndex][0].Value<int>(), thisMeta["compiled_sizes"][imageQuality.SelectedIndex][1].Value<int>());
            Graphics streetviewRenderer = Graphics.FromImage(streetviewImage);
            foreach (StreetviewTile thisTile in streetviewTiles)
            {
                streetviewRenderer.DrawImage(thisTile.image, thisTile.x, thisTile.y, tileWidth, tileHeight);
            }
            streetviewRenderer.Dispose();
            streetviewImage.Save(id + ".png");

            //Write some of the metadata locally
            JToken localMeta = JToken.Parse("{}");
            if (thisMeta["road"].Value<string>() == null) localMeta["location"] = new JArray { "Unknown", "Unknown" };
            else localMeta["location"] = new JArray { thisMeta["road"].Value<string>(), thisMeta["region"].Value<string>() };
            localMeta["coordinates"] = new JArray { thisMeta["coordinates"][0].Value<double>(), thisMeta["coordinates"][1].Value<double>() };
            localMeta["date"] = new JArray { thisMeta["date"][1].Value<int>(), thisMeta["date"][0].Value<int>() };
            localMeta["resolution"] = new JArray { thisMeta["compiled_sizes"][imageQuality.SelectedIndex][0].Value<int>(), thisMeta["compiled_sizes"][imageQuality.SelectedIndex][1].Value<int>() };
            localMeta["history"] = thisMeta["history"];
            if (thisMeta["is_ugc"].Value<bool>()) localMeta["creator"] = thisMeta["creator"];
            else localMeta["creator"] = "Google";
            localMeta["sun_x"] = processor.GetSunXPos(streetviewImage);
            Vector2 groundPos = processor.GuessGroundPositions(streetviewImage, (imageQuality.SelectedIndex * 5) + 15, true, (StraightLineBias)straightBias.SelectedIndex)[0].position;
            localMeta["ground_y"] = groundPos.y;
            File.WriteAllText(id + ".json", localMeta.ToString(Formatting.Indented));

            //Done!
            downloadCount++;
            downloadedIDs.Add(id);
            return thisMeta["neighbours"].Value<JArray>();
        }
    }
}
