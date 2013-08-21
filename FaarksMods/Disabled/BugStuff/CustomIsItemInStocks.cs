using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Runtime.Serialization;
using System.Diagnostics;
using Faark.Gnomoria.Modding;
using Game;
using Game.GUI;
using Game.GUI.Controls;
using GameLibrary;
using Faark.Util;
using Microsoft.Xna.Framework;

namespace Faark.Gnomoria.Mods
{
#if false
    //looks like i was wrong :)
    public class QuickfixIsItemInStocks : Mod
    {
        public override IMethodModification[] Hooks
        {
            get { return new IMethodModification[] { new MethodHook(typeof(StockManager).GetMethod("IsItemInStocks"), Method.Of<StockManager, Item, bool>(On_StockManager_IsItemInStocks), MethodHookType.Replace) }; }
        }
        public static bool On_StockManager_IsItemInStocks(StockManager self, Item item)
        {
            return item.InStockpile;
        }
    }
#endif
#if false
    /*
    Loading a game with countless items sucks. Currently it can wast a LOT of time (more than half in my case) in StockManager.IsItemInStocks. This func can for sure be optimized, in countless ways!
    Easiest way would likely be to replace the list in ItemsByMaterials.IsItemInStocks by a hash table, but only if this lists are perm. Currently it looks like that stuff is re-generated on every call *puke*
    Using some kind of enumerables to create lists some calls prior could be good as well (in itemsOfQuality[OrHigher])

    Likly best solution: Cache stuff and use custom enumerators to combine lists. For bool lookups may hashtables?
     * 
     * 
     * Update:
     * 
     * I may was wrong. Stock manger has sperate lists for both stocked an unstocked items. IsItemInStocks should only look at the stocked ones.
     * New plan: Make a list that measures time, lookups and such.
     * 
     * 
     * 
     * Update2:
     * http://bugzilla.gnomoria.com/show_bug.cgi?id=564 did not run very well.
     * New Plan: Fix it as a mod. Should be able to modify ItemsByQualit to replace ItemsByMaterial for the big default lists and thus make at least "Contains" checks scale well.
     * 
     * TODO:
     * - Stockmanager.ccf24a10c7327de88b84f12fefcb522ef fixes some bugs by directly manipulating. Have to replace it
     * //- Stockmanager.AreItemsAvailable should be custom ; nvm
     * //- Stockmanager.FindClosestItems should be custom ; nvm
    */
    public class CustomIsItemInStocks: Mod
    {
        public class LargeItemsByMaterial
        {
            public Dictionary<int, HashSet<Item>> Items
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public void AddItem(int material)
            {
                throw new NotImplementedException();
            }
            public void AddItem(Item item, int material)
            {
                throw new NotImplementedException();
            }
            public bool RemoveItem(Item item)
            {
                throw new NotImplementedException();
            }
            public ItemsByMaterial ToItemsByMaterial()
            {
                throw new NotImplementedException();
            }
        }

