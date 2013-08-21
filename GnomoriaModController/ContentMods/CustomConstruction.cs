using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using System.Reflection;
using Faark.Util;
using Faark.Util.Serialization;

using Game;
using GameLibrary;
using Microsoft.Xna.Framework;
#if false

    Content Mods are meant to be heavy helper classes, used by other mods. This ones are not fully finished (e.G. save related issues, or stockmanager&etc not showing them), with a lot of issues. Use at own risk, and if possible try to complete them.

namespace Faark.Gnomoria.Modding.ContentMods
{
    public class CustomConstructions: SupportMod
    {
        public CustomConstructions()
        {
            ModEnvironment.ResetSetupData += new EventHandler((sender, args) =>
            {
                newConstructions = new Dictionary<Type, string>();
            });
        }
        public static CustomConstructions Instance
        {
            get
            {
                return ModEnvironment.Mods.Get<CustomConstructions>();
            }
        }
        public override IEnumerable<IModification> Modifications
        {
            get
            {                
                #region GameEntityManager & Item IDs
                yield return new MethodHook(
                    typeof(GameEntityManager).GetConstructor(new Type[] { }),
                    Method.Of<GameEntityManager>(Hooks.OnCreate_GameEntityManager)
                    );
                yield return new MethodHook(
                    typeof(GameEntityManager).GetConstructor(new Type[] { typeof(System.IO.BinaryReader) }),
                    Method.Action<GameEntityManager, System.IO.BinaryReader>(Hooks.OnCreate_GameEntityManager)
                    );
                foreach (var el in newConstructions)
                {
                    yield return new EnumAddElement(typeof(GameLibrary.ConstructionID), el.Value);
                }
                #endregion
                #region Actual construction
                yield return new ClassCreationHook(
                    typeof(Game.Construction),
                    typeof(Game.BuildConstructionJob).GetMethod("Complete", BindingFlags.Public | BindingFlags.Instance),
                    Method.Of<Vector3, ConstructionID, List<Item>, Construction>(Hooks.Intercept_BuildConstructionJob_Complete_ConstructionObject)
                    );
                #endregion
            }
        }
        public override IEnumerable<ModDependency> Dependencies
        {
            get
            {
                yield return ModEnvironment.Mods.Get<HelperMods.ModRightClickMenu>();
            }
        }
        public override string SetupData
        {
            get
            {
                return SerializableDataBag.ToJSON(newConstructions.Select(kvp => Tuple.Create(kvp.Value ?? kvp.Key.Name, kvp.Key.AssemblyQualifiedName)));
            }
            set
            {
                foreach (var el in SerializableDataBag<string>.FromJSON(value))
                {
                    newConstructions.Add(Type.GetType(el.Value, true), el.Key);
                }
            }
        }
        public override void Initialize_PreGame()
        {
            foreach (var el in newConstructions)
            {
                HelperMods.ModRightClickMenu.AddItem("Build " + (el.Value ?? el.Key.Name) + " at click pos", () =>
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
                    var type = Hooks.CustomConstructionProvider.Single(kvp=>kvp.Value.GetType() == el.Key);
                    if (type.Value.IsSuitablePos(pos))
                    {
                        var conDef = GnomanEmpire.Instance.EntityManager.ConstructionDef(type.Key);
                        GnomanEmpire.Instance.Fortress.JobBoard.AddJob(new BuildConstructionJob(pos, new BuildConstructionJobData(type.Key)) { RequiredComponents = conDef.Components.Select(comp => new JobComponent(comp.ID, (int)Material.Count)).ToList() });
                    }
                });
            }
        }
        // Todo: Remove new once Hooks { get } is gone.
        public static new class Hooks
        {
            #region GameEntityManager
            public static Dictionary<ConstructionID, ICustomConstructionProvider> CustomConstructionProvider;
            public static int FindMaxConstructionID()
            {
                var maxConstrId = 0;
                foreach (int id in Enum.GetValues(typeof(ConstructionID)))
                {
                    maxConstrId = Math.Max(maxConstrId, id);
                }
                return maxConstrId;
            }
            private static int DefaultConstructionCount;
            private static void Init(GameEntityManager self)
            {
                if (Instance.newConstructions.Count <= 0)
                {
                    throw new Exception("No Construction providers specified, configuration appears to be missing.");
                }
                var newConstrTypes = new Dictionary<Type, string>(Instance.newConstructions);
                var constrDefField = typeof(GameEntityManager)
                    .GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                    .Single(field => field.FieldType == typeof(ConstructionDef[]));

                var existingConstrDefs = (ConstructionDef[])constrDefField.GetValue(self);
                var maxConstrId = FindMaxConstructionID();
                DefaultConstructionCount = existingConstrDefs.Length;
                if (existingConstrDefs.Length < (maxConstrId + 1))
                {
                    var newList = new ConstructionDef[maxConstrId + 1];
                    for (var i = 0; i < existingConstrDefs.Length; i++)
                    {
                        newList[i] = existingConstrDefs[i];
                    }
                    existingConstrDefs = newList;
                }
                CustomConstructionProvider = new Dictionary<ConstructionID, ICustomConstructionProvider>();
                foreach (ConstructionID constructionId in Enum.GetValues(typeof(ConstructionID)))
                {
                    if (newConstrTypes.ContainsValue(constructionId.ToString()))
                    {
                        var foundEl = newConstrTypes.Single(el => el.Value == constructionId.ToString());
                        var newProvider = CustomConstructionProvider[constructionId] = (ICustomConstructionProvider)Activator.CreateInstance(foundEl.Key);
                        newProvider.OnProviderCreated(constructionId);
                        existingConstrDefs[(int)constructionId] = newProvider.GetConstructionDefinition(self);
                        newConstrTypes.Remove(foundEl.Key);
                    }
                }
                constrDefField.SetValue(self, existingConstrDefs);
                if (newConstrTypes.Count > 0)
                {
                    throw new Exception("Could not refer all mods to ItemIDs. First missing is type [" + newConstrTypes.First().Key.FullName + "]");
                }
            }
            public static void OnCreate_GameEntityManager(GameEntityManager self)
            {
                Init(self);
            }
            public static void OnCreate_GameEntityManager(GameEntityManager self, System.IO.BinaryReader reader)
            {
                Init(self);
            }
            #endregion
            public static Construction Intercept_BuildConstructionJob_Complete_ConstructionObject(Vector3 position, ConstructionID constructionID, List<Item> components)
            {
                ICustomConstructionProvider conProv;
                if (CustomConstructionProvider.TryGetValue(constructionID, out conProv))
                {
                    return conProv.CreateConstruction(position, constructionID, components);
                }
                return new Construction(position, constructionID, components);
            }
        }


