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
            //string action = message.Data["action"].ToString();
         
            //string body = message.Data.ContainsKey("body") ? message.Data["body"] : "Looks like something went wrong, please contact kai.mayer@dieklingel.com";
            //string title = message.Data.ContainsKey("title") ? message.Data["title"] : "dieklingel.com";
            if(message.GetNotification() != null)
            {
                SendNotification(message.GetNotification().Title, message.GetNotification().Body, message.Data);
            }else
            {
                string title = message.Data?["Title"] ?? "Title missing";
                string body = message.Data?["Body"] ?? "Body missing";
                SendNotification(title, body, message.Data);
            }
            

            /*if (Global.IsInForeground)
            {
                //MessagingCenter.Send<object, string>(this, "Pushaction", action);
            }else
            {
                string body = message.Data.ContainsKey("body") ? message.Data["body"] : "Looks like something went wrong, please contact kai.mayer@dieklingel.com";
                string title = message.Data.ContainsKey("title") ? message.Data["title"] : "dieklingel.com";
                SendNotification(title, body, message.Data);
            } */
            
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
            //notificationBuilder = new NotificationCompat.Builder(this, "doorunit_notification_channel")
            notificationBuilder = new NotificationCompat.Builder(this, "fcm_fallback_notification_channel")
                                  .SetContentTitle(title)
                                  //.SetSmallIcon(Resource.Drawable.icon)
                                  //.SetLargeIcon(Android.Graphics.BitmapFactory.DecodeResource(BaseContext.Resources, Resource.Drawable.icon))
                                  .SetColor(0xffa100)
                                  .SetSmallIcon(Resource.Drawable.icon_16x16)
                                  .SetContentText(messageBody)
                                  .SetAutoCancel(true)
                                  .SetContentIntent(pendingIntent);

            // if(data.ContainsKey("image-path")) // old way payload
            if(data.ContainsKey("ImageUrl"))
            {
                try
                {
                    URLConnection connection = new URL(data["ImageUrl"]).OpenConnection();
                    connection.DoInput = true;
                    connection.Connect();
                    NotificationCompat.BigPictureStyle style = new NotificationCompat.BigPictureStyle()
                        .BigPicture(Android.Graphics.BitmapFactory.DecodeStream(connection.InputStream))
                        .SetSummaryText(messageBody);
                    notificationBuilder.SetStyle(style);
                }catch(Exception e)
                {
                    System.Diagnostics.Debug.WriteLine("An Error occured while downloading a notification Image", e.Message);
                }
            } 
            var notificationManager = NotificationManagerCompat.From(this);
            notificationManager.Notify(new Random().Next(), notificationBuilder.Build());
        }
    }
}