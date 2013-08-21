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
using Faark.Util;
using Microsoft.Xna.Framework;

namespace Faark.Gnomoria.Mods
{
#if true
    /// <summary>
    /// This mod allows access to the developer consol and item spawn menu.
    /// </summary>
    public class ShowDeveloperConsole: Mod
    {
        public override string Author
        {
            get
            {
                return "Faark";
            }
        }
        public override string Name
        {
            get
            {
                return "Show Developer Tools";
            }
        }
        public override string Description
        {
            get
            {
                return "Allows you to show the games own, though every limited developer console as well as an UI to spawn items. Use it at your own risk.";
            }
        }
        public override IEnumerable<IModification> Modifications
        {
            get
            {
                yield break;
            }
        }
        public override IEnumerable<ModDependency> Dependencies
        {
            get
            {
                yield return Modding.HelperMods.ModRightClickMenu.Instance;
            }
        }
        public override void Initialize_PreGeneration()
        {
            Modding.HelperMods.ModRightClickMenu.AddItem("Toggle Developer Console", ToggleDeveloperConsole);
            Modding.HelperMods.ModRightClickMenu.AddItem("Item spawn menu", ItemSpawnMenu);
            base.Initialize_PreGeneration();
        }
        private static ConsoleWindow console;
        public static void ToggleDeveloperConsole()
        {
            if ((console != null) && GnomanEmpire.Instance.GuiManager.Manager.Controls.Contains(console))
            {
                GnomanEmpire.Instance.GuiManager.Remove(console);
                console = null;
            }
            else
            {
                GnomanEmpire.Instance.GuiManager.Add(console = new ConsoleWindow(GnomanEmpire.Instance.GuiManager.Manager));
            }
        }
        public static void ItemSpawnMenu() 
        {
            var pos = (Vector3)typeof(Game.GUI.RightClickMenu)
                .GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                .Single(f => f.FieldType == typeof(Vector3))
                .GetValue(
                    typeof(Game.GUI.HUD)
                        .GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                        .Single(f => f.FieldType == typeof(Game.GUI.RightClickMenu))
                        .GetValue(
                            typeof(Game.GUI.InGameHUD)
                                .GetFields(BindingFlags.NonPublic| BindingFlags.Instance)
                                .Single(f => f.FieldType == typeof(Game.GUI.HUD))
                                .GetValue(GnomanEmpire.Instance.GuiManager.HUD)
                        )
                );
            GnomanEmpire.Instance.GuiManager.HUD.ShowWindow(new SpawnItemUI(GnomanEmpire.Instance.GuiManager.Manager, pos));
        }
    }
#endif
}