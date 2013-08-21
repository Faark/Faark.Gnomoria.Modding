using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Faark.Gnomoria.Modding;
using Game;
namespace Faark.Gnomoria.Mods
{
#if false

    Looks like i one day started to make a mod for this. Anyway, it should be in the stock game now. Ideas => Trash

    public class GrowGrassOnRamps : Mod
    {
        public override ModConfig GetConfig()
        {
            /*
            throw new NotImplementedException();
            //Game.Ramp r = null;
            //r.Update(0);
            var t = typeof(Game.Ramp);
            var tc = typeof(Game.Construction);
            var i = t.GetInterfaces();
            var ic = tc.GetInterfaces();
            */


            return new ModConfig(this,
                new MethodAddVirtual(
                    typeof(Game.Ramp),
                    Method.Of<float>(Method.CreateDummy<Game.Ramp>().Update),
                    Method.Of<Game.Ramp, float>(OnRampUpdate)
                    )
                );
        }

        // it follows a "copy" of Game.Grass.Update and depencies

        private static bool CanGrow_CheckBorderingCell(MapCell cell)
        {
            return (cell != null) 
                && !cell.HasNaturalWall() 
                && !cell.HasEmbeddedFloor() 
                && cell.HasFloor()
                && cell.Floor == 2 //according to savegame-extractor
                && !(cell.HasEmbeddedWall() 
                    && !(cell.EmbeddedWall is Tree) 
                    && !(cell.EmbeddedWall is Crop)
                );
        }
        private static bool CanGrow(Game.Ramp self)
        {
            if (!self.Cell().Outside)
                return false;
            var map = GnomanEmpire.Instance.Map;
            var cells = new MapCell[4];
            return
                CanGrow_CheckBorderingCell(map.GetCell((int)self.Position.Z, (int)self.Position.X + 1, (int)self.Position.Y))
                /*
                || CanGrow_CheckBorderingCell(map.GetCell((int)self.Position.Z, (int)self.Position.X - 1, (int)self.Position.Y))
                || CanGrow_CheckBorderingCell(map.GetCell((int)self.Position.Z, (int)self.Position.X, (int)self.Position.Y + 1))
                || CanGrow_CheckBorderingCell(map.GetCell((int)self.Position.Z, (int)self.Position.X, (int)self.Position.Y - 1))
                || CanGrow_CheckBorderingCell(map.GetCell((int)self.Position.Z, (int)self.Position.X + 1, (int)self.Position.Y))
                */
                || CanGrow_CheckBorderingCell(map.GetCell((int)self.Position.Z, (int)self.Position.X - 1, (int)self.Position.Y))
                || CanGrow_CheckBorderingCell(map.GetCell((int)self.Position.Z, (int)self.Position.X, (int)self.Position.Y + 1))
                || CanGrow_CheckBorderingCell(map.GetCell((int)self.Position.Z, (int)self.Position.X, (int)self.Position.Y - 1));
        }
        public static void OnRampUpdate(Game.Ramp self, float dt)
        {
            throw new Exception("We did it :)");
            if (!CanGrow(self))
            {
                //guess we do not have animations, so guess we don't need it & are safe to remove it ...
                //GnomanEmpire.Instance.EntityManager.RemoveFromUpdateList(self);

            }
            else if(GnomanEmpire.Instance.Region.Season() != Season.Winter )
            {
                //map.TerrainProperties[base.MaterialID].
            }
        }
    }
#endif
}
