using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Faark.Gnomoria.Modding;
using Faark.Util;

namespace Faark.Gnomoria.Mods
{
    /// <summary>
    /// Displays the seed for that automatically generated world on the start screen. You can click it to create a new world with that seed.
    /// </summary>
    public class ShowSeedOnMainMenu : Mod
    {
        public override IEnumerable<IModification> Modifications
        {
            get
            {
                return new IModification[]{
                    new MethodHook(
                        typeof(Game.GUI.MainMenuWindow).GetConstructor(new Type[] { typeof(Game.GUI.Controls.Manager) }),
                        Method.Of<Game.GUI.MainMenuWindow, Game.GUI.Controls.Manager>(OnAfter_MainMenuWindow_Created)
                        ),
                    new MethodHook(
                        typeof(Game.GnomanEmpire).GetMethod("PreviewMap", BindingFlags.Instance| BindingFlags.Public),
                        Method.Of<Game.GnomanEmpire, Game.CreateWorldOptions, bool, int>(OnAfter_PreviewMap)
                        )
                };
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
                return "Displays the seed for that automatically generated world on the start screen. You can click it to create a new world with that seed.";
            }
        }


        private static string seed = null;
        private static Game.GUI.Controls.Label version_label = null;
        private static Game.GUI.Controls.Label seed_label = null;
        private static uint last_seed;
        public static void OnAfter_MainMenuWindow_Created(Game.GUI.MainMenuWindow self, Game.GUI.Controls.Manager mgr)
        {
            seed_label = null;
            version_label = (Game.GUI.Controls.Label)typeof(Game.GUI.MainMenuWindow)
                .GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                .Single(field => field.FieldType == typeof(Game.GUI.Controls.Label))
                .GetValue(self);
            TryLabelUpdate();
        }
        public static void OnAfter_PreviewMap(Game.GnomanEmpire self, Game.CreateWorldOptions worldOptions, bool clear, int xyScale)
        {
            var task = (System.Threading.Tasks.Task)typeof(Game.GnomanEmpire)
                .GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static)
                .Single(field => field.FieldType == typeof(System.Threading.Tasks.Task))
                .GetValue(self);
            task.ContinueWith(new Action<System.Threading.Tasks.Task>((t) =>
            {
                seed = (last_seed = self.Map.WorldSeed).ToString();
                TryLabelUpdate();
            }));
        }
        private static void TryLabelUpdate()
        {
            if ((seed != null) && (version_label != null))
            {
                int right;
                if (seed_label == null)
                {
                    seed_label = new Game.GUI.Controls.Label(version_label.Manager);
                    seed_label.Init();
                    seed_label.Anchor = Game.GUI.Controls.Anchors.Left | Game.GUI.Controls.Anchors.Bottom;
                    seed_label.Top = version_label.Top;
                    var default_color = seed_label.TextColor = version_label.TextColor;
                    seed_label.ToolTip = new Game.GUI.Controls.ToolTip(version_label.Manager) { Text = "Create a new world with this Seed." };
                    seed_label.Passive = false;
                    seed_label.CanFocus = true;
                    /*seed_label.MouseMove += new Game.GUI.Controls.MouseEventHandler((sender, args) =>
                    {
                        (sender as Game.GUI.Controls.Label).TextColor = Microsoft.Xna.Framework.Color.LightGreen;
                    });*/
                    seed_label.MouseOver += new Game.GUI.Controls.MouseEventHandler((sender, args) =>
                    {
                        (sender as Game.GUI.Controls.Label).TextColor = Microsoft.Xna.Framework.Color.LightGreen;
                    });
                    seed_label.MouseOut += new Game.GUI.Controls.MouseEventHandler((sender, args) =>
                    {
                        (sender as Game.GUI.Controls.Label).TextColor = default_color;
                    });
                    seed_label.Click += new Game.GUI.Controls.EventHandler((sender, args) =>
                    {
                        Game.GnomanEmpire.Instance.GuiManager.MenuStack.PushWindow(
                            new Game.GUI.AdvancedSetupWindow(
                                ((Game.GUI.Controls.Control)sender).Manager, new Game.CreateWorldOptions() { 
                                    Seed = last_seed,
                                    KingdomName = Game.GnomanEmpire.Instance.World.LanguageManager.RandomFactionName(Game.GnomanEmpire.Instance.World.AIDirector.FactionDefs[0].Language)
                                })
                            );
                    });
                    version_label.Parent.Add(seed_label);
                    right = version_label.Left + version_label.Width;
                    version_label.Text += "; ";
                    version_label.Width = (int)version_label.Skin.Layers[0].Text.Font.Resource.MeasureString(version_label.Text).X + 2;
                }
                else
                {
                    right = seed_label.Left + seed_label.Width;
                }
                seed_label.Text = seed.ToString();
                seed_label.Width = (int)seed_label.Skin.Layers[0].Text.Font.Resource.MeasureString(seed_label.Text).X + 2;
                seed_label.Left = right - seed_label.Width;
                version_label.Left = seed_label.Left - version_label.Width;
                seed = null;
                version_label = null;
            }
        }
    }

}
