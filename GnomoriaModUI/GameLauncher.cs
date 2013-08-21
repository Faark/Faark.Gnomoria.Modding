using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GnomoriaModUI
{
    class GameLauncher
    {
        private class CustomDomainForModRuntime : MarshalByRefObject
        {
            // Warning: Make sure there are no dependency-related things in here that could trigger an auto load
            string base_dir;
            public void runGame()
            {
                //throw new Exception("LOGGING IS OFF!");
                base_dir = new System.IO.FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).Directory.FullName;
                AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
                AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
                var os = System.Environment.OSVersion;
                if (os.Version.Major > 5)
                {
                    AppDomain.CurrentDomain.FirstChanceException += new EventHandler<System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs>(CurrentDomain_FirstChanceException);
                }
                try
                {
                    var ass = System.Reflection.Assembly.Load(System.IO.File.ReadAllBytes("GnomoriaModded.dll"));
                    var ep = ass.EntryPoint;
                    //var inst = ass.GetType("Game.GnomanEmpire").GetProperty("Instance").GetGetMethod().Invoke(null, new object[] { });
                    //var obj = ass.CreateInstance(ep.Name);
                    ep.Invoke(null, new object[] { new String[] { "-noassemblyresolve", "-noassemblyloading" } });
                    return;
                }
                catch (Exception err)
                {
                    CustomErrorHandler(err);
                }
            }

            private Exception lastFirstChanceException = null;
            void CurrentDomain_FirstChanceException(object sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e)
            {
                handleStuff_Enter();
                if (!(e.Exception is System.Threading.ThreadAbortException))
                {
                    Faark.Gnomoria.Modding.RuntimeModController.Log.Write("FirstChanceException", e.Exception);
                    lastFirstChanceException = e.Exception;
                }
                handleStuff_Leave();
            }
            void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
            {
                handleStuff_Enter();
                if (e.ExceptionObject == lastFirstChanceException)
                {
                    Faark.Gnomoria.Modding.RuntimeModController.Log.Write("First Chance Exception is not handled" + (e.IsTerminating ? ", terminating." : "."));
                }
                else
                {
                    Faark.Gnomoria.Modding.RuntimeModController.Log.Write(e.IsTerminating ? "UnhandledException (t)" : "UnhandledException", e.ExceptionObject as Exception);
                }
                handleStuff_Leave();
            }
            private bool isCurrentlyHandlingSth = false;
            private void handleStuff_Enter()
            {
                if (isCurrentlyHandlingSth)
                {
                    AppDomain.CurrentDomain.UnhandledException -= CurrentDomain_UnhandledException;
                    AppDomain.CurrentDomain.FirstChanceException -= CurrentDomain_FirstChanceException;
                    Faark.Gnomoria.Modding.RuntimeModController.Log.Write("Tried to handle an error while already doing this.");
                    throw new Exception("Tried to handle an error while already doing it...");
                }
                isCurrentlyHandlingSth = true;
            }
            private void handleStuff_Leave()
            {
                isCurrentlyHandlingSth = false;
            }
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
                return null;
            }
            void CustomErrorHandler(Exception err)
            {
                Faark.Gnomoria.Modding.RuntimeModController.Log.Write("UnhandledException", err);
                System.Windows.Forms.MessageBox.Show(
                    "Sorry, but Gnomoria has crashed." + Environment.NewLine
                    + Environment.NewLine
                    + Environment.NewLine
                    + "Check out these logfiles for more information:" + Environment.NewLine
                    + Faark.Gnomoria.Modding.RuntimeModController.Log.GetLogfile().FullName + Environment.NewLine
                    + Faark.Gnomoria.Modding.RuntimeModController.Log.GetGameLogfile().FullName,
                    "Gnomoria [modded] has crashed.");
            }
        }

        public static void Run()
        {
            AppDomainSetup domainSetup = new AppDomainSetup()
            {
                //ApplicationBase = System.IO.Directory.GetCurrentDirectory(),
                ApplicationBase = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "Mods"),
                ConfigurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile,
                ApplicationName = AppDomain.CurrentDomain.SetupInformation.ApplicationName,
                LoaderOptimization = System.LoaderOptimization.SingleDomain
                //LoaderOptimization.MultiDomainHost
            };
            var ad = AppDomain.CreateDomain("GnomoriaDebugEnvironment", null, domainSetup);
            //ad.ExecuteAssemblyByName(AppDomain.CurrentDomain.FriendlyName, "-NoResolve");
            //ad.ExecuteAssembly("GnomoriaModded.exe");

            var cd = (CustomDomainForModRuntime)ad.CreateInstanceFromAndUnwrap(
                new Uri(typeof(CustomDomainForModRuntime).Assembly.CodeBase).LocalPath,
                typeof(CustomDomainForModRuntime).FullName
                );
            cd.runGame();
        }
    }
}
