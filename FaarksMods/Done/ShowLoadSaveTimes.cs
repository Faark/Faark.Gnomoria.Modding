using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;
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
    /// Displays how long it took to save & load a game.
    /// </summary>
    public class ShowLoadSaveTimes: Mod
    {
        public override IEnumerable<IModification> Modifications
        {
            get
            {
                yield return new MethodHook(
                    typeof(GnomanEmpire).GetMethod("LoadGame"),
                    Method.Of<GnomanEmpire, string, bool>(OnBefore_GnomanEmpire_LoadGame),
                     MethodHookType.RunBefore
                     );
                yield return new MethodHook(
                    typeof(GnomanEmpire).GetMethod("LoadGame"),
                    Method.Of<GnomanEmpire, string, bool>(OnAfter_GnomanEmpire_LoadGame),
                    MethodHookType.RunAfter
                    );
                yield return new MethodHook(
                    typeof(GnomanEmpire).GetMethod("FinishLoadingGame"),
                    Method.Of<GnomanEmpire>(On_GnomanEmpire_FinishLoadingGame)
                    );
                yield return new MethodHook(
                    typeof(GnomanEmpire).GetMethod("SaveGame"),
                    Method.Of<GnomanEmpire, bool>(OnBefore_GnomanEmpire_SaveGame),
                     MethodHookType.RunBefore
                     );
                yield return new MethodHook(
                    typeof(GnomanEmpire).GetMethod("SaveGame"),
                    Method.Of<Task, GnomanEmpire, bool, Task>(OnAfter_GnomanEmpire_SaveGame),
                    MethodHookType.RunAfter
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
                return "Displays how long it took to save & load a game.";
            }
        }
        private static DateTime LoadStart;
        private static DateTime LoadEnd;
        public static void OnBefore_GnomanEmpire_LoadGame(GnomanEmpire self, string file, bool fallen)
        {
            LoadStart = DateTime.Now;
        }
        public static void OnAfter_GnomanEmpire_LoadGame(GnomanEmpire self, string file, bool fallen)
        {
            LoadEnd = DateTime.Now;
        }
        public static void On_GnomanEmpire_FinishLoadingGame(GnomanEmpire self)
        {
            GnomanEmpire.Instance.World.NotificationManager.AddNotification("Game loaded within " + (LoadEnd - LoadStart).TotalSeconds.ToString("0.00") + " sec", false);
        }
        private static DateTime SaveStart;
        public static void OnBefore_GnomanEmpire_SaveGame(GnomanEmpire self, bool fallen)
        {
            SaveStart = DateTime.Now;
        }
        public static Task OnAfter_GnomanEmpire_SaveGame(Task result, GnomanEmpire self, bool fallen)
        {
            return result.ContinueWith((task) =>
            {
                GnomanEmpire.Instance.World.NotificationManager.AddNotification("Game saved within " + (DateTime.Now - SaveStart).TotalSeconds.ToString("0.00") + " sec", false);
            });
        }
    }
#endif
}
