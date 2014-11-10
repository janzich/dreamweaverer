using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DreamweaverReplacer
{
    public partial class Main : Form
    {

        public Main()
        {
            InitializeComponent();
        }

        private void btnPickDir_Click(object sender, EventArgs e)
        {
            folderBrowserDialog.ShowDialog();
        }

        private void btnRun_Click(object sender, EventArgs e)
        {

            webBrowserLog.DocumentText = Utils.GetResourceTextFile("log.html");

            btnRun.Enabled = false;

            System.Threading.ThreadPool.QueueUserWorkItem((s) =>
            {

                Replacer.Replace(textBoxDir.Text, (summary) =>
                {
                    Invoke((MethodInvoker)(() =>
                    {

                        webBrowserLog.Document.InvokeScript("addLogMessage", new object[] {
                            summary.Type == Replacer.FileReplacementResultType.Success ? "success" : "error",
                            summary.RelFilePath, summary.Message });

                        if (summary.FileIndex == 0)
                        {
                            lblProgressCount.Visible = true;
                            progressBar.Visible = true;
                            progressBar.Maximum = summary.FileCount;
                        }

                        lblProgressCount.Text = string.Format("{0} / {1}", summary.FileIndex + 1, summary.FileCount);
                        progressBar.Value = summary.FileIndex + 1;

                    }));
                });

                Invoke((MethodInvoker)(() =>
                {
                    btnRun.Enabled = true;
                    progressBar.Visible = false;
                    lblProgressCount.Visible = false;
                }));

            });

        }

    }
}
