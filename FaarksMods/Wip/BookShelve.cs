using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Runtime.Serialization;
using Faark.Gnomoria.Modding;
using Faark.Gnomoria.Modding.ContentMods;
using Game;
using Game.GUI;
using Game.GUI.Controls;
using GameLibrary;
using Faark.Util;
using Microsoft.Xna.Framework;

namespace Faark.Gnomoria.Mods
{
#if FALSE
    public class BookShelve: Mod
    {
        private class BookShelveItemProvider : SimpleDrawableItemProvider
        {
            public override bool IsFurniture { get { return true; } }

            public override ItemDef GetItemDefinition(GameEntityManager usedMgr)
            {
                var def = CreateDefinitionCopy(ItemID.Cabinet, usedMgr);
                def.Name = "book shelve";
                return def;
            }
            private SimpleDrawableComponent[] drawables;
            public override SimpleDrawableComponent[] GetDrawables()
            {
                return drawables;
            }
            public BookShelveItemProvider()
            {
                drawables = new SimpleDrawableComponent[]{ 
                    new SimpleDrawableComponent(
                        0,
                        new TextureDrawable(
                            CustomTextureManager.GetFromAssemblyResource(
                                Assembly.GetExecutingAssembly(),
                                "Faark.Gnomoria.Mods.Resources.sprite.png"
                                ),
                            new Rectangle(0, 32, 32, 32),
                            new Vector2(0, -12)
                            )
                        )
                };
            }
        }
        private class BookShelveRecipeProvider : CustomRecipeProvider
        {
            public override IEnumerable<WorkshopID> CraftedAt
            {
                get { yield return WorkshopID.Carpenter; }
            }
            public override CraftableItem GetCraftableItem(GameEntityManager usedMgr)
            {
                var craftableCabinet = usedMgr.WorkshopDef(WorkshopID.Carpenter).CraftableItems.Single(ci => ci.ItemID == ItemID.Cabinet);
                var craftableBookshelve = CopyCraftableItem(craftableCabinet);
                craftableBookshelve.ItemID = CustomItems.GetItemIdForType<BookShelveItemProvider>();
                return craftableBookshelve;
            }
        }
        private class BookShelveConstructionProvider : SimpleDrawableConstructionProvider
        {
            public override ConstructionDef GetConstructionDefinition(GameEntityManager usedMgr)
            {
                var def = CreateDefinitionCopy(ConstructionID.Cabinet, usedMgr);
                def.Name = "book shelve";
                def.Components[0].ID = CustomItems.GetItemIdForType<BookShelveItemProvider>();
                return def;
            }
            private SimpleDrawableComponent[] drawables;
            public override SimpleDrawableComponent[] GetDrawables()
            {
                return drawables;
            }
            public BookShelveConstructionProvider()
            {
                drawables = new SimpleDrawableComponent[]{ 
                    new SimpleDrawableComponent(
                        0,
                        new TextureDrawable(
                            CustomTextureManager.GetFromAssemblyResource(
                                Assembly.GetExecutingAssembly(),
                                "Faark.Gnomoria.Mods.Resources.sprite.png"
                                ),
                            new Rectangle(0, 32, 32, 32),
                            new Vector2(0, -12)
                            )
                        )
                };
            }
        }

        public override void  Initialize_PreGeneration()
        {
            CustomRecipes.AddRecipeType<BookShelveRecipeProvider>();
            CustomItems.AddItemType<BookShelveItemProvider>();
            CustomConstructions.AddConstructionType<BookShelveConstructionProvider>();
        }
        public override IEnumerable<IModification> Modifications
        {
            get
            {
                yield break;
            }
        }
        public override IEnumerable<ModDependency> Dependencies
        {
            get
            {
                yield return CustomItems.Instance;
                yield return CustomRecipes.Instance;
                yield return CustomConstructions.Instance;
            }
        }
    }
#endif
}
