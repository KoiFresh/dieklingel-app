//#define LINPHONE_LOG // to enable SIP Log in LinphoneManager.cs line 84

using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace dieKlingel
{
    public partial class App : Application
    {
        public static string ConfigFilePath { get; set; }
        public static string FactoryFilePath { get; set; }
        public Page MainPageContent { get; set; }
        public App(IntPtr context)
        {
            InitializeComponent();
            Global.Manager = new LinphoneManager();
            Global.Manager.Init(ConfigFilePath, FactoryFilePath, context);
            MainPageContent = new MainPage();
            MainPage = new NavigationPage(MainPageContent);
            Application.Current.RequestedThemeChanged += (s, a) =>
            {
                if(Application.Current.RequestedTheme == OSAppTheme.Dark)
                {
#if __IOS__
                    //ViedoFrame().BackgroundColor = Color.Black;
                    //Application.Current.UserAppTheme = OSAppTheme.Light;
                    UIKit.UIApplication.SharedApplication.StatusBarStyle = UIKit.UIStatusBarStyle.BlackTranslucent;
#endif
                }else
                    if(Application.Current.RequestedTheme == OSAppTheme.Light)
                {
#if __IOS__
                    //ViedoFrame().BackgroundColor = Color.Black;
                    //Application.Current.UserAppTheme = OSAppTheme.Light;
                    UIKit.UIApplication.SharedApplication.StatusBarStyle = UIKit.UIStatusBarStyle.Default;
#endif
                }
                // Respond to the theme change
            };
        }

        protected override void OnStart()
        {
            Global.IsInForeground = true;
            // Handle when your app starts
            Global.Manager.Start();
        }

        protected override void OnSleep()
        {
            Global.IsInForeground = false;
            Global.Core.TerminateAllCalls();
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            Global.IsInForeground = true;
            // Handle when your app resumes
        }

        public ContentView ViedoFrame()
        {
            return MainPageContent.FindByName<ContentView>("video_frame");
        }
    }
}