        public override IMethodModification[] Hooks
        {
            get
            {
                return new IMethodModification[]
                {
                    new MethodHook(
                        typeof(ItemsByQuality).GetProperty("Items").GetGetMethod(),
                        Method.Of<ItemsByQuality, Dictionary<ItemQuality, ItemsByMaterial>>( Get_ItemsByQuality_Items),
                        MethodHookType.Replace
                        ),
                    new MethodHook(
                        typeof(ItemsByQuality).GetMethod("AddItem", new Type[]{typeof(Item), typeof(ItemQuality), typeof(int)}),
                        Method.Of<ItemsByQuality, Item, ItemQuality, int>(On_ItemsByQuality_AddItem),
                        MethodHookType.Replace
                        ),
                    new MethodHook(
                        typeof(ItemsByQuality).GetMethod("AddItem", new Type[]{typeof(ItemQuality), typeof(int)}),
                        Method.Of<ItemsByQuality, ItemQuality, int>(On_ItemsByQuality_AddItem),
                        MethodHookType.Replace
                        ),
                    new MethodHook(
                        typeof(ItemsByQuality).GetMethod("ItemsOfQuality", new Type[]{typeof(ItemQuality), typeof(bool)}),
                        Method.Of<ItemsByQuality, ItemQuality, bool, ItemsByMaterial>(On_ItemsByQuality_ItemsOfQuality),
                        MethodHookType.Replace
                        ),
                    new MethodHook(
                        typeof(ItemsByQuality).GetMethod("ItemsOfQualityOrHigher", new Type[]{typeof(ItemQuality)}),
                        Method.Of<ItemsByQuality, ItemQuality, ItemsByMaterial>(On_ItemsByQuality_ItemsOfQualityOrHigher),
                        MethodHookType.Replace
                        ),
                    new MethodHook(
                        typeof(ItemsByQuality).GetMethod("QuantityInStock", new Type[]{typeof(ItemQuality), typeof(int)}),
                        Method.Of<ItemsByQuality, ItemQuality, int>(On_ItemsByQuality_QuantityInStock),
                        MethodHookType.Replace
                        ),
                    new MethodHook(
                        typeof(ItemsByQuality).GetMethod("RemoveItem", new Type[]{typeof(Item)}),
                        Method.Of<ItemsByQuality, Item, bool>(On_ItemsByQuality_RemoveItem),
                        MethodHookType.Replace
                        ),



                    new MethodHook(
                        typeof(StockManager).GetMethod("IsItemInStocks", new Type[]{typeof(Item)}),
                        Method.Of<StockManager, Item, bool>(On_StockManager_IsItemInStocks),
                        MethodHookType.Replace
                        ),
                    new MethodHook(
                        typeof(StockManager).GetMethod("QuantityInStock", new Type[]{typeof(ItemID), typeof(int)}),
                        Method.Of<StockManager, ItemID, int, int>(On_StockManager_QuantityInStock),
                        MethodHookType.Replace
                        ),
                    new MethodHook(
                        typeof(StockManager).GetMethod("AreItemsAvailable"),
                        Method.Of<StockManager, ItemID, int, int>(On_StockManager_AreItemsAvailable),
                        MethodHookType.Replace
                        ),
                    new MethodHook(
                        typeof(StockManager).GetMethod("FindClosestItems"),
                        Method.Of<StockManager, ItemID, int, int>(On_StockManager_FindClosestItems),
                        MethodHookType.Replace
                        )
                    /*
                    new MethodHook(
                        typeof(StockManager).GetMethod("IsItemInStocks", BindingFlags.Instance | BindingFlags.Public),
                        Method.Of<StockManager, Item>(OnBefore_StockManager_IsItemInStocks),
                        MethodHookType.RunBefore
                        ),
                    new MethodHook(
                        typeof(StockManager).GetMethod("IsItemInStocks", BindingFlags.Instance | BindingFlags.Public),
                        Method.Of<bool, StockManager,  Item, bool>(OnAfter_StockManager_IsItemInStocks),
                        MethodHookType.RunAfter
                        )*/
                };
            }
        }

        public static Dictionary<ItemsByQuality, Dictionary<ItemQuality, LargeItemsByMaterial>> myClasses = new Dictionary<ItemsByQuality, Dictionary<ItemQuality, LargeItemsByMaterial>>();
        public static Dictionary<ItemQuality, ItemsByMaterial> Get_ItemsByQuality_Items(ItemsByQuality self)
        {
            var result = new Dictionary<ItemQuality, ItemsByMaterial>();
            foreach (var el in myClasses[self])
            {
                result.Add(el.Key, el.Value.ToItemsByMaterial());
            }
            return result;
        }
        public static void On_ItemsByQuality_AddItem(ItemsByQuality self, Item item, ItemQuality quality, int material)
        {
            var items = myClasses[self];
            LargeItemsByMaterial itemsByMaterial = null;
            if (!items.TryGetValue(quality, out itemsByMaterial))
            {
                itemsByMaterial = new LargeItemsByMaterial();
                items[quality] = itemsByMaterial;
            }
            itemsByMaterial.AddItem(item, material);
        }
        public static void On_ItemsByQuality_AddItem(ItemsByQuality self, ItemQuality quality, int material)
        {
            var items = myClasses[self];
            LargeItemsByMaterial itemsByMaterial = null;
            if (!items.TryGetValue(quality, out itemsByMaterial))
            {
                itemsByMaterial = new LargeItemsByMaterial();
                items[quality] = itemsByMaterial;
            }
            itemsByMaterial.AddItem(material);
        }
        public static ItemsByMaterial On_ItemsByQuality_ItemsOfQuality(ItemsByQuality self, ItemQuality quality, bool atLeast)
        {
            if (quality == ItemQuality.Any)
            {
                return self.AllItems();
            }
            if (atLeast)
            {
                return self.ItemsOfQualityOrHigher(quality);
            }
            LargeItemsByMaterial result;
            if (myClasses[self].TryGetValue(quality, out result))
            {
                return result.ToItemsByMaterial();
            }
            return null;
        }
        public static ItemsByMaterial On_ItemsByQuality_ItemsOfQualityOrHigher(ItemsByQuality self, ItemQuality itemQuality)
        {
            var items = myClasses[self];
            if (itemQuality == ItemQuality.Any)
            {
                itemQuality = ItemQuality.Poor;
            }
            ItemsByMaterial itemsByMaterial = new ItemsByMaterial();
            foreach (KeyValuePair<ItemQuality, LargeItemsByMaterial> current in items)
            {
                if (current.Key >= itemQuality)
                {
                    foreach (KeyValuePair<int, HashSet<Item>> current2 in current.Value.Items)
                    {
                        List<Item> list;
                        if (itemsByMaterial.Items.TryGetValue(current2.Key, out list))
                        {
                            itemsByMaterial.Items[current2.Key] = itemsByMaterial.Items[current2.Key].Concat(current2.Value).ToList<Item>();
                        }
                        else
                        {
#warning have to find a performant way to replace this, likely by replacing lots a stockmanager funcs
                            itemsByMaterial.Items.Add(current2.Key, current2.Value);
                        }
                    }
                }
            }
            return itemsByMaterial;
        }
        public static int On_ItemsByQuality_QuantityInStock(ItemsByQuality self, ItemQuality quality)
        {
            throw new Exception("Thought this function is not used!?");
        }
        public static bool On_ItemsByQuality_RemoveItem(ItemsByQuality self, Item item)
        {
            LargeItemsByMaterial itemsByMaterial = null;
            return myClasses[self].TryGetValue(item.Quality, out itemsByMaterial) && itemsByMaterial.RemoveItem(item);
        }

