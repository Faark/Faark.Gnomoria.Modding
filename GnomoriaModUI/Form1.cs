using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using Faark.Gnomoria.Modding;
using Faark.Util;
using System.Threading.Tasks;

namespace GnomoriaModUI
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }



        ModEnvironmentConfiguration  current_config;
        DataTable table;
        List<Tuple<IMod, bool, DataRow>> found_valid_mods_data;
        bool is_build_required = true;
        ActivityCurtain mod_list_curtain;


        private void Form1_Load(object sender, EventArgs e)
        {
            mod_list_curtain = new ActivityCurtain(
                this, 
                pictureBox_loader,
                grid_modlist, 
                btn_reloadsettings, 
                btn_launchwithmods,
                btn_buildgame
                );
            mod_list_curtain.Show();
            grid_modlist.DataSource = table = new DataTable("Mods");
            grid_modlist.AllowUserToAddRows = false;
            grid_modlist.RowHeadersVisible = false;
            table.Columns.Add("Name", typeof(string)).ReadOnly = true;
            table.Columns.Add("Enabled", typeof(bool));
            table.Columns.Add("Author", typeof(string)).ReadOnly = true;
            table.Columns.Add("Description", typeof(string)).ReadOnly = true;

            Task.Factory.StartNew(() =>
            {
                DoStuff_EnsureDepenciesAreLoaded();
                DoStuff_InitAndLoadConfig();
                DoStuff_VerifyLoadedConfig();
                if (!is_build_required)
                {
                    mod_list_curtain.EnableElement(btn_launchwithmods);
                    mod_list_curtain.DisableElement(btn_buildgame);
                }
                DoStuff_SearchForInstalledMods();
                // syncing here...
                mod_list_curtain.Hide();
            });

        }
        private void btn_build_Click(object sender, EventArgs e)
        {
            mod_list_curtain.Show();
            var task = DoStuff_BuildModdedExe();
            task.ContinueWith((t) =>
            {
                mod_list_curtain.Hide();
                mod_list_curtain.DisableElement(btn_buildgame);
            }, TaskContinuationOptions.OnlyOnRanToCompletion);
            task.ContinueWith((t) =>
            {
                if (t.Exception != null)
                {
                    System.Windows.Forms.MessageBox.Show(t.Exception.ToString());
                }
                mod_list_curtain.Hide();
            }, TaskContinuationOptions.NotOnRanToCompletion);
        }
        private void btn_launchwithmods_Click(object sender, EventArgs e)
        {
            //var bla = Assembly.LoadFrom(gnomoria_modded_executable);
            //bla.EntryPoint.Invoke(null, new object[] { new string[] { } });
            mod_list_curtain.Show();
            if (is_build_required)
            {
                var task = DoStuff_BuildModdedExe();
                task.ContinueWith((t) =>
                {
                    DoStuff_LaunchWithMods();
                    this.Invoke(new Action(() =>
                    {
                        Close();
                    }));
                }, TaskContinuationOptions.OnlyOnRanToCompletion);
                task.ContinueWith((t) =>
                {
                    if (t.Exception != null)
                    {
                        System.Windows.Forms.MessageBox.Show(t.Exception.ToString());
                    }
                    mod_list_curtain.Hide();
                }, TaskContinuationOptions.NotOnRanToCompletion);
            }
            else
            {
                DoStuff_LaunchWithMods();
                Close();
            }
        }
        private void grid_modlist_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            is_build_required = true;
            btn_buildgame.Enabled = true;
        }
        private void btn_reloadsettings_Click(object sender, EventArgs e)
        {
            mod_list_curtain.Show();
            Task.Factory.StartNew(new Action(() =>
            {
                DoStuff_SearchForInstalledMods();
                mod_list_curtain.Hide();
            }));
        }
        private void grid_modlist_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (grid_modlist.IsCurrentCellDirty)
            {
                grid_modlist.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }


        private void DoStuff_EnsureDepenciesAreLoaded()
        {
            //Make sure all assemblys are loaded & can be used by/referenced our mods 
            foreach (var dep in ModManager.Dependencies)
            {
                var fullLoc = new System.IO.FileInfo(dep).FullName.ToUpper();
                var allLocs = AppDomain.CurrentDomain.GetAssemblies().Select(ass => ass.Location.ToUpper()).ToArray();
                if (AppDomain.CurrentDomain.GetAssemblies().Where(ass => ass.Location.ToUpper() == fullLoc).Count() <= 0)
                {
                    Assembly.LoadFrom(dep);
                }
            }
        }
        private void DoStuff_InitAndLoadConfig()
        {
            try
            {
                current_config = ModdingEnvironmentConfiguration.LoadOrCreate(ModManager.GameDirectory.ContainingFile(ModManager.config_file_name));
            }
            catch (Exception err)
            {
                MessageBox.Show(err.ToString());
            }
        }
        private void DoStuff_VerifyLoadedConfig()
        {
            var dir = ModManager.GameDirectory;
            var buildOk = true;
            if ( !current_config.CheckFilesValid(
                    dir.ContainingFile(ModManager.OriginalExecutable),
                    dir.ContainingFile(ModManager.ModdedExecutable),
                    dir.ContainingFile(ModManager.OriginalLibrary),
                    dir.ContainingFile(ModManager.ModdedLibrary)
                    )
                    //gnomoria_directoy, gnomoria_original_executable, gnomoria_modded_executable)
                )
            {
                buildOk = false;
            }

            if (buildOk)
            {
                var checked_files = new List<string>();
                foreach (var mod in current_config.ModReferences)
                {
                    if (!checked_files.Contains(mod.AssemblyFile.FullName))
                    {
                        var hash = mod.AssemblyFile.GenerateMD5Hash();
                        if (hash != mod.Hash)
                        {
                            buildOk = false;
                            break;
                        }
                        checked_files.Add(mod.AssemblyFile.FullName);
                    }
                }
            }

            is_build_required = !buildOk;
            if (is_build_required)
            {
                ModManager.GameDirectory.ContainingFile(ModManager.ModdedExecutable).Delete();
                ModManager.GameDirectory.ContainingFile(ModManager.ModdedLibrary).Delete();
                mod_list_curtain.EnableElementAfter(btn_buildgame);
            }
        }
        private void DoStuff_SearchForInstalledMods()
        {
            found_valid_mods_data = new List<Tuple<IMod, bool, DataRow>>();
            this.Invoke(new Action(() =>
            {
                table.Rows.Clear();
            }));

            var searcher = new LocalModsFinder();
            searcher.OnModFound += new EventHandler<EventArgs<IMod>>((send, args) =>
            {
                this.Invoke(new Action(() =>
                {
                    var dependencyItem = found_valid_mods_data.SingleOrDefault(found => (found.Item2 == false) && (found.Item1.GetType() == args.Argument.GetType()));
                    var active = current_config.ModReferences.SingleOrDefault(mod_in_config => mod_in_config.TryGetType() == args.Argument.GetType());
                    if (dependencyItem != null)
                    {
                        found_valid_mods_data[found_valid_mods_data.IndexOf(dependencyItem)] = new Tuple<IMod,bool,DataRow>(
                            dependencyItem.Item1,
                            true,
                            table.Rows.Add(
                                args.Argument.Name,
                                active != null,
                                args.Argument.Author,
                                args.Argument.Description
                                )
                            );
                    }
                    else
                    {
                        found_valid_mods_data.Add(
                            new Tuple<IMod, bool, DataRow>(
                                args.Argument,
                                true,
                                table.Rows.Add(
                                    args.Argument.Name,
                                    active != null,
                                    args.Argument.Author,
                                    args.Argument.Description
                                    )
                                )
                            );
                    }
                }));
//fixed it, see below #warning case of removed or not anymore existing mod is not checked! GUI looks okay, but make sure it is gone from config or refresh is true
            });
            searcher.OnDependencyFound += new EventHandler<EventArgs<IMod>>((sender, args) =>
            {
                found_valid_mods_data.Add(new Tuple<IMod, bool, DataRow>(args.Argument, false, null));
                //throw new NotImplementedException();
            });
            searcher.RunSync(ModManager.GameDirectory);
            var dupes = found_valid_mods_data.GroupBy(el => el.Item1.GetType().FullName).Where(group => group.Count() > 1);
            if (dupes.Count() > 0)
            {
                var flatternedDupes = dupes.SelectMany(group=>group).ToList();
                // Todo: this solution is crap, it does not catch dependencies.
                var msg = "Some mods are found in multiple files. You have to remove those conflicts"+/*, before you can use affected mods*/".\n\nAffected mods:\n";
                msg += flatternedDupes.Select(el => (" - " + el.Item1.GetType().FullName)).Distinct().Aggregate((el1, el2) => el1 + "\n" + el2);
                msg += "\n\nAffected files:\n";
                msg += flatternedDupes.Select(el =>
                {
                    string refPath;
                    var path = el.Item1.GetType().Assembly.Location;
                    if (FileExtensions.GetRelativePath(ModManager.GameDirectory.FullName, path, out refPath))
                    {
                        path = refPath;
                    }
                    return (" - " + path);
                }).Distinct().Aggregate((el1, el2) => el1 + "\n" + el2);
                msg+= "\n\nDeleting the outdated file usually helps.";
                System.Windows.Forms.MessageBox.Show(msg, "Error loading mods");
                found_valid_mods_data.RemoveAll(el =>
                {
                    if (flatternedDupes.Contains(el))
                    {
                        if (el.Item3 != null)
                        {
                            el.Item3.Table.Rows.Remove(el.Item3);
                        }
                        return true;
                    }
                    return false;
                });
                /*this.Invoke(new Action(() =>
                {
                    grid_modlist.Invalidate(true);
                }));*/
                // Todo: cant remove this throw til this goddman list is properly updated without crashing afterwards...
                throw new Exception("Some mods are invalid; TODO: implement better handling, at least for the new UI.");
            }
            foreach (var conf_mod in current_config.ModReferences)
            {
                if (found_valid_mods_data.Count(tup => tup.Item1.GetType() == conf_mod.TryGetType()) <= 0)
                {
                    //Mod not found; => remove
                    is_build_required = true;
                    return;
                }
            }
        }
        private Task DoStuff_BuildModdedExe()
        {
            return Task.Factory.StartNew(() =>
            {
                ModManager.GameDirectory.ContainingFile(RuntimeModController.Log.LogfileName).Delete();
                DoStuff_EnsureDepenciesAreLoaded();

                var newModConfig = new ModdingEnvironmentWriter(
                    found_valid_mods_data.Where(el => (el.Item3 != null) && ((bool)el.Item3.ItemArray[1])).Select(el => el.Item1).ToArray(),
                    found_valid_mods_data.Where(el => (el.Item3 == null) || !((bool)el.Item3.ItemArray[1])).Select(el => el.Item1).ToArray(),
                    false
                    );
                newModConfig.SaveEnvironmentConfiguration(ModManager.GameDirectory.ContainingFile(ModManager.config_file_name));
                is_build_required = false;
            });
        }
        private void DoStuff_LaunchWithMods()
        {
            var currentApp = new System.IO.FileInfo(Assembly.GetExecutingAssembly().Location);
            var launcherProcess = new System.Diagnostics.Process();
            launcherProcess.StartInfo.FileName = currentApp.FullName;
            launcherProcess.StartInfo.WorkingDirectory = currentApp.Directory.FullName;
            launcherProcess.StartInfo.Arguments = "-launch";
            launcherProcess.Start();
            /*
            var gameProcess = new System.Diagnostics.Process();
            gameProcess.StartInfo.FileName = ModManager.GameDirectory.ContainingFile(ModManager.ModdedExecutable).FullName;
            gameProcess.StartInfo.WorkingDirectory = ModManager.GameDirectory.FullName;
            gameProcess.Start();
            */
        }


        private class ActivityCurtain
        {
            private abstract class AAction
            {
                public Control ctrl;
                public abstract void DoIt(Form1 self);
                public AAction(Control ctr)
                {
                    this.ctrl = ctr;
                }
            }
            /*private abstract class AShowHide : AAction
            {
                public AShowHide(Control ctrl) : base(ctrl) { }
            }*/
            private class AHide : AAction
            {
                public AHide(Control ctr) : base(ctr) { }
                public override void DoIt(Form1 self)
                {
                    ctrl.Hide();
                }
            }
            private class AShow : AAction
            {
                public AShow(Control ctr) : base(ctr) { }
                public override void DoIt(Form1 self)
                {
                    ctrl.Show();
                }
            }
            /*private abstract class AEnableDisable : AAction
            {
                public AEnableDisable(Control ctrl) : base(ctrl) { }
            }*/
            private class AEnable : AAction
            {
                public AEnable(Control ctr) : base(ctr) { }
                public override void DoIt(Form1 self)
                {
                    ctrl.Enabled = true;
                }
            }
            private class ADisable : AAction
            {
                public ADisable(Control ctr) : base(ctr) { }
                public override void DoIt(Form1 self)
                {
                    ctrl.Enabled = false;
                }
            }
            private Form1 form;
            private List<AAction> onShowCurtainActions = new List<AAction>();
            private List<AAction> onHideCurtainActions = new List<AAction>();
            private List<AAction> onShowOnTimeCurtainActions = new List<AAction>();
            private List<AAction> onHideOnTimeCurtainActions = new List<AAction>();
            private Control[] disabledControls;
            public ActivityCurtain(Form1 self, Control activityStatus, params Control[] controls)
            {
                form = self;
                onShowCurtainActions.Add(new AShow(activityStatus));
                onHideCurtainActions.Add(new AHide(activityStatus));
                disabledControls = controls;
                /*foreach (var ctrl in controls)
                {
#warning make it "restore state" instead
                    onShowCurtainActions.Add(new ADisable(ctrl));
                    onHideCurtainActions.Add(new AEnable(ctrl));
                }*/
            }
            private void RemoveOneTimeHideActions(IEnumerable<AAction> el)
            {
                var list = el.ToArray();
                foreach (var e in list)
                {
                    onHideOnTimeCurtainActions.Remove(e);
                }
            }
            private bool isVisible = false;

            public bool Shown
            {
                get
                {
                    return isVisible;
                }
            }

            public event EventHandler AfterShowing;
            public void Show()
            {
                if (form.InvokeRequired)
                {
                    form.Invoke(new Action(() => this.Show()));
                }
                else
                {
                    if (isVisible)
                        throw new InvalidOperationException("Can't show, already shown");
                    isVisible = true;
                    foreach (var action in onShowCurtainActions)
                    {
                        action.DoIt(form);
                    }
                    foreach (var ctrl in disabledControls)
                    {
                        if (ctrl.Enabled == true)
                        {
                            ctrl.Enabled = false;
                            onHideOnTimeCurtainActions.Add(new AEnable(ctrl));
                        }
                    }
                    if (AfterShowing != null)
                    {
                        AfterShowing.Invoke(this, new EventArgs());
                    }
                    /*
                    form.Invoke(new Action(() =>
                    {
                        form.pictureBox_loader.Show();
                        form.grid_modlist.Enabled = false;
                        form.btn_buildgame.Enabled = false;
                        form.btn_launchwithmods.Enabled = false;
                        form.btn_reloadsettings.Enabled = false;
                    }));
                    */
                }
            }
            public event EventHandler AfterHiding;
            public void Hide()
            {
                if (form.InvokeRequired)
                {
                    form.Invoke(new Action(() => this.Hide()));
                }
                else
                {
                    if (!isVisible)
                        throw new InvalidOperationException("Can't hide, not visible!");
                    isVisible = false;
                    foreach (var action in onHideCurtainActions)
                    {
                        action.DoIt(form);
                    }
                    foreach (var action in onHideOnTimeCurtainActions)
                    {
                        action.DoIt(form);
                    }
                    if (AfterHiding != null)
                    {
                        AfterHiding.Invoke(this, new EventArgs());
                    }
                    /*
                    form.Invoke(new Action(() =>
                    {
                        form.pictureBox_loader.Hide();
                        form.grid_modlist.Enabled = true;
                        form.btn_buildgame.Enabled = form.is_build_required;
                        form.btn_launchwithmods.Enabled = true;
                    }));
                    */
                }
            }
            public void EnableElement(Control el)
            {
                RemoveOneTimeHideActions(onHideOnTimeCurtainActions.Where(a => (a.ctrl == el) && ((a is AEnable) || (a is ADisable))));
                form.Invoke(new Action(() =>
                {
                    el.Enabled = true;
                }));
            }
            public void DisableElement(Control el)
            {
                RemoveOneTimeHideActions(onHideOnTimeCurtainActions.Where(a => (a.ctrl == el) && ((a is AEnable) || (a is ADisable))));
                form.Invoke(new Action(() =>
                {
                    el.Enabled = false;
                }));
            }
            public void EnableElementAfter(Control el)
            {
                if (isVisible)
                    onHideOnTimeCurtainActions.Add(new AEnable(el));
                else
                    el.Enabled = true;
            }
            public void DisableElementAfter(Control el)
            {
                if (isVisible)
                    onHideOnTimeCurtainActions.Add(new ADisable(el));
                else
                    el.Enabled = false;
            }
        }


        private Assembly gnomoira_assembly()
        {
            //var names = AppDomain.CurrentDomain.GetAssemblies().Select(ass => ass.GetName().Name).ToArray();
            return AppDomain.CurrentDomain.GetAssemblies().First(ass=>ass.GetName().Name=="Gnomoria");
        }
        private string gnomoria_doStringLookup_dbb(int index)
        {
            //var loaderErrors = gnomoira_assembly().error
            var lookupType = gnomoira_assembly().GetType("A.c1b644453eb21426f5c82db1d63df5f7e");
            var lookupMethod = lookupType.GetMethod("c1d2b96876a333d66df46330c3229edbb", BindingFlags.Static | BindingFlags.NonPublic);
            return (string)lookupMethod.Invoke(null, new object[] { index });
        }
        private int gnomoria_doIntLookup_410(int index)
        {
            var lookupType = gnomoira_assembly().GetType("A.c06e20ef4ff40d34b3efb1fbaea234a1b");
            var lookupMethod = lookupType.GetMethod("ce712b56bdc0366b254182b3172155410", BindingFlags.Static | BindingFlags.NonPublic);
            return (int)lookupMethod.Invoke(null, new object[] { index });
        }
        private void button1_Click(object sender, EventArgs e)
        {

            /*
            var x = Game.GUI.Controls.Anchors.Vertical;
            var a1 = gnomoria_doIntLookup_410(35266);
            var a2 = (Game.GUI.Controls.Anchors)gnomoria_doIntLookup_410(46946);
            var a3 = gnomoria_doIntLookup_410(35270);
            var a4 = gnomoria_doIntLookup_410(35262);
            var mat = (GameLibrary.Material)80;
            var a5 = gnomoria_doIntLookup_410(46958);
            var a6 = gnomoria_doIntLookup_410(46962);
            var a7 = gnomoria_doIntLookup_410(35238);
            var a8 = gnomoria_doIntLookup_410(35238);
            var a9 = gnomoria_doIntLookup_410(35238);



            var gnomoria_game = Game.GnomanEmpire.Instance;
            //gnomoria_game.Content.RootDirectory = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "Content");
            //typeof(Game.GnomanEmpire).GetMethod("Initialize", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(gnomoria_game, null);


            //A.c1b644453eb21426f5c82db1d63df5f7e.c1d2b96876a333d66df46330c3229edbb(14396)
            //gnomoria_game.LoadGame("world0.sav", false);

            */
            return;
        }



    }
}
