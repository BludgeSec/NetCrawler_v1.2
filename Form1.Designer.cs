namespace NetCrawler
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.GetIP = new System.Windows.Forms.Button();
            this.ScanNW = new System.Windows.Forms.Button();
            this.listView1 = new System.Windows.Forms.ListView();
            this.lblLocIP = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.locaddsCmb = new System.Windows.Forms.ComboBox();
            this.auditBtn = new System.Windows.Forms.Button();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.SuspendLayout();
            // 
            // GetIP
            // 
            this.GetIP.Location = new System.Drawing.Point(12, 121);
            this.GetIP.Name = "GetIP";
            this.GetIP.Size = new System.Drawing.Size(125, 47);
            this.GetIP.TabIndex = 2;
            this.GetIP.Text = "Get Local IP";
            this.GetIP.UseVisualStyleBackColor = true;
            this.GetIP.Click += new System.EventHandler(this.GetIP_Click);
            // 
            // ScanNW
            // 
            this.ScanNW.Location = new System.Drawing.Point(143, 122);
            this.ScanNW.Name = "ScanNW";
            this.ScanNW.Size = new System.Drawing.Size(125, 46);
            this.ScanNW.TabIndex = 3;
            this.ScanNW.Text = "Scan Network";
            this.ScanNW.UseVisualStyleBackColor = true;
            this.ScanNW.Click += new System.EventHandler(this.ScanNW_Click);
            // 
            // listView1
            // 
            this.listView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView1.HideSelection = false;
            this.listView1.Location = new System.Drawing.Point(15, 255);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(702, 690);
            this.listView1.TabIndex = 4;
            this.listView1.UseCompatibleStateImageBehavior = false;
            // 
            // lblLocIP
            // 
            this.lblLocIP.AutoSize = true;
            this.lblLocIP.Location = new System.Drawing.Point(12, 209);
            this.lblLocIP.Name = "lblLocIP";
            this.lblLocIP.Size = new System.Drawing.Size(46, 13);
            this.lblLocIP.TabIndex = 5;
            this.lblLocIP.Text = "Local IP";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.InitialImage = ((System.Drawing.Image)(resources.GetObject("pictureBox1.InitialImage")));
            this.pictureBox1.Location = new System.Drawing.Point(405, 12);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(310, 210);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 6;
            this.pictureBox1.TabStop = false;
            // 
            // pictureBox2
            // 
            this.pictureBox2.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox2.Image")));
            this.pictureBox2.Location = new System.Drawing.Point(13, 12);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(386, 108);
            this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox2.TabIndex = 7;
            this.pictureBox2.TabStop = false;
            // 
            // locaddsCmb
            // 
            this.locaddsCmb.FormattingEnabled = true;
            this.locaddsCmb.Location = new System.Drawing.Point(15, 228);
            this.locaddsCmb.Name = "locaddsCmb";
            this.locaddsCmb.Size = new System.Drawing.Size(700, 21);
            this.locaddsCmb.TabIndex = 8;
            // 
            // auditBtn
            // 
            this.auditBtn.Location = new System.Drawing.Point(274, 121);
            this.auditBtn.Name = "auditBtn";
            this.auditBtn.Size = new System.Drawing.Size(125, 46);
            this.auditBtn.TabIndex = 9;
            this.auditBtn.Text = "Audit Network";
            this.auditBtn.UseVisualStyleBackColor = true;
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(13, 174);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(386, 23);
            this.progressBar1.TabIndex = 10;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(727, 957);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.auditBtn);
            this.Controls.Add(this.locaddsCmb);
            this.Controls.Add(this.pictureBox2);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.lblLocIP);
            this.Controls.Add(this.listView1);
            this.Controls.Add(this.ScanNW);
            this.Controls.Add(this.GetIP);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "NetCrawler v1.2";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }



        #endregion
        private System.Windows.Forms.Button GetIP;
        private System.Windows.Forms.Button ScanNW;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.Label lblLocIP;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.ComboBox locaddsCmb;
        private System.Windows.Forms.Button auditBtn;
        private System.Windows.Forms.ProgressBar progressBar1;
    }
}

