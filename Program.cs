string workingDirectory = Environment.CurrentDirectory;
string projectDirectory = Directory.GetParent(workingDirectory).Parent.Parent.FullName;
string filepath = Path.Combine(projectDirectory, "sample.txt");

string readText = File.ReadAllText(filepath);
Console.WriteLine("\"Original\":   " + readText);

string ProcessData(string originalInput)
{
    // do sth
    return originalInput;
}

string processedText = ProcessData(readText);
Console.WriteLine("\"Anonymized\": " + processedText);
