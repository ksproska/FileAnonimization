using System;
using System.Collections.Generic;
using System.Linq;

namespace KamilaDev
{
    public class ContextNameAndSurnameFinder
    {
        private readonly string[] _verbs;

        public ContextNameAndSurnameFinder(string[] verbs)
        {
            _verbs = verbs;
        }
        
        public IEnumerable<(string, string)> GetNamesAndSurnamesIfContext(string text)
        {
            return GetNamesAndSurnamesIfCapitalLetter(text).Concat(GetNamesAndSurnamesIfContextVerb(text))
                .Distinct()
                .Select(x => (x, "suspected name or surname"));
        }

        public IEnumerable<string> GetNamesAndSurnamesIfCapitalLetter(string text)
        {
            var pairs = SplitWordsIntoPairs(text).ToList();
            var punctuation = text.Where(Char.IsPunctuation).Distinct().ToArray();
            return pairs
                .Where(x => !x.Item1.EndsWith(".") && Char.IsUpper(x.Item2[0]))
                .Select(x => x.Item2)
                .Select(x => x.Trim(punctuation))
                .Distinct();
        }

        public IEnumerable<(string, string)> SplitWordsIntoPairs(string text)
        {
            var words = text.Split().ToList();
            var result = new List<(string, string)>();
            for (int i = 0; i < words.Count - 1; i++)
            {
                result.Add((words[i], words[i + 1]));
            }

            return result;
        }

        public IEnumerable<string> GetNamesAndSurnamesIfContextVerb(string text)
        {
            var pairs = SplitWordsIntoPairs(text).ToList();
            return pairs.Where(x => !x.Item1.EndsWith(".") && Char.IsUpper(x.Item1[0]) && _verbs.Contains(x.Item2))
                .Select(x => x.Item1)
                .Distinct();
        }
    }
}