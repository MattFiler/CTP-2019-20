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

namespace Img2Bin
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Bitmap img = (Bitmap)Image.FromFile("image.jpg");
            BinaryWriter bin = new BinaryWriter(File.OpenWrite("image.bin"));
            bin.BaseStream.SetLength(0);
            for (int x = 0; x < img.Width; x++)
            {
                for (int y = 0; y < img.Height; y++)
                {
                    bin.Write((float)img.GetPixel(x, y).R / 255.0f);
                    bin.Write((float)img.GetPixel(x, y).G / 255.0f);
                    bin.Write((float)img.GetPixel(x, y).B / 255.0f);
                }
            }
            bin.Close();
        }
    }
}
