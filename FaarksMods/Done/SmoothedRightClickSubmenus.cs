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
    /// <summary>
    /// Makes the right click menu a little smoother to use.
    /// </summary>
    public class SmoothedRightClickSubmenus : Mod
    {
        public override IEnumerable<IModification> Modifications
        {
            get
            {
                return new IModification[]{
                    new MethodHook(
                        typeof(ContextMenu).GetMethod("Show", new Type[] { typeof(Control), typeof(int), typeof(int) }),
                        Method.Of<ContextMenu, Control, int, int>(ContextMenu_Show)
                        ),
                    new MethodHook(
                        typeof(ContextMenu).GetMethod("OnMouseMove", BindingFlags.Instance | BindingFlags.NonPublic),
                        Method.Of<ContextMenu, MouseEventArgs, bool>(ContextMenu_OnMouseMove),
                        MethodHookType.RunBefore,
                        MethodHookFlags.CanSkipOriginal
                        ),
                    new MethodHook(
                        typeof(ContextMenu).GetMethod("OnMouseOut", BindingFlags.Instance | BindingFlags.NonPublic),
                        Method.Of<ContextMenu, MouseEventArgs>(ContextMenu_OnMouseOut)
                        ),
                    new MethodHook(
                        typeof(ContextMenu).GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic),
                        Method.Of<ContextMenu, GameTime>(ContextMenu_Update)
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
                return "Makes the right click menu a little smoother to use.";
            }
        }

        private static MethodInfo ContextMenu_OnMouseMoveFunc;
        private static MethodInfo ContextMenu_OnClickFunc;
        private static PropertyInfo MenuBase_ParentMenu;
        private class MoveTowardsVaildater
        {
            public Point UpperBound;
            public Point LowerBound;

            public Point LastPos;
            public MoveTowardsVaildater(Rectangle target_rect, Point abs_mouse_position)
            {
                var xpos = abs_mouse_position.X < target_rect.Left ? target_rect.Left : target_rect.Right;
                UpperBound = new Point(xpos, target_rect.Top);
                LowerBound = new Point(xpos, target_rect.Bottom);

                LastPos = abs_mouse_position;
            }

            private float Sign(Point p1, Point p2, Point p3)
            {
                return (p1.X - p3.X) * (p2.Y - p3.Y) - (p2.X - p3.X) * (p1.Y - p3.Y);
            }
            private bool IsPointInTri(Point pt, Point v1, Point v2, Point v3)
            {
                bool b1, b2, b3;

                b1 = Sign(pt, v1, v2) < 0.0f;
                b2 = Sign(pt, v2, v3) < 0.0f;
                b3 = Sign(pt, v3, v1) < 0.0f;

                return ((b1 == b2) && (b2 == b3));
            }
            public bool IsMovingTowards(Point abs_mouse_position)
            {
                var ret = IsPointInTri(abs_mouse_position, UpperBound, LowerBound, LastPos);
                LastPos = abs_mouse_position;
                /*if (Math.Abs(XPos - abs_mouse_position.X) < Math.Abs(XPos - LastPos.X))
                {
                    var moved_distance = new Point(LastPos.X - abs_mouse_position.X, LastPos.Y - abs_mouse_position.Y);
                    var trg_distance
                    if( 
                    return true;
                }*/
                return ret;
            }
        }
        private static ContextMenu LastOpenedContextMenu = null;
        private static ContextMenu LastOpeningMenu = null;
        private static MoveTowardsVaildater LastOpenedMoveValidater;
        private static Point LastMousePosition;
        private static DateTime OpenMenuWhenMouseStandingUntil;
        private static MouseEventArgs LastMouseEventArgs;
        public override void Initialize_PreGame()
        {
            MenuBase_ParentMenu = typeof(MenuBase).GetProperty("ParentMenu", BindingFlags.Instance | BindingFlags.NonPublic);
            ContextMenu_OnMouseMoveFunc = typeof(ContextMenu).GetMethod("OnMouseMove", BindingFlags.Instance | BindingFlags.NonPublic);
            ContextMenu_OnClickFunc = typeof(ContextMenu).GetMethod("OnClick", BindingFlags.Instance | BindingFlags.NonPublic);
        }
        public static void ContextMenu_Show(ContextMenu self, Control sender, int x, int y)
        {
            LastOpenedContextMenu = self;
            LastOpenedMoveValidater = new MoveTowardsVaildater(self.AbsoluteRect, LastMousePosition);
            OpenMenuWhenMouseStandingUntil = DateTime.MaxValue;
            LastOpeningMenu = (ContextMenu)MenuBase_ParentMenu.GetValue(self, new object[] { });

            //RuntimeModController.WriteLogO(self, LastOpeningMenu);
            //LastOpenedMenuRect = self.AbsoluteRect;
        }
        public static bool ContextMenu_OnMouseMove(ContextMenu self, MouseEventArgs args)
        {
            LastMouseEventArgs = args;
            var current_mouse_pos = LastMousePosition = new Point(self.AbsoluteLeft + args.Position.X, self.AbsoluteTop + args.Position.Y);
            if (LastOpenedContextMenu != null)
            {
                if (LastOpenedContextMenu != self)
                {
                    if (LastOpenedMoveValidater.LastPos == current_mouse_pos)
                    {
                        //should only happen if called by our update-hack...
                    }
                    else if (LastOpenedMoveValidater.IsMovingTowards(current_mouse_pos))
                    {
                        //RuntimeModController.WriteScreen("Skipped, " + DateTime.Now.Ticks);
                        OpenMenuWhenMouseStandingUntil = DateTime.Now + TimeSpan.FromMilliseconds(100);

                        return true;
                    }

                }
            }
            //RuntimeModController.WriteScreen(null, "Passed, " + DateTime.Now.Ticks);
            return false;
        }
        public static void ContextMenu_OnMouseOut(ContextMenu self, MouseEventArgs args)
        {
            if (LastOpeningMenu == self)
            {
                //RuntimeModController.WriteScreen(null, null, null, "Out, " + DateTime.Now.Ticks);
                LastOpenedContextMenu = null;
                LastOpeningMenu = null;
            }
        }
        public static void ContextMenu_Update(ContextMenu self, GameTime gt)
        {
            if ((LastOpenedContextMenu != null) && (LastOpeningMenu == self))
            {
                if (DateTime.Now > OpenMenuWhenMouseStandingUntil)
                {
                    //RuntimeModController.WriteLogO("CLICKING", self);

                    ContextMenu_OnMouseMoveFunc.Invoke(self, new object[] { LastMouseEventArgs });
                    ContextMenu_OnMouseMoveFunc.Invoke(self, new object[] { LastMouseEventArgs });
                    //ContextMenu_OnClickFunc.Invoke(self, new object[] { new MouseEventArgs(default(Microsoft.Xna.Framework.Input.MouseState), MouseButton.None, 0, Point.Zero) });
                    //RuntimeModController.WriteScreen(null, null, "Timeclick, " + DateTime.Now.Ticks);

                    OpenMenuWhenMouseStandingUntil = DateTime.MaxValue;
                }
            }
        }
    }
}
