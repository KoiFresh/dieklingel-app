using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using SkiaSharp.Views.Forms;
using SkiaSharp;



namespace dieKlingel.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Finder : ContentView
    {
        public ImageButton FirstIcon { get { return BtnSmartphone; } }
        public ImageButton SecondIcon { get { return BtnDoorunit; } }
        public bool IsSearching { get { return _IsSearching; }}

        private bool _IsSearching = false;
        private const int CIRCLES = 4;
        private int active_circle = 0;

        public Finder()
        {
            InitializeComponent();
            SkCanvas.PaintSurface += SkCanvas_PaintSurface;
        }

        public void StartSearching()
        {
            if(!IsSearching)
            {
                _IsSearching = true;
                _ = AnimationLoop();
            }
        }

        public void StopSearching()
        {
            _IsSearching = false;
            active_circle = 0;
            SkCanvas.InvalidateSurface();
        }

        private void SkCanvas_PaintSurface(object sender, SkiaSharp.Views.Forms.SKPaintSurfaceEventArgs e)
        {
            SKCanvas canvas = e.Surface.Canvas;
            int width = e.Info.Width;
            int height = e.Info.Height;

            canvas.Clear();

            SKRect rect = new SKRect(0, 0, width, height);
            //canvas.DrawRect(rect, paint);

            SKPaint signal_paint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = new SKColor(89, 209, 222),
                StrokeWidth = 8,
                MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Solid, 2)
            };

            SKPaint active_paint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = new SKColor(67, 130, 240),
                StrokeWidth = 8,
                MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Solid, 2)
            };

            int length = Math.Min(width, height);
            SKPoint center = new SKPoint((width / 2) - (length / 3), (height / 2) );
            for (int i = 0; i < CIRCLES; i++)
            {
                int side = (length / CIRCLES) * (i + 1);
                SKRect outer = new SKRect(center.X - (side / 2), center.Y - (side / 2), center.X + (side / 2), center.Y + (side / 2));
                //canvas.DrawRect(outer, signal_paint);
                using (SKPath path = new SKPath())
                {
                    path.AddArc(outer, -45, 90);
                    canvas.DrawPath(path, signal_paint);
                }
            }

            if(IsSearching)
            {
                if(active_circle < (length / CIRCLES) || active_circle > length)
                {
                    active_circle = (length / CIRCLES);
                }
                SKRect active = new SKRect(center.X - (active_circle / 2), center.Y - (active_circle / 2), center.X + (active_circle / 2), center.Y + (active_circle / 2));
                using (SKPath path = new SKPath())
                {
                    path.AddArc(active, -45, 90);
                    canvas.DrawPath(path, active_paint);
                }
                active_circle += 2;
            }
        }

        private async Task AnimationLoop()
        {
            while(IsSearching)
            {
                SkCanvas.InvalidateSurface();
                await Task.Delay(16);
            }
        }
    }
}