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
        }

        private void downloadStreetview_Click(object sender, EventArgs e)
        {
            //Get the ID to load images from
            string streetviewID = (streetviewURL.Text.Split(new string[] { "!1s" }, StringSplitOptions.None)[1].Split(new string[] { "!2e" }, StringSplitOptions.None)[0]).Replace("%2F", "/");

            //Load every tile
            int xOffset = 0;
            int yOffset = 0;
            List<StreetviewTile> streetviewTiles = new List<StreetviewTile>();
            for (int y = 0; y < 13; y++)
            {
                for (int x = 0; x < 26; x++)
                {
                    StreetviewTile newTile = new StreetviewTile();

                    var request = WebRequest.Create("https://geo1.ggpht.com/cbk?cb_client=maps_sv.tactile&authuser=0&hl=en&gl=uk&panoid=" + streetviewID + "&output=tile&x=" + x + "&y=" + y + "&zoom=5&nbt&fover=2");
                    using (var response = request.GetResponse())
                    using (var stream = response.GetResponseStream())
                    {
                        newTile.image = Bitmap.FromStream(stream);
                    }
                    newTile.x = xOffset;
                    newTile.y = yOffset;

                    streetviewTiles.Add(newTile);
                    xOffset += 512;
                }
                yOffset += 512;
                xOffset = 0;
            }
            
            //Compile all image tiles to one whole image
            Bitmap streetviewImage = new Bitmap(13312, 6656);
            Graphics streetviewRenderer = Graphics.FromImage(streetviewImage);
            foreach (StreetviewTile thisTile in streetviewTiles)
            {
                streetviewRenderer.DrawImage(thisTile.image, thisTile.x, thisTile.y, 512, 512);
            }
            streetviewRenderer.Dispose();
            streetviewImage.Save(streetviewID + ".png");

            //Finished
            streetviewURL.Text = "";
            MessageBox.Show("Download complete!");
        }
    }
}
