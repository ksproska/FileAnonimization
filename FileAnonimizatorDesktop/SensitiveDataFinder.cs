using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace FileAnonimizatorDesktop
{
    public class SensitiveDataFinder
    {
        private readonly List<String> _names;
        private readonly List<String> _surnames;

        public SensitiveDataFinder(List<String> names, List<String> surnames)
        {
            _names = names;
            _surnames = surnames;
        }
        
        public IEnumerable<(string, string)> GetWordsToAnonimize(string text)
        {
            var punctuation = text.Where(Char.IsPunctuation).Distinct().ToArray();
            var words = text.Split().Select(x => x.Trim(punctuation));
            var wordsToAnonymize = words.Select(x => (x, GetSensitiveInformationType(x))).Where(x => x.Item2 != "");
            return wordsToAnonymize;
        }

        public string GetSensitiveInformationType(string word)
        {
            if (IsPhoneNumber(word)) return "phone number";
            if (IsPesel(word)) return "pesel";
            if (IsDate(word)) return "date";
            if (IsName(word)) return "name";
            if (IsSurname(word)) return "surname";
            return "";
        }

        private static bool IsPhoneNumber(string text)
        {
            string pattern = @"^(\+[0-9]{11}|[0-9]{9}|[0-9]{3}-[0-9]{3}-[0-9]{3})$";
            return Regex.IsMatch(text, pattern);
        }

        public static bool IsPesel(string text)
        {
            if (!Regex.IsMatch(text, @"^[0-9]{11}$"))
            {
                return false;
            }
            return HasPeselCorrectChecksum(text);
        }
        
        private static bool HasPeselCorrectChecksum(string text)
        {
            var sum = 0;
            var weights = new[] { 1, 3, 7, 9, 1, 3, 7, 9, 1, 3 }; // https://obywatel.gov.pl/pl/dokumenty-i-dane-osobowe/czym-jest-numer-pesel
            for (int i = 0; i < weights.Length; i++)
            {
                int numbI = text[i] - '0';
                int digitOfMultiplication = (numbI * weights[i]) % 10;
                sum += digitOfMultiplication;
            }
            int controlNumb = text[text.Length - 1] - '0';
            int digitOfSum = (sum % 10);
            return (10 - digitOfSum) == controlNumb;
        }

        private static bool IsDate(string text)
        {
            var pattern = @"^" 
                          + @"(\d{1,2})\.(\d{1,2})\.(\d{4})" 
                          + @"|(\d{1,2})\-(\d{1,2})\-(\d{4})" 
                          + @"|(\d{4})\-(\d{1,2})\-(\d{1,2})" 
                          + @"|(\d{4})\/(\d{1,2})\/(\d{1,2})" 
                          + @"|(\d{1,2})\/(\d{1,2})\/(\d{4})" 
                          + @"$";
            return Regex.IsMatch(text, pattern);
        }

        private bool IsName(string text)
        {
            if (!Regex.IsMatch(text, @"^[A-Z][a-z]{2,12}$"))
            {
                return false;
            }
            return _names.Contains(text.ToUpper());
        }

        private bool IsSurname(string text)
        {
            if (!Regex.IsMatch(text, @"^[A-Z][a-z]{2,12}$"))
            {
                return false;
            }
            return _surnames.Contains(text.ToUpper());
        }
    }
}