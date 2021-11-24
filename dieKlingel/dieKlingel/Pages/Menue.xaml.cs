using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace dieKlingel.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Menue : ContentPage
    {
        public List<MenueListTemplate> MenueItems = new List<MenueListTemplate>();
        public Menue()
        {
            InitializeComponent();
            MenueItems.Add(new MenueListTemplate
            {
                Text = "Account",
                Clicked = () =>
                {
                    Navigation.PushAsync(new Pages.Account());
                    return 0;
                }
            });
            /*MenueItems.Add(new MenueListTemplate
            {
                Text = "Preview",
                Clicked = () =>
                {
                    Navigation.PushAsync(new Pages.Preview("https://dieklingel.com/images/x.jpg"));
                    return 0;
                }
            }); */
            MenueItems.Add(new MenueListTemplate
            {
                Text = "Verbinden",
                Clicked = () =>
                {
                    Navigation.PushAsync(new Pages.Connect());
                    return 0;
                }
            });
            MenueItems.Add(new MenueListTemplate
            {
                Text = "Impressum",
                Clicked = () =>
                {
                    Launcher.OpenAsync("https://app.dieklingel.com/info/impressum/");
                    return 0;
                }
            });
            MenueItems.Add(new MenueListTemplate
            {
                Text = "Datenschutzerklärung",
                Clicked = () =>
                {
                    Launcher.OpenAsync("https://app.dieklingel.com/info/datenschutzerklaerung/");
                    return 0;
                }
            });
            MenueItems.Add(new MenueListTemplate
            {
                Text = "Weitere Informationen",
                Clicked = () =>
                {
                    Launcher.OpenAsync("https://app.dieklingel.com/");
                    return 0;
                }
            });
            MenueItems.Add(new MenueListTemplate
            {
                Text = "About",
                Clicked = () =>
                {
                    Navigation.PushAsync(new Pages.About());
                    return 0;
                }
            });
            ListMenue.ItemsSource = MenueItems;
            BindingContext = this;
        }

        private void ListMenue_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            MenueListTemplate item = (MenueListTemplate)e.Item;
            Debug.WriteLine(item.Text);
            item.Clicked();
        }
    }

    public class MenueListTemplate
    {
        public string Text { get; set; }
        public Func<int> Clicked;
    }
}