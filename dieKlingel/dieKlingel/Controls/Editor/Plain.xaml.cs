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
    public partial class Plain : ContentView, IBase
    {
        public Plain()
        {
            InitializeComponent();
        }

        /*protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            editor.HeightRequest = (int)height;
        }*/
        public void ResizeRequest(int width, int heigth)
        {
            editor.HeightRequest = heigth;
        }

        public EditorType Type { get { return EditorType.Plain; } }
        public string Text
        {
            get
            {
                return editor.Text;
            }
            set
            {
                editor.Text = value;
            }
        }

        public event EventHandler<CustomCommandEventArgs> CustomCommand;
    }
}