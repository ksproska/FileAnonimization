using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Documents;
namespace FileAnonimizationWpfVS
{
    internal class TextFormatter
    {
        public static Paragraph FormatText(Paragraph paragraph, string text, List<string> words)
        {

            Run end = new Run(text);
            if (words.Any())
            {
                Run run = new Run(text);


                int startIndex = run.Text.IndexOf(words[0]);
                int length = words[0].Length;

                Run beforeRun = new Run(run.Text.Substring(0, startIndex));

                Run wordA = new Run(run.Text.Substring(startIndex, length));
                wordA.Background = Brushes.Red;


                Run remainingRun = new Run(run.Text.Substring(startIndex + length));
                //end = new Run(run.Text.Substring(startIndex + length));

                paragraph.Inlines.Add(beforeRun);
                paragraph.Inlines.Add(wordA);
                words.Remove(words[0]);
                FormatText(paragraph, remainingRun.Text, words);
            }
            else if(!words.Any())
            {
                paragraph.Inlines.Add(end);
            }
               
            return paragraph;
        }
    }
}
