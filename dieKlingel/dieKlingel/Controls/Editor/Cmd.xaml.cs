using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using Xamarin.Essentials;
using Acr.UserDialogs;

using IniParser.Parser;
using IniParser.Model;

namespace dieKlingel.Controls.Editor
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Cmd : ContentView, IBase
    {
        private List<NCmd.Cmd> list = new List<NCmd.Cmd>();
        private IniDataParser parser = new IniDataParser();
        private IniData data;
        private string rawText;
        public Cmd()
        {
            InitializeComponent();
        }

        public EditorType Type { get { return EditorType.Cmd;  } }

        public string Text 
        { 
            get 
            {
                return rawText;
            } 
            set
            {
                rawText = value;
                GenerateOptions(value);
            }
        }

        public event EventHandler<CustomCommandEventArgs> CustomCommand;
        private void GenerateOptions(string input)
        {
            data = parser.Parse(input);
            foreach (SectionData section in data.Sections)
            {
                foreach (KeyData key in section.Keys)
                {
                    NCmd.Cmd viewItem = new NCmd.Cmd();
                    viewItem.Text = key.KeyName;
                    viewItem.Command = key.Value;
                    viewItem.ConfirmationRequierd = key.Comments.Contains("CONFIRM");
                    list.Add(viewItem);
                }
            }
            ListSections.ItemsSource = list;
        }

        private async void ListSections_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            NCmd.Cmd cmd = (NCmd.Cmd)e.Item;
            bool confirmed = false;
            if (cmd.ConfirmationRequierd)
            {
                confirmed = await Acr.UserDialogs.UserDialogs.Instance.ConfirmAsync(cmd.Text, "Ausführen ?", "Ja", "Nein");
            }
            if((cmd.ConfirmationRequierd && confirmed) || !cmd.ConfirmationRequierd)
            {
                CustomCommandEventArgs args = new CustomCommandEventArgs();
                args.Command = cmd.Command;
                CustomCommand?.Invoke(this, args);
            }
        }
    }

    namespace NCmd
    {
        public class Cmd
        {
            public string Text { get; set; }
            public string Command { get; set; }
            public bool ConfirmationRequierd { get; set; }
        }
    }
}