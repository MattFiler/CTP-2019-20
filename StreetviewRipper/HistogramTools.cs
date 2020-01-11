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
        /* Create and output a luma histogram for a HDR bitmap */
        public void CreateHDR_LumaHistogram(HDRImage image, string filename)
        {
            List<int> dataLuma = new List<int>();
            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    HDRPixel thisPixel = image.GetPixel(x, y);
                    HDRPixelFloat thisPixelFloat = new HDRPixelFloat();
                    thisPixelFloat.FromRGBE(thisPixel.R, thisPixel.G, thisPixel.B, thisPixel.E);
                    dataLuma.Add((int)(thisPixelFloat.L * 255));
                }
            }
            JArray completeHistoData = new JArray();
            completeHistoData.Add(CalculateHistogram(dataLuma));
            OutputHistogram(filename, "HDR Luma Histogram", completeHistoData);
        }

        /* Create and output a luma histogram for an LDR bitmap */
        public void CreateLDR_LumaHistogram(Bitmap image, string filename)
        {
            List<int> dataLuma = new List<int>();
            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    dataLuma.Add(CalculateLuma(image.GetPixel(x, y)));
                }
            }
            JArray completeHistoData = new JArray();
            completeHistoData.Add(CalculateHistogram(dataLuma));
            OutputHistogram(filename, "LDR Luma Histogram", completeHistoData);
        }

        /* Calculate luma for an RGB pixel */
        private int CalculateLuma(Color pixel)
        {
            return (int)((0.2126f * pixel.R) + (0.7152f * pixel.G) + (0.0722f * pixel.B));
        }

        /* Create and output an RGB histogram for a HDR image */
        public void CreateHDR_RGBHistogram(HDRImage image, string filename)
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
            JArray completeHistoData = new JArray();
            completeHistoData.Add(CalculateHistogram(dataR));
            completeHistoData.Add(CalculateHistogram(dataG));
            completeHistoData.Add(CalculateHistogram(dataB));
            OutputHistogram(filename + "_noe", "HDR RGBE (Excluding E) Histogram", completeHistoData);
            completeHistoData.Add(CalculateHistogram(dataE));
            OutputHistogram(filename, "HDR RGBE Histogram", completeHistoData);
        }

        /* Create and output an RGB histogram for an LDR bitmap */
        public void CreateLDR_RGBHistogram(Bitmap image, string filename)
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
            JArray completeHistoData = new JArray();
            completeHistoData.Add(CalculateHistogram(dataR));
            completeHistoData.Add(CalculateHistogram(dataG));
            completeHistoData.Add(CalculateHistogram(dataB));
            OutputHistogram(filename, "LDR RGB Histogram", completeHistoData);
        }

        /* FLOAT: Calculate the histogram and return as JSON array */
        private JArray CalculateHistogram(List<float> data)
        {
            SortedDictionary<float, int> histogram = new SortedDictionary<float, int>();
            foreach (float item in data)
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
            foreach (KeyValuePair<float, int> pair in histogram)
            {
                JObject thisEntry = new JObject();
                thisEntry.Add(new JProperty("colour_value", pair.Key));
                thisEntry.Add(new JProperty("colour_occurance", pair.Value));
                histogramOutput.Add(thisEntry);
            }
            return histogramOutput;
        }

        /* INTEGER: Calculate the histogram and return as JSON array */
        private JArray CalculateHistogram(List<int> data)
        {
            SortedDictionary<uint, int> histogram = new SortedDictionary<uint, int>();
            for (uint i = 0; i < 256; i++)
            {
                histogram[i] = 0;
            }
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
            for (int i = 0; i < json.Count; i++) htmlPage = htmlPage.Replace("%DATA_URL" + (i+1).ToString() + "%", "data/" + filename + (i+1).ToString() + ".json");
            for (int i = 2; i < 5; i++) htmlPage = htmlPage.Replace("%USE_" + i + "%", (json.Count >= i) ? "true" : "false");
            Directory.CreateDirectory(Properties.Resources.Output_Histogram);
            File.WriteAllText(Properties.Resources.Output_Histogram + filename + ".html", htmlPage);

            //Save JSON
            Directory.CreateDirectory(Properties.Resources.Output_Histogram + "data/");
            for (int i = 0; i < json.Count; i++) File.WriteAllText(Properties.Resources.Output_Histogram + "data/" + filename + (i+1).ToString() + ".json", json[i].ToString());
        }
    }
}
