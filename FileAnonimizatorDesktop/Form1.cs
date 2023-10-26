using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.SharePoint;

namespace FileAnonimizatorDesktop
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            DragDropPanel.AllowDrop = true;
            DragDropPanel.AutoScroll = true;
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            //throw new System.NotImplementedException();
        }

        private void uploadedFile_SelectedIndexChanged(object sender, EventArgs e)
        {
            //throw new System.NotImplementedException();
        }

        private void DragDropPanel_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.All;
        }

        private void label1_Click(object sender, EventArgs e)
        {
        }
        private void DragDropPanel_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            //Console.WriteLine("\"Anonymized\": " + processedText);
            foreach (var file in files)
            {
                string text = WordFileExtractor.UploadAndExtractWordFile(file);
                try
                {
                    var anonimizator = new TextAnonimizator();
                    string processedText = anonimizator.Anonimize(text);
                    var punctuation = text.Where(Char.IsPunctuation).Distinct().ToArray();
                    var words = text.Split().Select(x => x.Trim(punctuation));
                    var sensitiveData = words.Where(x => anonimizator.DoesContainSensitiveInformation(x));
                    
                    StringBuilder wordsToAno = new StringBuilder();
                    foreach (var sensData in sensitiveData)
                    {
                        wordsToAno.Append(sensData + ", ");
                    }
                    Result.Text = processedText;
                    Result.ForeColor = Color.DarkRed;
                    SensistiveData.Text = wordsToAno.ToString();
                    SensistiveData.ForeColor = Color.DarkRed;
                    uploadedFile.Items.Add(wordsToAno);
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                }
            }
            uploadedFile.Visible = true;
        }

        private void label1_Click_1(object sender, EventArgs e)
        {
        }
    }
}