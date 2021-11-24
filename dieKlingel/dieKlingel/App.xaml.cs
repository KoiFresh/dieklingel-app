//#define LINPHONE_LOG // to enable SIP Log in LinphoneManager.cs line 84

using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Newtonsoft.Json;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace dieKlingel
{
    public partial class App : Application
    {
        #region private const
        private const string DOORUNIT_KEY = "doorunit";
        private const string PUSHTOKEN_KEY = "pushtoken";
        #endregion

        #region private variables
        private Doorunit doorunit = null;
        #endregion

        #region public variables
        public static string ConfigFilePath { get; set; }
        public static string FactoryFilePath { get; set; }
        public Page MainPageContent { get; set; }
        public LinphoneManager Manager { get; set; }
        public Linphone.Core Core 
        { 
            get
            {
                return Manager.Core;
            } 
        }
        public Doorunit Doorunit
        {
            get
            {
                if(null == doorunit)
                {
                    string plain = Xamarin.Essentials.Preferences.Get(DOORUNIT_KEY, null);
                    doorunit = (null == plain) ? new Doorunit() : JsonConvert.DeserializeObject<Doorunit>(plain);
                    doorunit.SaveStateChanged += OnSaveStateChanged;
                }
                doorunit.SetCore(Core);
                return doorunit;
            }
            set
            {
                doorunit = value;
                doorunit.SaveStateChanged += OnSaveStateChanged;
                string plain = JsonConvert.SerializeObject(value);
                Xamarin.Essentials.Preferences.Set(DOORUNIT_KEY, plain);
            }
        }

        public static string PushToken
        {
            get
            {
                return Xamarin.Essentials.Preferences.Get(PUSHTOKEN_KEY, "none");
            }
            set
            {
                Xamarin.Essentials.Preferences.Set(PUSHTOKEN_KEY, value);
            }
        }
        #endregion
  
        public App(IntPtr context)
        {
            InitializeComponent();
            Manager = new LinphoneManager();
            Manager.Init(ConfigFilePath, FactoryFilePath, context);
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

        #region private methods
        private void OnSaveStateChanged(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Doorunit State was Saved throug a callback event");
            Doorunit doorunit = (Doorunit)sender;
            string plain = JsonConvert.SerializeObject(doorunit);
            Xamarin.Essentials.Preferences.Set(DOORUNIT_KEY, plain);
        }
        #endregion

        #region override methods
        protected override void OnStart()
        {
            // Handle when your app starts
            Manager.Start();
        }

        protected override void OnSleep()
        {
            Core.TerminateAllCalls();
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
        #endregion

        #region public methods
        public ContentView ViedoFrame()
        {
            return MainPageContent.FindByName<ContentView>("video_frame");
        }
        #endregion
    }
}
