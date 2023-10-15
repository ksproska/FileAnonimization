using System;
using System.IO;
using NUnit.Framework;

namespace FileAnonimizationWithTests
{
    [TestFixture]
    public class Tests
    {
        string GetTextFromFile()
        {
            string workingDirectory = Environment.CurrentDirectory;
            string projectDirectory = Directory.GetParent(workingDirectory).Parent.Parent.FullName;
            string filepath = Path.Combine(projectDirectory, "FileAnonimizationWithTests", "sample.txt");

            string readText = File.ReadAllText(filepath);
            return readText;
        }

        [Test]
        public void Test1()
        {
            string readText = GetTextFromFile();
            Console.WriteLine("\"Original\":   " + readText);

            var anonimizator = new Anonimizator();
            string processedText = anonimizator.Anonimize(readText);
            Console.WriteLine("\"Anonymized\": " + processedText);
        }
    }
}