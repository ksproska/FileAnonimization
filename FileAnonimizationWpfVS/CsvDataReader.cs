using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileAnonimizationWpfVS
{
    internal class CsvDataReader
    {
        private readonly List<String> _names;
        private readonly List<String> _surnames;

        public CsvDataReader(string namesPath, string surnamesPath)
        {
            _names = new List<string>();
            _surnames = new List<string>();

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
