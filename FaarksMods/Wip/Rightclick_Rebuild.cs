using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Faark.Gnomoria.Modding;

namespace Faark.Gnomoria.Mods
{
#if true
    /// <summary>
    /// Adds a shortcut menu that lists the latest build constructions, so you can easily repeat building them without having to reselect matierals.
    /// 
    /// *THIS MOD IS WORK IN PROGRESS*
    /// 
    /// Todo:
    /// - Find a way to name the list items in a easy-to-understand way.
    /// - List and its lenght... it would be awesome to make it more dynamic, might even scroll-able?
    /// 
    /// </summary>
    public class Rightclick_Rebuild : Mod
    {
        private static FieldInfo RightClickMenu_ContextMenu;

        public override IEnumerable<ModType> InitAfter
        {
            get
            {
                yield return Modding.HelperMods.ModRightClickMenu.Instance;
            }
        }
        public override void Initialize_PreGame()
        {
            RightClickMenu_ContextMenu = typeof(Game.GUI.RightClickMenu)
                .GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                .Single(field => field.FieldType == typeof(Game.GUI.Controls.ContextMenu));
        }
        public override IEnumerable<IModification> Modifications
        {
            get
            {
                return new IModification[]{
                    new MethodHook(
                        typeof(Game.GUI.RightClickMenu).GetConstructor(new Type[] { }),
                        Method.Of<Game.GUI.RightClickMenu>(On_RightClickMenu_Created)
                        ),
                    new MethodHook(
                        typeof(Game.GUI.TileSelectionManager).GetMethod("SetMouseAction", new Type[] { typeof(Game.JobType), typeof(Game.JobData), typeof(bool), typeof(bool), typeof(bool), typeof(bool) }),
                        Method.Of<Game.GUI.TileSelectionManager, Game.JobType, Game.JobData, bool, bool, bool, bool>(On_TileSelectionManager_SetMouseAction)
                        ),
                    new MethodHook(
                        typeof(Game.GUI.TileSelectionManager).GetMethods().Single(m=>m.Name=="ComponentsSelected"),
                        Method.Of<Game.GUI.TileSelectionManager, GameLibrary.ConstructionID, List<int>>(On_TileSelectionManager_ComponentsSelected)
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
                return "Adds a shortcut menu that lists the latest build constructions, so you can easily repeat building them without having to reselect matierals.";
            }
        }
        public static void On_RightClickMenu_Created(Game.GUI.RightClickMenu self)
        {
            var context_menu = (Game.GUI.Controls.ContextMenu)(RightClickMenu_ContextMenu.GetValue(self));
            rebuildMenu = new Game.GUI.Controls.MenuItem("Rebuild");
            rebuildMenu.Enabled = false;
            context_menu.Items.Insert(context_menu.Items.Count - 1, rebuildMenu);
        }
        public static void On_TileSelectionManager_ComponentsSelected(Game.GUI.TileSelectionManager self, GameLibrary.ConstructionID con, List<int> mats)
        {
            if (initiatingJob != null && initiatingJob.Type == self.NextJob)
            {
                initiatingJob.ConstructionID = con;
                initiatingJob.Materials = mats.ToArray();
                Do_UpdateRightClickMenu(initiatingJob);
            }
        }
        public static void On_TileSelectionManager_SetMouseAction(Game.GUI.TileSelectionManager self, Game.JobType job, Game.JobData data, bool multiselect, bool rotatable, bool groundSelect, bool snapToGround)
        {
            initiatingJob = new JobConfiguration()
            {
                Type = job,
                Data = data,
                MultiSelect = multiselect,
                Rotatable = rotatable,
                GroundSelect = groundSelect,
                SnapToGround = snapToGround
            };
            if (data != null)
            {
                data.ToString();
                return;
            }
        }

        private static void Do_UpdateRightClickMenu(JobConfiguration newJob)
        {
            rebuildMenu.Enabled = true;
            for (var i = 0; i < lastJobs.Count; i++)
            {
                var job = lastJobs[i].Configuration;
                if ((job.Type == newJob.Type) && (job.ConstructionID == newJob.ConstructionID) && (job.Materials.SequenceEqual(newJob.Materials)))
                {
                    rebuildMenu.Items.Remove(lastJobs[i].MenuItem);
                    lastJobs.RemoveAt(i);
                }
            }
            while (lastJobs.Count > 5)
            {
                var last = lastJobs.Last();
                rebuildMenu.Items.Remove(last.MenuItem);
                lastJobs.Remove(last);
            }
            var newJobHistory = new JobHistoryItem()
            {
                Configuration = newJob,
                MenuItem = new Game.GUI.Controls.MenuItem(newJob.ToString())
            };
            newJobHistory.MenuItem.Click += new Game.GUI.Controls.EventHandler(RedoMenuItem_Click);

            lastJobs.Insert(0, newJobHistory);
            rebuildMenu.Items.Insert(0, newJobHistory.MenuItem);
        }

        private static void RedoMenuItem_Click(object sender, Game.GUI.Controls.EventArgs e)
        {
            var jobHistory = lastJobs.Single(jhi => jhi.MenuItem == sender);
            var jobConfig = jobHistory.Configuration;

            Game.GnomanEmpire.Instance.Region.TileSelectionManager.SetMouseAction(
                jobConfig.Type,
                jobConfig.Data,
                jobConfig.MultiSelect,
                jobConfig.Rotatable,
                jobConfig.GroundSelect,
                jobConfig.SnapToGround
                );
            var aw = Game.GnomanEmpire.Instance.GuiManager.HUD.ActiveWindow;
            if (aw is Game.GUI.BuildConstructionUI)
            {
                // may add sth similar to "ConstructionPanel"s CanConstruct() ? See also BuildConstructionUI's click handler
                Game.GnomanEmpire.Instance.Region.TileSelectionManager.ComponentsSelected(jobConfig.ConstructionID, jobConfig.Materials.ToList());
                aw.Close();
            }
            else
            {
                throw new Exception("Interesting, this can happen? Pls contact the mod author and tell him what you have done :)");
            }
            /*
            new Game.GUI.Controls.EventHandler((sender, args) =>
            {

            });
            throw new NotImplementedException();
            */
        }


        private static Game.GUI.Controls.MenuItem rebuildMenu;
        private static List<JobHistoryItem> lastJobs = new List<JobHistoryItem>();
        private static JobConfiguration initiatingJob;
        private class JobConfiguration
        {
            public Game.JobType Type;
            public Game.JobData Data;
            public bool MultiSelect;
            public bool Rotatable;
            public bool GroundSelect;
            public bool SnapToGround;

            public GameLibrary.ConstructionID ConstructionID;
            public int[] Materials;

            public override string ToString()
            {
                return Type.ToString() + " " + ConstructionID.ToString() + " (" + Materials.Select(m =>
                {
                    var mat = ((GameLibrary.Material)m);
                    return (mat == GameLibrary.Material.Count) ? "*" : mat.ToString();
                }).Aggregate((m1, m2) => m1 + ", " + m2) + ")";
            }

            public static string JobTypeToString(Game.JobType type)
            {
               
               switch (type)
                {
                    case Game.JobType.BuildWall:
                    case Game.JobType.BuildFloor:
                    case Game.JobType.BuildStairsUp:
                    case Game.JobType.BuildStairsDown:
                    case Game.JobType.BuildRampUp:
                    case Game.JobType.BuildRampDown:
                    case Game.JobType.BuildDoor:
                    case Game.JobType.BuildWell:
                    case Game.JobType.BuildConstruction:
                    case Game.JobType.BuildWorkshop:
                    case Game.JobType.BuildMechanism:
                    case Game.JobType.BuildContainer:
                    case Game.JobType.BuildFurniture:
                    case Game.JobType.BuildContainers:
                    case Game.JobType.BuildWorkshops:
                    case Game.JobType.BuildMechanisms:
                    case Game.JobType.BuildInclineUp:
                    case Game.JobType.BuildInclineDown:
                        return "Build";
                    case Game.JobType.PlantTree:
                    case Game.JobType.PlantSeed:
                        return "Plant";
                    case Game.JobType.ReplaceWall:
                    case Game.JobType.ReplaceFloor:
                        return "Replace";
                    default:
                        return type.ToString();
                }
            }
        }
        private class JobHistoryItem
        {
            public JobConfiguration Configuration;
            public Game.GUI.Controls.MenuItem MenuItem;
        }
    }
#endif
}
