using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Dynamic;
using System.Reflection;
using Faark.Util;
using Faark.Util.Serialization;

using Game;
using GameLibrary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

//    Content Mods are meant to be heavy helper classes, used by other mods. This ones are not fully finished (e.G. save related issues, or stockmanager&etc not showing them), with a lot of issues. Use at own risk, and if possible try to complete them.

namespace Faark.Gnomoria.Modding.ContentMods
{
    public static class CustomTextureManager
    {
        private static Dictionary<Assembly, Dictionary<string, Texture2D>> resourceTextures = null;
        public static Texture2D GetFromAssemblyResource(Assembly sourceAssembly, string manifestResourceName)
        {
            if (resourceTextures == null)
            {
                resourceTextures = new Dictionary<Assembly, Dictionary<string, Texture2D>>();
            }
            Dictionary<string, Texture2D> assembliesTextures;
            if (!resourceTextures.TryGetValue(sourceAssembly, out assembliesTextures))
            {
                assembliesTextures = resourceTextures[sourceAssembly] = new Dictionary<string, Texture2D>();
            }
            Texture2D alreadyLoadedTexture;
            if (assembliesTextures.TryGetValue(manifestResourceName, out alreadyLoadedTexture))
            {
                return alreadyLoadedTexture;
            }
            var gd = GnomanEmpire.Instance.GraphicsDevice;
            var stream = sourceAssembly.GetManifestResourceStream(manifestResourceName);
            //FileStream stream = new FileStream("Content/Tilesheet/default.png", FileMode.Open, FileAccess.Read);
            return assembliesTextures[manifestResourceName] = Texture2D.FromStream(gd, stream);
        }
    }
    #if false

    public interface ICustomDrawableEntitySource
    {
        /// <summary>
        /// Gets the actual drawable object, in case this is a factory or animation.
        /// </summary>
        /// <returns>This or a new instance of the drawable object, if necessary.</returns>
        ICustomDrawableEntity GetInstance();
    }
    public interface ICustomDrawableEntity : ICustomDrawableEntitySource
    {
        void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch, Vector2 screen_position, Color color);
    }
    public abstract class CustomDrawableEntity : ICustomDrawableEntity
    {
        public virtual ICustomDrawableEntity GetInstance()
        {
            return this;
        }
        public abstract void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch, Vector2 screen_position, Color color);
        /*
        {
            throw new NotImplementedException();
        }
        */
    }

    public class SimpleDrawableComponent
    {
        public int MaterialIndex { get; protected set; }
        public ICustomDrawableEntity Entity { get; protected set; }
        public SimpleDrawableComponent(int mat_id, ICustomDrawableEntitySource drawable)
        {
            MaterialIndex = mat_id;
            Entity = drawable.GetInstance();
        }

        public static void DrawItem(SpriteBatch spriteBatch, CustomItem item)
        {
            var map = GnomanEmpire.Instance.Map;
            var mapCell = (item.Parent == null) ? item.Cell() : item.Parent.Cell();
            var lightLevel = mapCell.LightLevel;
            //map.TerrainProperties[this.MaterialID].ConvertColor(lightLevel);
            Color color = GameEntity.Darken(Color.White, lightLevel);
            Vector2 pos = GnomanEmpire.Instance.Camera.MapIndexToScreenCoords(item.Position);
            GameEntityManager entityManager = GnomanEmpire.Instance.EntityManager;

            foreach (var drawableComponent in item.Drawables)
            {
                int num = item.History.MaterialAtIndex(drawableComponent.MaterialIndex);
                drawableComponent.Entity.Draw(spriteBatch, pos, (drawableComponent.MaterialIndex == -1) ? color : map.TerrainProperties[num].ConvertColor(lightLevel));
            }
        }

        /// <summary>
        /// NO ANIMATION SUPPORT FOR NOW! No support for all sides for now.
        /// </summary>
        /// <param name="spriteBatch"></param>
        /// <param name="construction"></param>
        public static void DrawConstruction(SpriteBatch spriteBatch, CustomConstruction construction)
        {
            var entityManager = GnomanEmpire.Instance.EntityManager;
            var map = GnomanEmpire.Instance.Map;
            float lightLevel = construction.Cell().LightLevel;
            Color color = map.TerrainProperties[construction.MaterialID].ConvertColor(lightLevel);
            Color color2 = GameEntity.Darken(TileDef.NoTintColor, lightLevel);
            Vector2 pos = GnomanEmpire.Instance.Camera.MapIndexToScreenCoords(construction.Position);

            foreach (var drawableComponent in construction.Drawables)
            {
                drawableComponent.Entity.Draw(spriteBatch, pos, (drawableComponent.MaterialIndex == 0) ? color : color2);
            }

            /*
            TileDesc[] array = construction.ConstructionDef.TileIDs;
            if (construction.Animation != null)
            {
                array = construction.Animation.TileDescs();
            }
            TileDesc[] array2 = array;
            for (int i = 0; i < array2.Length; i++)
            {
                TileDesc tileDesc = array2[i];
                int num = (int)tileDesc.GetTileID((Material)construction.MaterialID);
                if (construction.ConstructionDef.Rotates)
                {
                    Camera camera = GnomanEmpire.Instance.Camera;
                    num = (int)(num + ((int)construction.Orientation + (int)camera.Orientation) % (int)CameraOrientation.Count);
                }
                entityManager.DrawObjectIsoTile(spriteBatch, map.TileDef(num), pos, (tileDesc.MaterialIndex == 0) ? color : color2);
            }
            */
        }
    }
    public class TextureDrawable : CustomDrawableEntity
    {
        public Texture2D Texture { get; protected set; }
        public Rectangle SourceRectangle { get; protected set; }
        public Vector2 Offset { get; protected set; }
        public SpriteEffects Effect { get; protected set; }
        public TextureDrawable(Texture2D texture, Rectangle img_source_rect, Vector2 offset = default(Vector2), SpriteEffects effect = SpriteEffects.None)
        {
            Texture = texture;
            SourceRectangle = img_source_rect;
            Offset = offset;
            Effect = effect;
        }
        public override void Draw(SpriteBatch spriteBatch, Vector2 screen_position, Color color)
        {
            spriteBatch.Draw(
                Texture,
                Effect == SpriteEffects.FlipHorizontally ? screen_position - Offset : screen_position + Offset, 
                new Rectangle?(SourceRectangle),
                color,
                0f,
                new Vector2(
                    (float)((int)((float)SourceRectangle.Width * 0.5f)), 
                    (float)((int)((float)SourceRectangle.Height * 0.5f))
                ),
                1f, 
                Effect, 
                0f
                );
        }
    }

    /*
     * Does not make sense atm, since we use resources that are instantly loaded to Texture2D anyway.
     * 
    /// <summary>
    /// Should take care of texture loading at the best time and finally creates a DrawableTexture
    /// </summary>
    public class DrawableTextureFactory : ICustomDrawableSource
    {
        protected DrawableTexture Instance { get; set; }
        public FileInfo TextureSource { get; protected set; }
        public Rectangle SourceRectangle { get; protected set; }
        
        public DrawableTextureFactory(FileInfo textureSource, Rectangle img_source_rect)
        {
            TextureSource = textureSource;
            SourceRectangle = img_source_rect;
        }
        public DrawableTexture GetInstance()
        {
            return instance ?? (instance = new DrawableTexture(GnomanEmpire.Instance.Content.Load<Texture2D>(textureSou
        }
        ICustomDrawable ICustomDrawableSource.GetInstance()
        {
            return GetInstance();
        }
    }*/
    
#endif
}