using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DocumentFormat.OpenXml.Bibliography;

namespace FileAnonimizatorDesktop
{
    public class CsvDataReader
    {
        private List<String> names = new List<string>();
        private List<String> surnames = new List<string>();

        public CsvDataReader()
        {
            string workingDirectory = Environment.CurrentDirectory;
            Console.WriteLine(workingDirectory);
            string projectDirectory = Directory.GetParent(workingDirectory).Parent.FullName;
            string namespath = Path.Combine(projectDirectory, "data", "names.csv");
            var csvRowsNames = System.IO.File.ReadAllLines(namespath, Encoding.UTF8).ToList();

            foreach (var row in csvRowsNames.Skip(1))
            {
                var columns = row.Split(',');
                names.Add(columns[0]);
            }

            string surnamespath = Path.Combine(projectDirectory, "data", "surnames.csv");
            var csvRowsSurnames = System.IO.File.ReadAllLines(surnamespath, Encoding.UTF8).ToList();

            foreach (var row in csvRowsSurnames.Skip(1))
            {
                var columns = row.Split(',');
                surnames.Add(columns[0]);
            }
        }
    
        public List<String> GetNames()
        {
            return names;
        }

        public List<String> GetSurnames()
        {
            return surnames;
        }
    }
}