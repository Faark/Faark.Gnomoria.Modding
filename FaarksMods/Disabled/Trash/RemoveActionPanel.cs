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

namespace Faark.Gnomoria.Mods.CustomWorkshopUI
{
#if false
    exists ingame as setting... i should check that more often, even if creating a mod that does the same only takes few minutes^^
    public class RemoveActionPanel: Mod
    {
        public override IEnumerable<IModification> Modifications
        {
	        get 
	        {
                yield return new MethodHook(
                    typeof(HUD).GetConstructor(new Type[] { typeof(Manager) }),
                    Method.Of<HUD, Manager>(On_HUD_Created)
                    );
	        }
        }
        public override string Name
        {
            get
            {
                return "Goodbye Action Panel";
            }
        }
        public override string Description
        {
            get
            {
                return "This mod removes the clumpy menu box at the bottom of the screen, also known as action panel. Great for guys like me who only use the right click menu anyway.";
            }
        }
        public override string Author
        {
            get
            {
                // Made mentionable changes? Add yourself!
                return "Faark";
            }
        }
        public static void On_HUD_Created(HUD self, Manager mgr)
        {
            self.Remove(self.ClientArea.Controls.Single(ctrl => ctrl is ActionPanel));
        }
    }
#endif
}
