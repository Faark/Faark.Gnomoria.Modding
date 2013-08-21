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

namespace Faark.Gnomoria.Mods
{
#if false

    /*

    Status:
    Doable. I would have to replace a brown texture with a neutral one. Some material IDs are:
42
55
54
52
53
48
43

    Replacing the texture part would be the easiest approach, since anything else would require a new item class. Select texture part via ItemID=>ItemDef=>Tiles
             */
    public class ColoredLeather: Mod
    {
        public override IEnumerable<IModification> Modifications
        {
            get
            {
                yield return new MethodHook(
                    typeof(Map).GetConstructor(new Type[] { typeof(TileSet) }),
                    Method.Of<Map, TileSet>(On_Map_Created)
                    );
                yield return new MethodHook(
                    typeof(Map).GetConstructor(new Type[] {  typeof(TileSet), typeof(System.IO.BinaryReader) }),
                    Method.Action<Map, TileSet, System.IO.BinaryReader>(On_Map_Created)
                    );
            }
        }

        private static bool DataChanged = false;
        public static void On_Map_Created(Map self, TileSet set)
        {
            if (DataChanged)
                return;

            //var f = typeof(Map).GetFields(BindingFlags.Instance | BindingFlags.NonPublic).Single(field => field.FieldType == typeof(MaterialProperty[]));

            //var items = GnomanEmpire.Instance.Fortress.StockManager.ItemsByItemID(ItemID.RawHide);

            //var mats = (MaterialProperty[])f.GetValue(GnomanEmpire.Instance.Map);

            //mats.ToArray();
            //items.ToString();

            
            //var state = new Microsoft.Xna.Framework.Graphics.DepthStencilState();
            //state.DepthBufferFunction = Microsoft.Xna.Framework.Graphics.CompareFunction.Less;
            //state.DepthBufferWriteEnable = 
            var bla = new List<MapCell>();
            DataChanged = true;
        }
        public static void On_Map_Created(Map self, TileSet set, System.IO.BinaryReader reader)
        {
            On_Map_Created(self, set);
        }
    }
#endif
}
