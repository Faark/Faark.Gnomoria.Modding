using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Faark.Gnomoria.Modding;

namespace Faark.Gnomoria.Mods
{
#if false

    Exists in the stock game, now. So moved from ideas to trash.

    public class CustomStockpileCommand : Mod
    {
        private static FieldInfo RightClickMenu_ContextMenu;

        public override void Initialize_PreGame()
        {
            RightClickMenu_ContextMenu = typeof(Game.GUI.RightClickMenu)
                .GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                .Single(field => field.FieldType == typeof(Game.GUI.Controls.ContextMenu));
        }
        public override ModConfig GetConfig()
        {
            return new ModConfig(
                this,
                new MethodHook(
                    typeof(Game.GUI.RightClickMenu).GetConstructor(new Type[] { }),
                    Method.Of<Game.GUI.RightClickMenu>(OnCreated_RightClickMenu)
                    ));
        }
        public static void OnCreated_RightClickMenu(Game.GUI.RightClickMenu self)
        {
            var context_menu = (Game.GUI.Controls.ContextMenu)RightClickMenu_ContextMenu.GetValue(self);
            var myMenu = new Game.GUI.Controls.MenuItem("Mod Stuff");
            var menuItem = new Game.GUI.Controls.MenuItem();
            menuItem.Text = "Stockpile Area";
            menuItem.Click += new Game.GUI.Controls.EventHandler(OnStockpileArea);
            myMenu.Items.Add(menuItem);
            context_menu.Items.Add(myMenu);
        }

        static void OnStockpileArea(object sender, Game.GUI.Controls.EventArgs e)
        {
            var tsm = Game.GnomanEmpire.Instance.Region.TileSelectionManager;
            //tsm.SetMouseAction(Game.JobType.Mine, true, false, true);
            tsm.SetMouseAction(Game.JobType.StockItem, true, false, true);
            //throw new NotImplementedException();
        }
    }
#endif    
}
