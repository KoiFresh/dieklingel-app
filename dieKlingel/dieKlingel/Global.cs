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
        public static string CtUsername
        {
            get
            {
                return Preferences.Get("CtUsername", "");
            }
            set
            {
                Preferences.Set("CtUsername", value);
            }
        }
        public static string Key
        {
            get
            {
                return Preferences.Get("Key", "");
            }
            set
            {
                Preferences.Set("Key", value);
            }
        }
        public static string Registry
        {
            get
            {
                return Preferences.Get("Registry", "");
            }
            set
            {
                Preferences.Set("Registry", value);
            }
        }
        public static string CtDomain
        {
            get
            {
                return Preferences.Get("CtDomain", "https://dieklingel.com");
            }
            set
            {
                Preferences.Set("CtDomain", value);
            }
        }
        public static JObject User
        {
            get
            {
                return JObject.Parse(Preferences.Get("User", "{}"));
            }
            set
            {
                Preferences.Set("User", JsonConvert.SerializeObject(value));
            }
        }

        public static LinphoneManager Manager { get; set; }

        public static Core Core
        {
            get
            {
                return Manager.Core;
                //return ((App)App.Current).Manager.Core;
            }
        }
        public static bool IsInForeground { get; set; }
        public static RegistrationState RegistrationState { get; set; }
    } 
}
