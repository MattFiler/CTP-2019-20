namespace DeepLearningLanding
{
    partial class Landing
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.startTraining = new System.Windows.Forms.Button();
            this.launchDoodler = new System.Windows.Forms.Button();
            this.configure = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cudadir = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.vcdir = new System.Windows.Forms.TextBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // startTraining
            // 
            this.startTraining.Location = new System.Drawing.Point(12, 12);
            this.startTraining.Name = "startTraining";
            this.startTraining.Size = new System.Drawing.Size(271, 35);
            this.startTraining.TabIndex = 0;
            this.startTraining.Text = "Start Training";
            this.startTraining.UseVisualStyleBackColor = true;
            this.startTraining.Click += new System.EventHandler(this.startTraining_Click);
            // 
            // launchDoodler
            // 
            this.launchDoodler.Location = new System.Drawing.Point(12, 53);
            this.launchDoodler.Name = "launchDoodler";
            this.launchDoodler.Size = new System.Drawing.Size(271, 35);
            this.launchDoodler.TabIndex = 1;
            this.launchDoodler.Text = "Launch Doodler";
            this.launchDoodler.UseVisualStyleBackColor = true;
            this.launchDoodler.Click += new System.EventHandler(this.launchDoodler_Click);
            // 
            // configure
            // 
            this.configure.Location = new System.Drawing.Point(184, 97);
            this.configure.Name = "configure";
            this.configure.Size = new System.Drawing.Size(80, 35);
            this.configure.TabIndex = 2;
            this.configure.Text = "Save";
            this.configure.UseVisualStyleBackColor = true;
            this.configure.Click += new System.EventHandler(this.configure_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.configure);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.cudadir);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.vcdir);
            this.groupBox1.Location = new System.Drawing.Point(13, 94);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(270, 139);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Software Paths";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(4, 55);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(82, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "CUDA Directory";
            // 
            // cudadir
            // 
            this.cudadir.Location = new System.Drawing.Point(6, 71);
            this.cudadir.Name = "cudadir";
            this.cudadir.Size = new System.Drawing.Size(258, 20);
            this.cudadir.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(130, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Visual Studio VC Directory";
            // 
            // vcdir
            // 
            this.vcdir.Location = new System.Drawing.Point(6, 32);
            this.vcdir.Name = "vcdir";
            this.vcdir.Size = new System.Drawing.Size(258, 20);
            this.vcdir.TabIndex = 0;
            // 
            // Landing
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(296, 243);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.launchDoodler);
            this.Controls.Add(this.startTraining);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.Name = "Landing";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Deep Learning";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button startTraining;
        private System.Windows.Forms.Button launchDoodler;
        private System.Windows.Forms.Button configure;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox cudadir;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox vcdir;
    }
}

