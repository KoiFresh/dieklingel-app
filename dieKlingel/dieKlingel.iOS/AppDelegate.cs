using System;
using System.Collections.Generic;
using System.Linq;

using Foundation;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

namespace dieKlingel.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            global::Xamarin.Forms.Forms.Init();
            LoadApplication(new App(IntPtr.Zero));

            int width = (int)UIScreen.MainScreen.Bounds.Width;
            int height = (int)UIScreen.MainScreen.Bounds.Height + 400;

            UIView displayUiView = new UIView();
            ((App)App.Current).ViedoFrame().Content = displayUiView.ToView();
            NativeViewWrapper displayWrapper = (NativeViewWrapper)((App)App.Current).ViedoFrame().Content;
            UIView displayView = (UIView)displayWrapper.NativeView;

            Global.Core.NativeVideoWindowId = displayView.Handle;
            Global.Core.VideoDisplayEnabled = true;
            Global.Core.VideoPreviewEnabled = false;

            // Push notification

            if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
            {
                var pushSettings = UIUserNotificationSettings.GetSettingsForTypes(
                                   UIUserNotificationType.Alert | UIUserNotificationType.Badge | UIUserNotificationType.Sound,
                                   new NSSet());
                UIApplication.SharedApplication.RegisterUserNotificationSettings(pushSettings);
                UIApplication.SharedApplication.RegisterForRemoteNotifications();
            }
            else
            {
                UIRemoteNotificationType notificationTypes = UIRemoteNotificationType.Alert | UIRemoteNotificationType.Badge | UIRemoteNotificationType.Sound;
                UIApplication.SharedApplication.RegisterForRemoteNotificationTypes(notificationTypes);
            }

            return base.FinishedLaunching(app, options);
        }

        public override void RegisteredForRemoteNotifications(UIApplication application, NSData deviceToken)
        {
            // Get current device token
            var DeviceToken = deviceToken.Description;
            if (!string.IsNullOrWhiteSpace(DeviceToken))
            {
                DeviceToken = DeviceToken.Trim('<').Trim('>');
            }
            // Get previous device token
            var oldDeviceToken = NSUserDefaults.StandardUserDefaults.StringForKey("PushDeviceToken");
            // Has the token changed?
            if (string.IsNullOrEmpty(oldDeviceToken) || !oldDeviceToken.Equals(DeviceToken))
            {
                //TODO: Put your own logic here to notify your server that the device token has changed/been created!
                byte[] result = new byte[deviceToken.Length];
                System.Runtime.InteropServices.Marshal.Copy(deviceToken.Bytes, result, 0, (int)deviceToken.Length);
                var token = BitConverter.ToString(result).Replace("-", "");
                System.Diagnostics.Debug.WriteLine("Token:" + token);
                //Preferences.Set("Push/Token", token);
                //Settings.Push.Token.Value = token;
                Global.OwnPushToken = token;
            }
            // Save new device token
            NSUserDefaults.StandardUserDefaults.SetString(DeviceToken, "PushDeviceToken");
        }

        public override void FailedToRegisterForRemoteNotifications(UIApplication application, NSError error)
        {
            //new UIAlertView("Error registering push notifications", error.LocalizedDescription, null, "OK", null).Show();
            Acr.UserDialogs.AlertConfig alertConfig = new Acr.UserDialogs.AlertConfig();
            alertConfig.SetTitle("Error registering push notifications");
            alertConfig.SetOkText("OK");
            alertConfig.SetMessage(error.LocalizedDescription);
            Acr.UserDialogs.UserDialogs.Instance.Alert(alertConfig);
        }

        public override void DidReceiveRemoteNotification(UIApplication application, NSDictionary userInfo, Action<UIBackgroundFetchResult> completionHandler)
        {
            // base.DidReceiveRemoteNotification(application, userInfo, completionHandler);
            string action = userInfo["action"].ToString();

            if (application.ApplicationState != UIApplicationState.Active)
            {
                action = "direct" + action;
            }
            MessagingCenter.Send<object, string>(this, "Pushaction", action);
        }

        public override bool HandleOpenURL(UIApplication application, NSUrl url)
        {
            return base.HandleOpenURL(application, url);
        }
    }
}
