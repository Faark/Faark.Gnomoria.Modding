using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Faark.Gnomoria.Modding;

namespace Faark.Gnomoria.Mods
{
#if true
    /// <summary>
    /// This mod creates a copy of your new save file after every save.
    /// </summary>
    public class Game_CreateBackupSavegame : Mod
    {
        public override IEnumerable<IModification> Modifications
        {
            get
            {
                // this function have to return configuration data about this mod.

                // usually we want to hook into the game at some point, so to let the modui set this up
                // we first need a System.Reflection.MethodBase reference to both the functions we want to hook
                // into and our own that shall be called

                // there are 2 ways to get our method handles. Lets start with using System.Reflection. In that case it is easy,
                // since there is just one SaveGame that is public. Otherwise you would may have to do other stuff, may search in .GetMethods(flags)
                var original_function = typeof(Game.GnomanEmpire).GetMethod("SaveGame");

                // But for public static functions, such as all of ours, you should use the following little helper. 
                // The huge advantage: Compile time errors when e.g. renaming the method or just a spelling mistake
                var our_function = Method.Of<Task, Game.GnomanEmpire, bool, Task>(OnAfter_SaveGame_Started);

                // and finally we want to return the list of all hooks (currently only one)
                return new IModification[]{
                    new MethodHook(
                        original_function,
                        our_function
                    )
                };
            }
        }
        // A little effort to make your mod look nice in the mod list.
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
        public static System.Threading.Tasks.Task OnAfter_SaveGame_Started(System.Threading.Tasks.Task save_task, Game.GnomanEmpire self, bool fallenKingdom)
        {
            // This function is hooked into the game as specified by GetConfig().
            // Be careful to exactly match the required structure (see other doc, sth like [RetVal|void]([retVal][this][arguments]) )
            // Currently there isn't any flexibility, so you have to specifiy all that apply
            return save_task.ContinueWith((task) =>
            {
                try
                {
                    //scaning for the newest dated looks like the easies way to get the latest save, since i havent found any "get current save file"-func
                    var latest_save_file = new System.IO.DirectoryInfo(Game.GnomanEmpire.SaveFolderPath("Worlds\\"))
                        .GetFiles()
                        .Where(file => file.Extension.ToUpper() == ".SAV")
                        .Aggregate((a, b) =>
                        {
                            return a.LastWriteTime > b.LastWriteTime ? a : b;
                        });
                    var backup_folder = System.IO.Path.Combine(latest_save_file.DirectoryName, "Backups");
                    System.IO.Directory.CreateDirectory(backup_folder);
                    System.IO.File.Copy(
                        latest_save_file.FullName,
                        System.IO.Path.Combine(
                            backup_folder,
                            latest_save_file.Name.Remove(latest_save_file.Name.Length - latest_save_file.Extension.Length)
                            + "_" + DateTime.Now.ToString("yyyyMMddHHmmss")
                            + latest_save_file.Extension
                            )
                        );
                    return;
                }
                catch (Exception err)
                {
                    RuntimeModController.Log.Write(err);
                }
            });

            //don't want to return a modified value? In that case you have to return the return_val parameter! (usually first argument)
            //return save_task;
        }
    }
#endif
}
