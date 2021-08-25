using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Firebase.Messaging;
using Firebase.Iid;

namespace dieKlingel.Droid
{
    [Service]
    [IntentFilter(new[] { "com.google.firebase.INSTANCE_ID_EVENT" })]
    class MyFirebaseIIDService : FirebaseInstanceIdService
    {
        const string TAG = "MyFirebaseIIDService";
        public override void OnTokenRefresh()
        {
            base.OnTokenRefresh();
            System.Diagnostics.Debug.WriteLine("Token:" + FirebaseInstanceId.Instance.Token);
            //Preferences.Set("Push/Token", FirebaseInstanceId.Instance.Token);
            //Settings.Push.Token.Value = FirebaseInstanceId.Instance.Token;
            Global.OwnPushToken = FirebaseInstanceId.Instance.Token;
        }
    } 


    [BroadcastReceiver(Enabled = false, Exported = false)]
    public class SampleReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            // Do stuff here.

            String value = intent.GetStringExtra("key");
        }
    } 
}