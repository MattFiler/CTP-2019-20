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
            this.recurseNeighbours = new System.Windows.Forms.CheckBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.straightBias = new System.Windows.Forms.ComboBox();
            this.imageQuality = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.downloadProgress = new System.Windows.Forms.ProgressBar();
            this.SuspendLayout();
            // 
            // downloadStreetview
            // 
            this.downloadStreetview.Location = new System.Drawing.Point(409, 296);
            this.downloadStreetview.Name = "downloadStreetview";
            this.downloadStreetview.Size = new System.Drawing.Size(90, 29);
            this.downloadStreetview.TabIndex = 9;
            this.downloadStreetview.Text = "Download";
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
            // recurseNeighbours
            // 
            this.recurseNeighbours.AutoSize = true;
            this.recurseNeighbours.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.recurseNeighbours.Location = new System.Drawing.Point(15, 303);
            this.recurseNeighbours.Name = "recurseNeighbours";
            this.recurseNeighbours.Size = new System.Drawing.Size(69, 17);
            this.recurseNeighbours.TabIndex = 4;
            this.recurseNeighbours.Text = "Recurse:";
            this.recurseNeighbours.UseVisualStyleBackColor = true;
            // 
            // straightBias
            // 
            this.straightBias.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.straightBias.FormattingEnabled = true;
            this.straightBias.Items.AddRange(new object[] {
            "Top",
            "Middle",
            "Bottom"});
            this.straightBias.Location = new System.Drawing.Point(141, 301);
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
            this.imageQuality.Location = new System.Drawing.Point(287, 301);
            this.imageQuality.Name = "imageQuality";
            this.imageQuality.Size = new System.Drawing.Size(94, 21);
            this.imageQuality.TabIndex = 13;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(105, 304);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(30, 13);
            this.label3.TabIndex = 12;
            this.label3.Text = "Bias:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(239, 304);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(42, 13);
            this.label2.TabIndex = 14;
            this.label2.Text = "Quality:";
            // 
            // downloadProgress
            // 
            this.downloadProgress.Location = new System.Drawing.Point(15, 331);
            this.downloadProgress.Name = "downloadProgress";
            this.downloadProgress.Size = new System.Drawing.Size(484, 23);
            this.downloadProgress.Step = 1;
            this.downloadProgress.TabIndex = 5;
            // 
            // StreetviewGUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(511, 369);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.imageQuality);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.straightBias);
            this.Controls.Add(this.recurseNeighbours);
            this.Controls.Add(this.downloadProgress);
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
        private System.Windows.Forms.CheckBox recurseNeighbours;
        private System.Windows.Forms.ComboBox straightBias;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox imageQuality;
        private System.Windows.Forms.ProgressBar downloadProgress;
    }
}

