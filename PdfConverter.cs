using System.Net.Mime;
using System.Text;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;
using Path = System.IO.Path;

namespace FileAnonimization;

public class PdfConverter
{
    public string ExtractFromPdf()
    {
        string workingDirectory = Environment.CurrentDirectory;
        string projectDirectory = Directory.GetParent(workingDirectory).Parent.Parent.FullName;
        string filepath = Path.Combine(projectDirectory, "document.pdf");

        StringBuilder text = new StringBuilder();
        
        using(var pdf = PdfDocument.Open(@filepath))
        {
            foreach (var page in pdf.GetPages())
            {
                text.Append(ContentOrderTextExtractor.GetText(page));
                /*var otherText = string.Join(" ", page.GetWords());
                var rawText = page.Text;*/
            }
        }
        return text.ToString();
    }
}