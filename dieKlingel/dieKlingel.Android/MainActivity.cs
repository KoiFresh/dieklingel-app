using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using System.IO;
using Android.Content.Res;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Android.Media;
using System.Collections.Generic;
using Android;
using Android.Content;
using Plugin.CurrentActivity;
using Android.Util;
using Acr.UserDialogs;
using Newtonsoft.Json;
using System.Linq;

namespace dieKlingel.Droid
{
    [Activity(Label = "dieKlingel", Icon = "@drawable/icon", Theme = "@style/MainTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : FormsAppCompatActivity //global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        int PERMISSIONS_REQUEST = 101;
        TextureView displayCamera;
        internal static readonly string CHANNEL_ID = "doorunit_notification_channel";
        internal static readonly int NOTIFICATION_ID = 100;
        BReceiver receiver;

        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);
            // Set Flag to use the Expander Item in Editor.Json();
            Forms.SetFlags(new string[] { "Expander_Experimental" });
            //global::Xamarin.Forms.Forms.Init(this, bundle);
            global::Xamarin.Essentials.Platform.Init(this, bundle);

            UserDialogs.Init(this);

            receiver = new BReceiver();

            CreateNotificationChannel();

            CrossCurrentActivity.Current.Init(this, bundle);

            AssetManager assets = Assets;
            string path = FilesDir.AbsolutePath;
            string rc_path = path + "/default_rc";
            if (!File.Exists(rc_path))
            {
                using (StreamReader sr = new StreamReader(assets.Open("linphonerc_default")))
                {
                    string content = sr.ReadToEnd();
                    File.WriteAllText(rc_path, content);
                }
            }
            string factory_path = path + "/factory_rc";

            if (!File.Exists(factory_path))
            {
                using (StreamReader sr = new StreamReader(assets.Open("linphonerc_factory")))
                {
                    string content = sr.ReadToEnd();
                    File.WriteAllText(factory_path, content);
                }
            }

            Forms.Init(this, bundle);

            Firebase.FirebaseApp.InitializeApp(this);

            App.ConfigFilePath = rc_path;
            App.FactoryFilePath = factory_path;

            App app = new App(this.Handle);

            System.Diagnostics.Debug.WriteLine("DEVICE=" + Build.Device);
            System.Diagnostics.Debug.WriteLine("MODEL=" + Build.Model);
            System.Diagnostics.Debug.WriteLine("MANUFACTURER=" + Build.Manufacturer);
            System.Diagnostics.Debug.WriteLine("SDK=" + Build.VERSION.Sdk);

            //LibVLCSharp.Forms.Shared.LibVLCSharpFormsRenderer.Init();
            //CrossMediaManager.Current.Init();
            // Camera View

            // 2020-12-28, 20:24 by Kai Mayer // 

            LinearLayout fl = new LinearLayout(this);
            ViewGroup.LayoutParams lparams = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            fl.LayoutParameters = lparams;

            displayCamera = new TextureView(this);
            ViewGroup.LayoutParams dparams = new ViewGroup.LayoutParams((int)app.MainPageContent.Width, (int)app.MainPageContent.Height);
            displayCamera.LayoutParameters = dparams;

            /*LinearLayout fl2 = new LinearLayout(this);
            ViewGroup.LayoutParams lparams2 = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            fl2.LayoutParameters = lparams2;

            captureCamera = new TextureView(this);
            ViewGroup.LayoutParams cparams = new ViewGroup.LayoutParams(200, 300);
            captureCamera.LayoutParameters = cparams; */


            fl.AddView(displayCamera);
            //fl2.AddView(captureCamera);
            app.ViedoFrame().Content = fl.ToView();
            //app.SelfFrame().Content = fl2.ToView();

            //app.Core.NativePreviewWindowId = captureCamera.Handle;
            app.Core.NativeVideoWindowId = displayCamera.Handle;
            //            Global.Core.VideoDisplayEnabled = true;
            //            Global.Core.VideoPreviewEnabled = false;

            // Set audio to speaker
            AudioManager am = (AudioManager)Android.App.Application.Context.GetSystemService(Android.Content.Context.AudioService);
            am.Mode = Mode.InCall;
            am.SpeakerphoneOn = true;
            //am.SetStreamVolume(Android.Media.Stream.VoiceCall, am.GetStreamMaxVolume(Android.Media.Stream.VoiceCall), VolumeNotificationFlags.ShowUi);

            LoadApplication(app);
            if(null != Intent.Extras)
            {
                MessagingCenter.Send<object, IntentContent>(this, "intent", HandleIntent(Intent));
            }
        }

        protected override void OnResume()
        {
            base.OnResume();

            //if (Int32.Parse(Build.VERSION.Sdk) >= 23)
            if(Build.VERSION.SdkInt >= BuildVersionCodes.M)
            {
                List<string> Permissions = new List<string>();
                if (CheckSelfPermission(Manifest.Permission.Camera) != Permission.Granted)
                {
                    Permissions.Add(Manifest.Permission.Camera);
                }
                if (CheckSelfPermission(Manifest.Permission.RecordAudio) != Permission.Granted)
                {
                    Permissions.Add(Manifest.Permission.RecordAudio);
                }
                if (Permissions.Count > 0)
                {
                    RequestPermissions(Permissions.ToArray(), PERMISSIONS_REQUEST);
                }
            }
        }

        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);
            MessagingCenter.Send<object, IntentContent>(this, "intent", HandleIntent(intent));
        }

        private IntentContent HandleIntent(Intent intent)
        {
            IntentContent content = new IntentContent();
            if (null != intent.Extras)
            {
                Bundle bundle = intent.Extras;
                Dictionary<string, string> dict = bundle.KeySet().ToDictionary<string, string, string>(key => key, key => bundle.Get(key).ToString());
                string json = JsonConvert.SerializeObject(dict);
                content = JsonConvert.DeserializeObject<IntentContent>(json);
            }
            return content;
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            if (requestCode == PERMISSIONS_REQUEST)
            {
                int i = 0;
                foreach (string permission in permissions)
                {
                    Log.Info("dieKlingel", "Permission " + permission + " : " + grantResults[i]);
                    i += 1;
                }
            }
        }

        void CreateNotificationChannel()
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            {
                // Notification channels are new in API 26 (and not a part of the
                // support library). There is no need to create a notification
                // channel on older versions of Android.

                return;
            }


            NotificationChannel chan = new NotificationChannel("fcm_fallback_notification_channel", "Miscellaneous", NotificationImportance.High);
            chan.EnableVibration(true);
            chan.LockscreenVisibility = NotificationVisibility.Public;
            chan.SetSound(Android.Net.Uri.Parse("android.resource://com.dieklingel.app/raw/long_sound"), null);

            NotificationManager notificationManager = (NotificationManager)GetSystemService(NotificationService);
            notificationManager.CreateNotificationChannel(chan);

            

            // should working wit notification channel
            /*var alarmAttributes = new AudioAttributes.Builder()
                    .SetContentType(AudioContentType.Sonification)
                    .SetUsage(AudioUsageKind.Notification).Build();

            var alarmUri = Android.Net.Uri.Parse("android.resource://" + Android.App.Application.Context.PackageName + "/raw/long_sound");

            var channel = new NotificationChannel(CHANNEL_ID, "Doorunit Notifications", NotificationImportance.Default)
            {
                Description = "Doorbell Notifications appear in this channel",
            };

            channel.SetSound(alarmUri, alarmAttributes);

            var notificationManager = (NotificationManager)GetSystemService(Android.Content.Context.NotificationService);
            notificationManager.CreateNotificationChannel(channel); */


        }
    }
}