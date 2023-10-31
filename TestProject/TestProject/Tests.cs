using System.Collections.Generic;
using System.Linq;
using FileAnonimizatorDesktop;
using NUnit.Framework;

namespace TestProject
{
    [TestFixture]
    public class Tests
    {
        private static Dictionary<string, string> _dictionary = new Dictionary<string, string>()
        {
            {"name", "leave only first character"},
            {"surname", "leave only first character"},
            {"date", "stars"},
            {"pesel", "stars"},
            {"phone number", "stars"},
        };

        [Test]
        public void TestAnonymize()
        {
            var original = "My name is Kamila and my phone number is 123456789";
            var wordsToAnonymize = new[] { ("Kamila", "name"), ("123456789", "phone number") };
            var expected = "My name is K***** and my phone number is *********";

            var actual = new SensitiveDataCensor().Anonymize(original, wordsToAnonymize, _dictionary);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TestAnonimizationPositiveCases()
        {
            var anonimizator = new SensitiveDataFinder(new List<string>(){"KAMILA", "OLA"}, new List<string>(){"SPROSKA"});
            var censor = new SensitiveDataCensor();
            var positiveTestCases = new List<string>
            {
                "Kamila",
                "Ola",
                "Sproska",
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

            foreach (string testCase in positiveTestCases)
            {
                var wordsToAnonymize = anonimizator.GetWordsToAnonimize(testCase);
                string processedText = censor.Anonymize(testCase, wordsToAnonymize, _dictionary);
                Assert.AreNotEqual(testCase, processedText);
            }
        }

        [Test]
        public void TestGetTypes()
        {
            var anonimizator = new SensitiveDataFinder(new List<string>(){"KAMILA", "OLA"}, new List<string>(){"SPROSKA"});
            var inputText = "My name is Kamila and my phone number is 123456789";
            Assert.AreEqual(new []{("Kamila", "name"), ("123456789", "phone number")}, anonimizator.GetWordsToAnonimize(inputText).ToList());
        }

        [Test]
        public void TestAnonimizationNegativeCases()
        {
            var anonimizator = new SensitiveDataFinder(new List<string>(), new List<string>());
            var censor = new SensitiveDataCensor();
            var negativeTestCases = new List<string>
            {
                "Dzień",
                "12345678910",
            };

            foreach (string testCase in negativeTestCases)
            {
                var wordsToAnonymize = anonimizator.GetWordsToAnonimize(testCase);
                string processedText = censor.Anonymize(testCase, wordsToAnonymize, _dictionary);
                Assert.AreEqual(testCase, processedText);
            }
        }

        [Test]
        public void TestPesel()
        {
            var positiveTestCases = new List<string>
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

            foreach (string testCase in positiveTestCases)
            {
                Assert.True(SensitiveDataFinder.IsPesel(testCase));
            }
        }
        [Test]
        public void TestPeselNegative()
        {
            var positiveTestCases = new List<string>
            {
                "12345678910"
            };

            foreach (string testCase in positiveTestCases)
            {
                Assert.False(SensitiveDataFinder.IsPesel(testCase));
            }
        }
    }
}