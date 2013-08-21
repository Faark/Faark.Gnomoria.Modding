using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using Faark.Util;
using Faark.Util.Serialization;

using Game;
using GameLibrary;
using Microsoft.Xna.Framework;

#if false
    Content Mods are meant to be heavy helper classes, used by other mods. This ones are not fully finished (e.G. save related issues, or stockmanager&etc not showing them), with a lot of issues. Use at own risk, and if possible try to complete them.


namespace Faark.Gnomoria.Modding.ContentMods
{
    /// <summary>
    /// This mod will help you create Items.
    /// To create an item, use AddItemType on the static instance from your mods constructor. Also reference this support mod as your dependency.
    /// </summary>
    public class CustomRecipes : SupportMod
    {
        public static CustomRecipes Instance
        {
            get
            {
                return ModEnvironment.Mods.Get<CustomRecipes>();
            }
        }

        public CustomRecipes()
        {
            ModEnvironment.ResetSetupData += new EventHandler((sender, args) =>
            {
                newRecipes = new Dictionary<Type, string>();
            });
        }

        public override IEnumerable<IModification> Modifications
        {
            get
            {
                yield return new MethodHook(
                    typeof(GameEntityManager).GetConstructor(new Type[] { }),
                    Method.Of<GameEntityManager>(Hooks.OnCreate_GameEntityManager)
                    );
                yield return new MethodHook(
                    typeof(GameEntityManager).GetConstructor(new Type[] { typeof(System.IO.BinaryReader) }),
                    Method.Action<GameEntityManager, System.IO.BinaryReader>(Hooks.OnCreate_GameEntityManager)
                    );
                foreach (var el in newRecipes)
                {
                    break;
                    //yield return new EnumAddElement(typeof(GameLibrary.ItemID), el.Value);
                }
            }
        }
        public override string SetupData
        {
            get
            {
                return JSON.ToJSON(newRecipes.Select(kvp => kvp.Key.AssemblyQualifiedName).ToList());
            }
            set
            {
                foreach (var el in JSON.FromJSON<List<String>>(value))
                {
                    var t = Type.GetType(el);
                    Instance.newRecipes.Add(t, t.Name.Replace(".", "").Replace("_", "").Replace("+", ""));
                    //throw new NotImplementedException("It actually works?");
                }
            }
        }
        public override IEnumerable<ModType> InitAfter
        {
            get
            {
                yield return new ModType(typeof(CustomItems));
            }
        }

        // Todo: remove "new" once we finally are rid of Hooks {get}....
        public static new class Hooks
        {
            public static HashSet<ICustomRecipeProvider> CustomRecipeProviders;
            private static void Init(GameEntityManager self)
            {
                if (Instance.newRecipes == null)
                {
                    // we already loaded it into the definition. In that specific case we only have to do it once...
                    return;
                }
                if (Instance.newRecipes.Count <= 0)
                {
                    throw new Exception("No Item providers specified, configuration appears to be missing.");
                }
                CustomRecipeProviders = new HashSet<ICustomRecipeProvider>();
                foreach (var el in Instance.newRecipes)
                {
                    CustomRecipeProviders.Add((ICustomRecipeProvider)Activator.CreateInstance(el.Key));
                }
                var workshopDatas = new Dictionary<WorkshopID, Tuple<WorkshopDef, List<CraftableItem>>>();
                foreach (var crp in CustomRecipeProviders)
                {
                    var recipe = crp.GetCraftableItem(self);
                    foreach (var craftAt in crp.CraftedAt)
                    {
                        Tuple<WorkshopDef, List<CraftableItem>> wsData;
                        if (!workshopDatas.TryGetValue(craftAt, out wsData))
                        {
                            var wsdef = self.WorkshopDef(craftAt);
                            wsData = workshopDatas[craftAt] = Tuple.Create(wsdef, wsdef.CraftableItems.ToList());
                        }
                        wsData.Item2.Add(recipe);
                    }
                }
                foreach (var wsData in workshopDatas)
                {
                    wsData.Value.Item1.CraftableItems = wsData.Value.Item2.ToArray();
                }
                Instance.newRecipes = null;
            }
            public static void OnCreate_GameEntityManager(GameEntityManager self)
            {
                Init(self);
            }
            public static void OnCreate_GameEntityManager(GameEntityManager self, System.IO.BinaryReader reader)
            {
                Init(self);
            }
        }


        #region public stuff
        private Dictionary<Type, string> newRecipes = new Dictionary<Type, string>();
        public void AddNewRecipeType<T>(/*string sysName = null*/) where T : ICustomRecipeProvider, new()
        {
            newRecipes[typeof(T)] = /*sysName ?? */typeof(T).Name.Replace(".", "").Replace("_", "").Replace("+", "");
            //throw new NotImplementedException();
        }
        public static void AddRecipeType<T>(/*string sysName = null*/) where T : ICustomRecipeProvider, new()
        {
            Instance.AddNewRecipeType<T>(/*sysName*/);
        }
        #endregion
    }

    public interface ICustomRecipeProvider
    {
        CraftableItem GetCraftableItem(GameEntityManager usedMgr = null);
        IEnumerable<WorkshopID> CraftedAt { get; }
    }
    public abstract class CustomRecipeProvider : ICustomRecipeProvider
    {
        public static CraftableItem CopyCraftableItem(CraftableItem toCopy)
        {
            var copy = new CraftableItem();
            copy.AttributeUsed = toCopy.AttributeUsed.ToArray();
            copy.BlueprintID = toCopy.BlueprintID;
            copy.Components = toCopy.Components.ToArray();
            copy.ConversionMaterial = toCopy.ConversionMaterial;
            copy.Difficulty = toCopy.Difficulty;
            copy.ItemID = toCopy.ItemID;
            copy.Quantity = toCopy.Quantity;
            copy.RequiredSkillLevel = toCopy.RequiredSkillLevel;
            copy.SkillUsed = toCopy.SkillUsed;
            return copy;
        }
        public abstract IEnumerable<WorkshopID> CraftedAt { get; }
        public abstract CraftableItem GetCraftableItem(GameEntityManager usedMgr);
    }
}
#endif