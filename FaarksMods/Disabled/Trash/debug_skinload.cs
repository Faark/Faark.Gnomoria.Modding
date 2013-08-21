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
#if false
    /// <summary>
    /// Helped me to understand some issues. You won't need it.
    /// </summary>
    public class DebugSkinLoad : Mod
    {
        public override ModConfig GetConfig()
        {
            return new ModConfig(
                this,
                new MethodAddVirtual(
                    typeof(ArchiveManager),
                    typeof(ArchiveManager).GetMethod("Load", BindingFlags.Instance | BindingFlags.Public),
                    typeof(DebugSkinLoad).GetMethod("Load"),
                    MethodHookType.RunBefore
                    )
                );
        }
        public static void Load<T>(
            //Game.GUI.Controls.ArchiveManager self,
            ArchiveManager self,
            String assert_name
            )
        {


            Game.GnomanEmpire.Instance.Content.RootDirectory = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "Content");
            return;
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().OrderBy(a=>a.FullName).ToArray();
            var arrLocs = assemblies.Select(a => a.CodeBase).ToArray();
            arrLocs.ToString();
            var tt = typeof(T);
            RuntimeModController.WriteLogO(tt);
            RuntimeModController.WriteLog(assert_name);
            return;
        }
    }
#endif
}