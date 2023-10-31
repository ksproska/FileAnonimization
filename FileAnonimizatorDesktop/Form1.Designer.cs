using System.Drawing;

namespace FileAnonimizatorDesktop
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
            this.DragDropPanel = new System.Windows.Forms.Panel();
            this.uploadedFile = new System.Windows.Forms.ListBox();
            this.Result = new System.Windows.Forms.Label();
            this.SensistiveData = new System.Windows.Forms.Label();
            this.DragDropPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // DragDropPanel
            // 
            this.DragDropPanel.Controls.Add(this.uploadedFile);
            this.DragDropPanel.Location = new System.Drawing.Point(126, 85);
            this.DragDropPanel.Name = "DragDropPanel";
            this.DragDropPanel.Size = new System.Drawing.Size(444, 154);
            this.DragDropPanel.TabIndex = 0;
            this.DragDropPanel.DragDrop += new System.Windows.Forms.DragEventHandler(this.DragDropPanel_DragDrop);
            this.DragDropPanel.DragEnter += new System.Windows.Forms.DragEventHandler(this.DragDropPanel_DragEnter);
            this.DragDropPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.panel1_Paint);
            // 
            // uploadedFile
            // 
            this.uploadedFile.FormattingEnabled = true;
            this.uploadedFile.ItemHeight = 16;
            this.uploadedFile.Location = new System.Drawing.Point(16, 14);
            this.uploadedFile.Name = "uploadedFile";
            this.uploadedFile.Size = new System.Drawing.Size(404, 84);
            this.uploadedFile.TabIndex = 0;
            this.uploadedFile.SelectedIndexChanged += new System.EventHandler(this.uploadedFile_SelectedIndexChanged);
            // 
            // Result
            // 
            this.Result.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.Result.Location = new System.Drawing.Point(43, 356);
            this.Result.Name = "Result";
            this.Result.Size = new System.Drawing.Size(714, 43);
            this.Result.TabIndex = 1;
            this.Result.Text = "Przeciągnij plik by zanonimizować dane.";
            this.Result.Click += new System.EventHandler(this.label1_Click);
            // 
            // SensistiveData
            // 
            this.SensistiveData.Location = new System.Drawing.Point(43, 294);
            this.SensistiveData.Name = "SensistiveData";
            this.SensistiveData.Size = new System.Drawing.Size(714, 23);
            this.SensistiveData.TabIndex = 2;
            // this.SensistiveData.Text = "Sensitive Data";
            this.SensistiveData.Click += new System.EventHandler(this.label1_Click_1);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.SensistiveData);
            this.Controls.Add(this.Result);
            this.Controls.Add(this.DragDropPanel);
            this.Name = "Form1";
            this.Text = "Form1";
            this.DragDropPanel.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Label SensistiveData;

        private System.Windows.Forms.Label Result;

        private System.Windows.Forms.ListBox uploadedFile;

        private System.Windows.Forms.ListBox listBox1;

        private System.Windows.Forms.Panel DragDropPanel;

        #endregion
    }
}