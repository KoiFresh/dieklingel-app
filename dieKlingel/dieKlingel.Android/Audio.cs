using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dieKlingel
{
    namespace Controller
    {
        class Audio
        {
            /*
             * turns the speaker on
             * @return returns true if the operation was succesfull
             */
            private static SpeakerMode speakerState = SpeakerMode.OnEar;
            public static bool TurnSpeakerOn()
            {
                AudioManager am = (AudioManager)Android.App.Application.Context.GetSystemService(Android.Content.Context.AudioService);
                am.Mode = Mode.InCall;
                am.SpeakerphoneOn = true;
                speakerState = SpeakerMode.Speaker;
                return true;
            }

            /*
             * turns the speaker off
             * @return returns true if the operation was succesfull
             */
            public static bool TurnSpeakerOff()
            {
                AudioManager am = (AudioManager)Android.App.Application.Context.GetSystemService(Android.Content.Context.AudioService);
                am.Mode = Mode.InCall;
                am.SpeakerphoneOn = false;
                speakerState = SpeakerMode.OnEar;
                return true;
            }

            public static SpeakerMode SpeakerState
            {
                get
                {
                    return speakerState;
                }
            }

            public enum SpeakerMode
            {
                OnEar, 
                Speaker
            }
        }
    }
}