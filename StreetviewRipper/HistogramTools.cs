using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreetviewRipper
{
    class HistogramTools
    {
        /* Create and output a histogram for a HDR image */
        public void CreateHDRHistogram(HDRImage image, string filename)
        {
            List<int> dataR = new List<int>();
            List<int> dataG = new List<int>();
            List<int> dataB = new List<int>();
            List<int> dataE = new List<int>();
            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    HDRPixel thisPixel = image.GetPixel(x, y);
                    dataR.Add(thisPixel.R);
                    dataG.Add(thisPixel.G);
                    dataB.Add(thisPixel.B);
                    dataE.Add(thisPixel.E);
                }
            }
            OutputHistogram(filename + "_r", "HDR Histogram R", CalculateHistogram(dataR));
            OutputHistogram(filename + "_g", "HDR Histogram G", CalculateHistogram(dataG));
            OutputHistogram(filename + "_b", "HDR Histogram B", CalculateHistogram(dataB));
            OutputHistogram(filename + "_e", "HDR Histogram E", CalculateHistogram(dataE));
        }

        /* Create and output a histogram for an LDR bitmap */
        public void CreateLDRHistogram(Bitmap image, string filename)
        {
            List<int> dataR = new List<int>();
            List<int> dataG = new List<int>();
            List<int> dataB = new List<int>();
            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    Color thisPixel = image.GetPixel(x, y);
                    dataR.Add(thisPixel.R);
                    dataG.Add(thisPixel.G);
                    dataB.Add(thisPixel.B);
                }
            }
            OutputHistogram(filename + "_r", "LDR Histogram R", CalculateHistogram(dataR));
            OutputHistogram(filename + "_g", "LDR Histogram G", CalculateHistogram(dataG));
            OutputHistogram(filename + "_b", "LDR Histogram B", CalculateHistogram(dataB));
        }

        /* Calculate the histogram and return as JSON array */
        private JArray CalculateHistogram(List<int> data)
        {
            SortedDictionary<uint, int> histogram = new SortedDictionary<uint, int>();
            foreach (uint item in data)
            {
                if (histogram.ContainsKey(item))
                {
                    histogram[item]++;
                }
                else
                {
                    histogram[item] = 1;
                }
            }
            JArray histogramOutput = new JArray();
            foreach (KeyValuePair<uint, int> pair in histogram)
            {
                JObject thisEntry = new JObject();
                thisEntry.Add(new JProperty("colour_value", (int)pair.Key));
                thisEntry.Add(new JProperty("colour_occurance", pair.Value));
                histogramOutput.Add(thisEntry);
            }
            return histogramOutput;
        }

        /* Output the histogram */
        private void OutputHistogram(string filename, string title, JArray json)
        {
            //Rewrite html page
            string htmlPage = Properties.Resources.HistogramHTML.ToString();
            htmlPage = htmlPage.Replace("%TITLE%", title);
            htmlPage = htmlPage.Replace("%DATA_URL%", "data/" + filename + ".json");
            Directory.CreateDirectory("HistogramOutput/");
            File.WriteAllText("HistogramOutput/" + filename + ".html", htmlPage);

            //Save JSON
            Directory.CreateDirectory("HistogramOutput/data/");
            File.WriteAllText("HistogramOutput/data/" + filename + ".json", json.ToString());
        }
    }
}
