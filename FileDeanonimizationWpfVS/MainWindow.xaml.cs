using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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

        private void Dictionary_Click(object sender, RoutedEventArgs e)
        {

            ListBox lb = new ListBox();
            Window dict = new Window();
            dict.Width = 300;
            dict.Height = 300;
            
            string workingDirectory = Environment.CurrentDirectory;
            string fileDir = Directory.GetParent(workingDirectory).Parent.Parent.FullName;
            string fileDest = System.IO.Path.Combine(fileDir, "dictionary.txt");
            string dictContent;
            
            using (var sr = new StreamReader(fileDest))
            {
                dictContent = sr.ReadToEnd();
            }
            
            Grid grid = new Grid();
            RowDefinition rowDef1 = new RowDefinition();
            RowDefinition rowDef2 = new RowDefinition();
            ColumnDefinition colDef1 = new ColumnDefinition();
            ColumnDefinition colDef2 = new ColumnDefinition();
            ColumnDefinition colDef3 = new ColumnDefinition();


            grid.RowDefinitions.Add(rowDef1);
            grid.RowDefinitions.Add(rowDef2);
            grid.ColumnDefinitions.Add(colDef1);
            grid.ColumnDefinitions.Add(colDef2);
            grid.ColumnDefinitions.Add(colDef3);

            grid.RowDefinitions[0].Height = new GridLength(7, GridUnitType.Star);
            grid.ColumnDefinitions[0].Width = new GridLength(3, GridUnitType.Star);
            grid.ColumnDefinitions[1].Width = new GridLength(3, GridUnitType.Star);
            grid.ColumnDefinitions[2].Width = new GridLength(4, GridUnitType.Star);

            List<string> dictList = new List<string>();
            dictList = dictContent.Split("\n").ToList();
            //dictList = dictContent.Split("\r").ToList();

            dictList.Remove("");

            if (dictList != null)
            {
                for(int i = 0; i < dictList.Count; i++)
                {
                    dictList[i] = dictList[i].Replace("\r", "");
                }
            }

            

            lb.ItemsSource = dictList;
            
            Button deleteBtn = new Button();
            deleteBtn.Name = "Delete";
            deleteBtn.Content = "Remove";
            deleteBtn.Visibility = Visibility.Visible;
           
            
            deleteBtn.Click += new RoutedEventHandler(delegate (Object o, RoutedEventArgs a)
            {
                ObservableCollection<string> dictWords = new ObservableCollection<string>();
                foreach(string word in lb.Items) {
                    dictWords.Add(word);
                }
                dictWords.Remove(lb.SelectedItem as string);
                lb.ItemsSource = dictWords; 
            });

            Button saveBtn = new Button();
            saveBtn.Name = "Save";
            saveBtn.Content = "Save";
            saveBtn.Visibility = Visibility.Visible;

            saveBtn.Click += new RoutedEventHandler(delegate (Object o, RoutedEventArgs a)
            {
                ObservableCollection<string> dictWords = new ObservableCollection<string>();
                StreamWriter writeFile;
                writeFile = new StreamWriter(new IsolatedStorageFileStream(fileDest, FileMode.Truncate));
                writeFile.Close();

                if (!lb.Items.IsEmpty)
                {
                    File.AppendAllText(fileDest, (lb.Items[0] as string));
                    for(int i = 1; i < lb.Items.Count; i++)
                    {
                        File.AppendAllText(fileDest, Environment.NewLine + (lb.Items[i] as string));
                    }
                }
                dict.Close();
            });

            Button addBtn = new Button();
            addBtn.Name = "Add";
            addBtn.Content = "Add";
            addBtn.Visibility = Visibility.Visible;

            addBtn.Click += new RoutedEventHandler(delegate (Object o, RoutedEventArgs a)
            {
                ListAfter.Items.Add(lb.SelectedItem as string);
                lb.UnselectAll();
                ButtonMinus.IsEnabled = true;
                ApplySelected();
            });


            Grid.SetRow(lb, 0);
            Grid.SetRow(deleteBtn, 1);
            Grid.SetRow(saveBtn, 1);
            Grid.SetRow(addBtn, 1);
            Grid.SetColumn(deleteBtn, 0);
            Grid.SetColumn(saveBtn, 1);
            Grid.SetColumn(addBtn, 2);
            Grid.SetColumnSpan(lb, 3);

            grid.Children.Add(lb);
            grid.Children.Add(deleteBtn);
            grid.Children.Add(saveBtn);
            grid.Children.Add(addBtn);

            dict.Content = grid;
            dict.Show();
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void rtb_MouseUp(object sender, MouseEventArgs e)
        {
            ContextMenu cm = new ContextMenu();
            MenuItem item = new MenuItem();
            item.Header = "Add to dictionary";
            item.Click += new RoutedEventHandler(AddToDictinary);
            cm.Items.Add(item);
            ((RichTextBox)sender).ContextMenu = cm;

        }

        private void AddToDictinary(object sender, RoutedEventArgs e)
        {
            //TextWriter tw = new StreamWriter("dictionary.txt");
            string workingDirectory = Environment.CurrentDirectory;
            string fileDir = Directory.GetParent(workingDirectory).Parent.Parent.FullName;
            string fileDest = System.IO.Path.Combine(fileDir, "dictionary.txt");
            File.AppendAllText(fileDest, richTextBox.Selection.Text + Environment.NewLine);
           
        }

        private void richTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}