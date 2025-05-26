using System.Windows.Forms;

namespace AITheSomniumFilesChsPatch
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.HeaderImage = new System.Windows.Forms.PictureBox();
            this.SelectDirectoryLabel = new System.Windows.Forms.Label();
            this.GameDirectoryTextBox = new System.Windows.Forms.TextBox();
            this.SelectDirectoryButton = new System.Windows.Forms.Button();
            this.ApplyPatchButton = new System.Windows.Forms.Button();
            this.AuthorLabel = new System.Windows.Forms.LinkLabel();
            ((System.ComponentModel.ISupportInitialize)(this.HeaderImage)).BeginInit();
            this.SuspendLayout();
            // 
            // HeaderImage
            // 
            this.HeaderImage.Location = new System.Drawing.Point(10, 10);
            this.HeaderImage.Name = "HeaderImage";
            this.HeaderImage.Size = new System.Drawing.Size(460, 215);
            this.HeaderImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.HeaderImage.TabIndex = 0;
            this.HeaderImage.TabStop = false;
            // 
            // SelectDirectoryLabel
            // 
            this.SelectDirectoryLabel.AutoSize = true;
            this.SelectDirectoryLabel.Location = new System.Drawing.Point(10, 240);
            this.SelectDirectoryLabel.Name = "SelectDirectoryLabel";
            this.SelectDirectoryLabel.Size = new System.Drawing.Size(68, 17);
            this.SelectDirectoryLabel.TabIndex = 1;
            this.SelectDirectoryLabel.Text = "游戏文件夹：";
            // 
            // GameDirectoryTextBox
            // 
            this.GameDirectoryTextBox.AllowDrop = true;
            this.GameDirectoryTextBox.Location = new System.Drawing.Point(90, 240);
            this.GameDirectoryTextBox.Name = "GameDirectoryTextBox";
            this.GameDirectoryTextBox.Size = new System.Drawing.Size(350, 23);
            this.GameDirectoryTextBox.TabIndex = 2;
            this.GameDirectoryTextBox.DragDrop += new System.Windows.Forms.DragEventHandler(this.TextBoxDragDrop);
            this.GameDirectoryTextBox.DragEnter += new System.Windows.Forms.DragEventHandler(this.TextBoxDragEnter);
            // 
            // SelectDirectoryButton
            // 
            this.SelectDirectoryButton.Location = new System.Drawing.Point(450, 240);
            this.SelectDirectoryButton.Name = "SelectDirectoryButton";
            this.SelectDirectoryButton.Size = new System.Drawing.Size(20, 20);
            this.SelectDirectoryButton.TabIndex = 3;
            this.SelectDirectoryButton.Text = "...";
            this.SelectDirectoryButton.UseVisualStyleBackColor = true;
            this.SelectDirectoryButton.Click += new System.EventHandler(this.SelectDirectory);
            // 
            // ApplyPatchButton
            // 
            this.ApplyPatchButton.Enabled = false;
            this.ApplyPatchButton.Location = new System.Drawing.Point(140, 270);
            this.ApplyPatchButton.Name = "ApplyPatchButton";
            this.ApplyPatchButton.Size = new System.Drawing.Size(200, 30);
            this.ApplyPatchButton.TabIndex = 4;
            this.ApplyPatchButton.Text = "请选择文件夹";
            this.ApplyPatchButton.UseVisualStyleBackColor = true;
            this.ApplyPatchButton.Click += new System.EventHandler(this.ApplyPatch);
            // 
            // AuthorLabel
            // 
            this.AuthorLabel.AutoSize = true;
            this.AuthorLabel.Location = new System.Drawing.Point(10, 310);
            this.AuthorLabel.Name = "AuthorLabel";
            this.AuthorLabel.Size = new System.Drawing.Size(0, 17);
            this.AuthorLabel.TabIndex = 5;
            this.AuthorLabel.LinkClicked += new LinkLabelLinkClickedEventHandler(this.OpenLink);
            // 
            // MainForm
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(480, 365);
            this.Controls.Add(this.HeaderImage);
            this.Controls.Add(this.SelectDirectoryLabel);
            this.Controls.Add(this.GameDirectoryTextBox);
            this.Controls.Add(this.SelectDirectoryButton);
            this.Controls.Add(this.ApplyPatchButton);
            this.Controls.Add(this.AuthorLabel);
            this.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.MaximizeBox = false;
            this.Padding = new System.Windows.Forms.Padding(10);
            this.Text = "《AI：梦境档案》简体中文补丁 - Xzonn";
            ((System.ComponentModel.ISupportInitialize)(this.HeaderImage)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        private System.Windows.Forms.PictureBox HeaderImage;
        private System.Windows.Forms.Label SelectDirectoryLabel;
        private System.Windows.Forms.TextBox GameDirectoryTextBox;
        private System.Windows.Forms.Button SelectDirectoryButton;
        private System.Windows.Forms.Button ApplyPatchButton;
        private System.Windows.Forms.LinkLabel AuthorLabel;
    }
}