using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FileAnonimizatorDesktop
{
    public class CsvDataReader
    {
        private readonly List<String> _names = new List<string>();
        private readonly List<String> _surnames = new List<string>();

        public CsvDataReader(string namesPath, string surnamesPath)
        {
            List<String> csvRowsNames = File.ReadAllLines(namesPath, Encoding.UTF8).ToList();
            foreach (string row in csvRowsNames.Skip(1))
            {
                string[] columns = row.Split(',');
                _names.Add(columns[0]);
            }

            List<String> csvRowsSurnames = File.ReadAllLines(surnamesPath, Encoding.UTF8).ToList();
            foreach (string row in csvRowsSurnames.Skip(1))
            {
                string[] columns = row.Split(',');
                _surnames.Add(columns[0]);
            }
        }
    
        public List<String> GetNames()
        {
            return _names;
        }

        public List<String> GetSurnames()
        {
            return _surnames;
        }
    }
}