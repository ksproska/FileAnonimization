using System;
using System.Collections.Generic;
using System.Linq;

namespace FileAnonimizationWpfVS
{
    public class ContextFinder
    {
        private readonly string[] _verbs;

        public ContextFinder(string[] verbs)
        {
            _verbs = verbs;
        }
        
        public (string, string)[] GetNamesAndSurnamesIfContext(string text)
        {
            return GetNamesAndSurnamesIfCapitalLetter(text).Concat(GetNamesAndSurnamesIfContextVerb(text))
                .Distinct()
                .Select(x => (x, "suspected name or surname"))
                .ToArray();
        }

        public (string, string)[] GetAddresses(string text)
        {
            int numb;
            return SplitWordsIntoPairs(text)
                .Where(x => x.Item1.Length > 0 && x.Item2.Length > 0)
                .Where(x => Char.IsUpper(x.Item1[0]) && int.TryParse(x.Item2, out numb))
                // .SelectMany(m => new [] {m.Item1, m.Item2 })
                .Select(x => x.Item1 + " " + x.Item2)
                .Select(s => (s, "address"))
                .ToArray();
        }

        private List<string> GetNamesAndSurnamesIfCapitalLetter(string text)
        {
            var pairs = SplitWordsIntoPairs(text).ToList();
            var punctuation = text.Where(Char.IsPunctuation).Distinct().ToArray();
            return pairs
                .Where(x => x.Item1.Length > 0 && x.Item2.Length > 0)
                .Where(x => !x.Item1.EndsWith(".") && Char.IsUpper(x.Item2[0]))
                .Select(x => x.Item2)
                .Select(x => x.Trim(punctuation))
                .Where(x => x.Length > 0)
                .Distinct()
                .ToList();
        }

        private List<(string, string)> SplitWordsIntoPairs(string text)
        {
            var words = text.Split().ToList();
            var result = new List<(string, string)>();
            for (int i = 0; i < words.Count - 1; i++)
            {
                result.Add((words[i], words[i + 1]));
            }

            return result;
        }

        private List<string> GetNamesAndSurnamesIfContextVerb(string text)
        {
            var pairs = SplitWordsIntoPairs(text).ToList();
            return pairs
                .Where(x => x.Item1.Length > 0 && x.Item2.Length > 0)
                .Where(x => !x.Item1.EndsWith(".") && Char.IsUpper(x.Item1[0]) && _verbs.Contains(x.Item2))
                .Select(x => x.Item1)
                .Distinct()
                .ToList();
        }
    }
}