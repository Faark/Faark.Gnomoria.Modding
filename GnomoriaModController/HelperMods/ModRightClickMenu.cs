using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using System.Reflection;
using System.Runtime.Serialization;
using Faark.Util;
using Faark.Util.Serialization;

using Game;
using GameLibrary;
using Microsoft.Xna.Framework;

namespace Faark.Gnomoria.Modding.HelperMods
{
    public class ModRightClickMenu: SupportMod
    {
        [DataContract]
        private sealed class MethodRef
        {
            [DataMember]
            public string Method { get; private set; }
            [DataMember]
            public string DeclaringType { get; private set; }
            [DataMember]
            public IEnumerable<string> ParameterTypes { get; private set; }

            private static string typeToString(Type t)
            {
                return t.FullName + ", " + t.Assembly.GetName().Name;
            }
            public MethodRef(MethodInfo method)
            {
                Method = method.Name;
                ParameterTypes = method.GetParameters().Select(para => typeToString(para.ParameterType));
                DeclaringType = typeToString(method.DeclaringType);
            }
            private MethodRef() { }
            public MethodInfo GetMethod()
            {
                var declaringType = Type.GetType(DeclaringType, true);
                return declaringType.GetMethod(Method, ParameterTypes.Select(typeText => Type.GetType(typeText, true)).ToArray());
            }
        }
        #region public stuff
        private Dictionary<String, ModMenuItemClickedCallback> ModMenuItems = new Dictionary<string,ModMenuItemClickedCallback>();
        public static ModRightClickMenu Instance
        {
            get
            {
                return ModEnvironment.Mods.Get<ModRightClickMenu>();
            }
        }
        public ModRightClickMenu()
        {
            ModEnvironment.ResetSetupData += new EventHandler((sender, args) =>
            {
                ModMenuItems.Clear();
            });
        }
        public delegate void ModMenuItemClickedCallback();
        public static void AddItem(string text, ModMenuItemClickedCallback callback)
        {
            Instance.AddButton(text, callback);
        }
        #endregion
        #region Setup stuff
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
                return "Helper object, that makes it easy for other mods to create items in the right click menu under \"Mods\"";
            }
        }
        public override string SetupData
        {
            get
            {
                //return SerializableDataBag.ToJSON(ModMenuItems.Select(kvp => Tuple.Create(kvp.Key, kvp.Value.Method)));
                //return SerializableDataBag.ToJSON(ModMenuItems);
                return SerializableDataBag.ToJSON(ModMenuItems.Select(kvp => Tuple.Create(kvp.Key, new MethodRef(kvp.Value.Method))));
            }
            set
            {
                ModMenuItems = SerializableDataBag
                    .FromJSON<MethodRef>(value)
                    .ToDictionary<ModMenuItemClickedCallback>(
                        mref => (ModMenuItemClickedCallback)Delegate.CreateDelegate(typeof(ModMenuItemClickedCallback), mref.GetMethod())
                        );
            }
        }
        #endregion

        public void AddButton(string text, ModMenuItemClickedCallback callback)
        {
            if (ModEnvironment.Status != ModEnvironment.EnvironmentStatus.InGame)
            {
                if (!(callback.Method.IsStatic && callback.Method.IsPublic))
                    throw new ArgumentException("Click callback has to be public & static! " + callback.Method.Name + " is not.");
                if (UnmutableMethodModification.IsCompilerGenerated(callback.Method))
                    throw new ArgumentException("Click callback can not be compiler generated");
                UnmutableMethodModification.VerifyNestedPublicMethod(callback.Method);
            }
            if (ModMenuItems == null)
            {
                throw new InvalidOperationException("Looks like you are to late, menu already created...");
            }else{
                ModMenuItems.Add(text, callback);
            }
        }

        public override IEnumerable<IModification> Modifications
        {
            get
            {
                yield return new MethodHook(
                    typeof(Game.GUI.RightClickMenu).GetConstructor(new Type[] { }),
                    Method.Of<Game.GUI.RightClickMenu>(On_RightClickMenu_Created)
                    );
            }
        }
        private static FieldInfo RightClickMenu_ContextMenu;
        public override void Initialize_PreGame()
        {
            if ((ModMenuItems == null) )//|| (ModMenuItems.Count == 0))
            {
                throw new InvalidOperationException("Trying to initialize ModRightClickMenu without data.");
            }
            RightClickMenu_ContextMenu = typeof(Game.GUI.RightClickMenu)
                .GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                .Single(field => field.FieldType == typeof(Game.GUI.Controls.ContextMenu));
        }
        public static void On_RightClickMenu_Created(Game.GUI.RightClickMenu self)
        {
            /*
            var ih = typeof(Game.HistoryManager).GetFields(BindingFlags.Instance | BindingFlags.NonPublic).Single(f => f.FieldType == typeof(Dictionary<uint, ItemHistory>));
            var list = (Dictionary<uint, ItemHistory>)ih.GetValue(GnomanEmpire.Instance.Fortress.HistoryManager);

            var part = list.Where(el => el.Value.Components.Count > 0).ToList();
            var part2 = part.Where(el => el.Value.Components.Any(el2 => el2.History.Components.Count > 0)).ToList();
            var part3 = part2.Where(el => el.Value.Components.Any(el2 => el2.History.Components.Any(el3 => el3.History.Components.Count > 0))).ToList();
            var part4 = part3.Where(el => el.Value.Components.Any(el2 => el2.History.Components.Any(el3 => el3.History.Components.Any(el4 => el4.History.Components.Count > 0)))).ToList();
            var part5 = part4.Where(el => el.Value.Components.Any(el2 => el2.History.Components.Any(el3 => el3.History.Components.Any(el4 => el4.History.Components.Any(el5 => el5.History.Components.Count > 0))))).ToList();
            var part6 = part5.Where(el => el.Value.Components.Any(el2 => el2.History.Components.Any(el3 => el3.History.Components.Any(el4 => el4.History.Components.Any(el5 => el5.History.Components.Any(el6 => el6.History.Components.Count > 0)))))).ToList();
            part6.ToString();
            var sh = list.Where(el => el.Value.ItemID == ItemID.SkullHelmet).ToList();
            sh.ToString();
            */
            var context_menu = (Game.GUI.Controls.ContextMenu)(RightClickMenu_ContextMenu.GetValue(self));
            var modsGroup = new Game.GUI.Controls.MenuItem("Mods");
            foreach (var mod_kvp in Instance.ModMenuItems)
            {
                addMenuItem(mod_kvp, modsGroup);
            }
            if (!Instance.ModMenuItems.Any())
            {
                modsGroup.Enabled = false;
            }
            context_menu.Items.Insert(context_menu.Items.Count - 1, modsGroup);
        }
        private static void addMenuItem(KeyValuePair<string, ModMenuItemClickedCallback> mod_kvp, Game.GUI.Controls.MenuItem modsGroup)
        {
            var item = new Game.GUI.Controls.MenuItem(mod_kvp.Key);
            item.Click += new Game.GUI.Controls.EventHandler((sender, args) =>
            {
                mod_kvp.Value();
            });
            modsGroup.Items.Add(item);
        }

    }
}
