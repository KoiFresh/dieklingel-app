using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIKit;

namespace dieKlingel
{
    namespace Controller
    {
        class Audio
        {
            private static SpeakerMode speakerState = SpeakerMode.OnEar;
            // gibt true zurück wenn der Lautsprecher aktiviert wurde
            public static bool TurnSpeakerOn()
            {
                bool result = false;
                NSError error = AVFoundation.AVAudioSession.SharedInstance().SetCategory(AVFoundation.AVAudioSessionCategory.PlayAndRecord, AVFoundation.AVAudioSessionCategoryOptions.DefaultToSpeaker);

                if (error == null)
                {
                    if (AVFoundation.AVAudioSession.SharedInstance().SetMode(AVFoundation.AVAudioSession.ModeVideoChat, out error))
                    {
                        if (AVFoundation.AVAudioSession.SharedInstance().OverrideOutputAudioPort(AVFoundation.AVAudioSessionPortOverride.Speaker, out error))
                        {
                            error = AVFoundation.AVAudioSession.SharedInstance().SetActive(true);
                            result = true;
                            if (error != null)
                            {
                                System.Diagnostics.Debug.WriteLine(new Exception(error?.LocalizedDescription ?? "Cannot set active"));
                            }
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine(new Exception(error?.LocalizedDescription ?? "Cannot override output audio port"));
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine(new Exception("Cannot set mode"));
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine(new Exception(error?.LocalizedDescription ?? "Cannot set category"));
                }
                speakerState = result ? SpeakerMode.Speaker : SpeakerMode.OnEar;
                return result;
            }

            // gibt true zurück wenn der Lautsprecher deaktiviert wurde
            public static bool TurnSpeakerOff()
            {
                bool result = false;
                NSError error = AVFoundation.AVAudioSession.SharedInstance().SetCategory(AVFoundation.AVAudioSessionCategory.PlayAndRecord, AVFoundation.AVAudioSessionCategoryOptions.AllowBluetooth);

                if (error == null)
                {
                    if (AVFoundation.AVAudioSession.SharedInstance().SetMode(AVFoundation.AVAudioSession.ModeVoiceChat, out error))
                    {
                        if (AVFoundation.AVAudioSession.SharedInstance().OverrideOutputAudioPort(AVFoundation.AVAudioSessionPortOverride.None, out error))
                        {
                            error = AVFoundation.AVAudioSession.SharedInstance().SetActive(true);
                            result = true;
                            if (error != null)
                            {
                                System.Diagnostics.Debug.WriteLine(new Exception(error?.LocalizedDescription ?? "Cannot set active"));
                            }
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine(new Exception(error?.LocalizedDescription ?? "Cannot override output audio port"));
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine(new Exception("Cannot set mode"));
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine(new Exception(error?.LocalizedDescription ?? "Cannot set category"));
                }
                speakerState = result ? SpeakerMode.OnEar: SpeakerMode.Speaker;
                return result;
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