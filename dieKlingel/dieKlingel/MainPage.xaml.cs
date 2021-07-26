
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Linphone;
using Newtonsoft.Json.Linq;
using Acr.UserDialogs;

namespace dieKlingel
{
    public partial class MainPage : ContentPage
    {
        private const string grey = "#bababa";
        private const string darkgrey = "#969696";
        private const string red = "#f51414";
        private const string green = "#15ad17";
        private const string darkorange = "#e6c13e";

        private Color navBarTextColor;
        private Color navBarBackgroundColor;
 
        public bool MicrophoneIsMuted { get; set; }
        public bool MainControlsHidden { get; set; }
        private bool SpeakerIsEnabled { get; set; }

        private bool one_shot = false;
        public MainPage()
        {
            InitializeComponent();
            BindingContext = this;
            Global.Core.Listener.OnRegistrationStateChanged += OnRegistration;
            Global.Core.Listener.OnCallStateChanged += OnCall;
            Global.Core.SelfViewEnabled = false;
            SetMicMutedState(BtnMute, Global.Core.CurrentCall, true);
            InterpreteCallButtonState(BtnCall, Global.Core.CurrentCall);
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
                MessagingCenter.Subscribe<object, string>(this, "Pushaction", Pushaction);
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if(!one_shot)
            {
                one_shot = true;
                SpeakerIsEnabled = dieKlingel.Audio.Controller.TurnSpeakerOn();
            }
            //TbItemState.Text = Global.RegistrationState.ToString();
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height); //must be called
            video_frame.HeightRequest = (int)height;
            video_frame.WidthRequest = (int)width;
        }

        private async void Pushaction(object obj, string pushaction)
        {
            if (Global.User.Count > 0)
            {
                string number = "sip:" + (string)Global.User["doorunit"] + "@" + (string)Global.User["sip"]["domain"] + ":" + (string)Global.User["sip"]["port"];
                switch (pushaction)
                {
                    case "call":
                        Device.BeginInvokeOnMainThread(async () =>
                        {
                            Vibration.Vibrate();
                            bool answer = await DisplayAlert("DingDong", "Es wurde geklingelt, möchtest du annehemen ?", "Ja", "Nein");

                            if (answer)
                            {
                                await Navigation.PopToRootAsync();
                                InterpreteCallButtonState(BtnCall, Global.Core.CurrentCall, number, true);
                            }
                        });
                        break;
                    case "directcall":
                        Debug.WriteLine("Notify Call started");
                        await Navigation.PopToRootAsync();
                        InterpreteCallButtonState(BtnCall, Global.Core.CurrentCall, number, true);
                        break;
                }
            }
        }

        private void OnRegistration(Core ls, ProxyConfig config, RegistrationState state, string message)
        {
            Debug.WriteLine("Registration state changed: " + state);
            Global.RegistrationState = state;
            TbItemState.Text = state.ToString();
        }

        private void OnCall(Core lc, Call lcall, CallState state, string message)
        {
            Debug.WriteLine("Call state changed: " + state);
            SetMicMutedState(BtnMute, lcall, true);
            InterpreteCallButtonState(BtnCall, lcall);
            switch (lcall.State)
            {
                case CallState.Connected:
                    lcall.CameraEnabled = false;
                    //dieKlingel.Audio.Controller.TurnSpeakerOn();
                    if(SpeakerIsEnabled)
                    {
                        dieKlingel.Audio.Controller.TurnSpeakerOn();
                    }else
                    {
                        dieKlingel.Audio.Controller.TurnSpeakerOff();
                    }
                    DeviceDisplay.KeepScreenOn = true;
#if __IOS__
                    navBarBackgroundColor = ((NavigationPage)Application.Current.MainPage).BarBackgroundColor;
                    ((NavigationPage)Application.Current.MainPage).BarBackgroundColor = Color.Black;
                    navBarTextColor = ((NavigationPage)Application.Current.MainPage).BarTextColor;
                    ((NavigationPage)Application.Current.MainPage).BarTextColor = Color.White;
#endif
                    break;
                case CallState.End:
                    DeviceDisplay.KeepScreenOn = false;
#if __IOS__

                    ((NavigationPage)Application.Current.MainPage).BarBackgroundColor = navBarBackgroundColor;
                    ((NavigationPage)Application.Current.MainPage).BarTextColor = navBarTextColor;
                    UIKit.UIApplication.SharedApplication.StatusBarStyle = UIKit.UIStatusBarStyle.Default;
#endif
                    break;
            }
        }
        private void SetMicMutedState(ImageButton button, Call call, bool micMutedState, bool toogle = false)
        {
            // 2020-12-29, 14:33 by Kai Mayer // get Core.MicEnabled or Core.CurrentCall.MicrophoneMuted not working
            if (call != null && call.State == CallState.StreamsRunning)
            {
                micMutedState = toogle ? !MicrophoneIsMuted : micMutedState;
                button.IsEnabled = true;
                button.BackgroundColor = micMutedState ? Color.FromHex(red) : Color.FromHex(green);  // #f51414 = Rot #15ad17 = Gruen
                button.Source = micMutedState ? ImageSource.FromFile("micmuted.png") : ImageSource.FromFile("mic.png");
                button.Aspect = Aspect.AspectFill;
                Global.Core.MicEnabled = !micMutedState;
                MicrophoneIsMuted = micMutedState;
            }
            else
            {
                Global.Core.MicEnabled = false;
                MicrophoneIsMuted = true;
                button.IsEnabled = false;
                button.BackgroundColor = Color.FromHex(grey);
                button.Source = ImageSource.FromFile("micmuted.png");
            }
        }

