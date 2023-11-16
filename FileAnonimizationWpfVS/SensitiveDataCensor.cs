using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace FileAnonimizationWpfVS
{
    internal class SensitiveDataCensor
    {
        public string Anonymize(string text, (string, string)[] sensitiveData, Dictionary<string, string> rulebook)
        {
            foreach ((string, string) wordWithType in sensitiveData)
            {
                var word = wordWithType.Item1;
                var wordType = wordWithType.Item2;
                string rule;
                if (rulebook.TryGetValue(wordType, out rule))
                {
                    text = AnonymizeWordOfType(text, word, rule);
                }
            }
            return text;
        }

        private string AnonymizeWordOfType(string text, string word, string rule)
        {
            switch (rule)
            {
                case "leave only first character":
                    text = LeaveOnlyFirstLetter(text, word);
                    return text;
                case "censor last 6 digits":
                    text = CensorLastXDigits(text, word, 6);
                    return text;
                case "censor last 7 digits":
                    text = CensorLastXDigits(text, word, 7);
                    return text;
                case "leave only a year":
                    text = LeaveOnlyAYear(text, word);
                    return text;
                case "stars":
                    text = AnonymizeToStarsOfTheSameLength(text, word);
                    return text;
            }
            return text;
        }

        private string LeaveOnlyFirstLetter(string text, string word)
        {
            return text.Replace(word, word[0] + new String('*', word.Length - 1));
        }

        private string AnonymizeToStarsOfTheSameLength(string text, string word)
        {
            return text.Replace(word, new String('*', word.Length));
        }

        private string LeaveOnlyAYear(string text, string word)
        {
            var newWord = new StringBuilder(word);
            var current = word.Length - 1;
            while (current >= 0)
            {
                if (Char.IsDigit(newWord.ToString(), current))
                {
                    newWord[current] = '*';
                }
                current -= 1;
            }
            string pattern = @"\b\d{4}\b";
            Match m = Regex.Match(word, pattern, RegexOptions.IgnoreCase);
            if (m.Success)
                newWord = newWord.Replace("****", m.Value);
            return text.Replace(word, newWord.ToString());
        }

        private string CensorLastXDigits(string text, string word, int count)
        {
            var newWord = new StringBuilder(word);
            var current = word.Length - 1;
            while (count > 0 && current >= 0)
            {
                if (Char.IsDigit(newWord.ToString(), current))
                {
                    newWord[current] = '*';
                    count -= 1;
                }
                current -= 1;
            }
            return text.Replace(word, newWord.ToString());
        }
    }
}
