using System;
using System.Linq;

namespace FileAnonimizationWpfVS
{
    internal class AnonymizedDataFinder
    {

        public string[] GetAnonimizedWords(string text)
        {
            return text.Split().Where(x => x.Contains("*")).ToArray();
        }
    }
}