        private Dictionary<Type, string> newConstructions = new Dictionary<Type, string>();
        public void AddNewConstructionType<T>(/*string sysName = null*/) where T : ICustomConstructionProvider, new()
        {
            newConstructions[typeof(T)] = /*sysName ?? */typeof(T).Name.Replace(".", "").Replace("_", "").Replace("+", "");
            //throw new NotImplementedException();
        }
        public static void AddConstructionType<T>(/*string sysName = null*/) where T : ICustomConstructionProvider, new()
        {
            Instance.AddNewConstructionType<T>(/*sysName*/);
        }
    }
    public interface ICustomConstructionProvider
    {
        ConstructionID ConstructionID { get; }
        CustomConstruction CreateConstruction(Vector3 position, ConstructionID constructionID, List<Item> components);
        void OnProviderCreated(ConstructionID constructionId);
        Construction CreateSaveableFallbackConstruction();

        #region Construction
        //String ConstructionName(int materialId, Construction construction = null);
        String GroupName { get; }
        #endregion
        #region WorkshopDef
        WorkshopID ToWorkshopID { get; }
        #endregion
        #region MechanismDef
        MechanismID ToMechanismID { get; }
        #endregion


        ConstructionDef GetConstructionDefinition(GameEntityManager self);
        Boolean IsSuitablePos(Vector3 pos);
    }
    
    public abstract class CustomConstructionProvider : ICustomConstructionProvider
    {

        public static ConstructionDef CreateDefinitionCopy(ConstructionID from, GameEntityManager gem = null)
        {
            gem = gem ?? GnomanEmpire.Instance.EntityManager;
            if (gem == null)
            {
                throw new InvalidOperationException("No GameEntityManager given or does currently exist.");
            }
            var fromDef = gem.ConstructionDef(from);
            var newDef = new ConstructionDef();
            newDef.Animation = fromDef.Animation.Select(el => new AnimFrame() { TileIDs = el.TileIDs, Time = el.Time }).ToArray();
            newDef.BlueprintID = fromDef.BlueprintID;
            newDef.BuildAdjacent= fromDef.BuildAdjacent;
            newDef.Components = fromDef.Components.Select(el => new ItemComponent() { AllowedMaterials = el.AllowedMaterials.ToList(), ID = el.ID, Quantity = el.Quantity }).ToArray();
            newDef.Description = fromDef.Description;
            newDef.Destructible = fromDef.Destructible;
            newDef.Effects = fromDef.Effects.Select(el => new ItemEffect() { Amount = el.Amount, Effect = el.Effect }).ToArray();
            newDef.GroupName = fromDef.GroupName;
            newDef.Name = fromDef.Name;
            newDef.Prefix = fromDef.Prefix;
            newDef.Properties = new ConstructionProperties(fromDef.Properties.Flags);
            newDef.Rotates = fromDef.Rotates;
            newDef.TileIDs = fromDef.TileIDs.Select(el => new TileDesc() { MaterialIndex = el.MaterialIndex, TileID = el.TileID, TileIDByMaterial = el.TileIDByMaterial }).ToArray();
            newDef.ToolTip = fromDef.ToolTip;
            newDef.Value = fromDef.Value;
            return newDef;
        }



        //public ConstructionID FallbackConstructionID { get; }
        public ConstructionID ConstructionID { get; private set; }
        public virtual WorkshopID ToWorkshopID { get { return WorkshopID.Count; } }
        public virtual MechanismID ToMechanismID { get { return MechanismID.Count; } }
        public virtual void OnProviderCreated(ConstructionID id)
        {
            ConstructionID = id;
        }
        public virtual string GroupName { get { return null; } }

        public Construction CreateSaveableFallbackConstruction()
        {
            throw new NotImplementedException();
        }
        public abstract CustomConstruction CreateConstruction(Vector3 position, ConstructionID constructionID, List<Item> components);
        public abstract ConstructionDef GetConstructionDefinition(GameEntityManager usedManager);
        public virtual bool IsSuitablePos(Vector3 pos)
        {
            var cell = GnomanEmpire.Instance.Map.GetCell(pos);
            return !cell.HasWall() && cell.HasFloor() && (!cell.HasDesignation() || cell.Designation.AllowConstructions) && GnomanEmpire.Instance.Map.IsWalkable(pos);
        }
        // fallback-IDs: Walkable: Torch, NonWalkable: Pillar :)
    }
    public abstract class SimpleDrawableConstructionProvider : CustomConstructionProvider
    {
        public abstract SimpleDrawableComponent[] GetDrawables();
        public override CustomConstruction CreateConstruction(Vector3 position, ConstructionID constructionID, List<Item> components)
        {
            return new CustomConstruction(GetDrawables(), position, constructionID, components);
        }
    }
    public class CustomConstruction : Game.Construction
    {
        public Animation Animation { get { return base.mAnimation; } }
        public List<Item> Components { get { return base.mComponents; } }
        public ConstructionDef ConstructionDef { get { return base.mConstructionDef; } }

        public SimpleDrawableComponent[] Drawables { get; protected set; }
        public CustomConstruction(SimpleDrawableComponent[] drawables, Vector3 position, ConstructionID constrId, List<Item> components)
            : base(position, constrId, components)
        {
            Drawables = drawables;
            //throw new NotImplementedException();
        }
        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch, Vector3 position)
        {
            SimpleDrawableComponent.DrawConstruction(spriteBatch, this);
        } 
        public override void Serialize(System.IO.BinaryWriter writer)
        {
            throw new Exception("Basic game is trying to serialize a custom item. This cannot happen, since it would make the savegame incompatible for vanilla grepolis.");
        }
    }
}
#endif