using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Firebase.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms;
using Firebase.Iid;
using Android.Support.V4.App;
using Java.Net;

namespace dieKlingel.Droid
{
    [Service]
    [IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
    class MyFirebaseMessagingService : FirebaseMessagingService
    {
        public MyFirebaseMessagingService()
        {
        }

        public override void OnMessageReceived(RemoteMessage message)
        {
            base.OnMessageReceived(message);
            System.Diagnostics.Debug.WriteLine("Message");
            string action = message.Data["action"].ToString();
            if(Global.IsInForeground)
            {
                MessagingCenter.Send<object, string>(this, "Pushaction", action);
            }else
            {
                string body = message.Data.ContainsKey("body") ? message.Data["body"] : "Looks like something went wrong, please contact kai.mayer@dieklingel.com";
                string title = message.Data.ContainsKey("title") ? message.Data["title"] : "dieklingel.com";
                SendNotification(title, body, message.Data);
            }
            
            //new NotificationHelper().CreateNotification(message.GetNotification().Title, message.GetNotification().Body);
        }

        private void SendNotification(string title, string messageBody, IDictionary<string, string> data)
        {
            var intent = new Intent(this, typeof(MainActivity));
            intent.AddFlags(ActivityFlags.ClearTop);

            foreach (var key in data.Keys)
            {
                intent.PutExtra(key, data[key]);
            }
            var pendingIntent = PendingIntent.GetActivity(this, 100, intent, PendingIntentFlags.OneShot);

            NotificationCompat.Builder notificationBuilder;
            notificationBuilder = new NotificationCompat.Builder(this, "doorunit_notification_channel")

                                  .SetContentTitle(title)
                                  .SetSmallIcon(Resource.Drawable.icon)
                                  //.SetLargeIcon(Android.Graphics.BitmapFactory.DecodeResource(BaseContext.Resources, Resource.Drawable.icon))
                                  .SetContentText(messageBody)
                                  .SetAutoCancel(true)
                                  .SetContentIntent(pendingIntent);

            if(data.ContainsKey("image-path"))
            {
                URLConnection connection = new URL(data["image-path"]).OpenConnection();
                connection.DoInput = true;
                connection.Connect();

                NotificationCompat.BigPictureStyle style = new NotificationCompat.BigPictureStyle()
                    .BigPicture(Android.Graphics.BitmapFactory.DecodeStream(connection.InputStream))
                    .SetSummaryText(messageBody);

                notificationBuilder.SetStyle(style);
            }

            var notificationManager = NotificationManagerCompat.From(this);
            notificationManager.Notify(100, notificationBuilder.Build());
        }
    }
}