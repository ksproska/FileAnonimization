using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;
using System.Windows.Media;

namespace FileAnonimizationWpfVS
{
    internal class TextFormatter
    {
        static List<int> FindStartIndexes(string longerText, string shorterText)
        {
            List<int> indexes = new List<int>();

            int index = 0;
            while ((index = longerText.IndexOf(shorterText, index)) != -1)
            {
                indexes.Add(index);
                index += shorterText.Length;
            }

            return indexes;
        }

        static List<(int, string)> FilterDistinctFirstValues(List<(int, string)> inputList)
        {
            HashSet<int> uniqueFirstValues = new HashSet<int>();
            List<(int, string)> resultList = new List<(int, string)>();

            foreach (var tuple in inputList)
            {
                if (uniqueFirstValues.Add(tuple.Item1))
                {
                    // If the first value is unique, add the tuple to the result list
                    resultList.Add(tuple);
                }
            }

            return resultList;
        }

        public static List<(int, string)> orderedWordsList(string text, List<string> words)
        {
            var wordsWithIndexes = new List<(int, string)> { };
            foreach (var word in words)
            {
                var indexes = FindStartIndexes(text, word);
                foreach (var index in indexes)
                {
                    wordsWithIndexes.Add((index, word));
                }
            }

            var orderdWordsWithIndexes = wordsWithIndexes.OrderBy(x => x.Item1).ThenByDescending(x => x.Item2.Length)
                .Distinct().ToList();
            return FilterDistinctFirstValues(orderdWordsWithIndexes);
        }

        public static List<Run> FormatText(Paragraph paragraph, string text, List<string> words)
        {
            var highlighted = new List<Run> { };
            var orderedWordsWithInexes = orderedWordsList(text, words);
            var startInx = 0;
            foreach (var wordWithInex in orderedWordsWithInexes)
            {
                if (startInx < wordWithInex.Item1)
                {
                    var textBeforsSTr = text.Substring(startInx, wordWithInex.Item1 - startInx);
                    Run beforeText = new Run(textBeforsSTr);
                    paragraph.Inlines.Add(beforeText);
                    startInx += textBeforsSTr.Length;
                }
                Run selectedWord = new Run(wordWithInex.Item2);
                highlighted.Add(selectedWord);
                selectedWord.Background = Brushes.Yellow;
                paragraph.Inlines.Add(selectedWord);
                startInx += wordWithInex.Item2.Length;
            }
            var remainingText = text.Substring(startInx, text.Length - startInx);
            Run remainingRun = new Run(remainingText);
            paragraph.Inlines.Add(remainingRun);

            return highlighted;
        }
    }
}