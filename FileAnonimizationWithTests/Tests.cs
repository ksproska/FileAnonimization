using System;
using System.Collections.Generic;
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
        public void TestReadingFileWorks()
        {
            string readText = GetTextFromFile();
            Assert.IsNotEmpty(readText);
        }

        [Test]
        public void TestAnonimizeToStarsOfTheSameLength()
        {
            var original = "My name is Kamila and my phone number is 123456789";
            var wordsToAnonimize = new[] { "Kamila", "123456789" };
            var expected = "My name is ****** and my phone number is *********";

            var actual = Anonimizator.AnonimizeToStarsOfTheSameLength(original, wordsToAnonimize);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TestAnonimizationWorked()
        {
            var anonimizator = new Anonimizator();
            var positive_test_cases = new List<string>
            {
                "284192537",
                "12345678910",
                "14.03.2000"
            };

            foreach (string testCase in positive_test_cases)
            {
                string processedText = anonimizator.Anonimize(testCase);
                Assert.AreNotEqual(testCase, processedText);
            }
        }
    }
}