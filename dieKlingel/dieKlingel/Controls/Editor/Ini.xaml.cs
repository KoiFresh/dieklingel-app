using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using IniParser.Parser;
using IniParser.Model;
using System.Collections.ObjectModel;
using System.Security.Cryptography;

namespace dieKlingel.Controls.Editor
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Ini : ContentView, IBase
    {
        private List<NIni.ListGroup> list = new List<NIni.ListGroup>();
        private IniDataParser parser = new IniDataParser();
        private IniData data;

        

        public Ini()
        {
            InitializeComponent();
        }
        public EditorType Type { get { return EditorType.Ini; } }

        public string Text
        {
            get
            {
                return data.ToString();
            }
            set
            {
                GenerateOptions(value);
            }
        }

        public event EventHandler<CustomCommandEventArgs> CustomCommand;
        private void GenerateOptions(string input)
        {   
            data = parser.Parse(input);
            foreach (SectionData section in data.Sections)
            {
                NIni.ListGroup groupItem = new NIni.ListGroup(section.SectionName);
                foreach (KeyData key in section.Keys)
                {
                    NIni.ListViewTemplate viewItem = new NIni.ListViewTemplate();
                    //viewItem.Group = section.SectionName;
                    viewItem.Key = key.KeyName;    
                    viewItem.Value = key.Value;
                    viewItem.Group = section.SectionName;
                    if(key.Comments.Contains("SHA265_2"))
                    {
                        viewItem.DisplayValue = "*";
                        viewItem.Method = NIni.FormatMethod.SHA265_2;
                    }
                    else if(key.Comments.Contains("HIDDEN"))
                    {
                        viewItem.DisplayValue = "*";
                        viewItem.Method = NIni.FormatMethod.Hidden;
                    }
                    else
                    {
                        viewItem.DisplayValue = key.Value;
                        viewItem.Method = NIni.FormatMethod.Plain;
                    }
                    groupItem.Add(viewItem);
                }
                list.Add(groupItem);
            }
            ListSections.ItemsSource = list;
        }

        private void ListViewEntry_TextChanged(object sender, TextChangedEventArgs eventArgs)
        {
            NIni.ListViewTemplate item = (NIni.ListViewTemplate)(sender as Entry).BindingContext;
            Entry entry = (sender as Entry);
            if(entry.Text == "*")
            {
                data[item.Group][item.Key] = item.Value;
            } else
            {
                string val = "";
                switch(item.Method)
                {
                    case NIni.FormatMethod.SHA265_2:
                        val = CreateSHA256(CreateSHA256(entry.Text));
                        break;
                    case NIni.FormatMethod.Hidden:
                    case NIni.FormatMethod.Plain:
                    default:
                        val = entry.Text;
                        break;
                }
                data[item.Group][item.Key] = val;
            }

            
        }

        private string CreateSHA256(string input)
        {
            using (var sha256 = SHA256.Create())
            {
                var s = string.Format("{0}", input);
                byte[] b = Encoding.UTF8.GetBytes(s);
                return(ByteArrayToString(sha256.ComputeHash(b)));
            }
        }

        private static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

    }

    namespace NIni
    {
        public class ListGroup : ObservableCollection<ListViewTemplate>
        {
            public ListGroup(string group)
            {
                Group = group;
            }

            public string Group { get; private set; }
        }

        public class ListViewTemplate
        {
            public string Group { get; set; }
            public string Key { get; set; }
            public string Value { get; set; }
            public string DisplayValue { get; set; }
            public FormatMethod Method { get; set; }
        }

        public enum FormatMethod
        {
            Plain,
            Hidden,
            SHA265_2
        }
    }
}