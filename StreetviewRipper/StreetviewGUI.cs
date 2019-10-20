using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.Windows.Forms;

namespace StreetviewRipper
{
    public partial class StreetviewGUI : Form
    {
        public StreetviewGUI()
        {
            InitializeComponent();
            streetviewZoom.SelectedIndex = 0;
        }

        private void downloadStreetview_Click(object sender, EventArgs e)
        {
            //Get the ID to load images from
            string streetviewID = (streetviewURL.Text.Split(new string[] { "!1s" }, StringSplitOptions.None)[1].Split(new string[] { "!2e" }, StringSplitOptions.None)[0]).Replace("%2F", "/");

            //Work out what zoom to use
            StreetviewQualityDef thisQuality = new StreetviewQualityDef();
            switch (streetviewZoom.Text)
            {
                case "Ultra":
                    thisQuality.Set(5, 26, 13, 512);
                    break;
                case "High":
                    thisQuality.Set(4, 13, 7, 512);
                    break;
                case "Medium":
                    thisQuality.Set(3, 7, 4, 512);
                    break;
                case "Low":
                    thisQuality.Set(2, 4, 2, 512);
                    break;
                case "Lower":
                    thisQuality.Set(1, 2, 1, 512);
                    break;
                case "Lowest":
                    thisQuality.Set(0, 1, 1, 512);
                    break;
            }

            //Load every tile
            int xOffset = 0;
            int yOffset = 0;
            List<StreetviewTile> streetviewTiles = new List<StreetviewTile>();
            for (int y = 0; y < thisQuality.y; y++)
            {
                for (int x = 0; x < thisQuality.x; x++)
                {
                    StreetviewTile newTile = new StreetviewTile();

                    var request = WebRequest.Create("https://geo1.ggpht.com/cbk?cb_client=maps_sv.tactile&authuser=0&hl=en&gl=uk&panoid=" + streetviewID + "&output=tile&x=" + x + "&y=" + y + "&zoom=" + thisQuality.zoom);
                    using (var response = request.GetResponse())
                    using (var stream = response.GetResponseStream())
                    {
                        newTile.image = Bitmap.FromStream(stream);
                    }
                    newTile.x = xOffset;
                    newTile.y = yOffset;

                    streetviewTiles.Add(newTile);
                    xOffset += thisQuality.size;
                }
                yOffset += thisQuality.size;
                xOffset = 0;
            }
            
            //Compile all image tiles to one whole image
            Bitmap streetviewImage = new Bitmap(thisQuality.size * thisQuality.x, thisQuality.size * thisQuality.y);
            Graphics streetviewRenderer = Graphics.FromImage(streetviewImage);
            foreach (StreetviewTile thisTile in streetviewTiles)
            {
                streetviewRenderer.DrawImage(thisTile.image, thisTile.x, thisTile.y, thisQuality.size, thisQuality.size);
            }
            streetviewRenderer.Dispose();
            streetviewImage.Save(streetviewID + "_" + streetviewZoom.Text.ToLower() + ".png");

            //Finished
            streetviewURL.Text = "";
            MessageBox.Show("Download complete!");
        }
    }
}
