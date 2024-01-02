using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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
        private AnonymizedDataFinder _anonymizedDataFinder;
        private bool isMousePressed = false;
        private TextPointer selectionStart;
        private TextPointer selectionEnd;
        public string processedText = "";
        ObservableCollection<string> selectedElement;
        ObservableCollection<string> unselectedElement;
        string text = "";
        private static string[] anonimizedWords;
        private ObservableCollection<string> listBeforeItemsSource;
        private ObservableCollection<string> listAfterItemsSource;
        private List<Run> highlightedWords;

        public MainWindow()
        {
            InitializeComponent();
            string workingDirectory = Environment.CurrentDirectory;
            string projectDirectory = Directory.GetParent(workingDirectory).Parent.Parent.FullName;
            string namesPath = System.IO.Path.Combine(projectDirectory, "data", "names.csv");
            string surnamesPath = System.IO.Path.Combine(projectDirectory, "data", "surnames.csv");
            var csvDataReader = new CsvDataReader(namesPath, surnamesPath);
            _anonymizedDataFinder = new AnonymizedDataFinder();
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
                        anonimizedWords = _anonymizedDataFinder.GetAnonimizedWords(text);
                        FlowDocument flowDoc = new FlowDocument();
                        OriginalTextBox.Document = flowDoc;

                        Paragraph paragraph = new Paragraph();
                        highlightedWords = TextFormatter.FormatText(paragraph, text, anonimizedWords.ToList());
                        flowDoc.Blocks.Add(paragraph);
                        string t = new TextRange(paragraph.ContentStart, paragraph.ContentEnd).Text;
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(exception);
                        FlowDocument flowDoc = new FlowDocument();
                        OriginalTextBox.Document = flowDoc;
                        Paragraph paragraph = new Paragraph();
                        paragraph.Inlines.Add(exception.ToString());
                        flowDoc.Blocks.Add(paragraph);
                    }
                    try
                    {
                        Dictionary<char, int> hist = LettersHistogram.Histogram( Regex.Replace(text, @"\s", "").ToLower());
                        letterChart.ChartAreaStyle = new Style();
                        letterChart.DataContext = hist.OrderBy(kvp => kvp.Key).ToList();
                    }
                    catch (Exception exception)
                    {
                    }
                }
            }
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            ExportToWord.Application wordapp = new ExportToWord.Application();
            wordapp.Visible = true;
            ExportToWord.Document worddoc;
            object wordobj = System.Reflection.Missing.Value;
            worddoc = wordapp.Documents.Add(ref wordobj);
            wordapp.Selection.TypeText("this function is not yet implemented");
            wordapp = null;
        }
    }
}