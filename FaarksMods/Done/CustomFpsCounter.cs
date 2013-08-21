using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Faark.Gnomoria.Modding;
using Game;
using Game.GUI;
using Game.GUI.Controls;
using GameLibrary;
using Microsoft.Xna.Framework;

namespace Faark.Gnomoria.Mods
{
#if true
    /// <summary>
    /// A custom FPS counter. It was planned to add more metrics and/or convert it into a graph, but never got around to do it.
    /// </summary>
    public class CustomFpsCounter: Mod
    {
        public override IEnumerable<IModification> Modifications
        {
            get
            {
                yield return new MethodHook(
                    typeof(GnomanEmpire).GetMethod("Update", BindingFlags.NonPublic | BindingFlags.Instance),
                    Method.Of<GnomanEmpire, GameTime>(GnomanEmpire_Update),
                    MethodHookType.RunBefore
                    );
            }
        }
        public override string Author
        {
            get
            {
                return "Faark";
            }
        }
        public override string Description
        {
            get
            {
                return "A custom FPS counter. It was planned to add more metrics and/or convert it into a graph, but never got around to do it.";
            }
        }


        private static readonly TimeSpan UpdateDisplayEvery = TimeSpan.FromMilliseconds(150);
        private static readonly TimeSpan GatherDataTimespanShot = TimeSpan.FromSeconds(1);
        private static readonly TimeSpan GatherDataTimespanLong = TimeSpan.FromMinutes(1);

        private class HCtr
        {
            private TimeSpan totalTime = TimeSpan.Zero;
            private LinkedList<TimeSpan> times = new LinkedList<TimeSpan>();
            private TimeSpan recTime;
            public HCtr(TimeSpan recordTime)
            {
                recTime = recordTime;
            }
            public double Value
            {
                get
                {
                    return times.Count / totalTime.TotalSeconds;
                }
            }
            public string Text
            {
                get
                {
                    return Value.ToString("0.00");
                }
            }
            public void AddFrame(TimeSpan frameTime)
            {
                times.AddLast(frameTime);
                totalTime += frameTime;
                while ((totalTime - times.First.Value) > recTime)
                {
                    totalTime -= times.First.Value;
                    times.RemoveFirst();
                }
            }
        }
        private static HCtr shortRec = new HCtr(GatherDataTimespanShot);
        private static HCtr longRec = new HCtr(GatherDataTimespanLong);

        private static TimeSpan nextDisplayUpdate;
        private static Label fps_display;
        private static void UpdateDisplayedFps()
        {
            if (fps_display == null || fps_display.Manager != GnomanEmpire.Instance.GuiManager.Manager)
            {
                /*
                var r = new Random();
                var s = 4096 * 2;
                //var s = 4096;
                var tex = new Microsoft.Xna.Framework.Graphics.Texture2D(GnomanEmpire.Instance.GraphicsDevice, s, s, false, Microsoft.Xna.Framework.Graphics.SurfaceFormat.Color);
                tex.SetData(Enumerable.Repeat(1, s * s).Select(foo => new Color(r.Next(256), r.Next(256), r.Next(256))).ToArray());
                */
                fps_display = new Label(GnomanEmpire.Instance.GuiManager.Manager);
                fps_display.Init();
                fps_display.Anchor = Anchors.Bottom | Anchors.Left;
                fps_display.Width = 250;
                fps_display.Height = 25;
                fps_display.Top = GnomanEmpire.Instance.GuiManager.Manager.ScreenHeight - fps_display.Height;
                fps_display.Left = 10;
                //fps_display.Color = ;
                fps_display.TextColor = Color.LightGreen;
                /*fps_display.Draw += new DrawEventHandler((sender, args) =>
                {
                    args.Renderer.SpriteBatch.Draw(tex, Vector2.Zero, Color.White);
                });*/
                GnomanEmpire.Instance.GuiManager.Add(fps_display);
            }
            fps_display.Text = shortRec.Text + " FPS (" + longRec.Text + " avg)";
        }
        public static void GnomanEmpire_Update(GnomanEmpire self, GameTime gt)
        {
            shortRec.AddFrame(gt.ElapsedGameTime);
            longRec.AddFrame(gt.ElapsedGameTime);
            if (nextDisplayUpdate < gt.TotalGameTime)
            {
                nextDisplayUpdate = gt.TotalGameTime + UpdateDisplayEvery;
                UpdateDisplayedFps();
            }
        }
    }
#endif
}
