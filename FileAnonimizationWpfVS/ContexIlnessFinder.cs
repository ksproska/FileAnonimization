using System.Collections.Generic;
using System.Linq;

namespace FileAnonimizationWpfVS
{
    public class ContextIlnessFinder
    {
        private readonly List<string> _verbs;

        public ContextIlnessFinder(List<string> verbs)
        {
            _verbs = verbs;
        }
        
        public (string, string)[] GetIlnessIfContext(string text)
        {
            return GetIlnessIfContextVerb(text)
                .Distinct()
                .Select(x => (x, "suspected ilness"))
                .ToArray();
        }
        
        private List<(string, string)> SplitWordsIntoPairs(string text)
        {
            var words = text.Split().ToList();
            var result = new List<(string, string)>();
            for (int i = 0; i < words.Count - 2; i++)
            {
                result.Add((words[i] + " " + words[i + 1], words[i + 2]));
            }

            return result;
        }

        private List<string> GetIlnessIfContextVerb(string text)
        {
            var pairs = SplitWordsIntoPairs(text).ToList();
            return pairs
                .Where(x => x.Item1.Length > 0 && x.Item2.Length > 0)
                .Where(x => !x.Item1.EndsWith(".") && _verbs.Contains(x.Item1.ToLower()))
                .Select(x => x.Item2)
                .Distinct()
                .ToList();
        }
    }
}