        private void InterpreteCallButtonState(ImageButton button, Call call, string invite = null, bool hangoff = false)
        {
            Color color;
            CallParams callParams = Global.Core.CreateCallParams(null);
            callParams.VideoEnabled = true;

            if (call != null)
            {
                //call.CameraEnabled = false;
                color = Color.FromHex(red);
                //color = ((call.State == CallState.Released) || (call.State == CallState.End)) ? Color.FromHex(green) : Color.FromHex(red);
                if (call.State == CallState.Released || call.State == CallState.End)
                {
                    color = Color.FromHex(green);
                }
                if (hangoff)
                {
                    Global.Core.TerminateAllCalls();
                    color = Color.FromHex(green);
                }
                if (call.State == CallState.IncomingReceived) call.AcceptWithParams(callParams); //  Core.AcceptCallWithParams(call, CallParams);
            }
            else
            {
                color = Color.FromHex(green);
                if (invite != null) Global.Core.InviteWithParams(invite, callParams); // 2020-12-29, 16:30 by Kai Mayer // sip:Sven@mayer-schoch.ddns.net:5061"
            }
            button.BackgroundColor = color;
        }
        private async void BtnSettings_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new Pages.Menue());
            //await Navigation.PushAsync(new Pages.Settings.Menue());
            //await Navigation.PushAsync(new Pages.Settings());
        }

        private void BtnCall_Clicked(object sender, EventArgs e)
        {
            if(Global.User.Count > 0)
            {
                string number = "sip:" + (string)Global.User["doorunit"] + "@" + (string)Global.User["sip"]["domain"] + ":" + (string)Global.User["sip"]["port"];
                InterpreteCallButtonState(BtnCall, Global.Core.CurrentCall, number, true);
            }
            //string number = "sip:" + dieklingel.Settings.Sip.DoorunitUsername.Value + "@" + dieklingel.Settings.Sip.Domain.Value + ":" + dieklingel.Settings.Sip.Port.Value;
        }

        private void BtnMute_Clicked(object sender, EventArgs e)
        {
            SetMicMutedState((ImageButton)sender, Global.Core.CurrentCall, false, true);
        }

        private async void BtnDoorUnlock_Clicked(object sender, EventArgs e)
        {
            if(Global.User.Count > 0)
            {
                JObject res = Socket.Send(Socket.Context.Unlock, new JObject());
                if ((int)res["status_code"] == 200)
                {
                    // Unlock animation
                    ViewExtensions.CancelAnimations(lock_top);
                    ViewExtensions.CancelAnimations(lock_);

                    lock_.IsVisible = true;
                    await lock_.FadeTo(1, 300);

                    await lock_top.TranslateTo(0, -30, 200);
              
                    await lock_.FadeTo(0, 300);
                    lock_top.TranslateTo(0, 0, 0);
                    lock_.IsVisible = false;
                }
            }
            else
            {
                await Navigation.PushAsync(new Pages.Account());
            }
            
        }

        private void BtnHideControls_Clicked(object sender, EventArgs e)
        {
            HideMainControls(!MainControlsHidden);
        }

        private async void HideMainControls(bool hide=true)
        {
            ViewExtensions.CancelAnimations(FlMainControls);
            ViewExtensions.CancelAnimations(FrBtnSettings);
            MainControlsHidden = hide;
            if (hide)
            {
                FrBtnSettings.TranslateTo(100, 0, 200);
                await FlMainControls.TranslateTo(0, 200, 200);
            }else
            {
                FrBtnSettings.TranslateTo(0, 0, 200);
                await FlMainControls.TranslateTo(0, 0, 200);
            }
            
        }

        private void BtnSpeaker_Clicked(object sender, EventArgs e)
        {
            ToogleSpeaker();
        }

        private void ToogleSpeaker()
        {
            //SpeakerIsEnabled = !SpeakerIsEnabled;
            if(SpeakerIsEnabled)
            {
                SpeakerIsEnabled = !dieKlingel.Audio.Controller.TurnSpeakerOff();
            }else
            {
                SpeakerIsEnabled = dieKlingel.Audio.Controller.TurnSpeakerOn();
            }
            BtnSpeaker.Source = SpeakerIsEnabled ? ImageSource.FromFile("speaker.png") : ImageSource.FromFile("speakersilent.png");
        }
    }
}
