using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Collections;
using System.Globalization;
using System.Resources;
using System.Windows.Forms;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace Insurgency_theater_editor
{
    /// <summary>
    /// StartPage.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class StartPage : Page
    {
        public StartPage()
        {
            InitializeComponent();
        }

        private void Button_OpenProjectFolder(object sender, System.Windows.RoutedEventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    var pf = new ProjectFolder(fbd.SelectedPath);
                    var page = new MainEditor(pf);

                    this.NavigationService.Navigate(page);

                    var window = System.Windows.Window.GetWindow(this);
                    window.Title = "Insurgency theater editor : " + pf.FolderPath;
                }
            }
        }

        private void Button_CreateNewProject(object sender, System.Windows.RoutedEventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    /// DEV: Make dialog to get new theater name
                    string theater = Microsoft.VisualBasic.Interaction.InputBox("New theater name", "Theater name input box", "default");
                    if (string.IsNullOrWhiteSpace(theater) || theater.Contains(' ') || theater.Contains('\t'))
                    {
                        MessageBox.Show("Cannot use theater name with whitespace character", "Input error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    ResourceManager manager = new ResourceManager(typeof(Properties.Resources));
                    ResourceSet set = manager.GetResourceSet(CultureInfo.CurrentUICulture, true, true);
                    foreach(DictionaryEntry entry in set)
                    {
                        string path;
                        if (entry.Key.ToString().CompareTo("theater") == 0)
                        {
                            path = theater + ".theater";
                        }
                        else
                        {
                            path = theater + "_" + entry.Key.ToString() + ".theater";
                        }
                        path = Path.Combine(fbd.SelectedPath, path);
                        string content = Encoding.UTF8.GetString(entry.Value as byte[]).Replace("{theater}", theater);
                        File.WriteAllText(path, content);
                    }

                    var pf = new ProjectFolder(fbd.SelectedPath);
                    var page = new MainEditor(pf);

                    this.NavigationService.Navigate(page);

                    var window = System.Windows.Window.GetWindow(this);
                    window.Title = "Insurgency theater editor : " + pf.FolderPath;
                }
            }
        }
    }
}
