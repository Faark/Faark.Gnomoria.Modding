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
    /// <summary>
    /// This mod will help you create Items.
    /// To create an item, use AddItemType on the static instance from your mods constructor. Also reference this support mod as your dependency.
    /// </summary>
    public class CustomItems : SupportMod
    {
        public static CustomItems Instance
        {
            get
            {
                return ModEnvironment.Mods.Get<CustomItems>();
            }
        }

        public CustomItems()
        {
            ModEnvironment.ResetSetupData += new EventHandler((sender, args) =>
            {
                newItemTypes = new Dictionary<Type, string>();
            });
        }

        /*
        public static void TEMPLOADSTOCKMANGER(StockManager self, System.IO.BinaryReader reader)
        {
            try
            {
                GameEntityManager entityManager = GnomanEmpire.Instance.EntityManager;
                int num = Hooks.FindMaxItemID() + 1;
                var cf56aff2238f1af02e43639e3a7d9965f = new ItemsByQuality[160];
                var c1e1aa1d41f9a0420c53e30f691552fff = new ItemsByQuality[160];
                typeof(StockManager).GetFields(BindingFlags.NonPublic | BindingFlags.Instance).Single(field => field.Name == "cf56aff2238f1af02e43639e3a7d9965f").SetValue(self, cf56aff2238f1af02e43639e3a7d9965f);
                typeof(StockManager).GetFields(BindingFlags.NonPublic | BindingFlags.Instance).Single(field => field.Name == "c1e1aa1d41f9a0420c53e30f691552fff").SetValue(self, c1e1aa1d41f9a0420c53e30f691552fff);
                for (int i = 0; i < num; i++)
                {
                    int num2 = reader.ReadInt32();
                    int num3 = (int)Item.ConvertItemID(i, true);
                    if (num2 > 0)
                    {
                        cf56aff2238f1af02e43639e3a7d9965f[num3] = new ItemsByQuality();
                        c1e1aa1d41f9a0420c53e30f691552fff[num3] = new ItemsByQuality();
                    }
                    for (int j = 0; j < num2; j++)
                    {
                        ItemQuality itemQuality = (ItemQuality)reader.ReadInt32();
                        int num4 = reader.ReadInt32();
                        if (num4 > 0)
                        {
                            for (int k = 0; k < num4; k++)
                            {
                                int material = (int)Map.ConvertMaterialID(reader.ReadInt32());
                                int num5 = reader.ReadInt32();
                                cf56aff2238f1af02e43639e3a7d9965f[num3].AddItem(itemQuality, material);
                                for (int l = 0; l < num5; l++)
                                {
                                    Item item = entityManager.Entity(reader.ReadUInt32()) as Item;
                                    if (item != null)
                                    {
                                        cf56aff2238f1af02e43639e3a7d9965f[num3].AddItem(item, itemQuality, material);
                                        if (!item.InStockpile)
                                        {
                                            c1e1aa1d41f9a0420c53e30f691552fff[num3].AddItem(item, itemQuality, material);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                int num6 = reader.ReadInt32();
                var stocks = new List<Stockpile>(num6);
                typeof(StockManager).GetFields(BindingFlags.Instance | BindingFlags.NonPublic).Single(field => field.FieldType == typeof(List<Stockpile>)).SetValue(self, stocks);
                //stocks = new List<Stockpile>(num6);
                for (int m = 0; m < num6; m++)
                {
                    stocks.Add(new Stockpile(reader));
                }
                self.Food = new Game.Common.GameProperty<int>(reader.ReadInt32());
                self.Drink = new Game.Common.GameProperty<int>(reader.ReadInt32());
            }
            catch (Exception err)
            {
                err.ToString();
                throw;
            }
        }
        */

        public override IEnumerable<IModification> Modifications
        {
            get
            {

                /*yield return new MethodHook(
                    typeof(StockManager).GetConstructor(new Type[] { typeof(System.IO.BinaryReader) }),
                    Method.Of<StockManager, System.IO.BinaryReader>(TEMPLOADSTOCKMANGER),
                    MethodHookType.Replace
                    );*/
                #region GameEntityManager
                yield return new MethodHook(
                    typeof(GameEntityManager).GetConstructor(new Type[] { }),
                    Method.Of<GameEntityManager>(Hooks.OnCreate_GameEntityManager)
                    );
                yield return new MethodHook(
                    typeof(GameEntityManager).GetConstructor(new Type[] { typeof(System.IO.BinaryReader) }),
                    Method.Action<GameEntityManager, System.IO.BinaryReader>(Hooks.OnCreate_GameEntityManager)
                    );
                #endregion
                #region StockManager
                yield return new MethodHook(
                    typeof(StockManager).GetConstructor(new Type[] { }),
                    Method.Of<StockManager>(Hooks.OnCreate_StockManager)
                    );
                yield return new MethodHook(
                    typeof(StockManager).GetConstructor(new Type[] { typeof(System.IO.BinaryReader) }),
                    Method.Action<StockManager, System.IO.BinaryReader>(Hooks.OnCreate_StockManager)
                    );
                yield return new MethodHook(
                    typeof(StockManager).GetMethod("OnSerializationComplete"),
                    Method.Of<StockManager>(Hooks.On_StockManager_OnSerializationComplete)
                    );
                yield return new MethodHook(
                    typeof(StockManager).GetMethod("Serialize", BindingFlags.Instance | BindingFlags.Public),
                    Method.Of<StockManager, System.IO.BinaryWriter>(Hooks.OnBefore_StockManager_Serialize),
                    MethodHookType.RunBefore
                    );
                yield return new MethodHook(
                    typeof(StockManager).GetMethod("Serialize", BindingFlags.Instance | BindingFlags.Public),
                    Method.Of<StockManager, System.IO.BinaryWriter>(Hooks.OnAfter_StockManager_Serialize),
                    MethodHookType.RunAfter
                    );
                #endregion

                #region WeaponDef
                yield return new MethodHook(
                    typeof(WeaponDef).GetMethod("IsItemWeapon", BindingFlags.Static | BindingFlags.Public),
                    Method.Of<bool, ItemID, bool>(Hooks.On_WeaponDef_IsItemWeapon)
                    );
                yield return new MethodHook(
                    typeof(WeaponDef).GetMethod("IsItemArmor", BindingFlags.Static | BindingFlags.Public),
                    Method.Of<bool, ItemID, bool>(Hooks.On_WeaponDef_IsItemArmor)
                    );
                yield return new MethodHook(
                    typeof(WeaponDef).GetMethod("IsItem2HWeapon", BindingFlags.Static | BindingFlags.Public),
                    Method.Of<bool, ItemID, bool>(Hooks.On_WeaponDef_IsItem2HWeapon)
                    );
                #endregion
                #region StorageDef
                yield return new MethodHook(
                    typeof(StorageDef).GetMethod("StorageContainerID", BindingFlags.Static | BindingFlags.Public),
                    Method.Of<StorageID, ItemID, StorageID>(Hooks.On_StorageDef_StorageContainerID)
                    );
                #endregion
                #region ItemDef
                yield return new MethodHook(
                    typeof(ItemDef).GetMethod("ResourcePileID", BindingFlags.Static | BindingFlags.Public),
                    Method.Of<ItemID, ItemID, ItemID>(Hooks.On_ItemDef_ResourcePileID)
                    );
                yield return new MethodHook(
                    typeof(ItemDef).GetMethod("IsWeaponOrArmor", BindingFlags.Static | BindingFlags.Public),
                    Method.Of<bool, ItemID, bool>(Hooks.On_ItemDef_IsWeaponOrArmor)
                    );
                yield return new MethodHook(
                    typeof(ItemDef).GetMethod("ConvertsToResourcePile", BindingFlags.Static | BindingFlags.Public),
                    Method.Of<bool, ItemID, bool>(Hooks.On_ItemDef_ConvertsToResourcePile)
                    );
                yield return new MethodHook(
                    typeof(ItemDef).GetMethod("AllArmor", BindingFlags.Static | BindingFlags.Public),
                    Method.Of<List<ItemID>, List<ItemID>>(Hooks.On_ItemDef_AllArmor)
                    );
                yield return new MethodHook(
                    typeof(ItemDef).GetMethod("AllFurniture", BindingFlags.Static | BindingFlags.Public),
                    Method.Of<List<ItemID>, List<ItemID>>(Hooks.On_ItemDef_AllFurniture)
                    );
                yield return new MethodHook(
                    typeof(ItemDef).GetMethod("AllWeapons", BindingFlags.Static | BindingFlags.Public),
                    Method.Of<List<ItemID>, List<ItemID>>(Hooks.On_ItemDef_AllWeapons)
                    );
                #endregion
                #region AmmoDef
                yield return new MethodHook(
                    typeof(AmmoDef).GetMethod("ItemIDToAmmoType", BindingFlags.Static | BindingFlags.Public),
                    Method.Of<AmmoType, ItemID, AmmoType>(Hooks.On_AmmoDef_ItemIDToAmmoType)
                    );
                yield return new MethodHook(
                    typeof(AmmoDef).GetMethod("IsAmmoContainer", BindingFlags.Static | BindingFlags.Public),
                    Method.Of<bool, ItemID, bool>(Hooks.On_AmmoDef_IsAmmoContainer)
                    );
                yield return new MethodHook(
                    typeof(AmmoDef).GetMethod("ContainersByAmmoID", BindingFlags.Static | BindingFlags.Public),
                    Method.Of<List<ItemID>, ItemID, List<ItemID>>(Hooks.On_AmmoDef_ContainersByAmmoID)
                    );
                #endregion

                #region Item Creation Hooks
                yield return new MethodRefHook(
                    typeof(Fortress).GetMethod("CreateItem", BindingFlags.Instance | BindingFlags.Public),
                    typeof(Hooks).GetMethod("On_Fortress_CreateItem")
                    );
                yield return new BeforeAndAfterMethodHook(
                    typeof(CraftItemJob).GetMethod("Complete", BindingFlags.Instance | BindingFlags.Public),
                    Method.Of<CraftItemJob, Character>(Hooks.OnBefore_CraftItemJob_Complete),
                    Method.Of<CraftItemJob, Character>(Hooks.OnAfter_CraftItemJob_Complete)
                    );
                yield return new MethodHook(
                    typeof(StorageContainer).GetMethod("AddItem", BindingFlags.Instance | BindingFlags.Public),
                    Method.Of<StorageContainer, Item>(Hooks.On_StorageContainer_AddItem)
                    );
                yield return new MethodHook(
                    typeof(AIDirector).GetMethod("GenerateItem", BindingFlags.Instance | BindingFlags.Public),
                    Method.Of<Item, AIDirector, Vector3, ItemID, Material, ItemQuality, Item>(Hooks.On_AIDirector_GenerateItem)
                    );
                yield return new MethodHook(
                    typeof(Body).GetMethod("FarmItems", BindingFlags.Instance | BindingFlags.Public),
                    typeof(Hooks).GetMethod("On_Body_FarmItems", BindingFlags.Static | BindingFlags.Public)
                    );
                yield return new MethodHook(
                    typeof(BodyPart).GetMethod("HarvestItems", BindingFlags.Instance | BindingFlags.Public),
                    typeof(Hooks).GetMethod("On_BodyPart_HarvestItems", BindingFlags.Static | BindingFlags.Public)
                    );
                yield return new BeforeAndAfterMethodHook(
                    typeof(Crop).GetMethod("Forage", BindingFlags.Instance | BindingFlags.Public),
                    Method.Of<Crop>(Hooks.OnBefore_Crop_Forage),
                    Method.Of<Crop>(Hooks.OnAfter_Crop_Forage)
                    );
                yield return new BeforeAndAfterMethodHook(
                    typeof(FellTreeJob).GetMethod("Complete", BindingFlags.Instance | BindingFlags.Public),
                    Method.Of<FellTreeJob, Character>(Hooks.OnBefore_FellTreeJob_Complete),
                    Method.Of<FellTreeJob, Character>(Hooks.OnAfter_FellTreeJob_Complete)
                    );
                yield return new MethodHook(
                    typeof(Map).GetMethod("RemoveFloor", new Type[] { typeof(int), typeof(int), typeof(int), typeof(bool) }),
                    Method.Of<Item, Map, int, int, int, bool, Item>(Hooks.On_Map_RemoveFloor)
                    );
                yield return new MethodHook(
                    typeof(Map).GetMethod("RemoveWall", new Type[] { typeof(int), typeof(int), typeof(int), typeof(bool) }),
                    Method.Of<Item, Map, int, int, int, bool, Item>(Hooks.On_Map_RemoveWall)
                    );
                yield return new BeforeAndAfterMethodHook(
                    typeof(Mineral).GetMethod("OnDelete", BindingFlags.Instance | BindingFlags.Public),
                    Method.Of<Mineral>(Hooks.OnBefore_Mineral_OnDelete),
                    Method.Of<Mineral>(Hooks.OnAfter_Mineral_OnDelete)
                    );
                yield return new BeforeAndAfterMethodHook(
                    typeof(ReplaceFloorJob).GetMethod("Complete", BindingFlags.Instance | BindingFlags.Public),
                    Method.Of<ReplaceFloorJob, Character>(Hooks.OnBefore_ReplaceFloorJob_Complete),
                    Method.Of<ReplaceFloorJob, Character>(Hooks.OnAfter_ReplaceFloorJob_Complete)
                    );
                yield return new BeforeAndAfterMethodHook(
                    typeof(ReplaceWallJob).GetMethod("Complete", BindingFlags.Instance | BindingFlags.Public),
                    Method.Of<ReplaceWallJob, Character>(Hooks.OnBefore_ReplaceWallJob_Complete),
                    Method.Of<ReplaceWallJob, Character>(Hooks.OnAfter_ReplaceWallJob_Complete)
                    );
                yield return new BeforeAndAfterMethodHook(
                    typeof(Sapling).GetMethod("Deconstruct", BindingFlags.Instance | BindingFlags.Public),
                    Method.Of<Sapling>(Hooks.OnBefore_Sappling_Deconstruct),
                    Method.Of<Sapling>(Hooks.OnAfter_Sappling_Deconstruct)
                    );
                yield return new MethodHook(
                    typeof(Tree).GetMethod("SpawnClipping", BindingFlags.Instance | BindingFlags.Public),
                    Method.Of<Item, Tree, Item>(Hooks.On_Tree_SpawnClipping)
                    );
                yield return new MethodHook(
                    typeof(Tree).GetMethod("SpawnFruit", BindingFlags.Instance | BindingFlags.Public),
                    Method.Of<Item, Tree, Item>(Hooks.On_Tree_SpawnFruit)
                    );
                #endregion

                foreach (var el in newItemTypes)
                {
                    yield return new EnumAddElement(typeof(GameLibrary.ItemID), el.Value);
                }
            }
        }
        public override string SetupData
        {
            get
            {
                return SerializableDataBag.ToJSON(newItemTypes.Select(kvp => Tuple.Create(kvp.Value ?? kvp.Key.Name, kvp.Key.AssemblyQualifiedName)));
            }
            set
            {
                foreach (var el in SerializableDataBag<string>.FromJSON(value))
                {
                    Instance.newItemTypes.Add(Type.GetType(el.Value, true), el.Key);
                    //throw new NotImplementedException("It actually works?");
                }
            }
        }

        // Todo: Stockpile integration.

        // Todo: remove "new" once we finally are rid of Hooks {get}....
        public static new class Hooks
        {
            #region GameEntityManager and StockManager
            public static int FindMaxItemID()
            {
                var maxItemId = 0;
                foreach (int id in Enum.GetValues(typeof(ItemID)))
                {
                    maxItemId = Math.Max(maxItemId, id);
                }
                return maxItemId;
            }
            // got it? #warning Fix Stockmanager and ItemsByItemID!
            public static Dictionary<ItemID, ICustomItemProvider> CustomItemTypes;
            public static int DefaultItemIdCount;
            private static void Init_GameEntityManager(GameEntityManager self)
            {
                if (Instance.newItemTypes.Count <= 0)
                {
                    throw new Exception("No Item providers specified, configuration appears to be missing.");
                }
                var newItemTypes = new Dictionary<Type, string>(Instance.newItemTypes);
                var itemDefField = typeof(GameEntityManager)
                    .GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                    .Single(field => field.FieldType == typeof(ItemDef[]));

                var existingItemDefs = (ItemDef[])itemDefField.GetValue(self);
                var maxItemId = FindMaxItemID();
                DefaultItemIdCount = existingItemDefs.Length;
                if (existingItemDefs.Length < (maxItemId + 1))
                {
                    var newList = new ItemDef[maxItemId + 1];
                    for (var i = 0; i < existingItemDefs.Length; i++)
                    {
                        newList[i] = existingItemDefs[i];
                    }
                    existingItemDefs = newList;
                }
                CustomItemTypes = new Dictionary<ItemID, ICustomItemProvider>();
                foreach (ItemID itemId in Enum.GetValues(typeof(ItemID)))
                {
                    if (newItemTypes.ContainsValue(itemId.ToString()))
                    {
                        var foundEl = newItemTypes.Single(el => el.Value == itemId.ToString());
                        var newProvider = CustomItemTypes[itemId] = (ICustomItemProvider)Activator.CreateInstance(foundEl.Key);
                        newProvider.OnProviderCreated(itemId);
                        existingItemDefs[(int)itemId] = newProvider.GetItemDefinition(self);
                        newItemTypes.Remove(foundEl.Key);
                    }
                }
                itemDefField.SetValue(self, existingItemDefs);
                if (newItemTypes.Count > 0)
                {
                    throw new Exception("Could not refer all mods to ItemIDs. First missing is type [" + newItemTypes.First().Key.FullName + "]");
                }
            }
            public static void OnCreate_GameEntityManager(GameEntityManager self)
            {
                Init_GameEntityManager(self);
            }
            public static void OnCreate_GameEntityManager(GameEntityManager self, System.IO.BinaryReader reader)
            {
                Init_GameEntityManager(self);
            }
            private static List<Tuple<uint, CustomItem>> savingTemp_replacedItems = null;
            public static void OnBefore_GameEntityManager_Serialize(GameEntityManager self, System.IO.BinaryWriter writer)
            {
                var fallbackItems = self.Entities.Where(el => el.Value is CustomItem).Select(el => new
                {
                    id = el.Key,
                    item = ((CustomItem)el.Value),
                    fallback = (((CustomItem)el.Value).CreateFallbackSaveableItem())
                }).ToList();
                savingTemp_replacedItems = new List<Tuple<uint, CustomItem>>();
                foreach (var el in fallbackItems)
                {
                    savingTemp_replacedItems.Add(new Tuple<uint, CustomItem>(el.id, el.item));
                    el.fallback.ID = el.id;
                    self.Entities[el.id] = el.fallback;
                }
            }
            public static void OnAfter_GameEntityManager_Serialize(GameEntityManager self, System.IO.BinaryWriter writer)
            {
                foreach (var el in savingTemp_replacedItems)
                {
                    self.Entities[el.Item1] = el.Item2;
                }
                savingTemp_replacedItems = null;
            }
            private static void Init_StockManager(StockManager self)
            {
                var fields = typeof(StockManager).GetFields(BindingFlags.Instance | BindingFlags.NonPublic).Where(field => field.FieldType == typeof(ItemsByQuality[])).ToArray();
                var maxItemId = FindMaxItemID();
                foreach (var field in fields)
                {
                    var itemList = (ItemsByQuality[])field.GetValue(self);
                    if (itemList.Length <= maxItemId)
                    {
                        var newItemList = new ItemsByQuality[maxItemId + 1];
                        for (var i = 0; i < itemList.Length; i++)
                        {
                            newItemList[i] = itemList[i];
                        }
                        field.SetValue(self, newItemList);
                    }
                }
            }
            public static void OnCreate_StockManager(StockManager self)
            {
                Init_StockManager(self);
            }
            public static void OnCreate_StockManager(StockManager self, System.IO.BinaryReader reader)
            {
                Init_StockManager(self);
            }
            public static void On_StockManager_OnSerializationComplete(StockManager self)
            {
                if (GnomanEmpire.Instance.LoadingSaveVersion < 19u)
                {
                    Init_StockManager(self);
                }
            }
            private static Dictionary<FieldInfo, ItemsByQuality[]> customItemList_cache_while_saving;
            public static void OnBefore_StockManager_Serialize(StockManager self, System.IO.BinaryWriter writer)
            {
                customItemList_cache_while_saving = new Dictionary<FieldInfo, ItemsByQuality[]>();
                var fields = typeof(StockManager).GetFields(BindingFlags.Instance | BindingFlags.NonPublic).Where(field => field.FieldType == typeof(ItemsByQuality[])).ToArray();
                foreach (var field in fields)
                {
                    var customList = customItemList_cache_while_saving[field] = (ItemsByQuality[])field.GetValue(self);
                    var originalList = new ItemsByQuality[DefaultItemIdCount];
                    for (var i = 0; i < DefaultItemIdCount; i++)
                    {
                        originalList[i] = customList[i];
                    }
                    field.SetValue(self, originalList);
                }
            }
            public static void OnAfter_StockManager_Serialize(StockManager self, System.IO.BinaryWriter writer)
            {
                foreach (var kvp in customItemList_cache_while_saving)
                {
                    kvp.Key.SetValue(self, kvp.Value);
                }
            }
            #endregion
            #region Defs and similar stuff
            public static bool On_WeaponDef_IsItemWeapon(bool val, ItemID item)
            {
                ICustomItemProvider prov;
                if (CustomItemTypes.TryGetValue(item, out prov))
                    return prov.IsItemWeapon;
                return val;
            }
            public static bool On_WeaponDef_IsItemArmor(bool val, ItemID item)
            {
                ICustomItemProvider prov;
                if (CustomItemTypes.TryGetValue(item, out prov))
                    return prov.IsItemArmor;
                return val;
            }
            public static bool On_WeaponDef_IsItem2HWeapon(bool val, ItemID item)
            {
                ICustomItemProvider prov;
                if (CustomItemTypes.TryGetValue(item, out prov))
                    return prov.IsItem2HWeapon;
                return val;
            }
            public static StorageID On_StorageDef_StorageContainerID(StorageID val, ItemID item)
            {
                ICustomItemProvider prov;
                if (CustomItemTypes.TryGetValue(item, out prov))
                    return prov.StorageContainerID;
                return val;
            }
            public static ItemID On_ItemDef_ResourcePileID(ItemID val, ItemID item)
            {
                ICustomItemProvider prov;
                if (CustomItemTypes.TryGetValue(item, out prov))
                    return prov.ResourcePileID;
                return val;
            }
            public static bool On_ItemDef_IsWeaponOrArmor(bool val, ItemID item)
            {
                ICustomItemProvider prov;
                if (CustomItemTypes.TryGetValue(item, out prov))
                    return prov.IsWeaponOrArmor;
                return val;
            }
            public static bool On_ItemDef_ConvertsToResourcePile(bool val, ItemID item)
            {
                ICustomItemProvider prov;
                if (CustomItemTypes.TryGetValue(item, out prov))
                    return prov.ConvertsToResourcePile;
                return val;
            }
            private static List<ItemID> allCustomArmor = null;
            public static List<ItemID> On_ItemDef_AllArmor(List<ItemID> val)
            {
                if (allCustomArmor == null)
                {
                    allCustomArmor = CustomItemTypes.Where(kvp => kvp.Value.IsItemArmor).Select(kvp => kvp.Key).ToList();
                }
                val.AddRange(allCustomArmor);
                return val;
            }
            private static List<ItemID> allCustomFurniture = null;
            public static List<ItemID> On_ItemDef_AllFurniture(List<ItemID> val)
            {
                if (allCustomFurniture == null)
                {
                    allCustomFurniture = CustomItemTypes.Where(kvp => kvp.Value.IsFurniture).Select(kvp => kvp.Key).ToList();
                }
                val.AddRange(allCustomFurniture);
                return val;
            }
            private static List<ItemID> allCustomWeapons = null;
            public static List<ItemID> On_ItemDef_AllWeapons(List<ItemID> val)
            {
                if (allCustomWeapons == null)
                {
                    allCustomWeapons = CustomItemTypes.Where(kvp => kvp.Value.IsItemWeapon).Select(kvp => kvp.Key).ToList();
                }
                val.AddRange(allCustomWeapons);
                return val;
            }
            public static AmmoType On_AmmoDef_ItemIDToAmmoType(AmmoType val, ItemID item)
            {
                ICustomItemProvider prov;
                if (CustomItemTypes.TryGetValue(item, out prov))
                    return prov.ItemIDToAmmoType;
                return val;
            }
            public static bool On_AmmoDef_IsAmmoContainer(bool val, ItemID item)
            {
                ICustomItemProvider prov;
                if (CustomItemTypes.TryGetValue(item, out prov))
                    return prov.IsAmmoContainer;
                return val;
            }
            public static List<ItemID> On_AmmoDef_ContainersByAmmoID(List<ItemID> val, ItemID item)
            {
                ICustomItemProvider prov;
                if (CustomItemTypes.TryGetValue(item, out prov))
                    return prov.ContainersByAmmoID;
                return val;
            }
            #endregion

            /*
             *
             * Replacing items... kind a sucks. Hope the game switches to some kind of factory system soon. For now we have multiple kinds:
             * 
             * - Func creates it, calls Fortress.CreateItem, returns it: Intercept return value and call mayExchangeSimple
             * - Func creates it, calls Fortress.CreateItem and does nothing with it anymore: set "InterceptFortressCreate" bool true, On_Fortress_CreateItem will handle it, set it back to false afterwards
             * - Func creates it, calls EntityManager.SpawnEntityImmediate, returns it:
             */
            #region item creation
            private static Item createFrom(Item createdItem, ICustomItemProvider provider)
            {
                if (createdItem is CustomItem)
                    return createdItem;
                var comps = createdItem.History.Components;
                Item customItem;
                if (comps.Count > 0)
                {
                    customItem = provider.CreateItem(createdItem.Position, createdItem.ItemID, comps.ToList());
                }
                else
                {
                    customItem = provider.CreateItem(createdItem.Position, createdItem.ItemID, createdItem.MaterialID);
                }
                customItem.Quality = createdItem.Quality;
                customItem.CrafterHistory = createdItem.CrafterHistory;
                return customItem;
            }
            private static Item mayExchangeFortCreateSimple(Item createdItem)
            {
                var itemId = createdItem.ItemID;
                ICustomItemProvider provider;
                if (CustomItemTypes.TryGetValue(itemId, out provider))
                {
                    var customItem = createFrom(createdItem, provider);
                    GnomanEmpire.Instance.Fortress.RemoveItem(createdItem);
                    GnomanEmpire.Instance.Fortress.CreateItem(customItem);
                    return customItem;
                }
                return createdItem;
            }
            private static Item mayExchangeMgrSpawnImmeSimple(Item createdItem)
            {
                ICustomItemProvider provider;
                if (CustomItemTypes.TryGetValue(createdItem.ItemID, out provider))
                {
                    var customItem = createFrom(createdItem, provider);
                    GnomanEmpire.Instance.Fortress.RemoveItem(createdItem);
                    GnomanEmpire.Instance.Fortress.CreateItem(customItem);
                    return customItem;
                }
                return createdItem;
            }
            private static bool InterceptFortressCreate = false;
            public static void On_Fortress_CreateItem(Fortress self, ref Item item)
            {
                if (InterceptFortressCreate)
                {
                    if (item is CustomItem)
                        return;
                    ICustomItemProvider provider;
                    if (CustomItemTypes.TryGetValue(item.ItemID, out provider))
                    {
                        item = createFrom(item, provider);
                        //self.RemoveItem(item);
                        //self.CreateItem(customItem);
                        //item = customItem;
                    }
                }
            }
            private static bool interceptStorageAdd = false;
            public static void OnBefore_CraftItemJob_Complete(CraftItemJob self, Character chr)
            {
                ICustomItemProvider provider;
                if (CustomItemTypes.TryGetValue(((CraftItemJobData)self.Data).CraftableItem.ItemID, out provider))
                {
                    if (provider.ConvertsToResourcePile)
                        throw new NotImplementedException("Can't create custom items that converts to resource piles, atm.");
                    InterceptFortressCreate = true;
                    interceptStorageAdd = true;
                }
            }
            public static void OnAfter_CraftItemJob_Complete(CraftItemJob self, Character chr)
            {
                InterceptFortressCreate = false;
                interceptStorageAdd = false;
            }
            public static void On_StorageContainer_AddItem(StorageContainer self, Item item)
            {
                if (self is ResourcePile && interceptStorageAdd && !(item is CustomItem))
                {
                    ICustomItemProvider provider;
                    if (CustomItemTypes.TryGetValue(item.ItemID, out provider))
                    {
                        var newItem = createFrom(item, provider);
                        self.RemoveItem(item);
                        GnomanEmpire.Instance.Fortress.RemoveItem(item);
                        GnomanEmpire.Instance.EntityManager.DeleteEntityImmediate(item);
                        GnomanEmpire.Instance.EntityManager.SpawnEntityImmediate(newItem);
                        GnomanEmpire.Instance.Fortress.AddItem(newItem);
                        newItem.Cell().RemoveObject(newItem);
                        self.AddItem(newItem);
                    }
                }
            }
            public static Item On_AIDirector_GenerateItem(Item item, AIDirector self, Vector3 position, ItemID itemID, Material material, ItemQuality quality = ItemQuality.Average)
            {
                ICustomItemProvider provider;
                if (CustomItemTypes.TryGetValue(item.ItemID, out provider))
                {
                    return createFrom(item, provider);
                }
                return item;
            }
            public static void On_Body_FarmItems(Body self, ref List<Item> farmedItems)
            {
                for (var i = 0; i < farmedItems.Count; i++)
                {
                    var item = farmedItems[i];
                    if (!(item is CustomItem))
                    {
                        ICustomItemProvider provider;
                        if (CustomItemTypes.TryGetValue(item.ItemID, out provider))
                        {
                            farmedItems[i] = createFrom(item, provider);
                        }
                    }
                }
            }
            public static void On_BodyPart_HarvestItems(BodyPart bodyPart, ref List<Item> harvestedItems)
            {
                for (var i = 0; i < harvestedItems.Count; i++)
                {
                    var item = harvestedItems[i];
                    if (!(item is CustomItem))
                    {
                        ICustomItemProvider provider;
                        if (CustomItemTypes.TryGetValue(item.ItemID, out provider))
                        {
                            harvestedItems[i] = createFrom(item, provider);
                        }
                    }
                }
            }
            public static void OnBefore_Crop_Forage(Crop self)
            {
                InterceptFortressCreate = true;
            }
            public static void OnAfter_Crop_Forage(Crop self)
            {
                InterceptFortressCreate = false;
            }
            public static void OnBefore_FellTreeJob_Complete(FellTreeJob self, Character chr)
            {
                InterceptFortressCreate = true;
            }
            public static void OnAfter_FellTreeJob_Complete(FellTreeJob self, Character chr)
            {
                InterceptFortressCreate = false;
            }
            // Todo: c4930bf5f347d6f6469e612c9163e9d43 missing, creates ramps when removing stuff. So no raw material supported for now
            public static Item On_Map_RemoveFloor(Item createdItem, Map self, int level, int row, int col, bool spawnResource)
            {
                if (spawnResource)
                {
                    return mayExchangeFortCreateSimple(createdItem);
                }
                else
                {
                    return mayExchangeMgrSpawnImmeSimple(createdItem);
                }
            }
            public static Item On_Map_RemoveWall(Item createdItem, Map self, int level, int row, int col, bool spawnResource)
            {
                if (spawnResource)
                {
                    return mayExchangeFortCreateSimple(createdItem);
                }
                else
                {
                    return mayExchangeMgrSpawnImmeSimple(createdItem);
                }
            }
            public static void OnBefore_Mineral_OnDelete(Mineral self)
            {
                InterceptFortressCreate = true;
            }
            public static void OnAfter_Mineral_OnDelete(Mineral self)
            {
                InterceptFortressCreate = false;
            }
            public static void OnBefore_ReplaceFloorJob_Complete(ReplaceFloorJob self, Character chr)
            {
                InterceptFortressCreate = true;
            }
            public static void OnAfter_ReplaceFloorJob_Complete(ReplaceFloorJob self, Character chr)
            {
                InterceptFortressCreate = false;
            }
            public static void OnBefore_ReplaceWallJob_Complete(ReplaceWallJob self, Character chr)
            {
                InterceptFortressCreate = true;
            }
            public static void OnAfter_ReplaceWallJob_Complete(ReplaceWallJob self, Character chr)
            {
                InterceptFortressCreate = false;
            }
            public static void OnBefore_Sappling_Deconstruct(Sapling self)
            {
                InterceptFortressCreate = true;
            }
            public static void OnAfter_Sappling_Deconstruct(Sapling self)
            {
                InterceptFortressCreate = false;
            }
            public static Item On_Tree_SpawnClipping(Item original, Tree self)
            {
                return original == null ? null : mayExchangeFortCreateSimple(original);
            }
            public static Item On_Tree_SpawnFruit(Item original, Tree self)
            {
                return original == null ? null : mayExchangeFortCreateSimple(original);
            }
            #endregion
        }


        #region public stuff
        private Dictionary<Type, string> newItemTypes = new Dictionary<Type, string>();
        public void AddNewItemType<T>(string sysName = null) where T : ICustomItemProvider, new()
        {
            newItemTypes[typeof(T)] = sysName ?? typeof(T).Name.Replace(".", "").Replace("_", "").Replace("+", "");
            //throw new NotImplementedException();
        }
        public static ItemID GetItemIdForType<T>()
        {
            // Todo: a getExistingProvider would make more sense?
            //var kvp = Hooks.CustomItemTypes.SingleOrDefault(el => el.Value is T);
            var matches = Hooks.CustomItemTypes.Where(el => el.Value.GetType() == typeof(T));
            if (matches.Count() == 1)
            {
                return matches.First().Key;
            }
            else if (matches.Count() <= 0)
            {
                throw new Exception("ItemType [" + typeof(T).FullName + "] does not (yet) exist!");
            }
            else
            {
                throw new Exception("ItemType [" + typeof(T).FullName + "] does exist multiple times?!");
            }
            /*
            var type = new ModType(typeof(T));
            if (ModEnvironment.Mods.Has(type))
            {
                return ((ICustomItemProvider)ModEnvironment.Mods[type]).ItemID;
            }
             */
        }
        public static void AddItemType<T>(string sysName = null) where T : ICustomItemProvider, new()
        {
            Instance.AddNewItemType<T>(sysName);
        }
        #endregion
    }
    /// <summary>
    /// This Interface is an expansion to ItemDef and should contain everything that is missing there.
    /// 
    /// For now it does only support special item types (in terms of usage)
    /// </summary>
    public interface ICustomItemProvider
    {
        void OnProviderCreated(ItemID myItemId);
        ItemDef GetItemDefinition(GameEntityManager usedManager = null);
        ItemID ItemID { get; }
        ItemID NoModFallbackItemType { get; }// we remove them, now. Fallbacks only for constructions.
        CustomItem CreateItem(Vector3 pos, ItemID itemId, int material);
        CustomItem CreateItem(Vector3 pos, ItemID itemId, List<Item> components);

        #region WeaponDef
        bool IsItemWeapon { get; }
        bool IsItemArmor { get; }
        bool IsItem2HWeapon { get; }
        #endregion
        #region StorageDef
        StorageID StorageContainerID { get; }
        #endregion
        #region ItemDef
        ItemID ResourcePileID { get; }
        bool IsWeaponOrArmor { get; }
        bool ConvertsToResourcePile { get; }
        bool IsFurniture { get; }
        #endregion
        #region AmmoDef
        AmmoType ItemIDToAmmoType { get; }
        bool IsAmmoContainer { get; }
        List<ItemID> ContainersByAmmoID { get; }
        #endregion
        //Missing:
        //- ItemGroup. necessary? There appears to be an Add, so thats may not a good loc
    }
    // Todo: general warning about all definitions: should they be imutable?!?!!!
    public abstract class CustomItemProvider : ICustomItemProvider
    {
        public static ItemDef CreateDefinitionCopy(ItemID from, GameEntityManager gem = null)
        {
            gem = gem ?? GnomanEmpire.Instance.EntityManager;
            if (gem == null)
            {
                throw new InvalidOperationException("No GameEntityManager given or does currently exist.");
            }
            var fromDef = gem.ItemDef(from);
            var newDef = new ItemDef();
            newDef.BaseValue = fromDef.BaseValue;
            newDef.CombatRatingModifier = fromDef.CombatRatingModifier;
            newDef.Description = fromDef.Description;
            newDef.Effects = fromDef.Effects.ToArray();
            newDef.EquippedDetailTileID = fromDef.EquippedDetailTileID;
            newDef.EquippedJobPenalty = fromDef.EquippedJobPenalty;
            newDef.EquippedMovePenalty = fromDef.EquippedMovePenalty;
            newDef.EquippedTileID = fromDef.EquippedTileID;
            newDef.EquipSlot = fromDef.EquipSlot;
            newDef.GroupName = fromDef.GroupName;
            newDef.HasQuality = fromDef.HasQuality;
            newDef.HeldTileIDs = fromDef.HeldTileIDs.ToArray();
            newDef.Name = fromDef.Name;
            newDef.ObtainDescription = fromDef.ObtainDescription;
            newDef.Prefix = fromDef.Prefix;
            newDef.Size = fromDef.Size;
            newDef.Suffix = fromDef.Suffix;
            newDef.TileIDs = fromDef.TileIDs.ToArray();
            newDef.TwoHanded = fromDef.TwoHanded;
            newDef.Value = fromDef.Value;
            newDef.WeaponDef = fromDef.WeaponDef;
            newDef.WeaponSize = fromDef.WeaponSize;
            return newDef;
        }
        public virtual ItemID ItemID { get; private set; }
        public virtual void OnProviderCreated(ItemID myItemId)
        {
            ItemID = myItemId;
        }
        public virtual ItemID NoModFallbackItemType
        {
            get
            {
                return ItemID.PuzzleBox;
            }
        }
        public abstract CustomItem CreateItem(Vector3 pos, ItemID itemId, int material);
        public abstract CustomItem CreateItem(Vector3 pos, ItemID itemId, List<Item> components);
        public abstract ItemDef GetItemDefinition(GameEntityManager usedMgr);

        #region ...Def configs
        public virtual bool IsItemWeapon { get { return false; } }
        public virtual bool IsItemArmor { get { return false; } }
        public virtual bool IsItem2HWeapon { get { return false; } }
        public virtual bool IsFurniture { get { return false; } }
        public virtual StorageID StorageContainerID { get { return StorageID.Count; } }
        public virtual ItemID ResourcePileID { get { return ItemID.Count; } }
        public virtual bool IsWeaponOrArmor { get { return IsItemWeapon || IsItemArmor; } }
        public virtual bool ConvertsToResourcePile { get { return false; } }
        public virtual AmmoType ItemIDToAmmoType { get { return AmmoType.Count; } }
        public virtual bool IsAmmoContainer { get { return false; } }
        public List<ItemID> ContainersByAmmoID { get { return null; } }
        #endregion
    }
    public abstract class CustomItemProvider<T> : CustomItemProvider where T : CustomItem
    {
        public CustomItemProvider()
        {
            if (typeof(T).IsAbstract
                || (typeof(T).GetConstructor(new Type[] { typeof(Vector3), typeof(ItemID), typeof(int) }) == null)
                || (typeof(T).GetConstructor(new Type[] { typeof(Vector3), typeof(ItemID), typeof(List<Item>) }) == null))
            {
                throw new InvalidOperationException("CustomItemProvider<T>s generic CustomItem type can't be abstract and must have constructors (Vector3, ItemID, int) and (Vector3, ItemID, List<Item>)!");
            }
        }
        public override CustomItem CreateItem(Vector3 pos, ItemID itemId, int material)
        {
            return (T)Activator.CreateInstance(typeof(T), new Object[] { pos, itemId, material });
        }
        public override CustomItem CreateItem(Vector3 pos, ItemID itemId, List<Item> components)
        {
            return (T)Activator.CreateInstance(typeof(T), new Object[] { pos, itemId, components });
        }
    }
    /// <summary>
    /// Provider for simplified items, that beside its type just have a drawable. Drawable is set via GetDrawable()
    /// </summary>
    public abstract class SimpleDrawableItemProvider : CustomItemProvider
    {
        public abstract SimpleDrawableComponent[] GetDrawables();

        public override CustomItem CreateItem(Vector3 pos, ItemID itemId, int material)
        {
            return new CustomItem(GetDrawables(), pos, itemId, material);
        }
        public override CustomItem CreateItem(Vector3 pos, ItemID itemId, List<Item> components)
        {
            return new CustomItem(GetDrawables(), pos, itemId, components);
        }
    }
    // Todo: generic simple item? Doesnt make rly sense...
    public abstract class SimpleDrawableItemProvider<T> : SimpleDrawableItemProvider where T : CustomItem
    {
        public SimpleDrawableItemProvider()
        {
            if (typeof(T).IsAbstract
                || (typeof(T).GetConstructor(new Type[] { typeof(SimpleDrawableComponent[]), typeof(Vector3), typeof(ItemID), typeof(int) }) == null)
                || (typeof(T).GetConstructor(new Type[] { typeof(SimpleDrawableComponent[]), typeof(Vector3), typeof(ItemID), typeof(List<Item>) }) == null))
            {
                throw new InvalidOperationException("SimpleDrawableItemProvider<T>s generic CustomItem type can't be abstract and must have constructors (ICustomDrawableSource, Vector3, ItemID, int) and (ICustomDrawableSource, Vector3, ItemID, List<Item>)!");
            }
        }
        public override CustomItem CreateItem(Vector3 pos, ItemID itemId, int material)
        {
            return (T)Activator.CreateInstance(typeof(T), new Object[] { GetDrawables(), pos, itemId, material });
        }
        public override CustomItem CreateItem(Vector3 pos, ItemID itemId, List<Item> components)
        {
            return (T)Activator.CreateInstance(typeof(T), new Object[] { GetDrawables(), pos, itemId, components });
        }
    }
    public class CustomItem : Game.Item
    {
        // Todo: we must have a fallback for custom items anyway. just removing could make problems with items in use.... or make sure there aren't any!
        public SimpleDrawableComponent[] Drawables { get; protected set; }
        public CustomItem(SimpleDrawableComponent[] drawStuff, Vector3 pos, ItemID itemId, int material)
            : base(pos, itemId, material)
        {
            Drawables = drawStuff;
        }
        public CustomItem(SimpleDrawableComponent[] drawStuff, Vector3 pos, ItemID itemId, List<Item> components)
            : base(pos, itemId, components)
        {
            Drawables = drawStuff;
        }
        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch, Vector3 position)
        {
            SimpleDrawableComponent.DrawItem(spriteBatch, this);
            //Drawables.Draw(spriteBatch, position);
        }
        public virtual Item CreateFallbackSaveableItem()
        {
            if (History.Components.Count > 0)
            {
                return new Item(Position, CustomItems.Hooks.CustomItemTypes[ItemID].NoModFallbackItemType, History.Components.ToList());
            }
            else
            {
                return new Item(Position, CustomItems.Hooks.CustomItemTypes[ItemID].NoModFallbackItemType, MaterialID);
            }
        }
        public override void Serialize(System.IO.BinaryWriter writer)
        {
            throw new Exception("Basic game is trying to serialize a custom item. This cannot happen, since it would make the savegame incompatible for vanilla grepolis.");
        }
    }
}
#endif