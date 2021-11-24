using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using LibVLCSharp.Shared;
using LibVLCSharp.Forms;
using LibVLCSharp;
using LibVLCSharp.Shared;
using MediaManager;

namespace dieKlingel.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class About : ContentPage
    {
        //LibVLC _libvlc;
        public About()
        {
            InitializeComponent();
             //Core.Initialize();
            //_libvlc = new LibVLC();
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();

            // create mediaplayer objects,
            // attach them to their respective VideoViews
            // create media objects and start playback

            //VideoView0.MediaPlayer = new LibVLCSharp.Shared.MediaPlayer(_libvlc);
            //VideoView0.MediaPlayer.Play(new Media(_libvlc, "rtmp://dev.ct.dieklingel.com/live/STREAM_NAME", FromType.FromLocation));
            //VideoView0.MediaPlayer.Play(new Media(_libvlc, "rtsp://dev.ct.dieklingel.com:6554/stream", FromType.FromLocation));
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            //VideoView0.MediaPlayer.Stop();
        }
    }
}