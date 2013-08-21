//#define NO_ASSEMBLY_LOADING
// see also modding.cs, injector.cs

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;
using System.Text;
using System.Reflection;
using Faark.Gnomoria.Modding;
using Faark.Util;

namespace GnomoriaModUI
{
    internal class LocalModsFinder
    {
        public event EventHandler<EventArgs<IMod>> OnDependencyFound;
        public virtual void DependencyFound(IMod mod)
        {
            OnDependencyFound.TryRaise(this, mod);
        }
        public event EventHandler<EventArgs<IMod>> OnModFound;
        public virtual void ModFound(IMod mod)
        {
            OnModFound.TryRaise(this, mod);
        }
        public event EventHandler OnSearchEnded;

        private List<IMod> foundDependencies = new List<IMod>();
        private List<IMod> foundMods = new List<IMod>();

        public virtual void SearchEnded()
        {
            OnSearchEnded.TryRaise(this);
        }
        private void processModType(ModType modType, bool isDep = false)
        {
            try
            {
                var mod = ModEnvironment.Mods[modType];
                if (isDep)
                {
                    foundDependencies.Add(mod);
                }
                else
                {
                    foundMods.Add(mod);
                }
                mod.Initialize_ModDiscovery();
                foreach (var dep in mod.Dependencies)
                {
                    if (foundMods.Count(fmod => fmod.GetType() == dep.Type) <= 0)
                    {
                        if (foundDependencies.Count(fdep => fdep.GetType() == dep.Type) <= 0)
                        {
                            processModType(dep, true);
                        }
                    }
                }
                if (isDep)
                {
                    DependencyFound(mod);
                }
                else
                {
                    ModFound(mod);
                }
            }
            catch (Exception err)
            {
                MessageBox.Show(err.ToString());
                //to log...
            }
        }
        public void RunSync(System.IO.DirectoryInfo gnomoria_base)
        {
            try
            {
                //Make sure all assemblys are loaded & can be used by/referenced our mods 
                //processModType(typeof(Faark.Gnomoria.Modding.DemoMods.PopulationUI_SyncTabScrolls));
                //processModType(typeof(Faark.Gnomoria.Modding.DemoMods.Game_CreateBackupSavegame));


                var mod_dir = gnomoria_base.GetDirectories().Single(sub => sub.Name.ToUpper() == "MODS");
                var mod_files = mod_dir.GetFiles("*.dll");
                var mod_assemblies = new List<Assembly>();
                foreach (var mod_file in mod_files)
                {
                    var uri = new Uri(mod_file.FullName).AbsoluteUri;
                    var zone = System.Security.Policy.Zone.CreateFromUrl(uri);
                    //MessageBox.Show(zone.SecurityZone + "\n" + uri);
                    var maySecurityProblems = zone.SecurityZone == System.Security.SecurityZone.Internet || zone.SecurityZone == System.Security.SecurityZone.Untrusted;
                    try
                    {
                        mod_assemblies.Add(Assembly.LoadFrom(mod_file.FullName));
                    }
                    catch (BadImageFormatException)
                    {
                        // Todo: Skipping is the curr solution for non-net-dlls. find a better one or just remove once we use net-dir again.
                    }
                    catch (Exception err)
                    {
                        var msg = "Error loading file: "+mod_file.FullName+"\n\n";
                        if (maySecurityProblems)
                        {
                            msg += "This file could be blocked due to security stuff. Try right click it in explorer to open its properties. Check the general tab for 'Security' at the bottom.\n\n\n";
                        }
                        msg += err.ToString();
                        System.Windows.Forms.MessageBox.Show(msg);
                    }
                }
                var modTypes = new List<Type>();//todo: place this somewhere else?
                foreach (var assembly in mod_assemblies)
                {
                    try
                    {
                        var mod_types = assembly.GetTypes().Where(t =>
                        {
                            return typeof(IMod).IsAssignableFrom(t) && !(typeof(Faark.Gnomoria.Modding.SupportMod).IsAssignableFrom(t));
                        });
                        foreach (var mod_type in mod_types)
                        {
                            processModType(new ModType(mod_type));
                        }
                    }
                    catch (ReflectionTypeLoadException)
                    {
                        continue;
                    }
                    catch (Exception err)
                    {
                        System.Windows.Forms.MessageBox.Show(err.ToString());
                    }
                }
            }
            catch (Exception err)
            {
                System.Windows.Forms.MessageBox.Show(err.ToString());
            }

            SearchEnded();
        }
        public void RunAsync()
        {
            throw new NotImplementedException();
        }
    }
}
