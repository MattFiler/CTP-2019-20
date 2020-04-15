using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using StreetviewRipper;

namespace InscatteringStandalone
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            CloudCalculator calculator = new CloudCalculator(
                (Bitmap)Image.FromFile("../../DUMMY DATA/SKULUDwlJQa9o9yZot82ew_sky_trim.png"),
                (Bitmap)Image.FromFile("../../DUMMY DATA/SKULUDwlJQa9o9yZot82ew_classified_dewarped_resize.png"),
                (Bitmap)Image.FromFile("../../DUMMY DATA/SKULUDwlJQa9o9yZot82ew_hosek_trim.png"));
            calculator.RunInscatteringFormula();

            Application.Exit();
        }
    }
}
