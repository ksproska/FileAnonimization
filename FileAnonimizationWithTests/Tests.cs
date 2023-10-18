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
                "Kamila",
                "Ola",
                "Alexandra",
                "284192537",
                "+46284192537",
                "284-192-537",
                "86111771763",
                "14.03.2000",
                "14-03-2000",
                "2000-03-14",
                "2000/03/14",
                "14/03/2000"
            };

            foreach (string testCase in positive_test_cases)
            {
                string processedText = anonimizator.Anonimize(testCase);
                Assert.AreNotEqual(testCase, processedText);
            }
        }

        [Test]
        public void TestPesel()
        {
            var positive_test_cases = new List<string>
            {
                "86111771763",
                "94121916548",
                "02292498871",
                "07320263197",
                "99090876327",
                "54111815741",
                "05262075575",
                "80040719151",
                "68011171726",
                "85111139586"
            };

            foreach (string testCase in positive_test_cases)
            {
                Assert.True(Anonimizator.IsPesel(testCase));
            }
        }
    }
}