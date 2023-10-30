﻿using System;
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
        private TextAnonymizator _anonymizator;
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
            _anonymizator = new TextAnonymizator(csvDataReader.GetNames(), csvDataReader.GetSurnames());
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
                    var wordsToAnonimize = _anonymizator.GetWordsToAnonimize(text);
                    foreach ((string, string ) sensData in wordsToAnonimize)
                    {
                        uploadedFile.Items.Add(sensData.Item1 + " - " + sensData.Item2);
                    }
                    
                    string processedText = _anonymizator.AnonymizeToStarsOfTheSameLength(text, wordsToAnonimize);
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