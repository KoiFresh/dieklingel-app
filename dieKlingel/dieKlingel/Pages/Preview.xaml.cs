using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace dieKlingel.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Preview : ContentPage
    {
        public Preview(String url)
        {
            InitializeComponent();
            ShowImage(url);
        }

        private void ShowImage(String url)
        {
            image.Source = new UriImageSource
            {
                Uri = new Uri(url),
                CachingEnabled = false,
            };
        }
    }
}