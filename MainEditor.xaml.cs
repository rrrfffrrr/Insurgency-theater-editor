using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Documents;
using System.Windows.Controls;
using System.Windows.Input;

namespace Insurgency_theater_editor
{
    using Content_preset;
    /// <summary>
    /// MainEditor.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainEditor : Page
    {
        private static readonly string[] THEATEROPTIONS = { "<All>" };
        private readonly ProjectFolder PFolder;

        public string CurrentFileName { get; set; }
        private string LastContent = null;
        private TheaterStructure token = null;
        private CollectionPanel RootContent {
            get {
                return ContentView.Content as CollectionPanel;
            }
            set {
                ContentView.Content = value;
            }
        }

        public MainEditor()
        {
            InitializeComponent();
        }
        public MainEditor(ProjectFolder folder) : this()
        {
            PFolder = folder;
            LoadTheater();
            TheaterViewer.SelectedIndex = 0;
            LoadFileTree();
        }

        private bool ParseText(string text)
        {
            try
            {
                TheaterStructure tk = new TheaterStructure(text);
                token = tk;
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return false;
            }
        }

        #region script builder
        private string BuildText()
        {
            string newText;
            if (token == null)
            {
                newText = new TextRange(TextViewer.Document.ContentStart, TextViewer.Document.ContentEnd).Text;
            }
            else
            {
                newText = BuildTextCollection(RootContent, 0);
            }
            return newText;
        }
        private string BuildTextCollection(CollectionPanel panel, int level)
        {
            StringBuilder newText = new StringBuilder();
            for(int i = 0; i < level; ++i)
            {
                newText.Append("\t");
            }
            string indent = newText.ToString();
            newText.Clear();

            foreach (var i in panel.Data.Children)
            {
                CollectionPanel cp = i as CollectionPanel;
                KeyValuePanel kvp = i as KeyValuePanel;

                if (kvp != null)
                {
                    newText.Append(indent);
                    newText.Append("\"");
                    newText.Append(kvp.Data.Key);
                    newText.Append("\"\t");
                    newText.Append("\"");
                    newText.Append(kvp.Data.Value);
                    newText.Append("\"\n");
                } 
                else if (cp != null)
                {
                    newText.Append(indent);
                    newText.Append("\"");
                    newText.Append(cp.Header.SelectedValue as string);
                    newText.Append("\"\n");
                    newText.Append(indent);
                    newText.Append("{\n");
                    newText.Append(BuildTextCollection(cp, level + 1));
                    newText.Append(indent);
                    newText.Append("}\n");
                }
            }
            return newText.ToString();
        }
        #endregion

        #region Editor builder
        private void BuildEditor()
        {
            RootContent.Data.Children.Clear(); // clear content before start to build
            if (token == null)
                return;

            Stack<CollectionPanel> collections = new Stack<CollectionPanel>();
            collections.Push(RootContent);
            for (var iter = token.GetEnumerator(); iter.MoveNext();)
            {
                while (iter.Current.Level + 1 < collections.Count)
                    collections.Pop();

                var parent = collections.Peek().Data.Children;
                if (iter.Current.IsContainer)
                {
                    string[] header_list = new string[] { iter.Current.Key, CollectionPanel.REMOVE_TEXT };
                    int selectedIndex = 0;

                    switch (collections.Count) {
                        case 1:
                            header_list = new string[] { "theater", CollectionPanel.REMOVE_TEXT };
                            break;
                        case 2:
                            var list = new List<string>(TheaterStructure.CATEGORIS);
                            list.Add(CollectionPanel.REMOVE_TEXT);
                            header_list = list.ToArray();
                            selectedIndex = list.FindIndex((s) => { return (s.CompareTo(iter.Current.Key) == 0); });
                            break;
                        default:
                        break;
                    }
                    
                    var cp = CreateCollectionPanel(header_list, parent);

                    cp.Header.SelectedIndex = selectedIndex;

                    switch (collections.Count)
                    {
                        case 1:
                            break;
                        case 2:
                            break;
                        default:
                            cp.Header.IsEditable = true;
                            break;
                    }

                    collections.Push(cp);
                }
                else
                {
                    bool restrictKey = false;
                    switch (collections.Count)
                    {
                        case 1:
                            if (iter.Current.Key.CompareTo("#base") == 0)
                                restrictKey = true;
                            break;
                        default:
                            break;
                    }
                    var cp = CreateKeyValuePanel(iter.Current.Key, iter.Current.Value, parent);
                    cp.key.Focusable = !restrictKey;
                }
            }
        }
        /// <summary>
        /// Create new key value panel
        /// </summary>
        /// <param name="key">key of panel</param>
        /// <param name="value">value of panel</param>
        /// <param name="parent">Add to parent and remove handler automatically if it's not null</param>
        /// <param name="restrictKey">If true, disable interaction with key</param>
        /// <returns></returns>
        private KeyValuePanel CreateKeyValuePanel(string key, string value, UIElementCollection parent = null)
        {
            var panel = new KeyValuePanel();
            panel.Data.Key = key;
            panel.Data.Value = value;

            if (parent != null)
            {
                parent.Add(panel);
                panel.onRemoveRequested = (p) =>
                {
                    parent.Remove(p);
                };
            }

            return panel;
        }
        /// <summary>
        /// Create new collection panel
        /// </summary>
        /// <param name="header">Set header string, Editable when null passed, Removable collection must have "<Remove>" in header</Remove>"</param>
        /// <param name="parent">Add to parent and remove handler automatically if it's not null</param>
        /// <returns></returns>
        private CollectionPanel CreateCollectionPanel(IList<string> header = null, UIElementCollection parent = null)
        {
            var panel = new CollectionPanel(header);

            if (parent != null)
            {
                parent.Add(panel);
                panel.onRemoveRequested = (p) =>
                {
                    parent.Remove(p);
                };
            }

            return panel;
        }
        #endregion

        private void LoadTheater()
        {
            TheaterViewer.Items.Clear();
            foreach (string key in THEATEROPTIONS)
            {
                TheaterViewer.Items.Add(new ComboBoxItem() { Content = key });
            }
            foreach (string theater in PFolder.Theaters)
            {
                TheaterViewer.Items.Add(new ComboBoxItem() { Content = theater });
            }
        }
        /// <summary>
        /// Load theater file to text editor
        /// </summary>
        private void LoadCurrentSelectedFile()
        {
            Label value = FolderViewer.SelectedItem as Label;
            if (value == null)
                return;

            string newText = BuildText();
            if (LastContent != null && newText.CompareTo(LastContent) != 0)
            {
                MessageBoxResult result = MessageBox.Show("Save current file?", "Question", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
                switch (result)
                {
                    case MessageBoxResult.Yes:
                        SaveCurrentFile();
                        break;
                    case MessageBoxResult.No:
                        break;
                    case MessageBoxResult.Cancel:
                    default:
                        return; // keep current file loaded
                }
            }

            string name = value.Content.ToString().Substring(1);
            string file = Path.Combine(PFolder.FolderPath, name);
            string text = File.ReadAllText(file).Trim();

            CurrentFileName = name;
            if (ParseText(text))
            {
                var cp = new CollectionPanel(new string[] { name });
                RootContent = cp;

                // override panel data
                cp.ContentCreateButtion.Content = "Add new header";
                cp.ContentCreateButtion.Click += (sender, e) =>
                {
                    var kvp = cp.Data.Children[cp.Data.Children.Count - 1] as KeyValuePanel;
                    if (kvp != null)
                    {
                        cp.Data.Children.Remove(kvp);
                        for (int i = 0; i < cp.Data.Children.Count; ++i)
                        {
                            if ((cp.Data.Children[i] as CollectionPanel) != null)
                            {
                                cp.Data.Children.Insert(i, kvp);
                                break;
                            }
                        }
                        if (!cp.Data.Children.Contains(kvp))
                            cp.Data.Children.Add(kvp);
                        kvp.Data.Key = "#base";
                        kvp.Data.Value = "Make empty if want to remove.";
                        kvp.key.Focusable = false;
                    }
                };
                cp.CollectionCreateButtion.Content = "Add theater if not exist";
                cp.CollectionCreateButtion.Click += (sender, e) =>
                {
                    var kvp = cp.Data.Children[cp.Data.Children.Count - 1] as CollectionPanel;
                    if (kvp != null)
                    {
                        for (int i = 0; i < cp.Data.Children.Count - 1; ++i)
                        {
                            if ((cp.Data.Children[i] as CollectionPanel) != null)
                            {
                                cp.Data.Children.Remove(kvp);
                                return;
                            }
                        }
                        kvp.Header.ItemsSource = new string[] { "theater", CollectionPanel.REMOVE_TEXT};
                        kvp.Header.IsEditable = false;
                    }
                };

                BuildEditor();
                TextViewer.Visibility = Visibility.Hidden;
            }
            else
            {
                // Use plane text editor, so user can edit syntax errors.
                FlowDocument document = new FlowDocument();
                Paragraph p = new Paragraph(new Run(text));
                document.Blocks.Add(p);
                TextViewer.Document = document;
                TextViewer.Visibility = Visibility.Visible;
            }

            LastContent = text;
        }
        /// <summary>
        /// Load file tree from theater list
        /// </summary>
        private void LoadFileTree()
        {
            ComboBoxItem item = TheaterViewer.SelectedItem as ComboBoxItem;
            string value = item.Content.ToString();
            FolderViewer.Items.Clear();
            string[] list;
            if (value.CompareTo(THEATEROPTIONS[0]) == 0)
            {
                list = PFolder.GetFileList();
            }
            else
            {
                list = PFolder.GetFileListFromTheater(value);
            }

            foreach (string v in list)
            {
                FolderViewer.Items.Add(new Label() { Content = "_" + v });
            }
        }
        private void SaveCurrentFile()
        {
            using(StreamWriter writer = new StreamWriter(Path.Combine(PFolder.FolderPath, CurrentFileName)))
            {
                string newText = BuildText();
                writer.Write(newText);
            }
        }


        private void FolderViewer_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            LoadCurrentSelectedFile();
        }

        private void TheaterViewer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadFileTree();
        }

        private void Button_SaveCurrentFile(object sender, RoutedEventArgs e)
        {
            SaveCurrentFile();
        }

        private void Button_CreateFile(object sender, RoutedEventArgs e)
        {
            string theater = Microsoft.VisualBasic.Interaction.InputBox("New theater file name.\nEnter exclude with extension.", "Theater name input box", "default");
            using (var a = File.Create(Path.Combine(PFolder.FolderPath, theater + ".theater")))
            {

            }
            PFolder.ReloadFiles();
            LoadFileTree();
        }
    }
}
