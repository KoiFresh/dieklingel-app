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

namespace dieKlingel.Droid
{
    [Activity(Label = "dieKlingel", Icon = "@drawable/icon", Theme = "@style/MainTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : FormsAppCompatActivity //global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        int PERMISSIONS_REQUEST = 101;
        TextureView displayCamera;
        internal static readonly string CHANNEL_ID = "doorunit_notification_channel";
        internal static readonly int NOTIFICATION_ID = 100;
        SampleReceiver receiver;
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

            receiver = new SampleReceiver();
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

            App.ConfigFilePath = rc_path;
            App.FactoryFilePath = factory_path;

            App app = new App(this.Handle);

            System.Diagnostics.Debug.WriteLine("DEVICE=" + Build.Device);
            System.Diagnostics.Debug.WriteLine("MODEL=" + Build.Model);
            System.Diagnostics.Debug.WriteLine("MANUFACTURER=" + Build.Manufacturer);
            System.Diagnostics.Debug.WriteLine("SDK=" + Build.VERSION.Sdk);


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
            Global.Core.NativeVideoWindowId = displayCamera.Handle;
//            Global.Core.VideoDisplayEnabled = true;
//            Global.Core.VideoPreviewEnabled = false;

            // Set audio to speaker
            AudioManager am = (AudioManager)Android.App.Application.Context.GetSystemService(Android.Content.Context.AudioService);
            am.Mode = Mode.InCall;
            am.SpeakerphoneOn = true;
            //am.SetStreamVolume(Android.Media.Stream.VoiceCall, am.GetStreamMaxVolume(Android.Media.Stream.VoiceCall), VolumeNotificationFlags.ShowUi);

            if (Intent.Extras != null)
            {
                foreach (var key in Intent.Extras.KeySet())
                {
                    var value = Intent.Extras.GetString(key);
                    if (key == "action")
                    {
                        if (value?.Length > 0)
                        {
                            string action = "direct" + value;

                            MessagingCenter.Send<object, string>(this, "Pushaction", action);
                            break;
                        }
                    }
                }
            } 
            LoadApplication(app);
        }

        protected override void OnResume()
        {
            base.OnResume();

            if (Int32.Parse(Build.VERSION.Sdk) >= 23)
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
            if (intent.Extras != null)
            {
                foreach (var key in intent.Extras.KeySet())
                {
                    var value = intent.Extras.GetString(key);
                    if (key == "action")
                    {
                        if (value?.Length > 0)
                        {
                            string action = "direct" + value;

                            MessagingCenter.Send<object, string>(this, "Pushaction", action);
                            break;
                        }
                    }
                }
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            if (requestCode == PERMISSIONS_REQUEST)
            {
                int i = 0;
                foreach (string permission in permissions)
                {
                    Log.Info("LinphoneXamarin", "Permission " + permission + " : " + grantResults[i]);
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

            var alarmAttributes = new AudioAttributes.Builder()
                    .SetContentType(AudioContentType.Sonification)
                    .SetUsage(AudioUsageKind.Notification).Build();

            var alarmUri = Android.Net.Uri.Parse("android.resource://" + Android.App.Application.Context.PackageName + "/raw/long_sound");

            var channel = new NotificationChannel(CHANNEL_ID, "Doorunit Notifications", NotificationImportance.Default)
            {
                Description = "Firebase Cloud Messages appear in this channel",
            };

            channel.SetSound(alarmUri, alarmAttributes);

            var notificationManager = (NotificationManager)GetSystemService(Android.Content.Context.NotificationService);
            notificationManager.CreateNotificationChannel(channel);
        }
    }
}