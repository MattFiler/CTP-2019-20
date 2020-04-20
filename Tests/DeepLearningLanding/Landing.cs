using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DeepLearningLanding
{
    public partial class Landing : Form
    {
        string deepLearningFolder;
        string trainingDataFolder;
        string SW_visualStudioVC;
        string SW_cudaRoot;
        public Landing()
        {
            InitializeComponent();
            deepLearningFolder = AppDomain.CurrentDomain.BaseDirectory + "../../DeepLearning/";
            trainingDataFolder = AppDomain.CurrentDomain.BaseDirectory + "../StreetviewRipper/Output/Images/PulledClouds/BestMatch/";
            LoadConfig();
            vcdir.Text = SW_visualStudioVC;
            cudadir.Text = SW_cudaRoot;
        }

        /* Train deep doodle */
        private void startTraining_Click(object sender, EventArgs e)
        {
            if (!File.Exists("config.dl"))
            {
                MessageBox.Show("Software paths are not configured!", "Configuration incomplete.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string[] files = Directory.GetFiles(trainingDataFolder, "*.STREETVIEW_LDR.png", SearchOption.TopDirectoryOnly);
            string datagenPY = DeepLearningLanding.Properties.Resources.datagen.ToString();
            datagenPY = datagenPY.Replace("%%IMG_COUNT%%", files.Length.ToString());
            File.WriteAllText(deepLearningFolder + "datagen.py", datagenPY);

            ProcessStartInfo processInfo = new ProcessStartInfo("cmd.exe", "/c \"" + deepLearningFolder + "run.bat\"");
            processInfo.WorkingDirectory = deepLearningFolder;
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            Process process = Process.Start(processInfo);
            process.WaitForExit();
            process.Close();
        }

        /* Launch deep doodle */
        private void launchDoodler_Click(object sender, EventArgs e)
        {
            if (!File.Exists("config.dl"))
            {
                MessageBox.Show("Software paths are not configured!", "Configuration incomplete.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (!File.Exists(deepLearningFolder + "Model.h5"))
            {
                MessageBox.Show("Please train before trying to launch doodler!", "Not trained.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            ProcessStartInfo processInfo = new ProcessStartInfo("cmd.exe", "/c \"" + deepLearningFolder + "doodler.bat\"");
            processInfo.WorkingDirectory = deepLearningFolder;
            Process process = Process.Start(processInfo);
            process.WaitForExit();
            process.Close();
        }

        /* Create ProcessStartInfo from bat name */
        private ProcessStartInfo CreateProcess(string batName)
        {
            ProcessStartInfo processInfo = new ProcessStartInfo("cmd.exe", "/c \"" + deepLearningFolder + batName + "\"");
            processInfo.WorkingDirectory = deepLearningFolder;
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            return processInfo;
        }

        /* Software path config handling */
        private void LoadConfig()
        {
            if (!File.Exists("config.dl")) return;
            BinaryReader reader = new BinaryReader(File.OpenRead("config.dl"));
            SW_visualStudioVC = reader.ReadString();
            SW_cudaRoot = reader.ReadString();
            reader.Close();
        }
        private void SaveConfig()
        {
            BinaryWriter writer = new BinaryWriter(File.OpenWrite("config.dl"));
            writer.BaseStream.SetLength(0);
            writer.Write(SW_visualStudioVC);
            writer.Write(SW_cudaRoot);
            writer.Close();

            string configGPU = DeepLearningLanding.Properties.Resources.gpu.ToString();
            configGPU = configGPU.Replace("%%VC_DIR%%", SW_visualStudioVC);
            configGPU = configGPU.Replace("%%CUDA_DIR%%", SW_cudaRoot);
            File.WriteAllText(deepLearningFolder + "gpu.theanorc", configGPU);
        }

        /* Save config */
        private void configure_Click(object sender, EventArgs e)
        {
            if (vcdir.Text == "" || cudadir.Text == "" || !Directory.Exists(vcdir.Text) || !Directory.Exists(cudadir.Text))
            {
                MessageBox.Show("Please enter a valid software path!", "Invalid paths.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            SW_visualStudioVC = vcdir.Text;
            SW_cudaRoot = cudadir.Text;
            SaveConfig();
        }
    }
}
