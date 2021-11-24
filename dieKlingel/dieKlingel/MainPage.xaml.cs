
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
using dieKlingel;

/**
 * Main Page of the app
 * @author Kai Mayer
 */

namespace dieKlingel
{
    public partial class MainPage : ContentPage
    {
        #region private const
        private const string COLOR_GREY_HEX = "#bababa";
        private const string darkgrey = "#969696";
        private const string COLOR_RED_HEX = "#f51414";
        private const string COLOR_GREEN_HEX = "#15ad17";
        private const string COLOR_BLUE_HEX = "#0d9fbf"; // used in xaml
        private const string COLOR_YELLOW_HEX = "#e6c13e"; // used in xaml
        #endregion

        #region private variables
        private readonly App app = (App)App.Current;
        private Color navBarTextColor;
        private Color navBarBackgroundColor;
        private bool mainControlsVisible = true;
        private bool singleShotOnAppearing = false;
        private bool micEnabledSaveState = false;
        private bool micEnabled
        {
            get
            {
                return micEnabledSaveState;
            }
            set
            {
                micEnabledSaveState = value;
                app.Core.MicEnabled = micEnabledSaveState;
            }
        }
        #endregion

        #region public variables 
        #endregion

        public MainPage()
        {
            InitializeComponent();
            BindingContext = this;
            app.Core.Listener.OnRegistrationStateChanged += OnRegistrationStateChanged;
            app.Core.Listener.OnCallStateChanged += OnCallStateChanged;
            app.Core.SelfViewEnabled = false;
            MessagingCenter.Subscribe<object, IntentContent>(this, "intent", OnIntentContentAvailable);
            System.Diagnostics.Debug.WriteLine("PushToken: " + App.PushToken);
        }

        #region override methods
        /**
         * called everytime when the page goes into foreground
         */
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if(!singleShotOnAppearing)
            {
                singleShotOnAppearing = true;
                dieKlingel.Controller.Audio.TurnSpeakerOn();

                if((app.Doorunit?.IsRegisterd ?? false))
                {
                    Dup.Data payload = (Dup.Data)app.Doorunit.GetDeviceUpdateDataPayload(App.PushToken);
                    Dup.Notification notification = Dup.Notification.Build(Dup.Context.DeviceUpdate, payload);
                    await app.Doorunit.SendAsync(notification);
                }
                BtnMute.IsEnabled = false;
                BtnMute.BackgroundColor = Color.FromHex(COLOR_GREY_HEX);
                BtnCall.BackgroundColor = Color.FromHex(COLOR_GREEN_HEX);
                // check if not phone
                if (Device.Idiom != TargetIdiom.Phone)
                {
                    FlMainControls.Children.Remove((Frame)BtnSpeaker.Parent); // remove the speaker button if not on phone
                }
                app.Doorunit?.RefreshRegister();
            }
        }

