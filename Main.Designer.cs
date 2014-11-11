namespace DreamweaverReplacer
{
    partial class Main
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            this.btnPickDir = new System.Windows.Forms.Button();
            this.txtBoxDir = new System.Windows.Forms.TextBox();
            this.imgListLog = new System.Windows.Forms.ImageList(this.components);
            this.webBrowserLog = new System.Windows.Forms.WebBrowser();
            this.pnlWebBrowserContainer = new System.Windows.Forms.Panel();
            this.btnRun = new System.Windows.Forms.Button();
            this.lblDirectory = new System.Windows.Forms.Label();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.lblProgressCount = new System.Windows.Forms.Label();
            this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.pnlWebBrowserContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnPickDir
            // 
            this.btnPickDir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnPickDir.Location = new System.Drawing.Point(538, 23);
            this.btnPickDir.Name = "btnPickDir";
            this.btnPickDir.Size = new System.Drawing.Size(66, 23);
            this.btnPickDir.TabIndex = 0;
            this.btnPickDir.Text = "Select";
            this.btnPickDir.UseVisualStyleBackColor = true;
            this.btnPickDir.Click += new System.EventHandler(this.btnPickDir_Click);
            // 
            // txtBoxDir
            // 
            this.txtBoxDir.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtBoxDir.Location = new System.Drawing.Point(12, 25);
            this.txtBoxDir.Name = "txtBoxDir";
            this.txtBoxDir.Size = new System.Drawing.Size(520, 20);
            this.txtBoxDir.TabIndex = 1;
            this.txtBoxDir.Text = "C:\\Users\\Jan Zich\\Documents\\Dreamweaverer\\Test";
            // 
            // imgListLog
            // 
            this.imgListLog.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imgListLog.ImageStream")));
            this.imgListLog.TransparentColor = System.Drawing.Color.Transparent;
            this.imgListLog.Images.SetKeyName(0, "success.png");
            this.imgListLog.Images.SetKeyName(1, "error.png");
            // 
            // webBrowserLog
            // 
            this.webBrowserLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowserLog.Location = new System.Drawing.Point(0, 0);
            this.webBrowserLog.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowserLog.Name = "webBrowserLog";
            this.webBrowserLog.ScriptErrorsSuppressed = true;
            this.webBrowserLog.Size = new System.Drawing.Size(590, 478);
            this.webBrowserLog.TabIndex = 3;
            // 
            // pnlWebBrowserContainer
            // 
            this.pnlWebBrowserContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlWebBrowserContainer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlWebBrowserContainer.Controls.Add(this.webBrowserLog);
            this.pnlWebBrowserContainer.Location = new System.Drawing.Point(12, 52);
            this.pnlWebBrowserContainer.Name = "pnlWebBrowserContainer";
            this.pnlWebBrowserContainer.Size = new System.Drawing.Size(592, 480);
            this.pnlWebBrowserContainer.TabIndex = 5;
            // 
            // btnRun
            // 
            this.btnRun.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnRun.Location = new System.Drawing.Point(12, 538);
            this.btnRun.Name = "btnRun";
            this.btnRun.Size = new System.Drawing.Size(66, 23);
            this.btnRun.TabIndex = 2;
            this.btnRun.Text = "Run";
            this.btnRun.UseVisualStyleBackColor = true;
            this.btnRun.Click += new System.EventHandler(this.btnRun_Click);
            // 
            // lblDirectory
            // 
            this.lblDirectory.AutoSize = true;
            this.lblDirectory.Location = new System.Drawing.Point(9, 9);
            this.lblDirectory.Name = "lblDirectory";
            this.lblDirectory.Size = new System.Drawing.Size(49, 13);
            this.lblDirectory.TabIndex = 7;
            this.lblDirectory.Text = "Directory";
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(150, 543);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(454, 14);
            this.progressBar.TabIndex = 8;
            this.progressBar.Value = 50;
            this.progressBar.Visible = false;
            // 
            // lblProgressCount
            // 
            this.lblProgressCount.Location = new System.Drawing.Point(84, 543);
            this.lblProgressCount.Name = "lblProgressCount";
            this.lblProgressCount.Size = new System.Drawing.Size(60, 13);
            this.lblProgressCount.TabIndex = 9;
            this.lblProgressCount.Text = "10 / 100";
            this.lblProgressCount.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.lblProgressCount.Visible = false;
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(616, 573);
            this.Controls.Add(this.lblProgressCount);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.lblDirectory);
            this.Controls.Add(this.txtBoxDir);
            this.Controls.Add(this.btnPickDir);
            this.Controls.Add(this.btnRun);
            this.Controls.Add(this.pnlWebBrowserContainer);
            this.Name = "Main";
            this.ShowIcon = false;
            this.Text = "Dreamweaver replacer";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Main_FormClosed);
            this.Load += new System.EventHandler(this.Main_Load);
            this.pnlWebBrowserContainer.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnPickDir;
        private System.Windows.Forms.TextBox txtBoxDir;
        private System.Windows.Forms.ImageList imgListLog;
        private System.Windows.Forms.WebBrowser webBrowserLog;
        private System.Windows.Forms.Panel pnlWebBrowserContainer;
        private System.Windows.Forms.Button btnRun;
        private System.Windows.Forms.Label lblDirectory;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Label lblProgressCount;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
    }
}