        private static FieldInfo mStockManager_ItemsByQuality_InStockItems = null;
        public static FieldInfo StockManager_ItemsByQuality_InStockItems
        {
            get
            {
                if (mStockManager_ItemsByQuality_InStockItems == null)
                {
                    mStockManager_ItemsByQuality_InStockItems = typeof(StockManager).GetField("cf56aff2238f1af02e43639e3a7d9965f", BindingFlags.Instance | BindingFlags.NonPublic);
                }
                return mStockManager_ItemsByQuality_InStockItems;
            }
        }
        public static bool On_StockManager_IsItemInStocks(StockManager self, Item item)
        {
            var inStockItems = (ItemsByQuality[])StockManager_ItemsByQuality_InStockItems.GetValue(self);

            ItemsByQuality itemsByQuality = inStockItems[(int)item.ItemID];
            if (itemsByQuality == null)
            {
                return false;
            }
            var items = myClasses[itemsByQuality];
            foreach (var el in items)
            {
                foreach (var el2 in el.Value.Items)
                {
                    if (el2.Value.Contains(item))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public static int On_StockManager_QuantityInStock(StockManager self, ItemID itemID, int material)
        {
            ItemsByQuality itemsByQuality = ((ItemsByQuality[])StockManager_ItemsByQuality_InStockItems.GetValue(self))[(int)itemID];
            if (itemsByQuality == null)
            {
                return 0;
            }
            var largeItems = myClasses[itemsByQuality];
            if (largeItems == null)
            {
                return 0;
            }
            var count = 0;
            foreach (var cat in largeItems)
            {
                if (material == 80)
                {
                    foreach (var list in cat.Value.Items)
                    {
                        count += list.Value.Count;
                    }
                }
                else
                {
                    HashSet<Item> list;
                    if (cat.Value.Items.TryGetValue(material, out list))
                    {
                        count += list.Count;
                    }
                }
            }
            return count;
        }
        public static bool On_StockManager_AreItemsAvailable(StockManager self, Vector3 pos, ItemID itemID, uint quantity, int material, ItemQuality itemQuality, bool atLeastQuality)
        {
            if (GnomanEmpire.Instance.Map.GetCell(pos).NavGraphNode == null)
            {
                return false;
            }
            ItemsByQuality itemsByQuality = ((ItemsByQuality[])StockManager_ItemsByQuality_InStockItems.GetValue(self))[(int)itemID];
            if (itemsByQuality == null)
            {
                return false;
            }
            ItemsByMaterial itemsByMaterial = itemsByQuality.ItemsOfQuality(itemQuality, atLeastQuality);
            return itemsByMaterial != null && itemsByMaterial.AreItemsAvailable(pos, quantity, material);
        }
        /*
        static Stopwatch sw = new Stopwatch();
        private class logitem
        {
            public int count = 0;
            public TimeSpan time = TimeSpan.Zero;
        }
        static Dictionary<Item, logitem> data = new Dictionary<Item, logitem>();
        public static void OnBefore_StockManager_IsItemInStocks(StockManager self, Item item)
        {
            sw.Reset();
            sw.Start();
        }
        public static bool OnAfter_StockManager_IsItemInStocks(bool value, StockManager self, Item item)
        {
            sw.Stop();
            var time = sw.Elapsed;
            logitem line;
            if (!data.TryGetValue(item, out line))
            {
                data[item] = line = new logitem();
            }
            line.count++;
            line.time += time;
            return value;
        }
        /*
        public static bool On_StockManager_IsItemInStocks(StockManager self, bool value, Item item)
        {
            var myVal = false;
            var cell = GnomanEmpire.Instance.Map.GetCell(item.Position);
            if (cell != null)
            {
                var des = cell.Designation;
                if ((des != null) && (des is Stockpile))
                {
                    myVal = true;
                }
            }
            if (myVal != value)
            {
                throw new InvalidOperationException("Im wrong :(");
            }
            return myVal;
        }*/
    }
#endif
}
