using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Runtime.Serialization;
using Faark.Gnomoria.Modding;
using Game;
using Game.GUI;
using Game.GUI.Controls;
using GameLibrary;
using Faark.Util;
using Microsoft.Xna.Framework;


namespace Faark.Gnomoria.Mods.BugStuff
{
#if false
    /// <summary>
    /// Dumps all data into a json-file. Needs Json.Net & don't forget to change the path
    /// </summary>
    public class GetGnomodiaData : Mod
    {
        public override IMethodModification[] Hooks
        {
            get
            {
                //return new IMethodModification[0];
                return new IMethodModification[]{
                    new MethodHook(
                        typeof(GnomanEmpire).GetMethod("LoadGame"),
                        Method.Of<GnomanEmpire, string, bool>(On_GnomanEmpire_LoadGame)
                        )
                };
            }
        }
        public static void On_GnomanEmpire_LoadGame(GnomanEmpire self, string file, bool fallen)
        {
            var dc = new DataCollection();
            dc.ToString();
            //var seri = new System.Web.Script.Serialization.JavaScriptSerializer();
            //var text = seri.Serialize(dc);
            var text = Newtonsoft.Json.JsonConvert.SerializeObject(dc, Newtonsoft.Json.Formatting.Indented);
            System.IO.File.WriteAllText(@"C:\Users\Faark\Documents\Visual Studio 2010\Projects\GnomoriaModding\Release\Data.js", text);
            
        }
        public class DataCollection
        {
            public MaterialProperty[] TerrainProperties;
            public LayerDef[] Layers;
            public TileDef[] Tiles;

            public class CharacterData
            {
                public LanguageDef[] Language;
                public NameDef[] Names;
                public WordDef[] Words;
                public NameRules[] NameRules;
                public RaceDef[] Races;
                public SkillDef[] Skills;
                public BodyDef[] Bodys;
                public BodyPartDef[] BodyParts;
                public FactionDef[] Factions;
            }
            public CharacterData Characters = new CharacterData();

            public class ObjectData
            {
                public ItemDef[] Items;
                public WorkshopDef[] Workshops;
                public ConstructionDef[] Constructions;
                public PlantDef[] Plants;
                public StorageDef[] Storages;
                public MechanismDef[] Mechanisms;
                public TrapDef[] Traps;
                public BlueprintDef[] Blueprints;
                public LiquidDef[] Liquid;
                public AmmoDef[] Ammos;
            }
            public ObjectData Objects = new ObjectData();

            public HelpTopic[] Help;

            public DataCollection()
            {
                TerrainProperties = GnomanEmpire.Instance.Content.Load<MaterialProperty[]>("Data/material");
                Layers = GnomanEmpire.Instance.Content.Load<LayerDef[]>("Data/layer");
                Tiles = GnomanEmpire.Instance.Content.Load<TileDef[]>("Data/tile");
                Characters.Language = GnomanEmpire.Instance.Content.Load<LanguageDef[]>("Data/Characters/language");
                Characters.Names = GnomanEmpire.Instance.Content.Load<NameDef[]>("Data/Characters/name");
                Characters.Words = GnomanEmpire.Instance.Content.Load<WordDef[]>("Data/Characters/word");
                Characters.NameRules = GnomanEmpire.Instance.Content.Load<NameRules[]>("Data/Characters/namerules");
                Characters.Races = GnomanEmpire.Instance.Content.Load<RaceDef[]>("Data/Characters/race");
                Characters.Skills = GnomanEmpire.Instance.Content.Load<SkillDef[]>("Data/Characters/skill");
                Characters.Bodys = GnomanEmpire.Instance.Content.Load<BodyDef[]>("Data/Characters/body");
                Characters.BodyParts = GnomanEmpire.Instance.Content.Load<BodyPartDef[]>("Data/Characters/bodypart");
                Characters.Factions = GnomanEmpire.Instance.Content.Load<FactionDef[]>("Data/Characters/faction");
                Objects.Items = GnomanEmpire.Instance.Content.Load<ItemDef[]>("Data/Objects/item");
                Objects.Workshops = GnomanEmpire.Instance.Content.Load<WorkshopDef[]>("Data/Objects/workshop");
                Objects.Constructions = GnomanEmpire.Instance.Content.Load<ConstructionDef[]>("Data/Objects/construction");
                Objects.Plants = GnomanEmpire.Instance.Content.Load<PlantDef[]>("Data/Objects/plant");
                Objects.Storages = GnomanEmpire.Instance.Content.Load<StorageDef[]>("Data/Objects/storage");
                Objects.Mechanisms = GnomanEmpire.Instance.Content.Load<MechanismDef[]>("Data/Objects/mechanism");
                Objects.Traps = GnomanEmpire.Instance.Content.Load<TrapDef[]>("Data/Objects/trap");
                Objects.Blueprints = GnomanEmpire.Instance.Content.Load<BlueprintDef[]>("Data/Objects/blueprint");
                Objects.Liquid = GnomanEmpire.Instance.Content.Load<LiquidDef[]>("Data/Objects/liquid");
                Objects.Ammos = GnomanEmpire.Instance.Content.Load<AmmoDef[]>("Data/Objects/ammo");
                Help = GnomanEmpire.Instance.Content.Load<HelpTopic[]>("Data/help");
            }
        }
    }
#endif
}
