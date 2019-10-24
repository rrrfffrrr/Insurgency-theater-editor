using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Controls;
using System.Windows.Input;

namespace Insurgency_theater_editor
{
    /// <summary>
    /// MainEditor.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainEditor : Page
    {
        private static readonly string[] THEATEROPTIONS = { "<All>" };
        private ProjectFolder PFolder;

        public string CurrentFileName { get; set; }
        private string LastContent = null;
        private TheaterStructure token = null;

        public MainEditor()
        {
            InitializeComponent();
        }
        public MainEditor(ProjectFolder folder) : this()
        {
            this.PFolder = folder;
            LoadTheater();
            TheaterViewer.SelectedIndex = 0;
            LoadFileTree();
        }

        /// DEV: all...
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

            if (value == null) // DEV: add validate text change
            {
                MessageBoxResult result = MessageBox.Show("Save current file?", "Question", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
                switch (result)
                {
                    case MessageBoxResult.Yes:
                        // DEV: save current file
                        break;
                    case MessageBoxResult.No:
                        // DEV: discard current file
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

            }
            else
            {
                // Use plane text editor, so user can edit syntax errors.
                FlowDocument document = new FlowDocument();
                Paragraph p = new Paragraph(new Run(text));
                document.Blocks.Add(p);
                TextViewer.Document = document;
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

        private void FolderViewer_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            LoadCurrentSelectedFile();
        }

        private void TheaterViewer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadFileTree();
        }
    }
}
