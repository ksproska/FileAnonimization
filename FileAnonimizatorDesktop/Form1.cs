using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.SharePoint;

namespace FileAnonimizatorDesktop
{
    public partial class Form1 : Form
    {
        private SensitiveDataFinder _sensitiveDataFinder;
        private SensitiveDataCensor _sensitiveDataCensor;
        private Dictionary<string, string> _dictionary;
        public Form1()
        {
            InitializeComponent();
            DragDropPanel.AllowDrop = true;
            DragDropPanel.AutoScroll = true;
            
            string workingDirectory = Environment.CurrentDirectory;
            string projectDirectory = Directory.GetParent(workingDirectory).Parent.FullName;
            string namesPath = Path.Combine(projectDirectory, "data", "names.csv");
            string surnamesPath = Path.Combine(projectDirectory, "data", "surnames.csv");
            var csvDataReader = new CsvDataReader(namesPath, surnamesPath);
            _sensitiveDataFinder = new SensitiveDataFinder(csvDataReader.GetNames(), csvDataReader.GetSurnames());
            _sensitiveDataCensor = new SensitiveDataCensor();
            _dictionary = new Dictionary<string, string>()
            {
                {"name", "leave only first character"},
                {"surname", "leave only first character"},
                {"phone number", "censor last 6 digits"},
                {"pesel", "censor last 7 digits"},
                {"date", "leave only a year"},
            };
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

        private void label1_Click(object sender, EventArgs e) {}
        private void DragDropPanel_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            foreach (string file in files)
            {
                string text = WordFileExtractor.UploadAndExtractWordFile(file);
                try
                {
                    var wordsToAnonimize = _sensitiveDataFinder.GetWordsToAnonimize(text);
                    foreach ((string, string ) sensData in wordsToAnonimize)
                    {
                        uploadedFile.Items.Add(sensData.Item1 + " - " + sensData.Item2 + " - " + _dictionary[sensData.Item2]);
                    }
                    
                    string processedText = _sensitiveDataCensor.Anonymize(text, wordsToAnonimize, _dictionary);
                    Result.Text = processedText;
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                }
            }
            uploadedFile.Visible = true;
        }

        private void label1_Click_1(object sender, EventArgs e) {}
    }
}