using DocumentFormat.OpenXml.Office2016.Presentation.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using exporttoword = Microsoft.Office.Interop.Word;

namespace FileAnonimizationWpfVS
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ContextFinder _contextFinder;
        private ContextIlnessFinder _contextIlnessFinder;
        private SensitiveDataFinder _sensitiveDataFinder;
        private SensitiveDataCensor _sensitiveDataCensor;
        private Dictionary<string, string> _dictionary;
        private bool isMousePressed = false;
        private TextPointer selectionStart;
        private TextPointer selectionEnd;
        public string processedText = "";
        ObservableCollection<string> selectedElement;
        ObservableCollection<string> unselectedElement;
        string text = "";
        private static (string, string)[] wordsToAnonimize;
        public MainWindow()
        {
            InitializeComponent();

            _contextFinder = new ContextFinder(
                new []{ "jest", "był", "była", "ma", "miał", "miała"}
                );
            _contextIlnessFinder = new ContextIlnessFinder((new[] { "choruje na", "chorował na", "chorowała na",
                "cierpi na", "ma objawy", "wykazuje objawy", "chory na"}).ToList().Select(x => x.ToLower()).ToList());
            string workingDirectory = Environment.CurrentDirectory;
            string projectDirectory = Directory.GetParent(workingDirectory).Parent.Parent.FullName;
            string namesPath = System.IO.Path.Combine(projectDirectory, "data", "names.csv");
            string surnamesPath = System.IO.Path.Combine(projectDirectory, "data", "surnames.csv");
            var csvDataReader = new CsvDataReader(namesPath, surnamesPath);
            _sensitiveDataFinder = new SensitiveDataFinder(csvDataReader.GetNames(), csvDataReader.GetSurnames(), _contextFinder, _contextIlnessFinder);
            _sensitiveDataCensor = new SensitiveDataCensor();
            _dictionary = new Dictionary<string, string>()
            {
                {"name", "leave only first character"},
                {"surname", "leave only first character"},
                {"phone number", "censor last 6 digits"},
                {"pesel", "censor last 7 digits"},
                {"date", "leave only a year"},
                {"suspected name or surname", "leave only first character"},
                {"user selection", "stars"}
                {"suspected ilness",  "leave only first character"},
                {"address", "leave only first character"},
                {"postal code", "leave only minus sign"}
            };
            selectedElement = new ObservableCollection<string>();
            unselectedElement = new ObservableCollection<string>();
        }

        private void ellipse_MouseMove(object sender, MouseEventArgs e)
        {
            Rectangle rectangle = sender as Rectangle;
            if (rectangle != null && e.LeftButton == MouseButtonState.Pressed)
            {
                DragDrop.DoDragDrop(rectangle,
                                     rectangle.Fill.ToString(),
                                     DragDropEffects.Copy);
            }
        }

        private Brush _previousFill = null;
        private void ellipse_DragEnter(object sender, DragEventArgs e)
        {
            Border rectangle = sender as Border;
            if (rectangle != null)
            {
                // Save the current Fill brush so that you can revert back to this value in DragLeave.
                _previousFill = rectangle.Background;

                // If the DataObject contains string data, extract it.
                if (e.Data.GetDataPresent(DataFormats.StringFormat))
                {
                    string dataString = (string)e.Data.GetData(DataFormats.StringFormat);

                    // If the string can be converted into a Brush, convert it.
                    BrushConverter converter = new BrushConverter();
                    if (converter.IsValid(dataString))
                    {
                        Brush newFill = (Brush)converter.ConvertFromString(dataString);
                        rectangle.Background = newFill;
                    }
                }
            }
        }

        private void ellipse_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;

            // If the DataObject contains string data, extract it.
            if (e.Data.GetDataPresent(DataFormats.StringFormat))
            {
                string dataString = (string)e.Data.GetData(DataFormats.StringFormat);

                // If the string can be converted into a Brush, allow copying.
                BrushConverter converter = new BrushConverter();
                if (converter.IsValid(dataString))
                {
                    e.Effects = DragDropEffects.Copy | DragDropEffects.Move;
                }
            }
        }

        private void ellipse_DragLeave(object sender, DragEventArgs e)
        {
            Border rectangle = sender as Border;
            if (rectangle != null)
            {
                rectangle.Background = _previousFill;
            }
        }
        private void ellipse_Drop(object sender, DragEventArgs e)
        {
            Border rectangle = sender as Border;
            if (rectangle != null)
            {
                // If the DataObject contains string data, extract it.
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    string[] dataString = (string[])e.Data.GetData(DataFormats.FileDrop);
                    text = WordFileExtractor.UploadAndExtractWordFile(dataString[0]);
                    
                    BrushConverter converter = new BrushConverter();
                    if (converter.IsValid(dataString))
                    {
                        Brush newFill = (Brush)converter.ConvertFromString(dataString[0]);
                        rectangle.Background = newFill;
                    }
                    try
                    {
                    
                        wordsToAnonimize = _sensitiveDataFinder.GetWordsToAnonimize(text);
                        /*foreach ((string, string) sensData in wordsToAnonimize)
                        {
                            uploadedFile.Items.Add(sensData.Item1 + " - " + sensData.Item2 + " - " + _dictionary[sensData.Item2]);
                        }*/
                        
                        processedText = _sensitiveDataCensor.Anonymize(text, wordsToAnonimize, _dictionary);
                        FlowDocument flowDoc = new FlowDocument();
                        richTextBox.Document = flowDoc;
                       
                        Paragraph paragraph = new Paragraph();
                        var words = wordsToAnonimize.Select(g => g.Item1);
                        
                        var punctuation = text.Where(Char.IsPunctuation).Distinct().ToArray();
                        var wordsAll = text.Split().Select(x => x.Trim(punctuation));
                        var onlyWordsToAnonimize = wordsAll.Where(x => words.Contains(x));

                        Paragraph finalParagraph = TextFormatter.FormatText(paragraph, text, onlyWordsToAnonimize.ToList());
                        flowDoc.Blocks.Add(finalParagraph);

                        string t = new TextRange(finalParagraph.ContentStart, paragraph.ContentEnd).Text;

                        ListBefore.ItemsSource = words.Distinct().OrderBy(x => x).ToList();
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(exception);
                        FlowDocument flowDoc = new FlowDocument();
                        richTextBox.Document = flowDoc;
                        Paragraph paragraph = new Paragraph();
                        paragraph.Inlines.Add(exception.ToString());
                        flowDoc.Blocks.Add(paragraph);
                    }
                    
                }
                
            }
        }

        //OK button
        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            if (ListAfter.Items != null)
            {
                var w = new List<Tuple<string, string>>();

                for (int i = 0; i < ListAfter.Items.Count; i++)
                {
                    if (ListAfter.Items.GetItemAt(i) != null)
                    {
                        var t = new Tuple<string, string>(ListAfter.Items.GetItemAt(i).ToString(), _sensitiveDataFinder.GetSensitiveInformationTypeForSelectedWords(ListAfter.Items.GetItemAt(i).ToString()));
                        w.Add(t);
                    }

                }

                processedText = _sensitiveDataCensor.Anonymize(text, w.Select(x => (x.Item1, x.Item2)).ToArray(), _dictionary);
                TextBox2.Text = processedText;
            }
            else
            {
                MessageBox.Show("Select items");
            }
            TextBox2.Text = processedText;
        }

        //plus button
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {

            if (ListBefore.SelectedItem != null)
            {
                ListAfter.Items.Add(ListBefore.SelectedItem as string);
                ListBefore.UnselectAll();
            }
            else if (richTextBox.Selection != null && richTextBox.Selection.Text != "")
            {
                ListAfter.Items.Add(richTextBox.Selection.Text);
                ListBefore.UnselectAll();
            }
            else if (ListBefore.SelectedItem == null || ListBefore.SelectedItem == "" || richTextBox.Selection == null || richTextBox.Selection.Text != "")
            {
                MessageBox.Show("Select item");
            }
        }

        //minus button
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (ListAfter.SelectedItem != null)
            {
                ListAfter.Items.Remove(ListAfter.SelectedItem as string);
                ListAfter.UnselectAll();
            }
            else
            {
                MessageBox.Show("Select item");
            }
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }


        private void ListBox_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {

        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void ExportBtn_Click(object sender, RoutedEventArgs e)
        {
            exporttoword.Application wordapp = new exporttoword.Application();
            wordapp.Visible = true;
            exporttoword.Document worddoc;
            object wordobj = System.Reflection.Missing.Value;
            worddoc = wordapp.Documents.Add(ref wordobj);
            wordapp.Selection.TypeText(TextBox2.Text);
            wordapp = null;
        }
    }
}
