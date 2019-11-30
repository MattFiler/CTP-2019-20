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
            this.straightBias = new System.Windows.Forms.ComboBox();
            this.imageQuality = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.downloadTracker = new System.Windows.Forms.Label();
            this.stopThreadedDownload = new System.Windows.Forms.Button();
            this.statusText = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // downloadStreetview
            // 
            this.downloadStreetview.Location = new System.Drawing.Point(409, 296);
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
            this.streetviewURL.Size = new System.Drawing.Size(484, 267);
            this.streetviewURL.TabIndex = 1;
            this.toolTip1.SetToolTip(this.streetviewURL, "URLs to download - copy this from Streetview on Google Maps.");
            // 
            // straightBias
            // 
            this.straightBias.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.straightBias.FormattingEnabled = true;
            this.straightBias.Items.AddRange(new object[] {
            "Top",
            "Middle",
            "Bottom"});
            this.straightBias.Location = new System.Drawing.Point(49, 301);
            this.straightBias.Name = "straightBias";
            this.straightBias.Size = new System.Drawing.Size(79, 21);
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
            this.imageQuality.Location = new System.Drawing.Point(195, 301);
            this.imageQuality.Name = "imageQuality";
            this.imageQuality.Size = new System.Drawing.Size(94, 21);
            this.imageQuality.TabIndex = 13;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(13, 304);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(30, 13);
            this.label3.TabIndex = 12;
            this.label3.Text = "Bias:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(147, 304);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(42, 13);
            this.label2.TabIndex = 14;
            this.label2.Text = "Quality:";
            // 
            // downloadTracker
            // 
            this.downloadTracker.AutoSize = true;
            this.downloadTracker.Location = new System.Drawing.Point(12, 331);
            this.downloadTracker.Name = "downloadTracker";
            this.downloadTracker.Size = new System.Drawing.Size(152, 13);
            this.downloadTracker.TabIndex = 15;
            this.downloadTracker.Text = "Downloaded and processed: 0";
            // 
            // stopThreadedDownload
            // 
            this.stopThreadedDownload.Enabled = false;
            this.stopThreadedDownload.Location = new System.Drawing.Point(409, 331);
            this.stopThreadedDownload.Name = "stopThreadedDownload";
            this.stopThreadedDownload.Size = new System.Drawing.Size(90, 29);
            this.stopThreadedDownload.TabIndex = 16;
            this.stopThreadedDownload.Text = "Stop";
            this.toolTip1.SetToolTip(this.stopThreadedDownload, "Download the provided URLs with given settings.");
            this.stopThreadedDownload.UseVisualStyleBackColor = true;
            this.stopThreadedDownload.Click += new System.EventHandler(this.stopThreadedDownload_Click);
            // 
            // statusText
            // 
            this.statusText.AutoSize = true;
            this.statusText.Location = new System.Drawing.Point(12, 347);
            this.statusText.Name = "statusText";
            this.statusText.Size = new System.Drawing.Size(93, 13);
            this.statusText.TabIndex = 17;
            this.statusText.Text = "Currently: Finished";
            // 
            // StreetviewGUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(511, 368);
            this.Controls.Add(this.statusText);
            this.Controls.Add(this.stopThreadedDownload);
            this.Controls.Add(this.downloadTracker);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.imageQuality);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.straightBias);
            this.Controls.Add(this.streetviewURL);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.downloadStreetview);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "StreetviewGUI";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Streetview Ripper";
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
    }
}

