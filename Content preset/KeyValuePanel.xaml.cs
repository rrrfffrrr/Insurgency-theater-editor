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
    /// KeyValuePanel.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class KeyValuePanel : UserControl
    {
        public delegate void OnRemoveRequestedHandler(KeyValuePanel panel);
        public OnRemoveRequestedHandler onRemoveRequested;

        public KeyValue Data;
        public KeyValuePanel()
        {
            InitializeComponent();
            Data = new KeyValue();
            Data.onRemoveRequest = () => { if (onRemoveRequested != null && (Data.Key == "" || !key.Focusable)) { onRemoveRequested.Invoke(this); } };
            DataContext = Data;
        }
    }

    public class KeyValue
    {
        public delegate void OnRemoveRequestHandler();
        public OnRemoveRequestHandler onRemoveRequest;

        private string _key = "Key";
        public string Key {
            get { return _key; }
            set {
                _key = value;
                if (onRemoveRequest != null && string.IsNullOrWhiteSpace(_key))
                {
                    onRemoveRequest.Invoke();
                }
            }
        }
        private string _value;
        public string Value {
            get {
                return _value;
            }
            set {
                _value = value;
                if (onRemoveRequest != null && _key == "#base" && string.IsNullOrWhiteSpace(_value))
                {
                    onRemoveRequest.Invoke();
                }
            }
        }
    }
}
