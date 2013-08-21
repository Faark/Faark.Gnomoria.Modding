using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Faark.Gnomoria.Modding;
using Faark.Gnomoria.Modding.ContentMods;
using Game;
using Game.GUI;
using Game.GUI.Controls;
using GameLibrary;
using Faark.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Faark.Gnomoria.Mods
{
#if true
    /// <summary>
    /// Adds a button to insta-maximize the current window.
    /// </summary>
    public class MaximizeWindowButton: Mod
    {
        public override IEnumerable<IModification> Modifications
        {
            get
            {
                return new IModification[]{
                    new MethodAddVirtual(
                        typeof(Window),
                        typeof(Control).GetProperty("Resizable").GetSetMethod(),
                        Method.Of<Window, bool>(OnSet_Window_Resizable),
                        MethodHookType.RunBefore
                        ),
                    new MethodHook(
                        typeof(Skin).GetMethod("Init"),
                        Method.Of<Skin>(On_Skin_Init)
                        )
                };
            }
        }
        public override string Author
        {
            get
            {
                return "Faark";
            }
        }
        public override string Description
        {
            get
            {
                return "Adds a button to insta-maximize the current window.";
            }
        }
        private static Window CurrentSelf = null;
        private static Button CurrentWinMaxButton = null;

        private static Texture2D graphicsTex = null;
        private static Dictionary<Type, Rectangle> initialWindowPositions = new Dictionary<Type, Rectangle>();

        public static void On_Skin_Init(Skin self)
        {
            if ((graphicsTex == null) || (graphicsTex.GraphicsDevice != GnomanEmpire.Instance.GraphicsDevice) || graphicsTex.IsDisposed)
            {
                graphicsTex = CustomTextureManager.GetFromAssemblyResource(Assembly.GetExecutingAssembly(), "Faark.Gnomoria.Mods.Resources.maxButtons.png");
                //Texture2D.FromStream(GnomanEmpire.Instance.GraphicsDevice, Assembly.GetExecutingAssembly().GetManifestResourceStream( "Faark.Gnomoria.Mods.Resources.maxButtons.png"));
            }
            var maxImg = new SkinImage();
            maxImg.Resource = graphicsTex; // warning have to load it here!
            maxImg.Name = "Window.MaximizeButton";
            self.Images.Add(maxImg);

            var mySkinLayer = new SkinLayer();
            mySkinLayer.Name = "Control";
            mySkinLayer.Alignment = Alignment.MiddleLeft;
            mySkinLayer.ContentMargins = new Margins(6);
            mySkinLayer.SizingMargins = new Margins(6);
            mySkinLayer.Image = maxImg;
            mySkinLayer.Height = 28;
            mySkinLayer.Width = 28;
            mySkinLayer.States.Disabled.Index = 2;
            mySkinLayer.States.Enabled.Index = 2;
            mySkinLayer.States.Focused.Index = 0;
            mySkinLayer.States.Hovered.Index = 0;
            mySkinLayer.States.Pressed.Index = 2;
            mySkinLayer.Text = new SkinText(self.Controls["Window.CloseButton"].Layers[0].Text);

            var mySkinControl = new SkinControl();
            mySkinControl.Inherits = "Button";
            mySkinControl.ResizerSize = 4;
            mySkinControl.DefaultSize = new Size(28, 28);
            mySkinControl.Name = "Window.MaximizeButton";
            mySkinControl.Layers.Add(mySkinLayer);
            self.Controls.Add(mySkinControl);
        }

        public static void OnSet_Window_Resizable(Window self, bool newState)
        {
            var oldState = self.Resizable;
            if (newState != oldState)
            {
                if (newState)
                {
                    CurrentSelf = self;
                    CurrentWinMaxButton = new Button(self.Manager);
                    CurrentWinMaxButton.Skin = new SkinControl(self.Manager.Skin.Controls["Window.MaximizeButton"]);
                    CurrentWinMaxButton.Init();
                    CurrentWinMaxButton.Detached = true;
                    CurrentWinMaxButton.CanFocus = false;
                    CurrentWinMaxButton.Text = null;
                    CurrentWinMaxButton.Click += new Game.GUI.Controls.EventHandler((sender, args) =>
                    {
                        Rectangle initalPos;
                        if (!initialWindowPositions.TryGetValue(self.GetType(), out initalPos))
                        {
                            initalPos = initialWindowPositions[self.GetType()] = new Rectangle(self.Left, self.Top, self.Width, self.Height);
                        }

                        var isMax = true;
                        
                        if (((self.ResizeEdge & Anchors.Top) == Anchors.Top) || ((self.ResizeEdge & Anchors.Bottom) == Anchors.Bottom))
                        {
                            //var h = Math.Min(self.MaximumHeight, self.Manager.TargetHeight);
                            //var half_h = (self.Manager.TargetHeight / 2) - (Math.Min(self.MaximumHeight, self.Manager.TargetHeight) / 2);
                            var top = Math.Max(100, (int)(self.Manager.TargetHeight * 0.1f));
                            var bottom = Math.Max(60, (int)(self.Manager.TargetHeight * 0.09f));
                            var height = Math.Min(self.MaximumHeight, self.Manager.TargetHeight - top - bottom);
                            if ((self.Top != top) || (self.Height != height))
                            {
                                isMax = false;
                                self.Top = top;
                                self.Height = height;
                            }
                        }
                        if (((self.ResizeEdge & Anchors.Left) == Anchors.Left) || ((self.ResizeEdge & Anchors.Right) == Anchors.Right))
                        {
                            var w = Math.Min((int)(self.Manager.TargetWidth * 0.8f), self.MaximumWidth);
                            var left = (int)(self.Manager.TargetWidth * 0.1f);
                            if ((self.Left != left) || (self.Width != w))
                            {
                                self.Left = left;
                                self.Width = w;
                                isMax = false;
                            }
                        }

                        if (isMax)
                        {
                            self.Top = initalPos.Top;
                            self.Left = initalPos.Left;
                            self.Width = initalPos.Width;
                            self.Height = initalPos.Height;
                        }
                    });
                    var closeSkin = self.Manager.Skin.Controls["Window.MaximizeButton"];
                    SkinLayer skinLayer = closeSkin.Layers["Control"];
                    CurrentWinMaxButton.Width = skinLayer.Width;
                    CurrentWinMaxButton.Height = skinLayer.Height - closeSkin.OriginMargins.Vertical;
                    CurrentWinMaxButton.Left = self.OriginWidth - self.Skin.OriginMargins.Right - skinLayer.Width - closeSkin.OriginMargins.Horizontal + skinLayer.OffsetX - CurrentWinMaxButton.Width;
                    CurrentWinMaxButton.Top = self.Skin.OriginMargins.Top + skinLayer.OffsetY;
                    CurrentWinMaxButton.Anchor = (Anchors.Top | Anchors.Right);
                    self.Add(CurrentWinMaxButton, false);
                }
                else
                {
                    if (self == CurrentSelf)
                    {
                        self.Remove(CurrentWinMaxButton);
                        CurrentWinMaxButton = null;
                        CurrentSelf = null;
                    }
                }
            }
        }
    }
#endif
}
