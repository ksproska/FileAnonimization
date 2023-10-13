﻿using FileAnonimization;

string GetTextFromFile()
{
    string workingDirectory = Environment.CurrentDirectory;
    string projectDirectory = Directory.GetParent(workingDirectory).Parent.Parent.FullName;
    string filepath = Path.Combine(projectDirectory, "sample.txt");
    
    string readText = File.ReadAllText(filepath);
    return readText;
}
var pdfConverter = new PdfConverter();
string readText = pdfConverter.ExtractFromPdf();//GetTextFromFile();
Console.WriteLine("\"Original\":   " + readText);

var anonimizator = new Anonimizator();
string processedText = anonimizator.Anonimize(readText);
Console.WriteLine("\"Anonymized\": " + processedText);


