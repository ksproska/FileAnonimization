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

namespace FileAnonimizationWpfVS
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ContextNameAndSurnameFinder _contextNameAndSurnameFinder;
        private ContextIlnessFinder _contextIlnessFinder;
        private SensitiveDataFinder _sensitiveDataFinder;
        private SensitiveDataCensor _sensitiveDataCensor;
        private Dictionary<string, string> _dictionary;
        private bool isMousePressed = false;
        private TextPointer selectionStart;
        private TextPointer selectionEnd;
        public string processedText = "";
        ObservableCollection<string> selectedElement;
        string text = "";
        private static (string, string)[] wordsToAnonimize;
        public MainWindow()
        {
            InitializeComponent();

            _contextNameAndSurnameFinder = new ContextNameAndSurnameFinder(
                new []{ "jest", "był", "była", "ma", "miał", "miała"}
                );
            _contextIlnessFinder = new ContextIlnessFinder((new[] { "choruje na", "chorował na", "chorowała na",
                "cierpi na", "ma objawy", "wykazuje objawy", "chory na"}).ToList().Select(x => x.ToLower()).ToList());
            string workingDirectory = Environment.CurrentDirectory;
            string projectDirectory = Directory.GetParent(workingDirectory).Parent.Parent.FullName;
            string namesPath = System.IO.Path.Combine(projectDirectory, "data", "names.csv");
            string surnamesPath = System.IO.Path.Combine(projectDirectory, "data", "surnames.csv");
            var csvDataReader = new CsvDataReader(namesPath, surnamesPath);
            _sensitiveDataFinder = new SensitiveDataFinder(csvDataReader.GetNames(), csvDataReader.GetSurnames(), _contextNameAndSurnameFinder, _contextIlnessFinder);
            _sensitiveDataCensor = new SensitiveDataCensor();
            _dictionary = new Dictionary<string, string>()
            {
                {"name", "leave only first character"},
                {"surname", "leave only first character"},
                {"phone number", "censor last 6 digits"},
                {"pesel", "censor last 7 digits"},
                {"date", "leave only a year"},
                {"suspected name or surname", "leave only first character"},
                {"suspected ilness",  "leave only first character"}
            };
            selectedElement = new ObservableCollection<string>();
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

        private void RichTextBox_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //isMousePressed = true;
            Point mouseClickPointDown = e.GetPosition(richTextBox);
         
            selectionStart = richTextBox.GetPositionFromPoint(mouseClickPointDown, true);
        }
        private void RichTextBox_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Point mouseClickPointUp = e.GetPosition(richTextBox);
            selectionEnd = richTextBox.GetPositionFromPoint(mouseClickPointUp, true);
            if (selectionStart != null && selectionEnd != null)
            {
                TextPointer start = selectionStart.GetInsertionPosition(LogicalDirection.Forward);
                TextPointer end = selectionEnd.GetInsertionPosition(LogicalDirection.Backward);
                
                if (start != null && end != null)
                {
                    richTextBox.Selection.Select(start, end);
                    TextRange selectedWordRange = new TextRange(start, end);
                    string selectedWord = selectedWordRange.Text;

                    MessageBox.Show("Selected word: " + selectedWord);
                }
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            
            if (ListBefore.SelectedItem != null)
            {
                
                selectedElement.Add(ListBefore.SelectedItem as string);
                //((ObservableCollection<string>)ListBefore.ItemsSource).Remove(selectedElement[0] as string);
            }
            ListAfter.ItemsSource = selectedElement;
            //ListAfter.Items.Add(selectedElement);
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            if (ListAfter.Items != null)
            {
                var w = new List<Tuple<string, string>>();

                for (int i = 0; i < ListAfter.Items.Count; i++)
                {
                    if(ListAfter.Items.GetItemAt(i) != null)
                    {
                        var t = wordsToAnonimize
                            .Where(x => x.Item1.Contains(ListAfter.Items.GetItemAt(i).ToString()))
                            .FirstOrDefault((null, null));
                        if ((null, null) != t)
                        {
                            w.Add(new Tuple<string, string>(t.Item1, t.Item2));
                        }
                        
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
    }
}
