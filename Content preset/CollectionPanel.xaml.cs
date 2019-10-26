using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Insurgency_theater_editor.Content_preset
{
    /// <summary>
    /// CollectionPanel.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CollectionPanel : UserControl
    {
        public static readonly string REMOVE_TEXT = "<Remove>";
        public static readonly List<string> DEFAULT_HEADER = new List<string>() { "", REMOVE_TEXT };

        public delegate void OnRemoveRequestedHandler(CollectionPanel panel);
        public delegate void OnContentCreatedHandler(KeyValuePanel newPanel);
        public delegate void OnCollectionCreatedHandler(CollectionPanel newPanel);
        public OnRemoveRequestedHandler onRemoveRequested;
        public OnContentCreatedHandler OnContentCreated;
        public OnCollectionCreatedHandler OnCollectionCreated;

        private CollectionView _view;
        public IList<string> Items {
            set {
                if (value == null)
                {
                    _view = new CollectionView(DEFAULT_HEADER);
                    Header.IsEditable = true;
                }
                else
                {
                    _view = new CollectionView(value);
                    Header.IsEditable = false;
                }
                Header.ItemsSource = _view;
            }
        }

        public CollectionPanel()
        {
            InitializeComponent();
            Items = null;
        }
        public CollectionPanel(IList<string> header) : this()
        {
            Items = header;
        }

        private void Button_AddContent(object sender, RoutedEventArgs e)
        {
            var newPanel = new KeyValuePanel();
            Data.Children.Add(newPanel);
            newPanel.onRemoveRequested = (p) =>
            {
                Data.Children.Remove(p);
            };
            if (OnContentCreated != null)
            {
                OnContentCreated.Invoke(newPanel);
            }
        }
        private void Button_AddCollection(object sender, RoutedEventArgs e)
        {
            var newPanel = new CollectionPanel();
            Data.Children.Add(newPanel);
            newPanel.onRemoveRequested = (p) =>
            {
                Data.Children.Remove(p);
            };
            if (OnCollectionCreated != null)
            {
                OnCollectionCreated.Invoke(newPanel);
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selected = Header.SelectedValue as string;
            if (onRemoveRequested != null && !string.IsNullOrWhiteSpace(selected) && selected.CompareTo(REMOVE_TEXT) == 0)
            {
                onRemoveRequested.Invoke(this);
            }
        }
    }
}