        /**
         * called when the screen size changes e.g. on rotate
         */
        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height); //must be called
            video_frame.HeightRequest = (int)height;
            video_frame.WidthRequest = (int)width;
        }
        #endregion

        #region button clicked methods
        /**
         * button settings clicked
         */
        private async void BtnSettings_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new Pages.Menue());
        }

        /**
         * button call clicked
         */
        private async void BtnCall_Clicked(object sender, EventArgs e)
        {
            if (!app.Doorunit?.IsRegisterd ?? false)
            {
                await Navigation.PushAsync(new Pages.Account());
                return;
            }
            if (app.Core.CallsNb > 0)
            {
                // end all existing call 
                app.Core.TerminateAllCalls();
            }
            else
            {
                // start a new call
                CallParams callParams = app.Core.CreateCallParams(null);
                callParams.VideoEnabled = true;
                string inviteUrl = "sip:" + app.Doorunit.Account.Number + "@" + app.Doorunit.Account.Sip.Domain + ":" + app.Doorunit.Account.Sip.Port.ToString();
                app.Core.InviteWithParams(inviteUrl, callParams);
            }
        }

        /**
         * button mute clicked
         */
        private void BtnMute_Clicked(object sender, EventArgs e)
        {
            ToogleMic();
        }

        /**
         * button unlock clicked
         */
        private async void BtnDoorUnlock_Clicked(object sender, EventArgs e)
        {
            if (!app.Doorunit?.IsRegisterd ?? false)
            {
                await Navigation.PushAsync(new Pages.Account());
                return;
            }
            Dup.Notification notification = Dup.Notification.Build(Dup.Context.SecureUnlock, new Dup.Data());
            Dup.Response response = await app.Doorunit.SendAsync(notification);
            if (!response.Ok)
            {
                await DisplayAlert("Error", response.Message, "Ok");
                return;
            }
            PlayUnlockAnimation();
        }
        /**
         * on main surface clicked
         */
        private void BtnHideControls_Clicked(object sender, EventArgs e)
        {
            if ((app.Core.CurrentCall?.State ?? CallState.Error) == CallState.StreamsRunning)
            {
                SetMainControlsVisibile(!mainControlsVisible);
            }
        }

        /**
         * button speaker clicked
         */
        private void BtnSpeaker_Clicked(object sender, EventArgs e)
        {
            ToogleSpeaker();
        }
        #endregion
        /**
         * called when new intent content is available (click on push)
         */
        private async void OnIntentContentAvailable(object sender, IntentContent notification)
        {
            if (!string.IsNullOrEmpty(notification.ImageUrl))
            {
                if(Navigation.ModalStack.Count > 0)
                {
                    await Navigation.PopModalAsync();
                }
                await Navigation.PushModalAsync(new Pages.Preview(notification.ImageUrl));
            }
        }

        /**
         * called when the registration state of the linphone core changes
         */
        private void OnRegistrationStateChanged(Core ls, ProxyConfig config, RegistrationState state, string message)
        {
            System.Diagnostics.Debug.WriteLine("Registration state changed: " + state);
            TbItemState.Text = state.ToString();
        }

        /**
         * called when the call state of the current call changes;
         */
        private void OnCallStateChanged(Core lc, Call lcall, CallState state, string message)
        {
            Debug.WriteLine("Call state changed: " + state);
            //SetMicMutedState(BtnMute, lcall, true);
            //InterpreteCallButtonState(BtnCall, lcall);
            switch (lcall.State)
            {
                case CallState.IncomingReceived:
                    lcall.Decline(Reason.Declined);
                    break;
                case CallState.Connected:
                    lcall.CameraEnabled = false;
                    // show the video winodw
                    //dieKlingel.Audio.Controller.TurnSpeakerOn();

                    if (dieKlingel.Controller.Audio.SpeakerState == dieKlingel.Controller.Audio.SpeakerMode.Speaker)
                    {
                        dieKlingel.Controller.Audio.TurnSpeakerOn();
                    }else
                    {
                        dieKlingel.Controller.Audio.TurnSpeakerOff();
                    }
                    DeviceDisplay.KeepScreenOn = true;
#if __IOS__
                    navBarBackgroundColor = ((NavigationPage)Application.Current.MainPage).BarBackgroundColor;
                    ((NavigationPage)Application.Current.MainPage).BarBackgroundColor = Color.Black;
                    navBarTextColor = ((NavigationPage)Application.Current.MainPage).BarTextColor;
                    ((NavigationPage)Application.Current.MainPage).BarTextColor = Color.White;
#endif
                    break;
                case CallState.StreamsRunning:
                    BtnMute.IsEnabled = true;
                    BtnMute.BackgroundColor = Color.FromHex(COLOR_RED_HEX);
                    BtnMute.Source = ImageSource.FromFile("micmuted.png");
                    BtnCall.BackgroundColor = Color.FromHex(COLOR_RED_HEX);
                    video_frame.IsVisible = true;
                    micEnabled = false;
                    break;
                case CallState.End:
                    BtnMute.IsEnabled = false;
                    BtnMute.BackgroundColor = Color.FromHex(COLOR_GREY_HEX);
                    BtnMute.Source = ImageSource.FromFile("micmuted.png");
                    BtnCall.BackgroundColor = Color.FromHex(COLOR_GREEN_HEX);
                    DeviceDisplay.KeepScreenOn = false;
                    // hide the video window
                    video_frame.IsVisible = false;
                    SetMainControlsVisibile(true);
#if __IOS__
                    ((NavigationPage)Application.Current.MainPage).BarBackgroundColor = navBarBackgroundColor;
                    ((NavigationPage)Application.Current.MainPage).BarTextColor = navBarTextColor;
                    UIKit.UIApplication.SharedApplication.StatusBarStyle = UIKit.UIStatusBarStyle.Default;
#endif
                    break;
            }
        }

        private async void SetMainControlsVisibile(bool visible=true)
        {
            ViewExtensions.CancelAnimations(FlMainControls);
            ViewExtensions.CancelAnimations(FrBtnSettings);
            mainControlsVisible = visible;
            if (visible)
            {
                // move controls in to viewport area
                _ = FrBtnSettings.TranslateTo(0, 0, 200);
                await FlMainControls.TranslateTo(0, 0, 200);
            }
            else
            {
                // move controls out of viewport area
                _ = FrBtnSettings.TranslateTo(100, 0, 200);
                await FlMainControls.TranslateTo(0, 200, 200);
            }
            
        }

        /**
         * toogle the mic between on and off;
         */
        private void ToogleMic()
        {
            if(micEnabled)
            {
                // 2021-11-12 15:56 Kai Mayer - get the current state from Core.MicEnabled does not work, it always returns true. 
                micEnabled = false;
                BtnMute.BackgroundColor = Color.FromHex(COLOR_RED_HEX);
                BtnMute.Source = ImageSource.FromFile("micmuted.png");
            }else
            {
                micEnabled = true;
                BtnMute.BackgroundColor = Color.FromHex(COLOR_GREEN_HEX);
                BtnMute.Source = ImageSource.FromFile("mic.png");
            }
        }

        /**
         * toogle the speaker between ear and speaker
         */
        private void ToogleSpeaker()
        {
            if(dieKlingel.Controller.Audio.SpeakerState == dieKlingel.Controller.Audio.SpeakerMode.OnEar)
            {
                dieKlingel.Controller.Audio.TurnSpeakerOn();
                // BtnSpeaker could be null, if removed before when the app is not be running on a phone
                if(null != BtnSpeaker)
                {
                    BtnSpeaker.Source = ImageSource.FromFile("speaker.png");
                }
            }else
            {
                dieKlingel.Controller.Audio.TurnSpeakerOff();
                if (null != BtnSpeaker)
                {
                    BtnSpeaker.Source = ImageSource.FromFile("speakersilent.png");
                }
            }
        }

        /**
         * plays the animation of the lock
         */
        private async void PlayUnlockAnimation()
        {
            ViewExtensions.CancelAnimations(lock_top);
            ViewExtensions.CancelAnimations(lock_);

            lock_.IsVisible = true;
            await lock_.FadeTo(1, 300);
            await lock_top.TranslateTo(0, -30, 200);
            await lock_.FadeTo(0, 300);
            _ = lock_top.TranslateTo(0, 0, 0);
            lock_.IsVisible = false;
        }
    }
}
