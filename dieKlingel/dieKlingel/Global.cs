using Linphone;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Essentials;

namespace dieKlingel
{
    static class Global
    {
        public static string OwnPushToken
        {
            get
            {
                return Preferences.Get("OwnPushToken", "ERROR_NO_PUSH_TOKEN_STORED");
            }
            set
            {
                Preferences.Set("OwnPushToken", value);
            }
        }


       // public static LinphoneManager Manager { get; set; }

      /*  public static Core Core
        {
            get
            {
                return Manager.Core;
                //return ((App)App.Current).Manager.Core;
            }
        } */
    } 
}
