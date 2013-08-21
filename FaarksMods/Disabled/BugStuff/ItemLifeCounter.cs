using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Game;
using GameLibrary;
using Faark.Gnomoria.Modding;
using Faark.Util;
using Faark.Util.Serialization;

namespace Faark.Gnomoria.Mods.BugStuff
{
#if false
    /// <summary>
    /// Shows some stats about items...
    /// </summary>
    public class ItemLifeCounter: Mod
    {
        public class ItemBase: GameEntity, IDisposable
        {
            public ItemBase(System.IO.BinaryReader reader, GameEntityManager mgr)
                : base(reader, mgr)
            {
                Counter_Init();
            }
            public ItemBase(Microsoft.Xna.Framework.Vector3 pos)
                : base(pos)
            {
                Counter_Init();
            }

            protected void Counter_Init()
            {
                var stack = System.Environment.StackTrace;
                string cachedStack = null;
                foreach (var el in stacks)
                {
                    if (stack == el)
                    {
                        cachedStack = el;
                        break;
                    }
                }
                if (cachedStack == null)
                {
                    stacks.Add(cachedStack = stack);
                }
                all_items.Add(new Tuple<WeakReference<Item>, string>(
                    this as object as Item,
                    cachedStack
                    ));
                created++;
                refresh();
            }
            private bool disposed = false;
            protected virtual void Dispose(bool disposing)
            {
                if (!disposed)
                {
                    disposed = true;
                    released++;
                    refresh();
                }
            }
            void IDisposable.Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
            ~ItemBase()
            {
                Dispose(false);
            }
        }
        public override IEnumerable<IModification> Modifications
        {
            get
            {
                yield return new ClassChangeBase(typeof(Item), typeof(ItemBase));
                yield return new MethodHook(
                    typeof(GnomanEmpire).GetMethod("PlayGame", BindingFlags.Instance | BindingFlags.Public),
                    Method.Of<GnomanEmpire>(On_GnomanEmpire_PlayGame)
                    );
            }
        }
        private static List<String> stacks = new List<string>();
        private static List<Tuple<WeakReference<Item>, String>> all_items = new List<Tuple<WeakReference<Item>, string>>(2000000);
        private static Game.GUI.Controls.Label lbl;
        private static Game.GUI.Controls.Button btn;
        private static int created = 0;
        private static int released = 0;
        private static void refresh()
        {
            if (lbl != null)
            {
                lbl.Text = "Alive: " + (created - released) + Environment.NewLine + "Created: " + created + Environment.NewLine + "Released: " + released;
            }
        }
        public static void On_GnomanEmpire_PlayGame(GnomanEmpire self)
        {
            lbl = new Game.GUI.Controls.Label(self.GuiManager.Manager);
            lbl.Init();
            lbl.Top = 200;
            lbl.Left = 5;
            lbl.Width = 1000;
            lbl.Height = 100;
            refresh();
            self.GuiManager.Add(lbl);
            btn = new Game.GUI.Controls.Button(self.GuiManager.Manager);
            btn.Init();
            btn.Top = 300;
            btn.Left = 5;
            btn.Text = "Dump";
            btn.Click += new Game.GUI.Controls.EventHandler(btn_Click);
            self.GuiManager.Add(btn);
        }

        static void btn_Click(object sender, Game.GUI.Controls.EventArgs e)
        {
            all_items.RemoveAll(el => !el.Item1.IsAlive);
            var seri =  new System.Web.Script.Serialization.JavaScriptSerializer();
            var query = all_items.Select(el => el.Item2).GroupBy(el => el).Select(el => new { stack = el.Key, count = el.Count() }).OrderByDescending(el => el.count);
            
            var txt = seri.Serialize(query);
            System.IO.File.WriteAllText("D:\\Temp\\Stacks.txt", txt);

            var query2 = all_items.GroupBy(el => el.Item1.Target.ItemID).OrderByDescending(group=>group.Count()).Select(group =>
                {
                    var dict = new Dictionary<string, int>();

                    var regrouped = group.GroupBy(el2 => el2.Item1.Target.MaterialID);
                    if (regrouped.Count() > 1)
                    {
                        dict.Add("total", group.Count());
                    }
                    foreach (var el in regrouped)
                    {
                        dict.Add(((Material)el.Key).ToString(), el.Count());
                    }
                    return Tuple.Create(group.Key.ToString(), new SerializableDataBag<int>(dict));
                }).ToBag();
            using (var file = System.IO.File.OpenWrite("D:\\Temp\\Types.txt"))
            {
                JSON.ToJSON(query2, file, typeof(SerializableDataBag<int>));
            }
            //var txt2 = seri.Serialize(query2);
            //System.IO.File.WriteAllText("D:\\Temp\\Types.txt", txt2);
        }

    }
#endif
}
