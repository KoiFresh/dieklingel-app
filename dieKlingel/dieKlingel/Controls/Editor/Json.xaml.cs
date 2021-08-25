using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace dieKlingel.Controls.Editor
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Json : ContentView, IBase
    {
        public Json()
        {
            InitializeComponent();
        }
        public EditorType Type { get { return EditorType.Json; } }
        public string Text { get; set; }
        public event EventHandler<CustomCommandEventArgs> CustomCommand;
    }
}