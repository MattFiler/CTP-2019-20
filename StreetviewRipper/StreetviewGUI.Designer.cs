namespace StreetviewRipper
{
    partial class StreetviewGUI
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
            this.components = new System.ComponentModel.Container();
            this.downloadStreetview = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.streetviewURL = new System.Windows.Forms.TextBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.stopThreadedDownload = new System.Windows.Forms.Button();
            this.straightBias = new System.Windows.Forms.ComboBox();
            this.imageQuality = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.downloadTracker = new System.Windows.Forms.Label();
            this.statusText = new System.Windows.Forms.Label();
            this.processImages = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.stoppingText = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // downloadStreetview
            // 
            this.downloadStreetview.Location = new System.Drawing.Point(234, 19);
            this.downloadStreetview.Name = "downloadStreetview";
            this.downloadStreetview.Size = new System.Drawing.Size(90, 29);
            this.downloadStreetview.TabIndex = 9;
            this.downloadStreetview.Text = "Start";
            this.toolTip1.SetToolTip(this.downloadStreetview, "Download the provided URLs with given settings.");
            this.downloadStreetview.UseVisualStyleBackColor = true;
            this.downloadStreetview.Click += new System.EventHandler(this.downloadStreetview_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(151, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Streetview URLs (one per line)";
            // 
            // streetviewURL
            // 
            this.streetviewURL.Location = new System.Drawing.Point(15, 25);
            this.streetviewURL.Multiline = true;
            this.streetviewURL.Name = "streetviewURL";
            this.streetviewURL.Size = new System.Drawing.Size(524, 267);
            this.streetviewURL.TabIndex = 1;
            this.toolTip1.SetToolTip(this.streetviewURL, "URLs to download - copy this from Streetview on Google Maps.");
            // 
            // stopThreadedDownload
            // 
            this.stopThreadedDownload.Enabled = false;
            this.stopThreadedDownload.Location = new System.Drawing.Point(234, 54);
            this.stopThreadedDownload.Name = "stopThreadedDownload";
            this.stopThreadedDownload.Size = new System.Drawing.Size(90, 29);
            this.stopThreadedDownload.TabIndex = 16;
            this.stopThreadedDownload.Text = "Stop";
            this.toolTip1.SetToolTip(this.stopThreadedDownload, "Download the provided URLs with given settings.");
            this.stopThreadedDownload.UseVisualStyleBackColor = true;
            this.stopThreadedDownload.Click += new System.EventHandler(this.stopThreadedDownload_Click);
            // 
            // straightBias
            // 
            this.straightBias.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.straightBias.FormattingEnabled = true;
            this.straightBias.Items.AddRange(new object[] {
            "Top",
            "Middle",
            "Bottom"});
            this.straightBias.Location = new System.Drawing.Point(83, 63);
            this.straightBias.Name = "straightBias";
            this.straightBias.Size = new System.Drawing.Size(93, 21);
            this.straightBias.TabIndex = 11;
            // 
            // imageQuality
            // 
            this.imageQuality.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.imageQuality.FormattingEnabled = true;
            this.imageQuality.Items.AddRange(new object[] {
            "0 - Low",
            "1",
            "2",
            "3",
            "4",
            "5 - High"});
            this.imageQuality.Location = new System.Drawing.Point(89, 20);
            this.imageQuality.Name = "imageQuality";
            this.imageQuality.Size = new System.Drawing.Size(87, 21);
            this.imageQuality.TabIndex = 13;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 67);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(68, 13);
            this.label3.TabIndex = 12;
            this.label3.Text = "Ground Bias:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 23);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(74, 13);
            this.label2.TabIndex = 14;
            this.label2.Text = "Image Quality:";
            // 
            // downloadTracker
            // 
            this.downloadTracker.AutoSize = true;
            this.downloadTracker.Location = new System.Drawing.Point(6, 35);
            this.downloadTracker.Name = "downloadTracker";
            this.downloadTracker.Size = new System.Drawing.Size(43, 13);
            this.downloadTracker.TabIndex = 15;
            this.downloadTracker.Text = "Total: 0";
            // 
            // statusText
            // 
            this.statusText.AutoSize = true;
            this.statusText.Location = new System.Drawing.Point(6, 51);
            this.statusText.Name = "statusText";
            this.statusText.Size = new System.Drawing.Size(93, 13);
            this.statusText.TabIndex = 17;
            this.statusText.Text = "Currently: finished!";
            // 
            // processImages
            // 
            this.processImages.AutoSize = true;
            this.processImages.Checked = true;
            this.processImages.CheckState = System.Windows.Forms.CheckState.Checked;
            this.processImages.Location = new System.Drawing.Point(12, 44);
            this.processImages.Name = "processImages";
            this.processImages.Size = new System.Drawing.Size(164, 17);
            this.processImages.TabIndex = 18;
            this.processImages.Text = "Process Downloaded Images";
            this.processImages.UseVisualStyleBackColor = true;
            this.processImages.CheckedChanged += new System.EventHandler(this.processImages_CheckedChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.stoppingText);
            this.groupBox1.Controls.Add(this.stopThreadedDownload);
            this.groupBox1.Controls.Add(this.statusText);
            this.groupBox1.Controls.Add(this.downloadTracker);
            this.groupBox1.Controls.Add(this.downloadStreetview);
            this.groupBox1.Location = new System.Drawing.Point(207, 298);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(332, 93);
            this.groupBox1.TabIndex = 19;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Download progress";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.imageQuality);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.straightBias);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.processImages);
            this.groupBox2.Location = new System.Drawing.Point(15, 298);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(186, 93);
            this.groupBox2.TabIndex = 20;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Download options";
            // 
            // stoppingText
            // 
            this.stoppingText.AutoSize = true;
            this.stoppingText.ForeColor = System.Drawing.Color.Red;
            this.stoppingText.Location = new System.Drawing.Point(170, 69);
            this.stoppingText.Name = "stoppingText";
            this.stoppingText.Size = new System.Drawing.Size(58, 13);
            this.stoppingText.TabIndex = 18;
            this.stoppingText.Text = "Stopping...";
            this.stoppingText.Visible = false;
            // 
            // StreetviewGUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(552, 401);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.streetviewURL);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "StreetviewGUI";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Streetview Ripper";
            this.Load += new System.EventHandler(this.StreetviewGUI_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button downloadStreetview;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox streetviewURL;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.ComboBox straightBias;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox imageQuality;
        private System.Windows.Forms.Label downloadTracker;
        private System.Windows.Forms.Button stopThreadedDownload;
        private System.Windows.Forms.Label statusText;
        private System.Windows.Forms.CheckBox processImages;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label stoppingText;
    }
}

