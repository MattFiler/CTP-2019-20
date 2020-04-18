using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CloudmapBinToImg
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            BinaryReader reader = new BinaryReader(File.OpenRead("YUMPv2lQdMdUdWm5UXlavQ_cloudmap.bin"));
            Bitmap img = new Bitmap(reader.ReadInt32(), reader.ReadInt32());
            for (int x = 0; x < img.Width; x++)
            {
                for (int y = 0; y < img.Height; y++)
                {
                    Color thisColour = (reader.ReadBoolean()) ? Color.White : Color.Black;
                    img.SetPixel(x, y, thisColour);
                }
            }
            reader.Close();
            img.Save("test.jpg");
            pictureBox1.Image = img;
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
    }
}
