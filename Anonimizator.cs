using System.Text.RegularExpressions;

namespace FileAnonimization;

public class Anonimizator
{
    public string Anonimize(string text)
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

    private static bool DoesContainSensitiveInformation(string word)
    {
        if (IsPhoneNumber(word)) return true;
        if (IsPesel(word)) return true;
        if (IsDate(word)) return true;
        return false;
    }

    private static bool IsPhoneNumber(string text)
    {
        return Regex.IsMatch(text, @"^[0-9]{9}$");
    }

    private static bool IsPesel(string text)
    {
        return Regex.IsMatch(text, @"^[0-9]{11}$");
    }

    private static bool IsDate(string text)
    {
        return Regex.IsMatch(text, @"^(\d{1,2})\.(\d{1,2})\.(\d{4})$");
    }
}