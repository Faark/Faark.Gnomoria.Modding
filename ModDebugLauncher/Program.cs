using System;
using System.Linq;

namespace ModDebugLauncher
{
    static class Program
    {
        /// <summary>
        /// Start the modded game via this launcher, so you can debug your mod with the free version
        /// of visual studio as well. Also setting a breakpoint within that catch, etc could be useful :)
        /// </summary>
        [STAThread]
        static void Main(string [] args)
        {
            var gnomoria_directory = "C:\\Users\\Faark\\Documents\\Visual Studio 2010\\Projects\\GnomoriaModding\\Release";

            var launcher_dir = new System.IO.FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).Directory.FullName;


            if (!System.IO.File.Exists(System.IO.Path.Combine(gnomoria_directory, "GnomoriaModded.dll")))
            {
                var p = new System.Diagnostics.Process();
                p.StartInfo = new System.Diagnostics.ProcessStartInfo(System.IO.Path.Combine(gnomoria_directory, "GnomoriaModUI.exe"), "-build");//we do not have "-build", yet... but at least we get that ui for now
                p.StartInfo.WorkingDirectory = gnomoria_directory;
                p.Start();
                System.Threading.SpinWait.SpinUntil(() => p.HasExited, System.Threading.Timeout.Infinite); 
                if (!System.IO.File.Exists(System.IO.Path.Combine(gnomoria_directory, "GnomoriaModded.dll")))
                {
                    return;
                }
            }

            AppDomainSetup domainSetup = new AppDomainSetup()
            {
                ApplicationBase = launcher_dir,
                ConfigurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile,
                ApplicationName = AppDomain.CurrentDomain.SetupInformation.ApplicationName,
                LoaderOptimization = System.LoaderOptimization.SingleDomain
            };
            var ad = AppDomain.CreateDomain("GnomoriaDebugEnvironment", null, domainSetup);
            var cd = (CustomDomain)ad.CreateInstanceFromAndUnwrap(
                new Uri(typeof(CustomDomain).Assembly.CodeBase).LocalPath,
                typeof(CustomDomain).FullName
                );
            cd.runGame(gnomoria_directory);
        }


    }
    class CustomDomain : MarshalByRefObject
    {
        string base_dir;

        System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var ass = AppDomain.CurrentDomain.GetAssemblies();
            var trgName = new System.Reflection.AssemblyName(args.Name);
            foreach (var a in ass)
            {
                var aN = new System.Reflection.AssemblyName(a.FullName);
                if (aN.Name == trgName.Name)
                {
                    return a;
                }
            }
            if (trgName.Name == "gnomorialib")
            {
                return System.Reflection.Assembly.Load(System.IO.File.ReadAllBytes(System.IO.Path.Combine(base_dir, trgName.Name + "Modded.dll")));
            }
            var file = System.IO.Path.Combine(base_dir, trgName.Name + ".dll");
            if (trgName.Name == "GnomoriaModController")
            {
                return System.Reflection.Assembly.Load(System.IO.File.ReadAllBytes(file));
            }
            else if (System.IO.File.Exists(file))
            {
                return System.Reflection.Assembly.LoadFile(file);
            }
            else
            {
                file = System.IO.Path.Combine(base_dir, "Mods", trgName.Name + ".dll");
                if (System.IO.File.Exists(file))
                {
                    return System.Reflection.Assembly.LoadFile(file);
                }
            }
            return null;
        }
        void CurrentDomain_FirstChanceException(object sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e)
        {
            // a breakpoint in here could be useful
            return;
        }
        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            // and here another one...
            return;
        }
        public void runGame(string game_dir)
        {
            base_dir = game_dir;
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
            AppDomain.CurrentDomain.FirstChanceException += new EventHandler<System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs>(CurrentDomain_FirstChanceException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            try
            {
                System.IO.Directory.SetCurrentDirectory(base_dir);//we have to set it, since any other path stuff is [intentionally] removed by "Load(byte[])"
                var ass = System.Reflection.Assembly.Load(System.IO.File.ReadAllBytes(System.IO.Path.Combine(base_dir, "GnomoriaModded.dll")));
                var ep = ass.EntryPoint;
                ep.Invoke(null, new object[] { new String[] { "-noassemblyresolve", "-noassemblyloading" } });
                return;
            }
            catch (Exception err)
            {
                System.Windows.Forms.MessageBox.Show(err.ToString());
                return;
            }
        }




    }
}
