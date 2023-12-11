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
using ExportToWord = Microsoft.Office.Interop.Word;

namespace FileAnonimizationWpfVS
{
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
        private ObservableCollection<string> listBeforeItemsSource;
        private ObservableCollection<string> listAfterItemsSource;
        private List<Run> highlightedWords;

        public MainWindow()
        {
            InitializeComponent();

            _contextFinder = new ContextFinder(
                new[] { "jest", "był", "była", "ma", "miał", "miała" },
                new[] {"nr.", "numer", "number"}
            );
            _contextIlnessFinder = new ContextIlnessFinder((new[]
            {
                "choruje na", "chorował na", "chorowała na",
                "cierpi na", "ma objawy", "wykazuje objawy", "chory na"
            }).ToList().Select(x => x.ToLower()).ToList());
            string workingDirectory = Environment.CurrentDirectory;
            string projectDirectory = Directory.GetParent(workingDirectory).Parent.Parent.FullName;
            string namesPath = System.IO.Path.Combine(projectDirectory, "data", "names.csv");
            string surnamesPath = System.IO.Path.Combine(projectDirectory, "data", "surnames.csv");
            var csvDataReader = new CsvDataReader(namesPath, surnamesPath);
            _sensitiveDataFinder = new SensitiveDataFinder(csvDataReader.GetNames(), csvDataReader.GetSurnames(),
                _contextFinder, _contextIlnessFinder);
            _sensitiveDataCensor = new SensitiveDataCensor();
            _dictionary = new Dictionary<string, string>()
            {
                { "name", "leave only first character" },
                { "surname", "leave only first character" },
                { "phone number", "censor last 6 digits" },
                { "pesel", "censor last 7 digits" },
                { "date", "leave only a year" },
                { "suspected name or surname", "leave only first character" },
                { "user selection", "stars" },
                { "suspected ilness", "leave only first character" },
                { "address", "leave only first character" },
                { "postal code", "leave only minus sign" }
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
            Border border = sender as Border;
            if (border != null)
            {
                _previousFill = border.Background;
                if (e.Data.GetDataPresent(DataFormats.StringFormat))
                {
                    string dataString = (string)e.Data.GetData(DataFormats.StringFormat);
                    BrushConverter converter = new BrushConverter();
                    if (converter.IsValid(dataString))
                    {
                        Brush newFill = (Brush)converter.ConvertFromString(dataString);
                        border.Background = newFill;
                    }
                }
            }
        }

        private void ellipse_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;
            if (e.Data.GetDataPresent(DataFormats.StringFormat))
            {
                string dataString = (string)e.Data.GetData(DataFormats.StringFormat);
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
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    string[] filePaths = (string[])e.Data.GetData(DataFormats.FileDrop);
                    var firstFilePath = filePaths[0];
                    filepathLabel.Text = firstFilePath;
                    text = WordFileExtractor.UploadAndExtractWordFile(firstFilePath);


                    BrushConverter converter = new BrushConverter();
                    if (converter.IsValid(filePaths))
                    {
                        Brush newFill = (Brush)converter.ConvertFromString(firstFilePath);
                        rectangle.Background = newFill;
                    }

                    try
                    {
                        wordsToAnonimize = _sensitiveDataFinder.GetWordsToAnonimize(text);
                        processedText = _sensitiveDataCensor.Anonymize(text, wordsToAnonimize, _dictionary);
                        FlowDocument flowDoc = new FlowDocument();
                        richTextBox.Document = flowDoc;

                        Paragraph paragraph = new Paragraph();
                        var words = wordsToAnonimize.Select(g => g.Item1).ToList();

                        var punctuation = text.Where(Char.IsPunctuation).Distinct().ToArray();
                        var wordsAll = text.Split().Select(x => x.Trim(punctuation));
                        listBeforeItemsSource = new ObservableCollection<string>(words.Distinct().OrderBy(x => x).ToList());

                        highlightedWords = TextFormatter.FormatText(paragraph, text, words);
                        flowDoc.Blocks.Add(paragraph);
                        string t = new TextRange(paragraph.ContentStart, paragraph.ContentEnd).Text;

                        ListBefore.ItemsSource = listBeforeItemsSource;
                        ListAfter.ItemsSource = listAfterItemsSource;
                        ButtonPlus.IsEnabled = true;
                        ApplySelected();
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

        private void Plus_Button_Click(object sender, RoutedEventArgs e)
        {
            if (ListBefore.SelectedItem != null)
            {
                var listBeforeSelectedItem = ListBefore.SelectedItem as string;
                listBeforeItemsSource.Remove(listBeforeSelectedItem);
                ListBefore.UnselectAll();
                ListAfter.Items.Add(listBeforeSelectedItem);

                var selectedRuns = highlightedWords.Where(r => r.Text == listBeforeSelectedItem).ToList();
                selectedRuns.ForEach(r => r.Background = Brushes.LightSalmon);
            }
            else if (richTextBox.Selection != null && richTextBox.Selection.Text != "")
            {
                ListAfter.Items.Add(richTextBox.Selection.Text);
                ListBefore.UnselectAll();
            }
            else if (ListBefore.SelectedItem == null || ListBefore.SelectedItem == "" ||
                     richTextBox.Selection == null || richTextBox.Selection.Text != "")
            {
                MessageBox.Show("Select item");
            }

            ButtonMinus.IsEnabled = true;
            ApplySelected();
        }

        private void Minus_Button_Click(object sender, RoutedEventArgs e)
        {
            if (ListAfter.SelectedItem != null)
            {
                var listAfterSelectedItem = ListAfter.SelectedItem as string;
                ListAfter.Items.Remove(listAfterSelectedItem);
                ListAfter.UnselectAll();
                listBeforeItemsSource.Add(listAfterSelectedItem);
                
                var selectedRuns = highlightedWords.Where(r => r.Text == listAfterSelectedItem).ToList();
                selectedRuns.ForEach(r => r.Background = Brushes.Yellow);
            }
            else
            {
                MessageBox.Show("Select item");
            }
            ApplySelected();
        }

        private void ApplySelected()
        {
            if (ListAfter.Items != null)
            {
                var w = new List<Tuple<string, string>>();

                for (int i = 0; i < ListAfter.Items.Count; i++)
                {
                    if (ListAfter.Items.GetItemAt(i) != null)
                    {
                        var t = new Tuple<string, string>(ListAfter.Items.GetItemAt(i).ToString(),
                            _sensitiveDataFinder
                                .GetSensitiveInformationTypeForSelectedWords(
                                    ListAfter.Items.GetItemAt(i).ToString()
                                )
                        );
                        w.Add(t);
                    }
                }

                processedText =
                    _sensitiveDataCensor.Anonymize(text, w.Select(x => (x.Item1, x.Item2)).ToArray(), _dictionary);
                TextBox2.Text = processedText;
            }
            else
            {
                MessageBox.Show("Select items");
            }

            TextBox2.Text = processedText;
            ButtonExport.IsEnabled = true;
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            ExportToWord.Application wordapp = new ExportToWord.Application();
            wordapp.Visible = true;
            ExportToWord.Document worddoc;
            object wordobj = System.Reflection.Missing.Value;
            worddoc = wordapp.Documents.Add(ref wordobj);
            wordapp.Selection.TypeText(TextBox2.Text);
            wordapp = null;
        }
    }
}