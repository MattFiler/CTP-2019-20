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
            //Run for RGBE version
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
                OutputHistogram(filename + "_rgbe_noe", "HDR RGBE (Excluding E) Histogram", completeHistoData);
                completeHistoData.Add(CalculateHistogram(dataE));
                OutputHistogram(filename + "_rgbe", "HDR RGBE Histogram", completeHistoData);
            }

            //Run for RGBL version
            {
                List<float> dataR = new List<float>();
                List<float> dataG = new List<float>();
                List<float> dataB = new List<float>();
                List<float> dataL = new List<float>();
                for (int x = 0; x < image.Width; x++)
                {
                    for (int y = 0; y < image.Height; y++)
                    {
                        HDRPixel thisPixel = image.GetPixel(x, y);
                        HDRPixelFloat thisPixelFloat = new HDRPixelFloat();
                        thisPixelFloat.FromRGBE(thisPixel.R, thisPixel.G, thisPixel.B, thisPixel.E);

                        dataR.Add((float)decimal.Round((decimal)thisPixelFloat.R, 4));
                        dataR.Add((float)decimal.Round((decimal)thisPixelFloat.G, 4));
                        dataR.Add((float)decimal.Round((decimal)thisPixelFloat.B, 4));
                        dataR.Add((float)decimal.Round((decimal)thisPixelFloat.L, 4));
                    }
                }
                JArray completeHistoData = new JArray();
                completeHistoData.Add(CalculateHistogram(dataR));
                completeHistoData.Add(CalculateHistogram(dataG));
                completeHistoData.Add(CalculateHistogram(dataB));
                OutputHistogram(filename + "_rgbl_nol", "HDR RGBL (Excluding L) Histogram", completeHistoData);
                completeHistoData.Add(CalculateHistogram(dataL));
                OutputHistogram(filename + "_rgbl", "HDR RGBL Histogram", completeHistoData);
            }
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
            JArray completeHistoData = new JArray();
            completeHistoData.Add(CalculateHistogram(dataR));
            completeHistoData.Add(CalculateHistogram(dataG));
            completeHistoData.Add(CalculateHistogram(dataB));
            OutputHistogram(filename, "LDR Histogram", completeHistoData);
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
            htmlPage = htmlPage.Replace("%USE_4%", (json.Count == 4) ? "true" : "false");
            Directory.CreateDirectory("HistogramOutput/");
            File.WriteAllText("HistogramOutput/" + filename + ".html", htmlPage);

            //Save JSON
            Directory.CreateDirectory("HistogramOutput/data/");
            for (int i = 0; i < json.Count; i++) File.WriteAllText("HistogramOutput/data/" + filename + (i+1).ToString() + ".json", json[i].ToString());
        }
    }
}
