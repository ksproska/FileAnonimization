using System;
using System.Collections.Generic;
using System.Linq;
using System.Printing;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FileAnonimizationWpfVS
{
    internal class SensitiveDataFinder
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

        public string GetSensitiveInformationTypeForSelectedWords(string word)
        {
            if (IsPhoneNumber(word)) return "phone number";
            if (IsPesel(word)) return "pesel";
            if (IsDate(word)) return "date";
            if (IsName(word)) return "name";
            if (IsSurname(word)) return "surname";
            return "user selection";
        }

        private static bool IsPhoneNumber(string text)
        {
            string pattern = @"^(\+[0-9]{11}|[0-9]{9}|[0-9]{3}-[0-9]{3}-[0-9]{3})$";
            return Regex.IsMatch(text, pattern);
        }

        public static bool IsPesel(string text)
        {   
            if (!Regex.IsMatch(text, @"^[0-9OI]{11}$"))
            {
                return false;
            }
            if (!Regex.IsMatch(text, @"^[0-9]{11}$"))
            {
                Console.WriteLine(text);
                text = text.Replace("O", "0");
                text = text.Replace("I", "1");
                Console.WriteLine(text);
            }
            return HasPeselCorrectChecksum(text) && HasPeselDate(text);
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

        private static bool HasPeselDate(string text)
        {
            int currentYear = int.Parse(DateTime.Now.Year.ToString());
            int currentMonth = int.Parse(DateTime.Now.Month.ToString());
            int currentDay = int.Parse(DateTime.Now.Day.ToString());
            string pesel_year = text.Substring(0, 2);
            int month = int.Parse(text.Substring(2, 2));
            int day = int.Parse(text.Substring(4, 2));
            int year = 0;
            //Check if year is above 2000, if it's less than current year and normalize month and year
            if (21 <= month && month <= 32)
            {
                year += int.Parse("20" + pesel_year);
                month -= 20;
                if (year >= currentYear)
                {
                    return false;
                }

                if (year == currentYear && month >= currentMonth)
                {
                    return false;
                }

                if (year == currentYear && month == currentMonth && day >= currentDay)
                {
                    return false;
                }
            }
            else if (month >= 1 && month <= 12)
            {
                year += int.Parse("19" + pesel_year);
            }
            else
            {
                return false;
            }
            
            //Check if day is between 1 and num of Days in the month in the specific year
            if (day < 1 || day > DateTime.DaysInMonth(year, month))
            {
                return false;
            }

            return true;
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
