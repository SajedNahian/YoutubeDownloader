using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using YoutubeExtractor;

namespace YoutubeDownloader
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            resolutionComboBox.SelectedIndex = 0;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
        }

        private void downloadButton_Click(object sender, EventArgs e)
        {
            errorLabel.Text = "";
            progressBar.Minimum = 0;
            progressBar.Maximum = 100;

            var link = videoURL.Text;
            IEnumerable<VideoInfo> videoInfos;
            try
            {
                videoInfos = DownloadUrlResolver.GetDownloadUrls(link);
            }
            catch
            {
                videoURL.Text = "";
                errorLabel.Text = "Invalid Video URL";
                return;
            }

            VideoInfo video;
            try
            {
                video = videoInfos
                    .First(info =>
                        info.VideoType == VideoType.Mp4 && info.Resolution == Convert.ToInt32(resolutionComboBox.Text));
            }
            catch
            {
                errorLabel.Text = "Video doesn't support selected resolution";
                return;
            }

            if (video.RequiresDecryption)
            {
                DownloadUrlResolver.DecryptDownloadUrl(video);
            }

            //Console.WriteLine(Path.Combine(Application.StartupPath));
            var videoDownloader = new VideoDownloader(video, Path.Combine(Application.StartupPath + "/", fileName_textbox.Text + video.VideoExtension));

            videoDownloader.DownloadProgressChanged += VideoDownloaderOnDownloadProgressChanged;
            Thread thread = new Thread(() =>
            {
                videoDownloader.Execute();
            }) {IsBackground = true};
            thread.Start();
        }

        private void VideoDownloaderOnDownloadProgressChanged(object sender, ProgressEventArgs e)
        {
            Invoke(new MethodInvoker(delegate()
            {
                progressBar.Value = (int)e.ProgressPercentage;
                percentageLabel.Text = $"{string.Format("{0:0.##}", e.ProgressPercentage)}%";
                progressBar.Update();
            }));
        }
    }
}
