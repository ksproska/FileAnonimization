using System.Text.RegularExpressions;

string GetTextFromFile()
{
    string workingDirectory = Environment.CurrentDirectory;
    string projectDirectory = Directory.GetParent(workingDirectory).Parent.Parent.FullName;
    string filepath = Path.Combine(projectDirectory, "sample.txt");
    
    string readText = File.ReadAllText(filepath);
    return readText;
}

bool DoesContainSensitiveInformation(string word)
{
    if (IsPhoneNumber(word)) return true;
    if (IsPesel(word)) return true;
    if (IsDate(word)) return true;
    return false;
}

bool IsPhoneNumber(string text)
{
    return Regex.IsMatch(text, @"^[0-9]{9}$");
}

bool IsPesel(string text)
{
    return Regex.IsMatch(text, @"^[0-9]{11}$");
}

bool IsDate(string text)
{
    return Regex.IsMatch(text, @"^(\d{1,2})\.(\d{1,2})\.(\d{4})$");
}

string ProcessData(string text)
{
    var punctuation = text.Where(Char.IsPunctuation).Distinct().ToArray();
    var words = text.Split().Select(x => x.Trim(punctuation));
    var wordsToAnonimize = words.Where(x => DoesContainSensitiveInformation(x));
    
    foreach (string word in wordsToAnonimize)
    {
        text = text.Replace(word, new String('*', word.Length));
    }
    return text;
}

string readText = GetTextFromFile();
Console.WriteLine("\"Original\":   " + readText);

string processedText = ProcessData(readText);
Console.WriteLine("\"Anonymized\": " + processedText);
