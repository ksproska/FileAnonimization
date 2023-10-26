using System;
using System.Diagnostics.SymbolStore;
using System.Linq;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using DocumentFormat.OpenXml.Wordprocessing;

namespace FileAnonimizatorDesktop
{
    public class TextAnonimizator
    {
        
        private CsvDataReader csvData = new CsvDataReader();

        public string Anonimize(string text)
        {
            var punctuation = text.Where(Char.IsPunctuation).Distinct().ToArray();
            var words = text.Split().Select(x => x.Trim(punctuation));
            var wordsToAnonimize = words.Where(x => DoesContainSensitiveInformation(x));
            Console.WriteLine(string.Join(",", wordsToAnonimize));

            foreach (string word in wordsToAnonimize)
            {
                text = text.Replace(word, new String('*', word.Length));
            }

            return text;
        }

        public bool DoesContainSensitiveInformation(string word)
        {
            return IsPhoneNumber(word) || IsPesel(word) || IsDate(word) || IsName(word) || IsSurname(word);
        }

        private static bool IsPhoneNumber(string text)
        {
            return Regex.IsMatch(text, @"^[0-9]{9}$");
        }

        private static bool IsPesel(string text)
        {
            return Regex.IsMatch(text, @"^[0-9]{11}$");
        }

        private static bool IsDate(string text)
        {
            return Regex.IsMatch(text, @"^(\d{1,2})\.(\d{1,2})\.(\d{4})$");
        }

        private bool IsName(string text)
        {
            return csvData.GetNames().Contains(text.ToUpper());
        }

        private bool IsSurname(string text)
        {
            return csvData.GetSurnames().Contains(text.ToUpper());
        }
    }
}