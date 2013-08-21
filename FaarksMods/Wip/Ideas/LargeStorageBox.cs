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

    just some drawing test. Don't think you will find sth useful, but i leave it for now

    public class LargeStorageBox : Mod
    {
        public abstract class ModEntity : GameEntity
        {
            public ModEntity(Vector3 vec)
                : base(vec)
            {
            }
        }

        
        public class LargeChest : GameEntity//StorageContainer
        {
            public LargeChest(Vector3 pos) : base(pos) { }
            public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch, Vector3 position)
            {
                /*
                Map map = GnomanEmpire.Instance.Map;
                Color color = GameEntity.Darken(new Color(192, 0, 0), base.Cell().LightLevel);
                Vector2 pos = GnomanEmpire.Instance.Camera.MapIndexToScreenCoords(this.Position);
                GameEntityManager entityManager = GnomanEmpire.Instance.EntityManager;
                entityManager.DrawObjectIsoTile(spriteBatch, map.TileDef((int)this.locType_TileID), pos, color, this.locType_SpriteEffects, 0f);
                base.Draw(spriteBatch, position);



                if (this.loc_ContainedResources.Count == 0)
                {
                    return;
                }
                Map map = GnomanEmpire.Instance.Map;
                float lightLevel = base.Cell().LightLevel;
                Color color = map.TerrainProperties[base.MaterialID].ConvertColor(lightLevel);
                Color color2 = GameEntity.Darken(Color.White, lightLevel);
                Vector2 pos = GnomanEmpire.Instance.Camera.MapIndexToScreenCoords(this.Position);
                GameEntityManager entityManager = GnomanEmpire.Instance.EntityManager;
                TileDesc[] tileIDs = entityManager.ItemDef(this.loc_ContainedResources[0].ItemID).TileIDs;
                TileDesc[] array = tileIDs;
                for (int i = 0; i < array.Length; i++)
                {
                    TileDesc tileDesc = array[i];
                    entityManager.DrawObjectIsoTile(spriteBatch, map.TileDef((int)tileDesc.TileID), pos, (tileDesc.MaterialIndex == 0) ? color : color2);
                }




                Map map = GnomanEmpire.Instance.Map;
                MapCell mapCell = (this.Parent == null) ? base.Cell() : this.Parent.Cell();
                float lightLevel = mapCell.LightLevel;
                map.TerrainProperties[this.MaterialID].ConvertColor(lightLevel);
                Color color = GameEntity.Darken(Color.White, lightLevel);
                Vector2 pos = GnomanEmpire.Instance.Camera.MapIndexToScreenCoords(this.Position);
                GameEntityManager entityManager = GnomanEmpire.Instance.EntityManager;
                TileDesc[] tileIDs = this.locType_ItemDef.TileIDs;
                TileDesc[] array = tileIDs;
                for (int i = 0; i < array.Length; i++)
                {
                    TileDesc tileDesc = array[i];
                    int num = this.History.MaterialAtIndex(tileDesc.MaterialIndex);
                    entityManager.DrawObjectIsoTile(spriteBatch, map.TileDef((int)tileDesc.GetTileID((Material)num)), pos, (tileDesc.MaterialIndex == -1) ? color : map.TerrainProperties[num].ConvertColor(lightLevel));
                }
                */
                base.Draw(spriteBatch, position);
            }
            public override string Name()
            {
                return "LargeChest";
            }
        }

        public override ModConfig GetConfig()
        {
            return new ModConfig(
                this,
                new MethodHook(
                    typeof(World).GetMethod("Serialize", BindingFlags.Instance | BindingFlags.Public),
                    Method.Of<World, System.IO.BinaryWriter>(On_World_Serialize)
                    )
                );
        }

        public static void On_World_Serialize(World self, System.IO.BinaryWriter writer)
        {
            //everything else saved, lets save our stuff now.
        }
    }
#endif
}
