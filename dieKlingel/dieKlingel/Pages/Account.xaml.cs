using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;
using Acr.UserDialogs;

namespace dieKlingel.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Account : ContentPage
    {
        public Account()
        {
            InitializeComponent();
            EntryCtUsername.Text = Global.CtUsername;
            EntryCtPassword.Text = Global.Key;
            EntryCtDomain.Text = Global.CtDomain;
            if (Global.User.Count > 0)
            {
                BtnAccountInfo.IsVisible = true;
            }
        }

        private async void BtnSave_Clicked(object sender, EventArgs e)
        {
            if (EntryCtDomain.Text == String.Empty || EntryCtPassword.Text == String.Empty || EntryCtUsername.Text == String.Empty)
            {
                await DisplayAlert("Error", "Fülle alle Felder aus!", "Ok");
                return;
            }
            // einträge abspeichern
            Global.CtDomain = EntryCtDomain.Text;
            Global.Key = EntryCtPassword.Text;
            Global.CtUsername = EntryCtUsername.Text;
            string iv = Cryptonia.Normalize(Global.Key).Substring(0, 16);
            Global.Registry = Cryptonia.Encrypt(Global.CtUsername, Cryptonia.Normalize(Global.Key), iv);
            // senden
            JObject res = Socket.Send(Socket.Context.Register, new JObject());
            Debug.WriteLine(JsonConvert.SerializeObject(res));
            if((int)res["status_code"] == 404)
            {
                await DisplayAlert("Error", "Sieht so aus, als sei die gesuchte Klingel derzeit nicht Online", "Ok");
            }
            // result verarbeiten
            //string[] accounts = new string[];
            List<string> display_names = new List<string>();
            try
            {
                foreach (JObject acc in (JArray)res["body"]["data"])
                {
                    display_names.Add((string)acc["displayname"]);
                }
                string display_name = await DisplayActionSheet("Account Auswählen", "Abbrechen", null, display_names.ToArray());
                if (display_name != "Abbrechen" && display_name != null)
                {
                    JObject account = new JObject();
                    foreach (JObject acc in res["body"]["data"])
                    {
                        if ((string)acc["displayname"] == display_name)
                        {
                            account = acc;
                        }
                    }
                    string message = "Passwort";
                    bool repeat = true;
                    do
                    {
                        PromptResult result = await UserDialogs.Instance.PromptAsync(new PromptConfig {
                            InputType = InputType.Password,
                            OkText = "Ok",
                            CancelText = "Abbrechen",
                            Title = display_name,
                            Message = message
                        });


                        /*password = await DisplayPromptAsync(display_name, message, "Ok", "Abbrechen");
                        if (password == (string)account["password"])
                        {*/
                        repeat = result.Ok;
                        if (result.Ok && result.Text == (string)account["password"])
                        { 
                            Global.User = account;
                            repeat = false;
                            Socket.SipUpdate((string)Global.User["username"], (string)Global.User["password"], (string)Global.User["sip"]["domain"], (int)Global.User["sip"]["port"]);
                        }
                        message = "Leider Falsch! Bitte versuche es erneut";
                    } while (repeat);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            if (Global.User.Count > 0)
            {
                Socket.SipUpdate((string)Global.User["username"], (string)Global.User["password"], (string)Global.User["sip"]["domain"], (int)Global.User["sip"]["port"]);
                JObject obj = JObject.Parse("{\"body\":{\"data\":{}}}");
                obj["body"]["data"]["devicename"] = Plugin.DeviceInfo.CrossDeviceInfo.Current.Id;
                obj["body"]["data"]["os"] = DeviceInfo.Platform.ToString();
                obj["body"]["data"]["username"] = Global.User["username"];
                obj["body"]["data"]["token"] = Global.OwnPushToken;
#if __IOS__
                obj["body"]["data"]["server"] = "apple.com";
#endif
#if __ANDROID__
                obj["body"]["data"]["server"] = "googleapis.com";
#endif
                obj["body"]["data"]["sound"] = "normal_sound";
                Socket.Send(Socket.Context.DeviceUpdate, obj);
                await Navigation.PopToRootAsync();
            }
        }

        private async void BtnAccountInfo_Clicked(object sender, EventArgs e)
        {
            bool logout = !await DisplayAlert("Account", "Du bist als " + Global.User["displayname"] + " eingeloggt!", "Ok", "Ausloggen");
            if (logout)
            {
                if (Global.User.Count > 0)
                {
                    JObject obj = JObject.Parse("{\"body\":{\"data\":{}}}");
                    obj["body"]["data"]["devicename"] = Plugin.DeviceInfo.CrossDeviceInfo.Current.Id;
                    obj["body"]["data"]["os"] = DeviceInfo.Platform.ToString();
                    obj["body"]["data"]["username"] = "_unregisterd";
                    obj["body"]["data"]["token"] = Global.OwnPushToken;
#if __IOS__
                    obj["body"]["data"]["server"] = "apple.com";
#endif
#if __ANDROID__
                obj["body"]["data"]["server"] = "googleapis.com";
#endif
                    obj["body"]["data"]["sound"] = "normal_sound";
                    Socket.Send(Socket.Context.DeviceUpdate, obj);
                }

                Global.User = new JObject();
                BtnAccountInfo.IsVisible = false;
                Socket.SipClear();
            }
        }
    }
}