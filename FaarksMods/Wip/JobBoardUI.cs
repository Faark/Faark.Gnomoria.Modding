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
    internal static class DesignationExpands
    {
        /// <summary>
        /// Do stuff for every cell in a designation
        /// </summary>
        /// <param name="self"></param>
        /// <param name="level">Level param is necessary, since Designation has no accessible property and we dont want to use shitty reflection. TODO: Remove level once possible</param>
        /// <param name="func"></param>
        public static void ForAnyCell(this Designation self, int level, Action<MapCell, Vector3> func)
        {
            var map = GnomanEmpire.Instance.Map;
            foreach (var area in self.Areas)
            {
                for (int i = area.Top; i <= area.Bottom; i++)
                {
                    for (int j = area.Left; j <= area.Right; j++)
                    {
                        Vector3 pos = new Vector3(j, i, level);
                        MapCell cell = map.GetCell(pos);
                        func.Invoke(cell, pos);
                    }
                }
            }
        }
    }
    /// <summary>
    /// This is one of the larger mods to date. It adds a screen that lists all existing jobs and make it possible to re-arrange them. It also replaces the current "priority by job type" system of the game, so only prioities matter
    /// 
    /// *THIS MOD IS WORK IN PROGRESS, AND NOT EVEN CLOSE TO BEING FINISHED!*
    /// 
    /// Todo:
    /// - Sync prioities of rearranged jobs back to the old job objects. That not always done, atm, and not always easy.
    /// - Improve the GUI. Add details to the selected job, etc
    /// - Lots of stuff is not saved, unless its written back to game objects.
    /// - I'm not really satisfied with the serialization. Might use the one i'm currently working on that other project?
    /// - The mod currently tries to group similar jobs, to save performance on that "findjob" stuff. That causes some issues where gnomes walk between two groups due to their center distances. => Undo that or find a better way
    /// - I'm sure there are a few new job types since i last maintained that code. Handle them...
    /// There are also a bunch of other "// TODO:"s in that file... :(
    /// 
    /// </summary>
    public class JobBoardUI : Mod
    {
        public static double num() { return SimpleRNG.GetNormal(); }
        public override IEnumerable<IModification> Modifications
        {
            get
            {
                #region JobBoard
                yield return new MethodHook(
                    typeof(JobBoard).GetMethod("AddJob", BindingFlags.Instance | BindingFlags.Public),
                    Method.Of<JobBoard, Job>(JobBoardJobProvider.On_JobBoard_AddJob)
                    );
                yield return new MethodHook(
                    typeof(JobBoard).GetMethod("CancelJob", BindingFlags.Instance | BindingFlags.Public),
                    Method.Of<JobBoard, Job>(JobBoardJobProvider.OnBefore_JobBoard_CancelJob),
                    MethodHookType.RunBefore
                    );
                yield return new MethodHook(
                    typeof(JobBoard).GetMethod("CancelJob", BindingFlags.Instance | BindingFlags.Public),
                    Method.Of<JobBoard, Job>(JobBoardJobProvider.OnAfter_JobBoard_CancelJob),
                    MethodHookType.RunAfter
                    );
                yield return new MethodHook(
                    typeof(JobBoard).GetMethod("ClaimJob", BindingFlags.Instance | BindingFlags.Public),
                    Method.Of<JobBoard, Job, Character>(JobBoardJobProvider.On_JobBoard_ClaimJob),
                    MethodHookType.RunBefore
                    );
                yield return new MethodHook(
                    typeof(JobBoard).GetMethod("CompleteJob", new Type[] { typeof(Job), typeof(Character) }),
                    Method.Of<JobBoard, Job, Character>(JobBoardJobProvider.On_JobBoard_CompleteJob)
                    );
                yield return new MethodHook(
                    typeof(JobBoard).GetMethod("CompleteJob", new Type[] { typeof(int), typeof(Character) }),
                    Method.Of<JobBoard, int, Character>(JobBoardJobProvider.On_JobBoard_CompleteJob)
                    );
                yield return new MethodHook(
                    typeof(JobBoard).GetMethod("OnSerializationComplete", BindingFlags.Instance | BindingFlags.Public),
                    Method.Of<JobBoard>(JobBoardJobProvider.On_JobBoard_OnSerializationComplete)
                    );
                yield return new MethodHook(
                    typeof(JobBoard).GetMethod("RemoveJob", BindingFlags.Instance | BindingFlags.Public),
                    Method.Of<JobBoard, Job>(JobBoardJobProvider.On_JobBoard_RemoveJob)
                    );
                yield return new MethodHook(
                    typeof(TileSelectionManager).GetMethod("HandleInput", BindingFlags.Instance | BindingFlags.Public),
                    Method.Of<TileSelectionManager, float>(JobBoardJobProvider.OnAfter_TileSelectionManager_HandleInput),
                    MethodHookType.RunAfter
                    );
                yield return new MethodHook(
                    typeof(TileSelectionManager).GetMethod("HandleInput", BindingFlags.Instance | BindingFlags.Public),
                    Method.Of<TileSelectionManager, float>(JobBoardJobProvider.OnBefore_TileSelectionManager_HandleInput),
                    MethodHookType.RunBefore
                    );
                #endregion
                #region Hospitals
                yield return  new MethodHook(
                    typeof(RoomManager).GetMethod("CreateRoom", BindingFlags.Instance | BindingFlags.Public),
                    Method.Of<RoomManager, RoomType, Rectangle, int>(Hospitals.On_RoomManager_CreateRoom)
                    );
                yield return  new MethodHook(
                    typeof(RoomManager).GetMethod("RemoveRoom", BindingFlags.Instance | BindingFlags.Public),
                    Method.Of<RoomManager, Room>(Hospitals.On_RoomManager_RemoveRoom)
                    );
                yield return  new MethodHook(
                    typeof(Hospital).GetProperty("Priority").GetSetMethod(),
                    Method.Of<Hospital, int>(Hospitals.OnSet_Room_Priority),
                    MethodHookType.RunBefore
                    );
                #endregion
                #region Workshops
                yield return new MethodHook(
                    typeof(Workshop).GetMethod("OnSpawn", BindingFlags.Instance | BindingFlags.Public),
                    Method.Of<Workshop>(WorkshopJobProvider.On_Workshop_OnSpawn)
                    );
                yield return new MethodHook(
                    typeof(Workshop).GetMethod("OnDelete", BindingFlags.Instance | BindingFlags.Public),
                    Method.Of<Workshop>(WorkshopJobProvider.On_Workshop_OnDelete)
                    );
                yield return new MethodHook(
                    typeof(Workshop).GetProperty("Priority").GetSetMethod(),
                    Method.Of<Workshop, int>(WorkshopJobProvider.OnSet_Workshop_Priority),
                    MethodHookType.RunBefore
                    );
                #endregion
                #region Farms
                yield return new MethodHook(
                    typeof(FarmManager).GetMethod("CreateFarm", BindingFlags.Instance | BindingFlags.Public),
                    Method.Of<FarmManager, Rectangle, int>(Farms.On_FarmManager_CreateFarm)
                    );
                yield return new MethodHook(
                    typeof(FarmManager).GetMethod("RemoveFarm", BindingFlags.Instance | BindingFlags.Public),
                    Method.Of<FarmManager, Farm>(Farms.On_FarmManager_RemoveFarm)
                    );
                yield return new MethodHook(
                    typeof(Farm).GetProperty("Priority").GetSetMethod(),
                    Method.Of<Farm, int>(Farms.OnSet_Farm_Priority),
                    MethodHookType.RunBefore
                    );
                #endregion
                #region Groves
                yield return new MethodHook(
                    typeof(GroveManager).GetMethod("CreateGrove", BindingFlags.Instance | BindingFlags.Public),
                    Method.Of<GroveManager, Rectangle, int>(Groves.On_GroveManager_CreateGrove)
                    );
                yield return new MethodHook(
                    typeof(GroveManager).GetMethod("RemoveGrove", BindingFlags.Instance | BindingFlags.Public),
                    Method.Of<GroveManager, Grove>(Groves.On_GroveManager_RemoveGrove)
                    );
                yield return new MethodHook(
                    typeof(Grove).GetProperty("Priority").GetSetMethod(),
                    Method.Of<Grove, int>(Groves.OnSet_Grove_Priority),
                    MethodHookType.RunBefore
                    );
                #endregion
                #region Pastures
                yield return new MethodHook(
                    typeof(PastureManager).GetMethod("CreatePasture", BindingFlags.Instance | BindingFlags.Public),
                    Method.Of<PastureManager, Rectangle, int>(Ranches.On_PastureManager_CreatePasture)
                    );
                yield return new MethodHook(
                    typeof(PastureManager).GetMethod("RemovePasture", BindingFlags.Instance | BindingFlags.Public),
                    Method.Of<PastureManager, Pasture>(Ranches.On_PastureManager_RemovePasture)
                    );
                yield return new MethodHook(
                    typeof(Pasture).GetProperty("Priority").GetSetMethod(),
                    Method.Of<Pasture, int>(Ranches.OnSet_Pasture_Priority),
                    MethodHookType.RunBefore
                    );
                #endregion
                #region Tinker
                yield return new MethodHook(
                    typeof(TinkerBench).GetMethod("OnSpawn", BindingFlags.Instance | BindingFlags.Public),
                    Method.Of<TinkerBench>(TinkerJobProvider.On_TinkerBench_OnSpawn)
                    );
                yield return new MethodHook(
                    typeof(TinkerBench).GetMethod("OnDelete", BindingFlags.Instance | BindingFlags.Public),
                    Method.Of<TinkerBench>(TinkerJobProvider.On_TinkerBench_OnDelete)
                    );
                yield return new MethodHook(
                    typeof(TinkerBench).GetProperty("Priority").GetSetMethod(),
                    Method.Of<TinkerBench, int>(TinkerJobProvider.OnSet_TinkerBench_Priority),
                    MethodHookType.RunBefore
                    );
                #endregion
                #region Hauling
                yield return new MethodHook(
                    typeof(StockManager).GetMethod("CreateStockpile", BindingFlags.Instance | BindingFlags.Public),
                    Method.Of<StockManager, Rectangle, int>(Stockpiles.On_StockManager_CreateStockpile)
                    );
                yield return new MethodHook(
                    typeof(StockManager).GetMethod("RemoveStockpile", BindingFlags.Instance | BindingFlags.Public),
                    Method.Of<StockManager, Stockpile>(Stockpiles.On_StockManager_RemoveStockpile)
                    );
                yield return new MethodHook(
                    typeof(Stockpile).GetProperty("Priority").GetSetMethod(),
                    Method.Of<Stockpile, int>(Stockpiles.OnSet_Stockpile_Priority),
                    MethodHookType.RunBefore
                    );
                #endregion

                #region top level hooks
                yield return new MethodHook(
                    typeof(Fortress).GetConstructor(new Type[] { }),
                    Method.Of<Fortress>(OnCreate_Fortress)
                    );
                    /*
                yield return new MethodHook(
                    typeof(Fortress).GetConstructor(new Type[] { typeof(System.IO.BinaryReader) }),
                    Method.Action<Fortress, System.IO.BinaryReader>(On_Fortress_OnSerializationComplete)
                    );
#warning having both does not make sense... todo: fix. but first find other bug...
                    */
                yield return new MethodHook(
                    typeof(Fortress).GetMethod("OnSerializationComplete", BindingFlags.Instance | BindingFlags.Public),
                    Method.Of<Fortress>(On_Fortress_OnSerializationComplete)
                    );
                yield return new MethodHook(
                    typeof(Character).GetMethod("FindJob"),
                    typeof(JobBoardUI).GetMethod("On_Character_FindJob", BindingFlags.Public | BindingFlags.Static),
                    MethodHookType.RunBefore,
                    MethodHookFlags.CanSkipOriginal
                    );
                yield return new MethodHook(
                    typeof(KingdomUI).GetConstructor(new Type[] { typeof(Manager) }),
                    Method.Of<KingdomUI, Manager>(KingdomUI_Created)
                    );
                #endregion
                #region Professions that use original job find
                yield return new MethodHook(
                    typeof(PopulationProfessionUI).GetMethod("SetupPanel", BindingFlags.Instance | BindingFlags.Public),
                    Method.Of<PopulationProfessionUI>(ProfessionsThatUseOrignalJobFind.On_PopulationProfessionUI_SetupPanel)
                    );
                // ViewCharacterProfessionUI aka A.cab5d0ce0ee9dc57f23ad33479e5c70f1 ... :(
                yield return new MethodHook(
                    Type.GetType("A.cab5d0ce0ee9dc57f23ad33479e5c70f1, Gnomoria").GetMethod("SetupPanel", BindingFlags.Instance | BindingFlags.Public),
                    typeof(ProfessionsThatUseOrignalJobFind).GetMethod("On_ViewCharacterProfessionUI_SetupPanel", BindingFlags.Public | BindingFlags.Static)
                    );
                yield return new MethodHook(
                    typeof(Profession).GetConstructor(new Type[] { typeof(System.IO.BinaryReader) }),
                    Method.Of<Profession, System.IO.BinaryReader>(ProfessionsThatUseOrignalJobFind.OnCreate_Profession)
                    );
                yield return new MethodHook(
                    typeof(Profession).GetMethod("Serialize", BindingFlags.Instance | BindingFlags.Public),
                    Method.Of<Profession, System.IO.BinaryWriter>(ProfessionsThatUseOrignalJobFind.On_Profession_Serialize)
                    );
                #endregion
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
                return "Adds an overview with all availalbe job providers and changes how the default priority system works.";
            }
        }



        public static void CopyStream(System.IO.Stream input, System.IO.Stream output)
        {
            byte[] buffer = new byte[8 * 1024];
            int len;
            while ((len = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, len);
            }
        }
        public override void PreGameLoaded(ModSaveData data)
        {
            ProfessionsThatUseOrignalJobFind.PreLoad(data.GetData<List<ProfessionsThatUseOrignalJobFind.SavedProfessionReference>>("ExcludedProfessions", null));
            /*
            System.IO.Stream s = null;//var file = GnomanEmpire.Instance.GuiManager.Manager.Skin.ccacb39729210693581588ee5a6149d2b.GetFileStream("fonts/default.xnb");
            using (System.IO.Stream file = System.IO.File.OpenWrite("D:\\Temp\\Font.xnb"))
            {
                CopyStream(s, file);
            }*/
            /*
            var resLoader = new Microsoft.Xna.Framework.Content.ContentManager(GnomanEmpire.Instance.Services);
            resLoader.RootDirectory = "C:\\Users\\Faark\\Documents\\Visual Studio 2010\\Projects\\GnomoriaModding\\Release\\Content";
            var font = resLoader.Load<Microsoft.Xna.Framework.Graphics.SpriteFont>("font");

            
            font.ToString();
            Microsoft.Xna.Framework.Graphics.Texture2D tex = null;
            var writer = System.IO.File.OpenWrite("D:\\Temp\\chars.png");
            tex.SaveAsPng(writer, tex.Width, tex.Height);
            writer.Flush(true);
            writer.Close();
             */
            return;
            //jobCategories = data.GetData("Categories", new TopLevelJobCollection());
            //jobConfigurations = data.GetData("DefaultConfig", new DefaultJobConfig());
        }
        public override void PreGameSaved(ModSaveData data)
        {
            ProfessionsThatUseOrignalJobFind.StartSave();
        }
        public override void AfterGameSaved(ModSaveData data)
        {
            data.SetData("ExcludedProfessions", ProfessionsThatUseOrignalJobFind.GetToSave());
            //data.SetData("Categories", jobCategories);
            //data.SetData("DefaultConfig", jobConfigurations);
        }


        #region Job Providers
        [DataContract]
        private class SerializableGameEntity
        {
            [DataMember(Name = "EntityId")]
            public string EntityId;
            [DataMember(Name = "EntityModData")]
            public Dictionary<string, string> EntityData = new Dictionary<string, string>();
            public string Data
            {
                get
                {
                    return EntityData["default"];
                }
            }
            public SerializableGameEntity(string id, string data = null)
            {
                EntityId = id;
                if (data != null)
                {
                    EntityData["default"] = data;
                }
            }
        }
        public interface IJob
        {
            //ISerializedJob GetSerializeableJob();
            CharacterSkillType UsedSkill { get; }
            string Text { get; }
            Vector3 Position { get; }
            bool Suspended { get; set; }
            JobCollection Parent { get; set; }
            Job TryGetJob(Character gnome);
            //DetailsPanel GetDetailsPanel(Manager mgr);
        }
        public abstract class JobProvider : IJob
        {
            public CharacterSkillType UsedSkill { get; set; }
            public abstract Job TryGetJob(Character gnome);
            public JobProvider(CharacterSkillType skill)
            {
                UsedSkill = skill;
            }
            public abstract string Text { get; }
            public abstract Vector3 Position { get; }
            public abstract bool Suspended { get; set; }

            private JobCollection mParent = null;
            public virtual JobCollection Parent
            {
                get
                {
                    return mParent;
                }
                set
                {
                    if (mParent != null)
                    {
                        mParent.SubItems.Remove(this);
                    }
                    mParent = value;
                    if (mParent != null)
                    {
                        mParent.SubItems.Add(this);
                    }
                }
            }

            public static void Helper_Add<T>(List<T> list, T newJP, int priority) where T : JobProvider
            {
                list.Add(newJP);
                jobCategories.AddJobProvider(newJP, priority);
            }
            public static void Helper_AddIfNotExistant<T>(List<T> list, Func<T, bool> checkFunc, T newInCaseOfAdd, int priority) where T : JobProvider
            {
                var contains = list.SingleOrDefault(checkFunc);
                if (contains == null)
                {
                    list.Add(newInCaseOfAdd);
                    jobCategories.AddJobProvider(newInCaseOfAdd, priority);
                }
            }
            public static void Helper_AddMissing<T, T2>(IEnumerable<T2> anyOfList, IEnumerable<T2> notInList, List<T> listToAdd, Func<T2, Tuple<T, int>> newJobProviderFactory) where T : JobProvider
            {
                foreach (var missing in anyOfList.Where(i => !notInList.Contains(i)))
                {
                    var addData = newJobProviderFactory(missing);
                    Helper_Add(listToAdd, addData.Item1, addData.Item2);
                }
            }
            public static void Helper_RemoveIfExistant<T>(List<T> list, Func<T, bool> checkFunc) where T : JobProvider
            {
                list.RemoveAll(el =>
                {
                    var i = checkFunc(el);
                    if (i)
                    {
                        el.Parent = null;
                    }
                    return i;
                });
                /*
                foreach (var toRemove in list.Where(checkFunc))
                {
                    list.Remove(toRemove);
                    jobCategories.RemoveJobProvider(toRemove);
                    return;
                }*/
            }
            public static TopLevelJobCollection.PriorityJobCollection Helper_GetPriorityCollection(IJob jp)
            {
                if (jp.Parent == null)
                    return null;
                do
                {
                    jp = jp.Parent;
                } while (jp.Parent != null);
                return jp as TopLevelJobCollection.PriorityJobCollection;
            }
            public static void Helper_AdjustPriority(IEnumerable<JobProvider> jpl, int newPrio)
            {
                foreach (var jp in jpl)
                {
                    var priority = Helper_GetPriorityCollection(jp);
                    if ((priority == null) || (priority.Priority != newPrio))
                    {
                        jobCategories.AddJobProvider(jp, newPrio);
                    }
                    /*
                    var current_prio = jobCategories.priorities.SingleOrDefault(prio => prio.ContainsJobProvider(jp));
                    if (current_prio != null)
                    {
                        if (current_prio.Priority != newPrio)
                        {
                            current_prio.RemoveJobProvider(jp);
                            jobCategories.AddJobProvider(jp, newPrio);
                        }
                    }
                    else
                    {
                        jobCategories.AddJobProvider(jp, newPrio);
                    }*/
                }
            }
        }
        #region JobBoard
        public class JobBoardJobProvider : JobProvider
        {
            // TODO: In case we ever entirely replace JobBoard we have to double-check that everything is done using this provider. Currently lots of stuff is sill done by JobBoard!
            /*Rules: We just group similar jobs into the same provider!*/
            List<Job> SubJobs = new List<Job>();
            List<Job> SubJobsInProgress = new List<Job>();
            /*public JobBoardJobProvider() : base(CharacterSkillType.Count) { }*/
            Job debug_initialJob; //TODO: debug, remove once bug found
            public JobBoardJobProvider(Job job, bool isInProgress = false)
                : base(job.SkillUsed)
            {
                debug_initialJob = job;
                AddJob(job, isInProgress);
            }
            public override Job TryGetJob(Character gnome)
            {
                if ((SubJobs.Count > 0) && (gnome.Mind.AllowedToPerformJob(SubJobs[0])))
                {
                    foreach (var job in SubJobs.OrderBy(j => Map.DistanceSquaredWithBias(gnome.Position, j.Position)))
                    {
                        if (!job.IsSuspended() && (!job.ToolRequired() || gnome.Body.HasItem(job.ToolID) || GnomanEmpire.Instance.Fortress.StockManager.IsItemAvailable(gnome.Position, job.ToolID, 80)) && job.AllComponentsExist(gnome) && job.PathTo(gnome) && GnomanEmpire.Instance.Fortress.JobBoard.ClaimJob(job, gnome))
                        {
                            return job;
                        }
                    }
                }
                return null;
            }
            public override String Text
            {
                get
                {
                    var j = SubJobs.Count > 0 ? SubJobs[0] : SubJobsInProgress[0];
                    return (SubJobs.Count + SubJobsInProgress.Count) + "x " + j.JobActionName();
                }
            }
            public override Vector3 Position
            {
                get
                {
                    // Todo: update only on add, not on remove is fine? Okay then remove this after considering...
                    if (!center.HasValue)
                    {
                        var vecs = SubJobs.Union(SubJobsInProgress)
                            .Select(j => Tuple.Create(j.Position, j.Position))
                            .Aggregate((vecs1, vecs2) => Tuple.Create(Vector3.Min(vecs1.Item1, vecs2.Item1), Vector3.Max(vecs1.Item2, vecs2.Item2)));
                        center = vecs.Item1 + ((vecs.Item2 - vecs.Item1) / 2);
                    }
                    return center.Value;
                }
            }
            public override bool Suspended { get; set; }
            private Vector3? center = null;
            public void AddJob(Job job, bool isInProgress = false)
            {
                if (isCancel)
                    return;
                center = null;
                if (SubJobs.Contains(job) || SubJobsInProgress.Contains(job))
                {
                    throw new Exception("This should not happen as well!");
                }
                if (jobLog.ContainsKey(job))
                {
                    switch (jobLog[job].Last().Item2)
                    {
                        default:
                            jobLog.ToArray();
                            break;
                    }
                }
                jobLibrary[job] = this;
                AddLog(job, "Added " + isInProgress.ToString(), this);
                if (isInProgress)
                {
                    SubJobsInProgress.Add(job);
                }
                else
                {
                    SubJobs.Add(job);
                }
            }
            public void ClaimJob(Job job)
            {
                if (job != null)
                {
                    SubJobsInProgress.Add(job);
                    SubJobs.Remove(job);
                }
                AddLog(job, "Claimed", this);
            }
            private static bool isCancel;
            public void CancelJob(Job job)
            {
                if (SubJobsInProgress.Remove(job))
                {
                    SubJobs.Add(job);
                }
                else
                {
                    return;
                }
                AddLog(job, "Canceled",this);
            }
            // TODO: This is log, filling up memory. Remove it asap once everything works as intended.
            private static Dictionary<Job, List<Tuple<DateTime, String, int, int>>> jobLog = new Dictionary<Job, List<Tuple<DateTime, string, int, int>>>();
            private static void AddLog(Job j, String t, JobProvider jp)
            {
                if (!jobLog.ContainsKey(j))
                {
                    jobLog[j] = new List<Tuple<DateTime, string, int, int>>();
                }
                jobLog[j].Add(Tuple.Create(DateTime.Now, t, jp.GetHashCode(), jobLibrary[j].GetHashCode()));
            }
            public void CompleteJob(Job job)
            {
                AddLog(job, "Completed", this);
                SubJobsInProgress.Remove(job);
                if (SubJobs.Contains(job))
                {
                    throw new Exception("Schouldn't happne?");
                }
                jobLibrary.Remove(job);
                MayAllCompleted();
            }
            public void RemoveJob(Job job)
            {
                AddLog(job, "Removed", this);
                SubJobsInProgress.Remove(job);
                SubJobs.Remove(job);
                jobLibrary.Remove(job);
                MayAllCompleted();
            }
            public void MayAllCompleted()
            {
                if ((SubJobs.Count == 0) && (SubJobsInProgress.Count == 0))
                {
                    Helper_RemoveIfExistant(existing, jp => jp == this);
                }
            }

            private class CanJobPePartOfJobComponentComparer : IEqualityComparer<JobComponent>
            {
                public bool Equals(JobComponent j1, JobComponent j2)
                {
                    return j1.Material == j2.Material && j1.ItemID == j2.ItemID;
                }
                public int GetHashCode(JobComponent j)
                {
                    return (((int)j.ItemID * 65536) & j.Material).GetHashCode();
                }
            }
            private static CanJobPePartOfJobComponentComparer CanJobPePartOfJobComponentComparerInstance = null;
            public bool CanJobBePartOf(Job job)
            {
                if (CanJobPePartOfJobComponentComparerInstance == null)
                {
                    CanJobPePartOfJobComponentComparerInstance = new CanJobPePartOfJobComponentComparer();
                }
                var jt = job.GetType();
                var any = SubJobs.Count > 0 ? SubJobs[0] : SubJobsInProgress[0];
                if (any.GetType() == jt
                    && any.Type == job.Type
                    && any.ToolID == job.ToolID
                    && any.SkillUsed == job.SkillUsed
                    && any.RequiredComponents.SequenceEqual(job.RequiredComponents, CanJobPePartOfJobComponentComparerInstance)
                    && any.AttributeUsed == job.AttributeUsed
                    )
                {
                    // Todo: Looks liek we have to add "is equal"-checks for every single thing here *puke*. Hopefully just something like "Construction.ConstructionID"?
                    if ((jt == typeof(MineJob)))
                    {
                    }
                    else
                    {
                        jt.ToString();
                    }
                    return true;
                    /*
                     * dont need this anymore, since we look up the neighboring job providers anyway...
                    foreach (var other in SubJobs.Union(SubJobsInProgress))
                    {
                        if ((
                                (job.Position.X == other.Position.X)
                                && (
                                    (job.Position.Y == (other.Position.Y - 1))
                                    || (job.Position.Y == (other.Position.Y + 1))
                                )
                            ) || (
                                (job.Position.Y == other.Position.Y)
                                && (
                                    (job.Position.X == (other.Position.X - 1))
                                    || (job.Position.X == (other.Position.X + 1))
                                )
                            ))
                        {
                            return true;
                        }
                    }*/
                }
                else
                {
                    return false;
                }
            }

            public static List<JobBoardJobProvider> existing = new List<JobBoardJobProvider>();
            public static JobBoardJobProvider current_provider = null;
            public static Dictionary<Job, JobBoardJobProvider> jobLibrary = new Dictionary<Job, JobBoardJobProvider>();
            private static bool is_collection = false;

            public static void On_JobBoard_AddJob(JobBoard self, Job job)
            {
                if (isCancel)
                    return;
                if (is_collection)
                {
                    if (current_provider == null)
                    {
                        current_provider = new JobBoardJobProvider(job);
                    }
                    else
                    {
                        current_provider.AddJob(job);
                    }
                }
                else
                {
                    // Todo: those kind of jobs (like supplying engines) should be an permanent jobprovider that allows you prioritize it!
                    JobProvider.Helper_Add(existing, new JobBoardJobProvider(job), jobConfigurations.Get(jobConfigurations.CustomCommand, 1));
                }
            }
            public static void OnBefore_JobBoard_CancelJob(JobBoard self, Job job)
            {
                isCancel = true;
            }
            public static void OnAfter_JobBoard_CancelJob(JobBoard self, Job job)
            {
                isCancel = false;
                JobBoardJobProvider jprov;
                if (jobLibrary.TryGetValue(job, out jprov))
                {
                    jprov.CancelJob(job);
                }
            }
            public static void On_JobBoard_ClaimJob(JobBoard self, Job job, Character gnome)
            {
                jobLibrary[job].ClaimJob(job);
            }
            public static void On_JobBoard_CompleteJob(JobBoard self, Job job, Character gnome)
            {
                //looks like tihs func is always called, regardless if this job is actually managed by the job board or not
                JobBoardJobProvider myProv;
                if (jobLibrary.TryGetValue(job, out myProv))
                {
                    myProv.CompleteJob(job);
                }
            }
            public static void On_JobBoard_CompleteJob(JobBoard self, int jobIndex, Character gnome)
            {
                throw new NotImplementedException("This function should be unused");
            }
            //TODO: switch to lookup via neighbour cells & dict!
            //private static Dictionary<MapCell, Job>
            public static void On_JobBoard_OnSerializationComplete(JobBoard self)
            {
                //var jobToProvider = 
                if ((self.Jobs.Count + self.InProgressJobs.Count) > 0)
                {
                    var allJobsToProcess = new Dictionary<Vector3, List<Tuple<Job, bool>>>(self.Jobs.Count + self.InProgressJobs.Count);
                    Action<Job, bool, Dictionary<Vector3, List<Tuple<Job, bool>>>> add = (job, inProg, allJobs) =>
                    {
                        List<Tuple<Job, bool>> list;
                        if (!allJobsToProcess.TryGetValue(job.Position, out list))
                        {
                            allJobsToProcess[job.Position] = list = new List<Tuple<Job, bool>>();
                        }
                        list.Add(Tuple.Create(job, inProg));
                    };
                    foreach (var job in self.Jobs)
                    {
                        add(job, false, allJobsToProcess);
                    }
                    foreach (var job in self.InProgressJobs)
                    {
                        add(job, true, allJobsToProcess);
                    }

                    while (allJobsToProcess.Count > 0)
                    {
                        var currentKvp_PosJobs = allJobsToProcess.First();
                        allJobsToProcess.Remove(currentKvp_PosJobs.Key);
                        foreach (var jobEl in currentKvp_PosJobs.Value)
                        {
                            JobBoardJobProvider jbp = new JobBoardJobProvider(jobEl.Item1, jobEl.Item2);
                            Helper_Add(existing, jbp, jobConfigurations.Get(jobConfigurations.CustomCommand, 1));
                            var currentPositionsToProcess = new Stack<Vector3>();
                            currentPositionsToProcess.Push(currentKvp_PosJobs.Key);

                            while (currentPositionsToProcess.Count > 0)
                            {
                                var currentPos = currentPositionsToProcess.Pop();
                                for (var x = -1; x < 2; x++)
                                {
                                    for (var y = -1; y < 2; y++)
                                    {
                                        for (var z = -1; z < 2; z++)
                                        {
                                            List<Tuple<Job, bool>> jobsAtPosition;
                                            var currentNeigborPos = currentPos + new Vector3(x, y, z);
                                            if (allJobsToProcess.TryGetValue(currentNeigborPos, out jobsAtPosition))
                                            {
                                                for (var i = jobsAtPosition.Count - 1; i >= 0; i--)
                                                {
                                                    var trgJob = jobsAtPosition[i];
                                                    if (jbp.CanJobBePartOf(trgJob.Item1))
                                                    {
                                                        jbp.AddJob(trgJob.Item1, trgJob.Item2);
                                                        allJobsToProcess.Remove(currentNeigborPos);
                                                        currentPositionsToProcess.Push(currentNeigborPos);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }


                        /*
                        currentPassJobsToProcess.Push(currentKvp_PosJob);
                        while (currentPassJobsToProcess.Count > 0)
                        {
                            var currentPassJob = currentPassJobsToProcess.Pop();
                        }*/
                    }
                    /*
                    foreach (var job in self.Jobs.Union(self.InProgressJobs))
                    {
                        var nCells = new List<MapCell>();
                        for (var x = -1; x < 2; x++)
                        {
                            for (var y = -1; y < 2; y++)
                            {
                                for (var z = -1; z < 2; z++)
                                {
                                    nCells.Add(GnomanEmpire.Instance.Map.GetCell(job.Position + new Vector3(x, y, z)));
                                }
                            }
                        }
                        bool gotGroupAssoc = false;
                        JobBoardJobProvider jbp;
                        foreach (var cell in nCells)
                        {
                            if ((cell != null) && (cell.ProposedJob != null) && (jobLibrary.TryGetValue(cell.ProposedJob, out jbp)))
                            {
                                if (jbp.CanJobBePartOf(job))
                                {
                                    jbp.AddJob(job, self.InProgressJobs.Contains(job));
                                    gotGroupAssoc = true;
                                    break;
                                }
                            }
                        }
                        if (!gotGroupAssoc)
                        {
                            Helper_Add(existing, new JobBoardJobProvider(job, self.InProgressJobs.Contains(job)), jobConfigurations.Get(jobConfigurations.CustomCommand, 1));
                        }
                    }*/
                }
                return;
                /*
                 * Possible cases:
                 * - we have custom data somewere else. Load it and build stuff accordingly
                 * - we do not have any mod-save-data. Try to group everything and insert to default priority
                 */
            }   
            public static void On_JobBoard_RemoveJob(JobBoard self, Job job)
            {
                JobBoardJobProvider jprov;
                if (jobLibrary.TryGetValue(job, out jprov))
                {
                    jprov.RemoveJob(job);
                }
            }

            public static void OnBefore_TileSelectionManager_HandleInput(TileSelectionManager self, float dt)
            {
                is_collection = true;
            }
            public static void OnAfter_TileSelectionManager_HandleInput(TileSelectionManager self, float dt)
            {
                if (current_provider != null)
                {
                    JobProvider.Helper_Add(existing, current_provider, jobConfigurations.Get(jobConfigurations.CustomCommand, 1));
                    current_provider = null;
                }
                is_collection = false;
            }
        }
        #endregion
        #region Hospitals
        public static class Hospitals
        {
            [DataContract]
            public abstract class HospitalJobProvider : JobProvider
            {
                public Hospital Hospital;
                public HospitalJobProvider(Hospital h, CharacterSkillType type = CharacterSkillType.Medic)
                    : base(type)
                {
                    Hospital = h;
                }
                public override Vector3 Position
                {
                    get { return Hospital.CenterPosition(); }
                }
                public override bool Suspended
                {
                    get
                    {
                        return Hospital.Suspended;
                    }
                    set
                    {
                        Hospital.Suspended = value;
                    }
                }
            }
            public class HealPatientJobProvider : HospitalJobProvider
            {
                public static List<HealPatientJobProvider> existing = new List<HealPatientJobProvider>();
                public HealPatientJobProvider(Hospital h) : base(h) { }
                public override Job TryGetJob(Character gnome)
                {
                    var map = GnomanEmpire.Instance.Map;
                    if (!Hospital.Suspended)
                    {
                        foreach (Construction current2 in Hospital.Furniture)
                        {
                            if (current2.Claimed)
                            {
                                MapCell cell = map.GetCell(current2.Position);
                                if (cell.ProposedJob == null && (current2.ClaimedBy.Body.NeedsCast() || current2.ClaimedBy.Body.NeedsBandage()) && current2.ClaimedBy != gnome)
                                {
                                    StockManager stockManager = GnomanEmpire.Instance.Fortress.StockManager;
                                    Item item = stockManager.FindClosestItem(gnome.Position, ItemID.Bandage, 80, ItemQuality.Poor, true, false);
                                    if (item != null)
                                    {
                                        HealPatientJob healPatientJob = new HealPatientJob(current2.Position);
                                        if (healPatientJob.PathTo(gnome))
                                        {
                                            healPatientJob.RequiredComponents.Add(new JobComponent(item.ItemID, item.MaterialID));
                                            return healPatientJob;
                                        }
                                        healPatientJob.Clear();
                                    }
                                }
                            }
                            else
                            {
                                MapCell cell2 = map.GetCell(current2.Position);
                                if (cell2.ProposedJob != null && cell2.ProposedJob is HealPatientJob)
                                {
                                    cell2.ProposedJob.Clear();
                                }
                            }
                        }
                    }
                    return null;
                }
                public override String Text
                {
                    get
                    {
                        return "Treat patients at hospital \"" + Hospital.Name + "\"";
                    }
                }
            }
            public class FeedPatientJobProvider : HospitalJobProvider
            {
                public static List<FeedPatientJobProvider> existing = new List<FeedPatientJobProvider>();
                public FeedPatientJobProvider(Hospital h) : base(h, CharacterSkillType.Caretaker) { }
                private static Item FindItemToFeed(Character gnome, ItemEffectType type)
                {
                    if (gnome.FindEntity(type))
                    {
                        var res = gnome.CurrentNeedGoal as Item;
                        gnome.CurrentNeedGoal = null;
                        return res;
                    }
                    return null;
                }
                public override Job TryGetJob(Character gnome)
                {
                    var map = GnomanEmpire.Instance.Map;
                    if (!Hospital.Suspended)
                    {
                        foreach (Construction current2 in Hospital.Furniture)
                        {
                            if (current2.Claimed)
                            {
                                MapCell cell = map.GetCell(current2.Position);
                                if (cell.ProposedJob == null && (current2.ClaimedBy.Body.IsHungry || current2.ClaimedBy.Body.IsThirsty) && current2.ClaimedBy != gnome)
                                {
                                    StockManager expr_153 = GnomanEmpire.Instance.Fortress.StockManager;
                                    Item item = null;
                                    var trgBody = current2.ClaimedBy.Body;
                                    if (trgBody.IsDyingOfThirst || trgBody.IsStarving)
                                    {
                                        if (trgBody.IsDyingOfThirst)
                                        {
                                            item = FindItemToFeed(gnome, ItemEffectType.Drink);
                                        }
                                        if (trgBody.IsStarving && (item == null))
                                        {
                                            item = FindItemToFeed(gnome, ItemEffectType.Food);
                                        }
                                    }
                                    else
                                    {
                                        if (trgBody.IsThirsty)
                                        {
                                            item = FindItemToFeed(gnome, ItemEffectType.Drink);
                                        }
                                        if (trgBody.IsHungry && (item == null))
                                        {
                                            item = FindItemToFeed(gnome, ItemEffectType.Food);
                                        }
                                    }
                                    if (item != null)
                                    {
                                        FeedPatientJob feedPatientJob = new FeedPatientJob(current2.Position);
                                        if (feedPatientJob.PathTo(gnome))
                                        {
                                            feedPatientJob.RequiredComponents.Add(new JobComponent(item.ItemID, item.MaterialID));
                                            return feedPatientJob;
                                        }
                                        feedPatientJob.Clear();
                                    }
                                }
                            }
                            else
                            {
                                MapCell cell2 = map.GetCell(current2.Position);
                                if (cell2.ProposedJob != null && cell2.ProposedJob is HealPatientJob)
                                {
                                    cell2.ProposedJob.Clear();
                                }
                            }
                        }
                    }
                    return null;
                }
                public override String Text
                {
                    get
                    {
                        return "Take care of patients at hospital \"" + Hospital.Name + "\"";
                    }
                }
            }

            public static void On_RoomManager_CreateRoom(RoomManager self, RoomType type, Rectangle rect, int level)
            {

                List<CommunityRoom> hospitals;
                if ((type == RoomType.Hospital) && self.CommunityRooms.TryGetValue(RoomType.Hospital, out hospitals))
                {


                    JobProvider.Helper_AddMissing(
                        hospitals,
                        HealPatientJobProvider.existing.Select(hjp => hjp.Hospital),
                        HealPatientJobProvider.existing,
                        h =>
                        {
                            var ho = (Hospital)h;
                            return Tuple.Create(new HealPatientJobProvider(ho), jobConfigurations.Get(jobConfigurations.Hospitals_Heal, ho.Priority));
                        });
                    JobProvider.Helper_AddMissing(
                         hospitals,
                         FeedPatientJobProvider.existing.Select(hjp => hjp.Hospital),
                         FeedPatientJobProvider.existing,
                         h =>
                         {
                             var ho = (Hospital)h;
                             return Tuple.Create(new FeedPatientJobProvider(ho), jobConfigurations.Get(jobConfigurations.Hospitals_Feed, ho.Priority));
                         });
                }
            }
            public static void On_RoomManager_RemoveRoom(RoomManager self, Room room)
            {
                if (room is Hospital)
                {
                    JobProvider.Helper_RemoveIfExistant(HealPatientJobProvider.existing, jp => jp.Hospital == room);
                    JobProvider.Helper_RemoveIfExistant(FeedPatientJobProvider.existing, jp => jp.Hospital == room);
                }
            }
            public static void OnSet_Room_Priority(Hospital self, int prio)
            {
                JobProvider.Helper_AdjustPriority(HealPatientJobProvider.existing.Where(jp => jp.Hospital == self), prio);
                JobProvider.Helper_AdjustPriority(FeedPatientJobProvider.existing.Where(jp => jp.Hospital == self), prio);
            }
        }
        #endregion
        #region Workshops
        public class WorkshopJobProvider : JobProvider
        {
            public Workshop Workshop;
            public WorkshopJobProvider(Workshop w)
                : base(CharacterSkillType.Count)
            {
                Workshop = w;
            }

            /*static Dictionary<WorkshopJobProvider, long> jobs = new Dictionary<WorkshopJobProvider, long>();

            Tuple<long,Workshop>[] jobList()
            {
                return jobs.OrderByDescending(kvp => kvp.Value).Select(kvp => Tuple.Create(kvp.Value, kvp.Key.Workshop)).ToArray();
            }*/
            public override Job TryGetJob(Character gnome)
            {
                if ((!Workshop.HasAssignedWorker || Workshop.Worker == gnome) && !(Workshop is TinkerBench))
                {
                    /*
                    if( !jobs.ContainsKey(this) ){
                        jobs[this] = 0;
                    }
                    var sw = new System.Diagnostics.Stopwatch();
                    sw.Start();*/
                    Job job = Workshop.GetJob(gnome);
                    if (job != null && gnome.Mind.IsSkillAllowed(job.SkillUsed) && job.AllComponentsExist(gnome) && job.PathTo(gnome))
                    {
                        //sw.Stop();
                        //jobs[this] += sw.ElapsedTicks;
                        return job;
                    }
                    //sw.Stop();
                    //jobs[this] += sw.ElapsedTicks;
                }
                return null;
            }
            public override String Text
            {
                get
                {
                    return "Work at " + Workshop.Name();
                }
            }
            public override Vector3 Position
            {
                get { return Workshop.CraftPos(); }
            }
            public override bool Suspended
            {
                get
                {
                    return Workshop.Suspended;
                }
                set
                {
                    Workshop.Suspended = value;
                }
            }

            public static List<WorkshopJobProvider> existing = new List<WorkshopJobProvider>();
            public static void On_Workshop_OnSpawn(Workshop self)
            {
                JobProvider.Helper_Add(existing, new WorkshopJobProvider(self), jobConfigurations.Get(jobConfigurations.Workshop, self.Priority));
            }
            public static void On_Workshop_OnDelete(Workshop self)
            {
                JobProvider.Helper_RemoveIfExistant(existing, jp => jp.Workshop == self);
            }
            public static void OnSet_Workshop_Priority(Workshop self, int prio)
            {
                Helper_AdjustPriority(existing.Where(jp => jp.Workshop == self), prio);
            }
        }
        #endregion
        #region Farms
        public static class Farms
        {
            public abstract class FarmJobProvider : JobProvider
            {
                public Farm Farm;
                public FarmJobProvider(Farm f)
                    : base(CharacterSkillType.Farming)
                {
                    Farm = f;
                }
                public override Vector3 Position
                {
                    get { return Farm.CenterPosition(); }
                }
                public override bool Suspended
                {
                    get { return Farm.Suspended; }
                    set { Farm.Suspended = value; }
                }
            }
            public class HarvestFarmJobProvider : FarmJobProvider
            {
                public static List<HarvestFarmJobProvider> existing = new List<HarvestFarmJobProvider>();
                public HarvestFarmJobProvider(Farm f) : base(f) { }
                public override Job TryGetJob(Character gnome)
                {
                    Vector3 vector = Farm.HarvestSpot();
                    if (vector != -Vector3.One)
                    {
                        ForageJob forageJob = new ForageJob(vector);
                        if (forageJob.PathTo(gnome))
                        {
                            return forageJob;
                        }
                        forageJob.Clear();
                    }
                    return null;
                }
                public override String Text
                {
                    get
                    {
                        return "Harvest crop at farm \"" + Farm.Name + "\"";
                    }
                }
            }
            public class PlantSeedJobProvider : FarmJobProvider
            {
                public static List<PlantSeedJobProvider> existing = new List<PlantSeedJobProvider>();
                public PlantSeedJobProvider(Farm f) : base(f) { }
                public override Job TryGetJob(Character gnome)
                {
                    Vector3 vector = Farm.UnplantedSpot();
                    if (vector != -Vector3.One && Farm.SeedMaterial != -1)
                    {
                        StockManager stockManager = GnomanEmpire.Instance.Fortress.StockManager;
                        Item item = stockManager.FindClosestItem(gnome.Position, ItemID.Seed, Farm.SeedMaterial, ItemQuality.Poor, true, false);
                        if (item != null)
                        {
                            PlantSeedJob plantSeedJob = new PlantSeedJob(vector);
                            if (plantSeedJob.PathTo(gnome))
                            {
                                plantSeedJob.RequiredComponents.Add(new JobComponent(item.ItemID, item.MaterialID));
                                return plantSeedJob;
                            }
                            plantSeedJob.Clear();
                        }
                    }
                    return null;
                }
                public override String Text
                {
                    get
                    {
                        return "Plant seeds at farm \"" + Farm.Name + "\"";
                    }
                }
            }
            public class TillSoilJobProvider : FarmJobProvider
            {
                public static List<TillSoilJobProvider> existing = new List<TillSoilJobProvider>();
                public TillSoilJobProvider(Farm f) : base(f) { }
                public override Job TryGetJob(Character gnome)
                {
                    Vector3 vector = Farm.UntilledSpot();
                    if (vector != -Vector3.One)
                    {
                        TillSoilJob tillSoilJob = new TillSoilJob(vector);
                        if (tillSoilJob.PathTo(gnome))
                        {
                            return tillSoilJob;
                        }
                        tillSoilJob.Clear();
                    }
                    return null;
                }
                public override String Text
                {
                    get
                    {
                        return "Till soil for farm \"" + Farm.Name + "\"";
                    }
                }
            }
            public static void On_FarmManager_CreateFarm(FarmManager self, Rectangle rect, int level)
            {
                JobProvider.Helper_AddMissing(
                    self.Farms,
                    HarvestFarmJobProvider.existing.Select(x => x.Farm),
                    HarvestFarmJobProvider.existing,
                    farm => Tuple.Create(new HarvestFarmJobProvider(farm), jobConfigurations.Get(jobConfigurations.Farms_Harvest, farm.Priority))
                    );
                JobProvider.Helper_AddMissing(
                    self.Farms,
                    PlantSeedJobProvider.existing.Select(x => x.Farm),
                    PlantSeedJobProvider.existing,
                    farm => Tuple.Create(new PlantSeedJobProvider(farm), jobConfigurations.Get(jobConfigurations.Farms_Plant, farm.Priority))
                    );
                JobProvider.Helper_AddMissing(
                    self.Farms,
                    TillSoilJobProvider.existing.Select(x => x.Farm),
                    TillSoilJobProvider.existing,
                    farm => Tuple.Create(new TillSoilJobProvider(farm), jobConfigurations.Get(jobConfigurations.Farms_Tile, farm.Priority))
                    );
            }
            public static void On_FarmManager_RemoveFarm(FarmManager self, Farm farm)
            {
                JobProvider.Helper_RemoveIfExistant(HarvestFarmJobProvider.existing, jp => jp.Farm == farm);
                JobProvider.Helper_RemoveIfExistant(PlantSeedJobProvider.existing, jp => jp.Farm == farm);
                JobProvider.Helper_RemoveIfExistant(TillSoilJobProvider.existing, jp => jp.Farm == farm);
            }
            public static void OnSet_Farm_Priority(Farm self, int prio)
            {
                JobProvider.Helper_AdjustPriority(HarvestFarmJobProvider.existing.Where(jp => jp.Farm == self), prio);
                JobProvider.Helper_AdjustPriority(PlantSeedJobProvider.existing.Where(jp => jp.Farm == self), prio);
                JobProvider.Helper_AdjustPriority(TillSoilJobProvider.existing.Where(jp => jp.Farm == self), prio);
            }
        }
        #endregion
        #region Groves
        public static class Groves
        {
            public abstract class GroveJobProvider : JobProvider
            {
                public Grove Grove;
                public GroveJobProvider(Grove g, CharacterSkillType type = CharacterSkillType.Horticulture)
                    : base(type)
                {
                    Grove = g;
                }
                public override Vector3 Position
                {
                    // Todo: general perf suggestion: Currently Designation.CenterPosition() is calced every time. What about caching it?!
                    get { return Grove.CenterPosition(); }
                }
                public override bool Suspended
                {
                    get
                    {
                        return Grove.Suspended;
                    }
                    set
                    {
                        Grove.Suspended = value;
                    }
                }
            }
            public class PlantTreeJobProvider : GroveJobProvider
            {
                public static List<PlantTreeJobProvider> existing = new List<PlantTreeJobProvider>();
                public PlantTreeJobProvider(Grove g) : base(g) { }
                public override Job TryGetJob(Character gnome)
                {

                    Vector3 vector = Grove.PlantSpot();
                    if (vector != -Vector3.One && Grove.ClippingMaterial != -1 && GnomanEmpire.Instance.Fortress.StockManager.IsItemAvailable(gnome.Position, ItemID.Clipping, Grove.ClippingMaterial, ItemQuality.Poor, true))
                    {
                        PlantTreeJob plantTreeJob = new PlantTreeJob(vector);
                        if (plantTreeJob.PathTo(gnome))
                        {
                            plantTreeJob.RequiredComponents.Add(new JobComponent(ItemID.Clipping, Grove.ClippingMaterial));
                            return plantTreeJob;
                        }
                        plantTreeJob.Clear();
                    }
                    return null;
                }
                public override String Text
                {
                    get
                    {
                        return "Plant tree at grove \"" + Grove.Name + "\"";
                    }
                }
            }
            public class TakeClippingJobProvider : GroveJobProvider
            {
                public static List<TakeClippingJobProvider> existing = new List<TakeClippingJobProvider>();
                public TakeClippingJobProvider(Grove g) : base(g) { }
                public override Job TryGetJob(Character gnome)
                {
                    Vector3 vector = Grove.ClipSpot();
                    if (vector != -Vector3.One)
                    {
                        TakeClippingJob takeClippingJob = new TakeClippingJob(vector);
                        if (takeClippingJob.PathTo(gnome))
                        {
                            return takeClippingJob;
                        }
                        takeClippingJob.Clear();
                    }
                    return null;
                }
                public override String Text
                {
                    get
                    {
                        return "Take clippings at grove \"" + Grove.Name + "\"";
                    }
                }
            }
            public class ForageJobProvider : GroveJobProvider
            {
                public static List<ForageJobProvider> existing = new List<ForageJobProvider>();
                public ForageJobProvider(Grove g) : base(g, CharacterSkillType.Farming) { }
                public override Job TryGetJob(Character gnome)
                {
                    Vector3 vector = Grove.HarvestSpot();
                    if (vector != -Vector3.One)
                    {
                        ForageJob forageJob = new ForageJob(vector);
                        if (forageJob.PathTo(gnome))
                        {
                            return forageJob;
                        }
                        forageJob.Clear();
                    }
                    return null;
                }
                public override String Text
                {
                    get
                    {
                        return "Forage trees at grove \"" + Grove.Name + "\"";
                    }
                }
            }
            public class FellTreeJobProvider : GroveJobProvider
            {
                public static List<FellTreeJobProvider> existing = new List<FellTreeJobProvider>();
                public FellTreeJobProvider(Grove g) : base(g, CharacterSkillType.WoodCutting) { }
                public override Job TryGetJob(Character gnome)
                {
                    Vector3 vector = Grove.FellSpot();
                    if (vector != -Vector3.One)
                    {
                        FellTreeJob fellTreeJob = new FellTreeJob(vector);
                        if (fellTreeJob.ToolRequired() && !gnome.Body.HasItem(fellTreeJob.ToolID) && !GnomanEmpire.Instance.Fortress.StockManager.IsItemAvailable(gnome.Position, fellTreeJob.ToolID, 80, ItemQuality.Poor, true))
                        {
                            fellTreeJob.Clear();
                        }
                        else
                        {
                            if (fellTreeJob.PathTo(gnome))
                            {
                                return fellTreeJob;
                            }
                            fellTreeJob.Clear();
                        }
                    }
                    return null;
                }
                public override String Text
                {
                    get
                    {
                        return "Fell trees at grove \"" + Grove.Name + "\"";
                    }
                }
            }
            public static void On_GroveManager_CreateGrove(GroveManager self, Rectangle rect, int level)
            {
                JobProvider.Helper_AddMissing(
                    self.Groves,
                    PlantTreeJobProvider.existing.Select(jp => jp.Grove),
                    PlantTreeJobProvider.existing,
                    grove => Tuple.Create(new PlantTreeJobProvider(grove), jobConfigurations.Get(jobConfigurations.Grove_Plant, grove.Priority))
                    );
                JobProvider.Helper_AddMissing(
                    self.Groves,
                    TakeClippingJobProvider.existing.Select(jp => jp.Grove),
                    TakeClippingJobProvider.existing,
                    grove => Tuple.Create(new TakeClippingJobProvider(grove), jobConfigurations.Get(jobConfigurations.Grove_Clipping, grove.Priority))
                    );
                JobProvider.Helper_AddMissing(
                    self.Groves,
                    ForageJobProvider.existing.Select(jp => jp.Grove),
                    ForageJobProvider.existing,
                    grove => Tuple.Create(new ForageJobProvider(grove), jobConfigurations.Get(jobConfigurations.Grove_Forage, grove.Priority))
                    );
                JobProvider.Helper_AddMissing(
                    self.Groves,
                    FellTreeJobProvider.existing.Select(jp => jp.Grove),
                    FellTreeJobProvider.existing,
                    grove => Tuple.Create(new FellTreeJobProvider(grove), jobConfigurations.Get(jobConfigurations.Grove_Fell, grove.Priority))
                    );
            }
            public static void On_GroveManager_RemoveGrove(GroveManager self, Grove grove)
            {
                JobProvider.Helper_RemoveIfExistant(PlantTreeJobProvider.existing, jp => jp.Grove == grove);
                JobProvider.Helper_RemoveIfExistant(TakeClippingJobProvider.existing, jp => jp.Grove == grove);
                JobProvider.Helper_RemoveIfExistant(ForageJobProvider.existing, jp => jp.Grove == grove);
                JobProvider.Helper_RemoveIfExistant(FellTreeJobProvider.existing, jp => jp.Grove == grove);
            }
            public static void OnSet_Grove_Priority(Grove self, int prio)
            {
                JobProvider.Helper_AdjustPriority(PlantTreeJobProvider.existing.Where(jp => jp.Grove == self), prio);
                JobProvider.Helper_AdjustPriority(TakeClippingJobProvider.existing.Where(jp => jp.Grove == self), prio);
                JobProvider.Helper_AdjustPriority(ForageJobProvider.existing.Where(jp => jp.Grove == self), prio);
                JobProvider.Helper_AdjustPriority(FellTreeJobProvider.existing.Where(jp => jp.Grove == self), prio);
            }
        }
        #endregion
        #region Ranches
        public static class Ranches
        {
            public abstract class PastureJobProvider : JobProvider
            {
                public Pasture Pasture;
                public PastureJobProvider(Pasture p)
                    : base(CharacterSkillType.AnimalHusbandry)
                {
                    Pasture = p;
                }
                public override Vector3 Position
                {
                    get { return Pasture.CenterPosition(); }
                }
                public override bool Suspended
                {
                    get
                    {
                        return Pasture.Suspended;
                    }
                    set
                    {
                        Pasture.Suspended = value;
                    }
                }
            }
            public class PastureAnimalJobProvider : PastureJobProvider
            {
                public static List<PastureAnimalJobProvider> existing = new List<PastureAnimalJobProvider>();
                public PastureAnimalJobProvider(Pasture p) : base(p) { }
                public override Job TryGetJob(Character gnome)
                {
                    var trgPos = Pasture.RandomPosition();
                    if (gnome.CanReach(trgPos, false))
                    {
                        var animal = Pasture.FindUnpasturedAnimal(gnome);
                        if (animal != null)
                        {
                            if (animal.CanReach(trgPos, false))
                            {
                                var pastureAnimalJob = new PastureAnimalJob(trgPos, animal, 1f, CharacterSkillType.AnimalHusbandry, 1);
                                if (pastureAnimalJob.PathTo(gnome))
                                {
                                    return pastureAnimalJob;
                                }
                                pastureAnimalJob.Clear();
                            }
                        }
                    }
                    return null;
                    /*
                    Character character2 = Pasture.FindUnpasturedAnimal(gnome);
                    if (character2 != null)
                    {
                        PastureAnimalJob pastureAnimalJob = new PastureAnimalJob(Pasture.RandomPosition(), character2, 1f, CharacterSkillType.AnimalHusbandry, 1);
                        if (character2.CanReach(pastureAnimalJob.Position, false) && pastureAnimalJob.PathTo(gnome))
                        {
                            return pastureAnimalJob;
                        }
                        pastureAnimalJob.Clear();
                    }
                    return null;
                    */
                }
                public override String Text
                {
                    get
                    {
                        return "Pasture animals at pasture \"" + Pasture.Name + "\"";
                    }
                }

            }
            public class FarmAnimalJobProvider : PastureJobProvider
            {
                public static List<FarmAnimalJobProvider> existing = new List<FarmAnimalJobProvider>();
                public FarmAnimalJobProvider(Pasture p) : base(p) { }
                public override Job TryGetJob(Character gnome)
                {
                    Character character2 = Pasture.FindFarmableAnimal();
                    if (character2 != null)
                    {
                        FarmAnimalJob farmAnimalJob = new FarmAnimalJob(character2.Position, character2, 3f, CharacterSkillType.AnimalHusbandry, 1);
                        if (farmAnimalJob.PathTo(gnome))
                        {
                            return farmAnimalJob;
                        }
                        farmAnimalJob.Clear();
                    }
                    return null;
                }
                public override String Text
                {
                    get
                    {
                        return "Farm animals at pasture \"" + Pasture.Name + "\"";
                    }
                }
            }


            public static void On_PastureManager_CreatePasture(PastureManager self, Rectangle rect, int level)
            {
                JobProvider.Helper_AddMissing(
                    self.Pastures,
                    PastureAnimalJobProvider.existing.Select(jp => jp.Pasture),
                    PastureAnimalJobProvider.existing,
                    pasture => Tuple.Create(new PastureAnimalJobProvider(pasture), jobConfigurations.Get(jobConfigurations.Ranch_Pasture, pasture.Priority))
                    );

                JobProvider.Helper_AddMissing(
                    self.Pastures,
                    FarmAnimalJobProvider.existing.Select(jp => jp.Pasture),
                    FarmAnimalJobProvider.existing,
                    pasture => Tuple.Create(new FarmAnimalJobProvider(pasture), jobConfigurations.Get(jobConfigurations.Ranch_Farm, pasture.Priority))
                    );
            }
            public static void On_PastureManager_RemovePasture(PastureManager self, Pasture pasture)
            {
                JobProvider.Helper_RemoveIfExistant(PastureAnimalJobProvider.existing, jp => jp.Pasture == pasture);
                JobProvider.Helper_RemoveIfExistant(FarmAnimalJobProvider.existing, jp => jp.Pasture == pasture);
            }
            public static void OnSet_Pasture_Priority(Pasture self, int prio)
            {
                JobProvider.Helper_AdjustPriority(PastureAnimalJobProvider.existing.Where(jp => jp.Pasture == self), prio);
                JobProvider.Helper_AdjustPriority(FarmAnimalJobProvider.existing.Where(jp => jp.Pasture == self), prio);
            }
        }
        #endregion
        #region Tinker
        // TODO: do we need tinker job? Why does the job part exist in game in the first place? Tinker should be handled by Workshop! It should also introduce a bug (assigned gnomes are ignored). Only possible use are default priorities
        public class TinkerJobProvider : JobProvider
        {
            public static List<TinkerJobProvider> existing = new List<TinkerJobProvider>();
            private TinkerBench myBench;
            public override Job TryGetJob(Character gnome)
            {
                var tinkerJob = myBench.GetJob(gnome);

                if (tinkerJob != null && !tinkerJob.Claimed && tinkerJob.PathTo(gnome))
                    return tinkerJob;
                return null;
            }
            public override String Text
            {
                get
                {
                    return "Tinker at \"" + myBench.Name() + "\"";
                }
            }
            public override Vector3 Position
            {
                get { return myBench.CraftPos(); }
            }
            public override bool Suspended
            {
                get
                {
                    return myBench.Suspended;
                }
                set
                {
                    myBench.Suspended = value;
                }
            }

            public TinkerJobProvider(TinkerBench bench)
                : base(CharacterSkillType.Tinkering)
            {
                myBench = bench;
            }
            public static void On_TinkerBench_OnSpawn(TinkerBench self)
            {
                JobProvider.Helper_Add(existing, new TinkerJobProvider(self), jobConfigurations.Get(jobConfigurations.Tinker, self.Priority));
            }
            public static void On_TinkerBench_OnDelete(TinkerBench self)
            {
                JobProvider.Helper_RemoveIfExistant(existing, jp => jp.myBench == self);
            }
            public static void OnSet_TinkerBench_Priority(Workshop self, int prio)
            {
                Helper_AdjustPriority(existing.Where(jp => jp.myBench == self), prio);
            }
        }
        #endregion
        #region Stockpiles
        public static class Stockpiles
        {
            private static List<ItemID> StockPileTypes = null;
            private static List<ItemID> GetStockpileTypes()
            {
                if (StockPileTypes != null)
                    return StockPileTypes;
                List<ItemID> list = StockPileTypes = new List<ItemID>();
                list.Add(ItemID.ResourcePile);
                list.Add(ItemID.MusketRoundPile);
                list.Add(ItemID.CrossbowBoltPile);
                list.Add(ItemID.StrawPile);
                foreach (ItemID itemID in Enum.GetValues(typeof(ItemID)))
                {
                    if (itemID != ItemID.StorageStart && itemID != ItemID.Barrel && itemID != ItemID.Bag && itemID != ItemID.ResourcePile && itemID != ItemID.MusketRoundPile && itemID != ItemID.CrossbowBoltPile && itemID != ItemID.StrawPile && itemID != ItemID.Count && itemID != ItemID.Any && itemID != ItemID.Any2H)
                    {
                        list.Add(itemID);
                    }
                }
                return list;
            }
            public abstract class StockpileJobProvider : JobProvider
            {
                public Stockpile Stockpile;
                public StockpileJobProvider(Stockpile s)
                    : base(CharacterSkillType.Hauling)
                {
                    Stockpile = s;
                }
                public override Vector3 Position
                {
                    get { return Stockpile.CenterPosition(); }
                }
                public override bool Suspended
                {
                    get
                    {
                        return Stockpile.Suspended;
                    }
                    set
                    {
                        Stockpile.Suspended = value;
                    }
                }



            }
            public class HaulingJobProvider : StockpileJobProvider
            {
                public static List<HaulingJobProvider> existing = new List<HaulingJobProvider>();
                public HaulingJobProvider(Stockpile s) : base(s) { }
                public override String Text
                {
                    get
                    {
                        return "Haul items for stockpile \"" + Stockpile.Name + "\"";
                    }
                }
                public override Job TryGetJob(Character gnome)
                {
                    /* Mostly the same what Game.StockManager.FindStockJob() does */
                    var list = GetStockpileTypes();
                    if (Stockpile.HasRoom())
                    {
                        foreach (ItemID current2 in list)
                        {
                            if (Stockpile.AllowedItems.IsItemAllowed(current2))
                            {
                                Dictionary<int, List<Item>> dictionary = GnomanEmpire.Instance.Fortress.StockManager.UnstockedItemsByItemID(current2);
                                if (dictionary != null)
                                {
                                    var randPos = Stockpile.RandomPosition();
                                    Dictionary<int, StorageContainer> dictionary2 = new Dictionary<int, StorageContainer>();
                                    Dictionary<int, Vector3> dictionary3 = new Dictionary<int, Vector3>();
                                    Vector3 vector = -Vector3.One;
                                    List<Item> list2 = new List<Item>();
                                    foreach (KeyValuePair<int, List<Item>> current3 in dictionary)
                                    {
                                        if (Stockpile.IsMaterialAllowed(current2, current3.Key))
                                        {
                                            StorageContainer value = null;
                                            Vector3 vector2 = Stockpile.OpenSpot(current3.Key, current2, out value);
                                            MapCell cell = GnomanEmpire.Instance.Map.GetCell(vector2);
                                            if (cell != null)
                                            {
                                                dictionary2[current3.Key] = value;
                                                dictionary3[current3.Key] = vector2;
                                                list2 = list2.Concat(current3.Value).ToList<Item>();
                                            }
                                        }
                                    }
                                    IOrderedEnumerable<Item> orderedEnumerable2 = list2.OrderBy(item => Map.DistanceSquaredWithBias(randPos, item.Position));
                                    foreach (Item current4 in orderedEnumerable2)
                                    {
                                        if (!current4.InStockpile && !current4.Claimed)
                                        {
                                            StorageContainer storageContainer = current4 as StorageContainer;
                                            if (storageContainer != null && storageContainer.ItemID != ItemID.BodyPart && storageContainer.ItemID != ItemID.Corpse)
                                            {
                                                if (!storageContainer.AllItemsAllowed(Stockpile))
                                                {
                                                    continue;
                                                }
                                                StorageDef storageDef = GnomanEmpire.Instance.EntityManager.StorageDef(StorageDef.StorageContainerID(current4.ItemID));
                                                if (!storageDef.AllowedItems.AreAnyAllowed(Stockpile.AllowedItems))
                                                {
                                                    continue;
                                                }
                                            }
                                            StorageContainer storageContainer2 = dictionary2[current4.MaterialID];
                                            vector = dictionary3[current4.MaterialID];
                                            MapCell cell2 = GnomanEmpire.Instance.Map.GetCell(vector);
                                            if (storageContainer2 != null)
                                            {
                                                storageContainer2.ReserveSlot(current4);
                                            }
                                            uint containerGUID = (storageContainer2 == null) ? 4294967295u : storageContainer2.ID;
                                            StockItemJob stockItemJob = new StockItemJob(vector, current4, new StockItemJobData(current4.ID, containerGUID));
                                            if (gnome.CanReach(current4.Position, false) && stockItemJob.PathTo(gnome))
                                            {
                                                Stockpile.AddJob(stockItemJob);
                                                if (storageContainer2 == null)
                                                {
                                                    cell2.ProposedJob = stockItemJob;
                                                }
                                                else
                                                {
                                                    storageContainer2.AddStockJob(stockItemJob);
                                                }
                                                return stockItemJob;
                                            }
                                            stockItemJob.Clear();
                                        }
                                    }
                                }
                            }
                        }
                    }
                    return null;
                }
            }
            /*
             * 
             * Well, that meant to be my first custom job provider. Never rly worked on that...
            public class RestockJobProvider : StockpileJobProvider
            {
                public static List<RestockJobProvider> existing = new List<RestockJobProvider>();
                public RestockJobProvider(Stockpile s) : base(s) { }
                public override String Text
                {
                    get
                    {
                        return "Haul items for stockpile \"" + Stockpile.Name + "\"";
                    }
                }
                public override Job TryGetJob(Character gnome)
                {
                    throw new NotImplementedException();
                    var stockTypes = GetStockpileTypes();
                    if (Stockpile.HasRoom())
                    {
                        foreach (var item in stockTypes)
                        {

                        }
                    }
                }
            }
            */



            public static void On_StockManager_CreateStockpile(StockManager self, Rectangle rect, int level)
            {
                JobProvider.Helper_AddMissing(
                    self.Stockpiles,
                    HaulingJobProvider.existing.Select(s => s.Stockpile),
                    HaulingJobProvider.existing,
                    stock => Tuple.Create(new HaulingJobProvider(stock), stock.Priority)
                    );
                /*
                JobProvider.Helper_AddMissing(
                    self.Stockpiles,
                    RestockJobProvider.existing.Select(s => s.Stockpile),
                    RestockJobProvider.existing,
                    stock => Tuple.Create(new RestockJobProvider(stock), stock.Priority * 2)
                    );
                 */
            }
            public static void On_StockManager_RemoveStockpile(StockManager self, Stockpile pile)
            {
                JobProvider.Helper_RemoveIfExistant(HaulingJobProvider.existing, jp => jp.Stockpile == pile);
                //JobProvider.Helper_RemoveIfExistant(RestockJobProvider.existing, jp => jp.Stockpile == pile);
            }
            public static void OnSet_Stockpile_Priority(Stockpile self, int prio)
            {
                JobProvider.Helper_AdjustPriority(HaulingJobProvider.existing.Where(jp => jp.Stockpile == self), prio);
                //JobProvider.Helper_AdjustPriority(RestockJobProvider.existing.Where(jp => jp.Stockpile == self), prio);
            }
        }
        #endregion
        #endregion
        #region Job Collection
        [DataContract]
        public abstract class JobCollection : IJob
        {
            public enum JobCollectionOrderType
            {
                ByDistance,
                AsInList
            }

            public abstract string Text { get; }

            private JobCollection mParent = null;
            public virtual JobCollection Parent
            {
                get
                {
                    return mParent;
                }
                set
                {
                    if (mParent != null)
                    {
                        mParent.SubItems.Remove(this);
                    }
                    mParent = value;
                    if (mParent != null)
                    {
                        mParent.SubItems.Add(this);
                    }
                }
            }

            [DataMember]
            public JobCollectionOrderType OrderType;
            [DataMember]
            public bool Suspended { get; set; }
            public CharacterSkillType UsedSkill
            {
                get
                {
                    return CharacterSkillType.Count;
                }
            }
            //[DataMember]
            public List<IJob> SubItems;
            public Job TryGetJob(Character gnome)
            {
                IEnumerable<IJob> list;
                if (OrderType == JobCollectionOrderType.ByDistance)
                {
                    list = SubItems.OrderBy(item => Map.DistanceSquaredWithBias(gnome.Position, item.Position));
                }
                else
                {
                    list = SubItems;
                }
                foreach (var el in list)
                {
                    if (!el.Suspended && ((el.UsedSkill == CharacterSkillType.Count) || (gnome.Mind.IsSkillAllowed(el.UsedSkill))))
                    {
                        var job = el.TryGetJob(gnome);
                        if (job != null)
                            return job;
                    }
                }
                return null;
            }
            public Vector3 Position
            {
                get { return SubItems[0].Position; }
            }

            [OnDeserializing]
            private void Init(StreamingContext context)
            {
                Init();
            }
            private void Init()
            {
                Suspended = false;
                SubItems = new List<IJob>();
                OrderType = JobCollectionOrderType.ByDistance;
            }
            public JobCollection()
            {
                Init();
            }
        }
        [DataContract]
        private class CustomJobCollection : JobCollection
        {
            // TODO: Make entire job collections suspendable...
            [DataMember]
            public string Name;
            public override String Text
            {
                get
                {
                    return Name;
                }
            }
        }

        [DataContract]
        public class TopLevelJobCollection
        {
            [DataContract]
            public class PriorityJobCollection : JobCollection
            {
                [DataMember(Name = "Priority")]
                public int Priority { get; set; }
                public override string Text
                {
                    get
                    {
                        return "Priority " + Priority;
                    }
                }

                public PriorityJobCollection(int prio)
                {
                    Priority = prio;
                }
            }
            [DataMember(Name="Priorities")]
            public PriorityJobCollection[] priorities = new PriorityJobCollection[0];
            public Job TryGetJob(Character gnome)
            {
                foreach (var prioColl in priorities)
                {
                    if (!prioColl.Suspended)
                    {
                        var job = prioColl.TryGetJob(gnome);
                        if (job != null)
                            return job;
                    }
                }
                return null;
            }
            public PriorityJobCollection GetOrAddPriority(int priority)
            {
                var existingPrioCat = priorities.SingleOrDefault(p => p.Priority == priority);
                if (existingPrioCat == null)
                {
                    var newcoll = new PriorityJobCollection(priority);
                    priorities = priorities
                        .Union(new PriorityJobCollection[] { newcoll })
                        .OrderBy(priocoll => priocoll.Priority)
                        .ToArray();
                    return newcoll;
                }
                else
                {
                    return existingPrioCat;
                }
            }
            
            public void AddJobProvider(IJob provider, int priority)
            {
                provider.Parent = GetOrAddPriority(priority);
            }
        }
        #endregion
        #region UI - Details Panal
        private class DetailsPanel: LoweredPanel
        {
            public event System.EventHandler OnClose;
            public DetailsPanel(Manager mgr)
                : base(mgr)
            {
                this.Init();

                var closeButton = new Button(mgr);
                closeButton.Skin = new SkinControl(this.Manager.Skin.Controls["Window.CloseButton"]);
                closeButton.Init();
                closeButton.Detached = true;
                closeButton.CanFocus = false;
                closeButton.Text = null;
                closeButton.Click += new Game.GUI.Controls.EventHandler((sender, args) => { OnClose.TryRaise(this); });
                closeButton.SkinChanged += new Game.GUI.Controls.EventHandler((sender, args) => { closeButton.Skin = new SkinControl(this.Manager.Skin.Controls["Window.CloseButton"]); });

                SkinLayer skinLayer = closeButton.Skin.Layers["Control"];
                closeButton.Width = skinLayer.Width - closeButton.Skin.OriginMargins.Horizontal;
                closeButton.Height = skinLayer.Height - closeButton.Skin.OriginMargins.Vertical;
                closeButton.Left = this.OriginWidth - this.Skin.OriginMargins.Right - closeButton.Width + skinLayer.OffsetX;
                closeButton.Top = this.Skin.OriginMargins.Top + skinLayer.OffsetY;
                closeButton.Anchor = (Anchors.Top | Anchors.Right);

                this.Add(closeButton);
                // TODO:
                // Add close button
                // Add name
                // Suspended
            }

            /*
             * Possible content:
             * 
             * - view or change name
             * - suspend cb
             * - priority stuff??
             * - goto button
             * - open window button
             * - sort sub jobs by...
             * - status stuff? shitload of work...
             * - groups: add logic blocks (condition may does it?)
             */
            private void GetSuspendElement()
            {
            }
            private void GetPriorityElement()
            {
            }
        }
        private class JobTreeView : LoweredPanel
        {
            private class subButton
            {
                public int lvl;
                public Button btn;
                public IJob jobEl;
                public bool isOpen = false;
            }


            LinkedList<subButton> displayedButtons = new LinkedList<subButton>();
            LinkedListNode<subButton> currentDragTarget = null;
            private enum insertMode { before, after, into };
            private int index(LinkedListNode<subButton> e)
            {
                // Todo: debug, remove soon
                var i = 0;
                foreach (var el in displayedButtons)
                {
                    if (e.Value == el)
                        return i;
                    i++;
                }
                return -1;
            }
            private void updateRange(LinkedListNode<subButton> from, LinkedListNode<subButton> to)
            {
                var pos = from;
                if (from == null)
                {
                    pos = displayedButtons.First;
                }
                else if (from.Previous != null)
                {
                    pos = from.Previous;
                }
                if (to.Next != null)
                {
                    to = to.Next;
                }
                var top = pos.Value.btn.Top;
                while (pos != null)
                {
                    var ct = pos.Value.jobEl.Text;
                    pos.Value.btn.Text = ct;
                    pos.Value.btn.Top = top;
                    top = pos.Value.btn.Height + top + 3;
                    if (pos == to)
                    {
                        break;
                    }
                    pos = pos.Next;
                }
            }
            private LinkedListNode<subButton> createNewPriorityBefore(LinkedListNode<subButton> trg)
            {
                var prio = jobCategories.priorities.Single(jp => jp == trg.Value.jobEl);
                var prev = jobCategories.priorities.SingleOrDefault(jp => jp.Priority == (prio.Priority - 1));
                if ((prio.Priority <= 1) || prev != null)
                {
//#warning have to add a shitload of updating here... Update: All handled by range-ud in moveTo now
                    foreach (var cat in jobCategories.priorities)
                    {
                        if (cat.Priority >= prio.Priority)
                        {
                            cat.Priority++;
                        }
                    }
                }
//#warning do actual inserting and UI update... currently missing, since we do not update ui for now.
                return addButton(jobCategories.GetOrAddPriority(prio.Priority - 1), trg.Value.lvl, trg.Value.btn.Manager, trg.Previous);
            }
            private void moveTo(LinkedListNode<subButton> movedNode, LinkedListNode<subButton> targetNode, insertMode insertMode, bool moveUpwards)
            {
                // Todo: This doesn't support collections at all. Will be disabled for now
                // get target, may create for toplevel-before
                // insert
                // push back priorities?
                // check old group, remove if empty

                // Todo: think about cleaner ways to do this function...

                LinkedListNode<subButton> top;
                LinkedListNode<subButton> bottom;
                if (moveUpwards)
                {
                    top = movedNode;
                    bottom = movedNode.Next;
                }
                else
                {
                    top = movedNode.Previous;
                    bottom = movedNode;
                }
                


                var movedJob = movedNode.Value.jobEl;
                displayedButtons.Remove(movedNode);
                if ((targetNode.Value.lvl == 0) && (insertMode == JobTreeView.insertMode.before))
                {
                    targetNode = createNewPriorityBefore(targetNode);
                    if (moveUpwards)
                    {
                        top = targetNode;
                    }
                    insertMode = JobTreeView.insertMode.into;
                }

                if (insertMode == JobTreeView.insertMode.into)
                {
                    movedJob.Parent = targetNode.Value.jobEl as JobCollection;
                    //(targetNode.Value.jobEl as JobCollection).subAddJobProvider(movedJob);
                    var inserted = false;
                    var pos = targetNode;
                    while (pos.Next != null)
                    {
                        pos = pos.Next;
                        if (pos.Value.lvl == targetNode.Value.lvl)
                        {
                            inserted = true;
                            displayedButtons.AddBefore(pos, movedNode);
                            break;
                        }
                    }
                    if (!inserted)
                    {
                        displayedButtons.AddLast(movedNode);
                    }
                }
                else
                {
                    // Todo: this shit is shit. As well as the current remove/add nesting seaches. Add an actual way that makes any sense, likely adding "JobCollection parent {get;}" to ijob
                    var trgJob = targetNode.Value.jobEl;
                    JobCollection containingCollection = trgJob.Parent;
                    /*CustomJobCollection containingCollection = jobCategories.priorities.Single(jc => jc.ContainsJobProvider(trgJob));
                    while (!containingCollection.SubItems.Contains(trgJob))
                    {
                        containingCollection = (CustomJobCollection)containingCollection.SubItems.Single(si =>
                        {
                            var col = si as CustomJobCollection;
                            return col != null && col.ContainsJobProvider(trgJob);
                        });
                    }*/
                    movedJob.Parent = containingCollection;
                    // Todo: get rid of this following line...
                    containingCollection.SubItems.Remove(movedJob);

                    if (insertMode == JobTreeView.insertMode.after)
                    {
                        containingCollection.SubItems.Insert(containingCollection.SubItems.IndexOf(trgJob) + 1, movedJob);
                        displayedButtons.AddAfter(targetNode, movedNode);
                    }
                    else
                    {
                        containingCollection.SubItems.Insert(containingCollection.SubItems.IndexOf(trgJob), movedJob);
                        displayedButtons.AddBefore(targetNode, movedNode);
                    }
                }
                Control win = this;
                while (!(win is Window))
                {
                    win = win.Parent;
                }
                updateRange(top, bottom);

            }
            private void resizeButton(subButton el)
            {
                el.btn.Left = (el.lvl * 30) + 1;
                el.btn.Width = ClientWidth - el.btn.Left - 1;
            }
            private LinkedListNode<subButton> addButton(IJob job, int level, Manager mgr, LinkedListNode<subButton> insertAfter = null)
            {
                var newEl = new subButton()
                {
                    btn = new Button(mgr),
                    lvl = level,
                    jobEl = job
                };
                //displayedButtons.AddLast(newEl);
                newEl.btn.Init();
                newEl.btn.Anchor = Anchors.Top | Anchors.Left;
                newEl.btn.Click += new Game.GUI.Controls.EventHandler((sender, args) =>
                {
                    this.DetailScreenRequested.TryRaise(this, job);
                });
                LinkedListNode<subButton> node;
                if (insertAfter == null)
                {
                    node = displayedButtons.AddFirst(newEl);
                }
                else
                {
                    node = displayedButtons.AddAfter(insertAfter, newEl);
                }
                if (job is JobProvider)
                {
                    // Todo: moving is currently only supported for job providers, not for collections
                    newEl.btn.MouseDown += new MouseEventHandler((sender, args) =>
                    {
                        currentDragTarget = node;
                    });
                    String mayClear = null;
                    insertMode insMode = 0;
                    bool isDropDragetAboveDropSource = false;
                    newEl.btn.MouseUp += new MouseEventHandler((sender, args) =>
                    {
                        if (mayClear != null)
                        {
                            currentDragTarget.Value.btn.Text = mayClear;
                        }
                        if ((currentDragTarget != null) && (currentDragTarget != node))
                        {
                            moveTo(node, currentDragTarget, insMode, isDropDragetAboveDropSource);
                        }
                        mayClear = null;
                        currentDragTarget = null;
                    });
                    newEl.btn.MouseMove += new MouseEventHandler((sender, args) =>
                    {
                        var oldTrg = currentDragTarget;
                        if (currentDragTarget != null)
                        {
                            var myTop = newEl.btn.Top;
                            while ((currentDragTarget.Previous != null) && ((currentDragTarget.Value.btn.Top - myTop) > args.Position.Y))
                            {
                                if (currentDragTarget == node)
                                {
                                    isDropDragetAboveDropSource = true;
                                }
                                currentDragTarget = currentDragTarget.Previous;
                            }
                            while ((currentDragTarget.Next != null) && (currentDragTarget.Next.Value.btn.Top - myTop) < args.Position.Y)
                            {
                                if (currentDragTarget == node)
                                {
                                    isDropDragetAboveDropSource = false;
                                }
                                currentDragTarget = currentDragTarget.Next;
                            }
                            if ((mayClear != null) && (currentDragTarget != oldTrg))
                            {
                                oldTrg.Value.btn.Text = mayClear;
                                mayClear = null;
                            }
                            if (currentDragTarget != node)
                            {
                                var mpos = args.Position.Y - (currentDragTarget.Value.btn.Top - myTop);
                                if (mayClear == null)
                                {
                                    mayClear = currentDragTarget.Value.btn.Text;
                                }
                                if (mpos > (currentDragTarget.Value.btn.Height / 2))
                                {
                                    if (currentDragTarget.Value.jobEl is JobCollection)
                                    {
                                        currentDragTarget.Value.btn.Text = "Insert into: " + job.Text;
                                        insMode = insertMode.into;
                                    }
                                    else
                                    {
                                        currentDragTarget.Value.btn.Text = "Insert after: " + job.Text;
                                        insMode = insertMode.after;
                                    }
                                }
                                else
                                {
                                    currentDragTarget.Value.btn.Text = "Insert before: " + job.Text;
                                    insMode = insertMode.before;
                                }
                            }
                            else
                            {
                                mayClear = null;
                            }
                        }
                        //newEl.btn.Text = "AT " + args.Position.Y;// +" of " + ((possibleDragElement == null) ? ("NULL") : (possibleDragElement.Value.jobEl.Text));
                        /*if ((possibleDragElement != null) && (possibleDragElement != node))
                        {
                            args.Position.ToString();
                        }*/
                    });
                }
                resizeButton(newEl);
                this.Add(newEl.btn);
                return node;
            }
            private LinkedListNode<subButton> addLevel(IEnumerable<IJob> jobs, int lvl, Manager mgr, LinkedListNode<subButton> insertAfter)
            {
                foreach (var j in jobs)
                {
                    insertAfter = addButton(j, lvl, mgr, insertAfter);
                }
                return insertAfter;
            }
            public event EventHandler<EventArgs<IJob>> DetailScreenRequested;


            public JobTreeView(TopLevelJobCollection jobs, Manager mgr)
                : base(mgr)
            {
                Init();
                AutoScroll = true;
                Passive = true;
                CanFocus = false;
                HorizontalScrollBarEnabled = false;
                HorizontalScrollBarShow = false;

                LinkedListNode<subButton> currentParent = null;
                foreach (var cat in jobs.priorities)
                {
                    currentParent = addButton(cat, 0, mgr, currentParent);
                    currentParent = addLevel(cat.SubItems, 1, mgr, currentParent);
                }
                displayedButtons.First.Value.btn.Top = 0;
                // Todo: the line above appears to crash if there is no button at all (newly created game)
                updateRange(displayedButtons.First, displayedButtons.Last);
            }
            protected override void OnResize(ResizeEventArgs e)
            {
                base.OnResize(e);
                var curBtn = displayedButtons.First;
                while (curBtn != null)
                {
                    resizeButton(curBtn.Value);
                    curBtn = curBtn.Next;
                }
            }
        }
        #endregion

        [Serializable]
        private class DefaultJobConfig
        {
            public int CustomCommand = 2;
            public int Hospitals_Heal = 4;
            public int Hospitals_Feed = 4;
            public int Workshop = 5;
            public int Farms_Harvest = 7;
            public int Farms_Plant = 7;
            public int Farms_Tile = 7;
            public int Grove_Plant = 8;
            public int Grove_Clipping = 8;
            public int Grove_Forage = 8;
            public int Grove_Fell = 8;
            public int Ranch_Pasture = 9;
            public int Ranch_Farm = 9;
            public int Tinker = 10;
            public int Hauling = 11;

            [NonSerialized]
            public bool is_Loading = false;
            public int Get(int conf_val, int shop_prio)
            {
                if (conf_val == -1 || is_Loading)
                    return shop_prio;
                return conf_val;
            }
        }
        private static DefaultJobConfig jobConfigurations = new DefaultJobConfig();
        private static TopLevelJobCollection jobCategories = new TopLevelJobCollection();
        /*
        private class JobInfo
        {
            public Job Job;
            public object JobGroupOrProvider;
            public JobInfo(Job job, object jobGroup)
            {
                Job = job;
                JobGroupOrProvider = jobGroup;
            }
        }
        private class JobSource
        {
            public List<JobInfo> Jobs = new List<JobInfo>();
            public event EventHandler<EventArgs<JobInfo>> JobFound;
            public event EventHandler<EventArgs<JobInfo>> JobRemoved;
            public event EventHandler<EventArgs<JobInfo>> JobUpdated;
            protected virtual void OnJobFound(JobInfo j)
            {
                Jobs.Add(j);
                JobFound.TryRaise(this, j);
            }
            protected virtual void OnJobRemoved(JobInfo j)
            {
                Jobs.Remove(j);
                JobRemoved.TryRaise(this, j);
            }
            protected virtual void OnJobUpdated(JobInfo j)
            {
                JobUpdated.TryRaise(this, j);
            }
        }
        private class JobBoardData
        {
            private JobSource[] Sources;
            public void AddSource(JobSource source)
            {
            }
        }


        private static class Sources
        {
            private class AccessibleJobSource : JobSource
            {
                public new void OnJobFound(JobInfo j)
                {
                    base.OnJobFound(j);
                }
                public new void OnJobRemoved(JobInfo j)
                {
                    base.OnJobRemoved(j);
                }
                public new void OnJobUpdated(JobInfo j)
                {
                    base.OnJobUpdated(j);
                }
            }
            public static class FarmJobs
            {
                private static AccessibleJobSource source = new AccessibleJobSource();
                public static void On_FarmManager_RemoveFarm(FarmManager self, Farm farm)
                {
                    var toRemove = source.Jobs.Where(ji => ji.JobGroupOrProvider == farm).ToList();
                    foreach (var ji in toRemove)
                    {
                        source.OnJobRemoved(ji);
                    }
                }
                public static void On_FarmManager_CreateFarm(FarmManager self, Rectangle rect, int newFarmLevel)
                {
#warning currently no way to detect what farm => just re-create all jobs on that level :)
                    var map = GnomanEmpire.Instance.Map;
                    foreach (var farm in self.Farms)
                    {
                        var level = (int)Designation_Level_FieldInfo.GetValue(farm);
                        if (newFarmLevel == level)
                        {
                            farm.ForAnyCell(level, (cell, pos) =>
                            {
                                Job newJob = null;
                                TilledSoil tilledSoil = cell.EmbeddedFloor as TilledSoil;
                                if ((tilledSoil == null) && cell.ProposedJob == null)
                                {
                                    newJob = new TillSoilJob(pos);
                                    cell.ProposedJob = newJob;
                                }
                                else if (cell.EmbeddedWall is Crop && cell.ProposedJob == null)
                                {
                                    newJob = new ForageJob(pos);
                                    cell.ProposedJob = newJob;
                                }
                                else if (tilledSoil != null && cell.ProposedJob == null && tilledSoil.PlantedSeed == null)
                                {
                                    newJob = new PlantSeedJob(pos);
                                    newJob.RequiredComponents.Add(new JobComponent(ItemID.Seed, farm.SeedMaterial));
                                }
                                if (newJob != null)
                                {
                                    source.OnJobFound(new JobInfo(newJob, farm));
                                }
                            });
                        }
                    }
                }
                public static MethodHook[] GetHooks()
                {
                    return new MethodHook[]{
                        new MethodHook(
                            typeof(FarmManager).GetMethod("RemoveFarm", BindingFlags.Instance | BindingFlags.Public),
                            Method.Of<FarmManager, Farm>(On_FarmManager_RemoveFarm)
                            ),
                        new MethodHook(
                            typeof(FarmManager).GetMethod("CreateFarm", BindingFlags.Instance | BindingFlags.Public),
                            Method.Of<FarmManager, Rectangle, int>(On_FarmManager_CreateFarm)
                            )
                    };
                }
            }
            public static class GroveJobs
            {
                private static AccessibleJobSource source = new AccessibleJobSource();
                public static void On
                public static MethodHook[] GetHooks()
                {
                    return new MethodHook[]{
                    };
                }
            }
        }*/


        public static class ProfessionsThatUseOrignalJobFind
        {
            [DataContract]
            public class SavedProfessionReference: IEquatable<SavedProfessionReference>
            {
                [DataMember]
                public String Name;
                [DataMember]
                public String Allowed;
                public SavedProfessionReference(Profession prof)
                {
                    Name = prof.Title;
                    Allowed = String.Join("-", prof.AllowedSkills.AllowedSkills.Select(num => num.ToString()));
                }
                public static bool Equals(SavedProfessionReference a, SavedProfessionReference b)
                {
                    return ReferenceEquals(a, b) || (!ReferenceEquals(a, null) && !ReferenceEquals(b, null) && (a.Name == b.Name) && (a.Allowed == b.Allowed));
                }
                public bool Equals(SavedProfessionReference other)
                {
                    return Equals(this, other);
                }
            }
            private static List<SavedProfessionReference> professionsToSaveOrLoad = null;
            public static HashSet<Profession> Professions = new HashSet<Profession>();
            private static void CustomizeProfessionPanelAndReturnInvalidateFunc(TabbedWindowPanel self, Func<int, Profession> indexToProfession = null)
            {
                if (indexToProfession == null)
                {
                    indexToProfession = num => num >= 0 ? GnomanEmpire.Instance.Fortress.Professions[num] : null;
                }
                ListBox aboveListBox = (ListBox)self.ClientArea.Controls.Single(ctrl => ctrl.GetType() == typeof(ListBox));
                var moveUpButton = self.ClientArea.Controls.ElementAfter(aboveListBox);
                var moveDownButton = self.ClientArea.Controls.ElementAfter(moveUpButton);
                var comboBoxes = self.ClientArea.Controls.Where(ctrl => ctrl.GetType() == typeof(ComboBox)).Cast<ComboBox>();
                if ((moveDownButton.Text.ToUpper() != "MOVE DOWN") || (comboBoxes.Count() != 2))
                {
                    throw new Exception("Failed to identify original gui elements");
                }
                ComboBox professionList = comboBoxes.First();
                Profession profession = professionList.ItemIndex >= 0 ? GnomanEmpire.Instance.Fortress.Professions[professionList.ItemIndex] : null;



                Button disablePriosButton = new Button(self.Manager);
                disablePriosButton.Init();
                disablePriosButton.Anchor = Anchors.Bottom | Anchors.Right;
                disablePriosButton.Text = "Use Job\nPriorities";
                disablePriosButton.Left = moveUpButton.Left;
                disablePriosButton.Width = moveUpButton.Width;
                disablePriosButton.Height = (moveUpButton.Height * 2) - 10;
                disablePriosButton.Top = (aboveListBox.Top + aboveListBox.Height) - disablePriosButton.Height;

                Panel panel = null;

                Action showPanel = () =>
                {
                    panel = new LoweredPanel(self.Manager);
                    panel.Init();
                    //panel.BackColor = new Color(0, 0, 0, 0.5f);
                    panel.Anchor = Anchors.Vertical | Anchors.Right;
                    panel.Top = aboveListBox.Top;
                    panel.Left = aboveListBox.Left;
                    panel.Height = aboveListBox.Height;
                    panel.Width = (moveUpButton.Left + moveUpButton.Width) - aboveListBox.Left;
                    var lbl = new Label(self.Manager);
                    lbl.Init();
                    //lbl.Anchor = Anchors.None;
                    lbl.Anchor = Anchors.Horizontal;
                    lbl.Alignment = Alignment.BottomCenter;
                    lbl.Ellipsis = false;
                    lbl.Width = panel.ClientWidth;
                    lbl.Height = (panel.ClientHeight / 2) - 5;
                    lbl.Text = Util.Neoforce.Helpers.BreakAndCenterStringAccoringToLineLength("Priorities are disabled for this profession. Jobs priorities will be used directly, instead.", (float)lbl.ClientWidth * 0.9f /* 90% of half */, lbl.Skin.Layers[0].Text.Font.Resource);
                    //lbl.Height = 50;
                    lbl.Left = 0;
                    lbl.Top = 0;
                    //lbl.Left = (panel.ClientWidth / 2) - (lbl.Width / 2);
                    //lbl.Top = (panel.ClientHeight/ 2) - lbl.Height;
                    panel.Add(lbl);
                    var btn = new Button(self.Manager);
                    btn.Init();
                    btn.Anchor = Anchors.None;
                    btn.Text = "Use non-Mod behavior";
                    btn.Width = 215;
                    btn.Top = (panel.ClientHeight / 2);
                    btn.Left = (panel.ClientWidth / 2) - (btn.Width / 2);
                    btn.Click += new Game.GUI.Controls.EventHandler((sender, args) =>
                    {
                        self.Remove(panel);
                        self.Invalidate();
                        panel = null;
                        ProfessionsThatUseOrignalJobFind.Professions.Add(profession);
                    });
                    panel.Add(btn);
                    self.Add(panel);
                };

                Action updateProfession = () =>
                {
                    profession = indexToProfession(professionList.ItemIndex);
                    disablePriosButton.Enabled = profession != null;
                    if (ProfessionsThatUseOrignalJobFind.Professions.Contains(profession))
                    {
                        if (panel != null)
                        {
                            self.Remove(panel);
                            self.Invalidate();
                            panel = null;
                        }
                    }
                    else
                    {
                        if (panel == null)
                        {
                            showPanel();
                        }
                    }
                };
                disablePriosButton.Click += new Game.GUI.Controls.EventHandler((sender, args) =>
                {
                    ProfessionsThatUseOrignalJobFind.Professions.Remove(profession);
                    showPanel();
                });
                professionList.ItemIndexChanged += new Game.GUI.Controls.EventHandler((sender, args) =>
                {
                    updateProfession();
                });
                self.Add(disablePriosButton);
                updateProfession();
            }
            public static void On_PopulationProfessionUI_SetupPanel(PopulationProfessionUI self)
            {
                CustomizeProfessionPanelAndReturnInvalidateFunc(self);
            }
            public static void On_ViewCharacterProfessionUI_SetupPanel(TabbedWindowPanel self)
            {
                var character = (Character)Type.GetType("A.cab5d0ce0ee9dc57f23ad33479e5c70f1, Gnomoria").GetFields(BindingFlags.NonPublic | BindingFlags.Instance).Single(field => field.FieldType == typeof(Character)).GetValue(self);
                CustomizeProfessionPanelAndReturnInvalidateFunc(self, num => character.Mind.Profession);
            }
            public static void OnCreate_Profession(Profession self, System.IO.BinaryReader reader)
            {
                // Todo: may add some sync stuff that detects now missing profs?
                var profData = new SavedProfessionReference(self);
                if (professionsToSaveOrLoad.Contains(profData))
                {
                    Professions.Add(self);
                }
            }
            public static void On_Profession_Serialize(Profession self, System.IO.BinaryWriter writer)
            {
                if (Professions.Contains(self))
                {
                    var prof = new SavedProfessionReference(self);
                    if (!professionsToSaveOrLoad.Contains(prof))
                    {
                        professionsToSaveOrLoad.Add(prof);
                    }
                }
            }
            internal static void PreLoad(List<SavedProfessionReference> data)
            {
                professionsToSaveOrLoad = data ?? new List<SavedProfessionReference>();
            }
            internal static void StartSave()
            {
                professionsToSaveOrLoad = new List<SavedProfessionReference>();
            }
            internal static List<SavedProfessionReference> GetToSave()
            {
                var list = professionsToSaveOrLoad;
                professionsToSaveOrLoad = null;
                return list.Count > 0 ? list : null;
            }
        }




        /*
         * We just re-check all designations for now...
        public override void Initialize_PreGame()
        {
            Designation_Level_FieldInfo = typeof(Designation).GetFields(BindingFlags.NonPublic | BindingFlags.Instance).Single(fi => fi.DeclaringType == typeof(int));
        }
#warning perf sucks, replace with sth good!
        private static System.Reflection.FieldInfo Designation_Level_FieldInfo;
        */

        public static void OnCreate_Fortress(Fortress self)
        {
            jobCategories = new TopLevelJobCollection();
            jobConfigurations = new DefaultJobConfig();
            ProfessionsThatUseOrignalJobFind.Professions = new HashSet<Profession>();
            IdleCount = new Dictionary<Character, int>();
            IdleSkippys = new List<Character>();
            JobBoardJobProvider.existing.Clear();
            JobBoardJobProvider.jobLibrary.Clear();
            Hospitals.FeedPatientJobProvider.existing.Clear();
            Hospitals.HealPatientJobProvider.existing.Clear();
            WorkshopJobProvider.existing.Clear();
            Farms.HarvestFarmJobProvider.existing.Clear();
            Farms.PlantSeedJobProvider.existing.Clear();
            Farms.TillSoilJobProvider.existing.Clear();
            Groves.FellTreeJobProvider.existing.Clear();
            Groves.PlantTreeJobProvider.existing.Clear();
            Groves.ForageJobProvider.existing.Clear();
            Groves.TakeClippingJobProvider.existing.Clear();
            Ranches.PastureAnimalJobProvider.existing.Clear();
            Ranches.FarmAnimalJobProvider.existing.Clear();
            TinkerJobProvider.existing.Clear();
            //Stockpiles.RestockJobProvider.existing.Clear();
            Stockpiles.HaulingJobProvider.existing.Clear();
        }
        public static void On_Fortress_OnSerializationComplete(Fortress self)
        {
            jobConfigurations.is_Loading = true;
            //JobBoardJobProvider has its own OnSerializationComplete...
            Hospitals.On_RoomManager_CreateRoom(self.RoomManager, RoomType.Hospital, Rectangle.Empty, 0);
            foreach (var ws in self.Workshops.Where(ws => !(ws is TinkerBench)))
            {
                WorkshopJobProvider.On_Workshop_OnSpawn(ws);
            }
            Farms.On_FarmManager_CreateFarm(self.FarmManager, Rectangle.Empty, 0);
            Groves.On_GroveManager_CreateGrove(self.GroveManager, Rectangle.Empty, 0);
            Ranches.On_PastureManager_CreatePasture(self.PastureManager, Rectangle.Empty, 0);
            foreach (var tb in self.Workshops.Select(ws => ws as TinkerBench).Where(tb => tb != null))
            {
                TinkerJobProvider.On_TinkerBench_OnSpawn(tb);
            }
            Stockpiles.On_StockManager_CreateStockpile(self.StockManager, Rectangle.Empty, 0);
            jobConfigurations.is_Loading = false;
            // Todo: load general structure here or just remove this todo?
        }
        /* Removed, it just was used in the constructor => wrong
        public static void On_Fortress_OnSerializationComplete(Fortress self, System.IO.BinaryReader reader)
        {
            On_Fortress_OnSerializationComplete(self);
        }*/
        private static List<Character> IdleSkippys = new List<Character>();
        private static DateTime nextContinue = DateTime.MinValue;
        private static Dictionary<Character, int> IdleCount = new Dictionary<Character, int>();
        public static bool On_Character_FindJob(Character self, out bool result)
        {
            if (ProfessionsThatUseOrignalJobFind.Professions.Contains(self.Mind.Profession))
            {
                result = true;//whatever
                return false;
            }
            if (IdleSkippys.Contains(self))
            {
                if (nextContinue < DateTime.Now)
                {
                    nextContinue = DateTime.Now + TimeSpan.FromMilliseconds(50);
                    IdleSkippys.Remove(self);
                }
                else
                {
                    result = false;
                    return true;
                }
            }
            if (!IdleCount.ContainsKey(self))
            {
                IdleCount[self] = 0;
            }
            self.DropUnusableTools();
            if (self.CurrentNeedGoal != null)
            {
                IdleCount[self] = 0;
                result = false;
                return true;
            }
            // Todo: ShouldRunFromTarget-Block currently disabled, since not eays accessible...
            /*
            if (this.ca32c13dacedad703e3e4723bde8b4e99 > 0f)
            {
                return false;
            }*/
            if (self.Mind.IsUpset)
            {
                IdleCount[self] = 0;
                result = false;
                return true;
            }
            /*
             * Militia gone, as of 0.8.36
            SquadPosition squadPosition = self.SquadPosition();
            if (squadPosition != null && squadPosition.Perk != SquadPositionPerk.Militia)
            {
                IdleCount[self] = 0;
                return false;
            }
            */
            var job = jobCategories.TryGetJob(self);//getFirstJobFromSourcesOrderedByWhatever
            if (job == null)
            {
                IdleCount[self]++;
                if (IdleCount[self] > 5)
                {
                    IdleSkippys.Add(self);
                }
                result = false;
                return true;
            }
            if (IdleSkippys.Count > 0)
            {
                IdleSkippys.RemoveAt(0);
            }
            self.TakeJob(job);
            IdleCount[self] = 0;
            result = true;
            return true;
        }

        public static void KingdomUI_Created(KingdomUI self, Manager mgr)
        {
            typeof(KingdomUI).GetMethod("AddPage", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(self, new object[] { "Jobs", new KingdomJobUI(mgr) });
            self.MinimumHeight = self.Height;
            self.MinimumWidth= self.Width;
            self.Resizable = true;
        }

        
        public class KingdomJobUI : TabbedWindowPanel
        {
            JobTreeView currentTreeView = null;
            DetailsPanel currentDetailsPanel = null;
            private void removeDetailsPanel()
            {
                this.Remove(currentDetailsPanel);
                this.currentTreeView.Width = ClientWidth;
            }
            public override void SetupPanel()
            {
                var board = GnomanEmpire.Instance.Fortress.JobBoard;


                /*
                var someSpachingControl = new Label(mgr);
                someSpachingControl.Init();
                someSpachingControl.Text = "Job board:";

                someSpachingControl.Margins = new Margins(4, 4, 4, 0);
                someSpachingControl.Top = someSpachingControl.Margins.Top;
                someSpachingControl.Left = someSpachingControl.Margins.Left;
                someSpachingControl.Width = 200;
                this.Add(someSpachingControl);
                */

                currentTreeView = new JobTreeView(jobCategories, this.Manager);
                currentTreeView.Anchor = Anchors.All;
                currentTreeView.Top = 0;
                currentTreeView.Left = 0;
                currentTreeView.Width = ClientWidth;
                currentTreeView.Height = ClientHeight;
                currentTreeView.DetailScreenRequested += new EventHandler<EventArgs<IJob>>((sender, args) =>
                {
                    if (currentDetailsPanel != null)
                    {
                        removeDetailsPanel();
                    }

                    currentDetailsPanel = new DetailsPanel(this.Manager);
                    currentDetailsPanel.Left = ClientWidth - 150;
                    currentDetailsPanel.Top = 0;
                    currentDetailsPanel.Height = ClientHeight;
                    currentDetailsPanel.Width = 149;
                    currentDetailsPanel.Anchor = Anchors.Vertical | Anchors.Right;
                    currentDetailsPanel.OnClose += new System.EventHandler((csender, cargs) =>
                    {
                        removeDetailsPanel();
                    });
                    this.Add(currentDetailsPanel);
                    currentTreeView.Width = ClientWidth - 151;

                });
                this.Add(currentTreeView);
                /*
                var locType_LoweredPanel = new LoweredPanel(this.Manager);
                locType_LoweredPanel.Init();
                locType_LoweredPanel.Anchor = Anchors.Left | Anchors.Top; //Anchors.None;//Anchors.All;
                locType_LoweredPanel.Top = 20;//someSpachingControl.Top;
                locType_LoweredPanel.Left = 20;//someSpachingControl.Left + someSpachingControl.Width + someSpachingControl.Margins.Right + locType_LoweredPanel.Margins.Left;
                locType_LoweredPanel.Width = 400;//this.ClientWidth - locType_LoweredPanel.Left - locType_LoweredPanel.Margins.Right - 50;
                locType_LoweredPanel.Height = 400;//this.Height - locType_LoweredPanel.Top - locType_LoweredPanel.Margins.Bottom;
                locType_LoweredPanel.AutoScroll = true;
                //locType_LoweredPanel.VerticalScrollBarEnabled = true;
                //locType_LoweredPanel.AutoScroll = true;
                //locType_LoweredPanel.HorizontalScrollBarEnabled = false;
                //locType_LoweredPanel.VerticalScrollBarShow = true;
                //locType_LoweredPanel.Passive = true;
                //locType_LoweredPanel.CanFocus = false;
                //locType_LoweredPanel.ScrollTo(0, 0);
                this.Add(locType_LoweredPanel);


                var btn_start_pos = 0;
                for (var i = 0; i < Math.Min(board.Jobs.Count ,1 ); i++)
                {
                    var job = board.Jobs[i];
                    var btn = new Button(mgr);

                    btn.Init();
                    btn.Margins = new Margins(4, 1, 4, 1);
                    btn.Text = job.JobActionName() + " / " + job.JobName();
                    btn.Height = 20;
                    btn.Left = btn.Margins.Left;
                    btn.Width = locType_LoweredPanel.ClientWidth - btn.Margins.Horizontal;
                    btn.Tag = job;
                    //btn.Click += new EventHandler(scope.ce5224b95776bdc5898d16a2581945237);
                    btn_start_pos += btn.Margins.Top;
                    btn.Top = btn_start_pos;
                    btn_start_pos += btn.Height + btn.Margins.Bottom;
                    locType_LoweredPanel.Add(btn);
                    //this.locType_List1_Button.Add(scope.locType_Button);
                    //c272968faf6bfc086f05402f0e0fa08a4 += ceab7084a63442ec788a411cf031e074d.TotalWorth();
                }*/
            }
            public KingdomJobUI(Manager mgr)
                : base(mgr)
            {
            }
        }
    }
#endif
}
