using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Faark.Gnomoria.Modding;

namespace Faark.Gnomoria.Mods
{
#if false

    The tabs this mod used to sync where merged together. You might find sth interesting in here anyway.

    public class PopulationUI_SyncTabScrolls : Mod
    {
        /*
         * this example is kind of problematic, since i am accessing a lot of private properties via reflection
         * make sure this does not happen in performance-critical sections
         */

        public override ModConfig GetConfig()
        {
            var targetFun = typeof(Game.GUI.PopulationUI)
                .GetConstructors()
                .Single(
                    con =>
                        con.GetParameters().Length == 1
                        && con.GetParameters()[0].ParameterType == typeof(Game.GUI.Controls.Manager)
                );
            return new ModConfig(
                this,
                new MethodHook(
                    targetFun, 
                    Method.Of<Game.GUI.PopulationUI, Game.GUI.Controls.Manager>(OnAfter_PopulationUI_Created)
                    )
                );
        }
        private static Game.GUI.Controls.ScrollBar getScrollbarFromEl(Game.GUI.Controls.Control ctrl)
        {
            foreach (var scrollBarField in controlScrollBarFieldsInfoList)
            {
                var sb = (Game.GUI.Controls.ScrollBar)scrollBarField.GetValue(ctrl);
                var or = (Game.GUI.Controls.Orientation)scrollBarOrientationFieldInfo.GetValue(sb);
                if(or == Game.GUI.Controls.Orientation.Vertical){
                    return sb;
                }
            }
            throw new Exception("This control does not have a vertical scrollbar");
        }
        public static void OnAfter_PopulationUI_Created(Game.GUI.PopulationUI self, Game.GUI.Controls.Manager arg)
        {
            var tabCtrl = (Game.GUI.Controls.TabControl)tabControlVarInfo.GetValue(self);
            var tabPanelList = (List<Game.GUI.TabbedWindowPanel>)tabControlPanelListVarInfo.GetValue(self);

            Game.GUI.Controls.ScrollBar popPermScrollBar = null;
            Game.GUI.Controls.ScrollBar popStatusScrollBar = null;

            //Game.GUI.PopulationPermissionUI popPerm = null;
            //Game.GUI.PopulationStatusUI popStatus = null;
            foreach (var panel in tabPanelList)
            {
                if (panel is Game.GUI.PopulationPermissionUI)
                {
                    popPermScrollBar = getScrollbarFromEl(panel);

                }
                else if (panel is Game.GUI.PopulationStatusUI)
                {
                    popStatusScrollBar = getScrollbarFromEl(panel);
                }
            }

            if ((popPermScrollBar == null) || (popStatusScrollBar == null))
                throw new Exception("Couldn't find scrollbars for both tabs!");

            var last_page = 0;

            tabControlEventInfo.AddEventHandler(tabCtrl, new Game.GUI.Controls.EventHandler((sender, args) =>
            {
                if ((last_page == 1) && (tabCtrl.SelectedIndex == 2))
                {
                    popPermScrollBar.Value = popStatusScrollBar.Value;
                    //popStatus.ScrollBarValue.Horizontal = popPerm.ScrollBarValue.Horizontal;
                }
                else if ((last_page == 2) && (tabCtrl.SelectedIndex == 1))
                {
                    popStatusScrollBar.Value = popPermScrollBar.Value;
                }
                last_page = tabCtrl.SelectedIndex;
            }));
        }

        private static System.Reflection.FieldInfo tabControlVarInfo;
        private static System.Reflection.EventInfo tabControlEventInfo;
        private static System.Reflection.FieldInfo tabControlPanelListVarInfo;
        private static System.Reflection.FieldInfo[] controlScrollBarFieldsInfoList;
        private static System.Reflection.FieldInfo scrollBarOrientationFieldInfo;

        public override void Initialize_PreGame()
        {
            // caching the reflection stuff should speed up stuff at least a little bit. Reflection isn't very fast, anyway. Try to avoid it in hot code.
            tabControlVarInfo = typeof(Game.GUI.TabbedWindow).GetField("mTabControl", BindingFlags.NonPublic | BindingFlags.Instance);
            tabControlEventInfo = typeof(Game.GUI.Controls.TabControl).GetEvent("PageChanged");
            tabControlPanelListVarInfo = typeof(Game.GUI.TabbedWindow).GetFields(BindingFlags.NonPublic | BindingFlags.Instance).Single(field => field.FieldType == typeof(List<Game.GUI.TabbedWindowPanel>));
            controlScrollBarFieldsInfoList = typeof(Game.GUI.Controls.Container).GetFields(BindingFlags.NonPublic | BindingFlags.Instance).Where(field => field.FieldType == typeof(Game.GUI.Controls.ScrollBar)).ToArray();
            scrollBarOrientationFieldInfo = typeof(Game.GUI.Controls.ScrollBar).GetFields(BindingFlags.NonPublic | BindingFlags.Instance).Single(field => field.FieldType == typeof(Game.GUI.Controls.Orientation));
        }
    }
#endif
}
