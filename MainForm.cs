using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace AITheSomniumFilesChsPatch
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            AuthorLabel.Text = "作者：Xzonn · 版本：3.0 · 严禁商用 · 不欢迎下载站转载\n其乐论坛：https://keylol.com/t839952-1-1\n使用方法：https://www.bilibili.com/video/BV1At4y1P7B7\n技术细节：https://xzonn.top/posts/AI-The-Somnium-Files-Chs-Patch.html";
            AuthorLabel.Links.Add(41, 30, "https://keylol.com/t839952-1-1");
            AuthorLabel.Links.Add(77, 43, "https://www.bilibili.com/video/BV1At4y1P7B7");
            AuthorLabel.Links.Add(126, 59, "https://xzonn.top/posts/AI-The-Somnium-Files-Chs-Patch.html");
            LabelAutoPosition(AuthorLabel, null, 480, 680);
            AuthorLabel.SizeChanged += (sender, e) => LabelAutoPosition(sender, e, 480, 680);
            LabelAutoPosition(SelectDirectoryLabel, null, 180, 500, Position.RightMiddle);
            SelectDirectoryLabel.SizeChanged += (sender, e) => LabelAutoPosition(sender, e, 180, 500, Position.RightMiddle);
            Assembly asm = Assembly.GetExecutingAssembly();
            Stream stream = asm.GetManifestResourceStream("AITheSomniumFilesChsPatch.Includes.Header.jpg");
            HeaderImage.Image = System.Drawing.Image.FromStream(stream);
        }

        private void OpenLink(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string target = (string)e.Link.LinkData;
            if (!string.IsNullOrEmpty(target))
            {
                System.Diagnostics.Process.Start(target);
            }
        }

        private void ApplyPatch(object sender, EventArgs e)
        {
            string assetsPath = GameDirectoryTextBox.Text;
            string tempPath = "";
            ApplyPatchButton.Text = "应用中……";
            ApplyPatchButton.Enabled = false;
            try
            {
                Program.ApplyPatch(assetsPath, out tempPath);
                MessageBox.Show("应用成功！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ApplyPatchButton.Text = "应用成功！";
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ApplyPatchButton.Text = "出现错误！";
            }
            if (!string.IsNullOrEmpty(tempPath) && Directory.Exists(tempPath))
            {
                Directory.Delete(tempPath, true);
            }
        }

        private bool CheckIfDirectoryExists(ref string basePath)
        {
            if (string.IsNullOrEmpty(basePath))
            {
                return false;
            }
            if (!Directory.Exists(basePath) && !File.Exists(basePath))
            {
                MessageBox.Show($"文件夹/文件不存在：{basePath}。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            string tempPath = basePath;
            while (!File.Exists(Path.Combine(tempPath, "AI_TheSomniumFiles.exe")))
            {
                string parent = Path.GetDirectoryName(tempPath);
                if (string.IsNullOrEmpty(parent) || parent == tempPath)
                {
                    MessageBox.Show($"未在该文件夹及其上级文件夹找到游戏可执行文件：{basePath}。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                tempPath = parent;
            }
            basePath = tempPath;
            var resourcesAssetsPath = Path.Combine(basePath, "AI_TheSomniumFiles_Data/resources.assets");
            var fontsAssetsPath = Path.Combine(basePath, "AI_TheSomniumFiles_Data/StreamingAssets/AssetBundles/StandaloneWindows64/fonts");
            if (!File.Exists(resourcesAssetsPath) || !File.Exists($"{resourcesAssetsPath}.resS") || !File.Exists(fontsAssetsPath))
            {
                MessageBox.Show($"未找到必要的资源文件，请检查游戏文件是否完整。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }

        private void SelectDirectory(object sender, EventArgs e)
        {
            var dialog = new FolderBrowserDialog
            {
                ShowNewFolderButton = false,
                Description = "选择",
                SelectedPath = GameDirectoryTextBox.Text
            };
            dialog.ShowDialog();
            string path = dialog.SelectedPath;
            if (CheckIfDirectoryExists(ref path))
            {
                GameDirectoryTextBox.Text = path;
                ApplyPatchButton.Text = "应用补丁";
                ApplyPatchButton.Enabled = true;
            }
            else
            {
                GameDirectoryTextBox.Text = "";
                ApplyPatchButton.Enabled = false;
            }
        }

        private void TextBoxDragDrop(object sender, DragEventArgs e)
        {
            string path = ((string[])e.Data.GetData(DataFormats.FileDrop))[0];
            if (CheckIfDirectoryExists(ref path))
            {
                ((TextBox)sender).Text = path;
                ApplyPatchButton.Text = "应用补丁";
                ApplyPatchButton.Enabled = true;
            }
            else
            {
                GameDirectoryTextBox.Text = "";
                ApplyPatchButton.Enabled = false;
            }
        }

        private void TextBoxDragEnter(object sender, DragEventArgs e)
        {

            if (e.Data.GetDataPresent(DataFormats.FileDrop) && ((Array)e.Data.GetData(DataFormats.FileDrop)).Length == 1)
            {
                e.Effect = DragDropEffects.Link;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private enum Position
        {
            LeftTop,
            CenterTop,
            RightTop,
            LeftMiddle,
            CenterMiddle,
            RightMiddle,
            LeftBottom,
            CenterBottom,
            RightBottom
        }

        // I don't know why, but it works
        private void LabelAutoPosition(object sender, EventArgs e, int x, int y, Position position = Position.CenterMiddle)
        {
            Label label = (Label)sender;
            // 192F is from FormPatch.Designer.cs
            float scaleX = CurrentAutoScaleDimensions.Width / 192F, scaleY = CurrentAutoScaleDimensions.Height / 192F;
            int posX = 0, posY = 0;
            switch (position)
            {
                case Position.LeftTop:
                case Position.LeftMiddle:
                case Position.LeftBottom:
                    posX = (int)(x * scaleX);
                    break;
                case Position.CenterTop:
                case Position.CenterMiddle:
                case Position.CenterBottom:
                    posX = (int)((x * scaleX) - label.Width / 2F);
                    break;
                case Position.RightTop:
                case Position.RightMiddle:
                case Position.RightBottom:
                    posX = (int)((x * scaleX) - label.Width);
                    break;
            }
            switch (position)
            {
                case Position.LeftTop:
                case Position.CenterTop:
                case Position.RightTop:
                    posY = (int)(y * scaleY);
                    break;
                case Position.LeftMiddle:
                case Position.CenterMiddle:
                case Position.RightMiddle:
                    posY = (int)((y * scaleY) - label.Height / 2F);
                    break;
                case Position.LeftBottom:
                case Position.CenterBottom:
                case Position.RightBottom:
                    posY = (int)((y * scaleY) - label.Height);
                    break;
            }
            label.Location = new System.Drawing.Point(posX, posY);
        }
    }
}
