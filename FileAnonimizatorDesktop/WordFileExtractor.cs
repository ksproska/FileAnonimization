using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using DocumentFormat.OpenXml.Packaging;
using Microsoft.SharePoint;


namespace FileAnonimizatorDesktop
{
    public class WordFileExtractor
    {
        public static string UploadAndExtractWordFile(string filePath)
        {
            const string wordmlNamespace = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";
            StringBuilder textBuilder = new StringBuilder();
            
            using (WordprocessingDocument wdDoc = WordprocessingDocument.Open(filePath, false))
            {
                // Manage namespaces to perform XPath queries.  
                NameTable nt = new NameTable();
                XmlNamespaceManager nsManager = new XmlNamespaceManager(nt);
                nsManager.AddNamespace("w", wordmlNamespace);

                // Get the document part from the package.  
                // Load the XML in the document part into an XmlDocument instance.  
                XmlDocument xdoc = new XmlDocument(nt);
                xdoc.Load(wdDoc.MainDocumentPart.GetStream());

                XmlNodeList paragraphNodes = xdoc.SelectNodes("//w:p", nsManager);
                foreach (XmlNode paragraphNode in paragraphNodes)
                {
                    XmlNodeList textNodes = paragraphNode.SelectNodes(".//w:t", nsManager);
                    foreach (System.Xml.XmlNode textNode in textNodes)
                    {
                        textBuilder.Append(textNode.InnerText);
                    }

                    textBuilder.Append(Environment.NewLine);
                }
            }

            return textBuilder.ToString();
        }
    }
}