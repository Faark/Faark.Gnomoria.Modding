using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace GnomoriaModUI
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            //throw new Exception("Faild :=)");
            if (System.IO.File.Exists("Gnomoria.exe"))
            {
                // Todo: Offer a "just build [now], do not start" mode
                var buildAndQuit = false;
                foreach (var arg in args)
                {
                    switch (arg.ToUpper())
                    {
                        case "-LAUNCH":
                            GameLauncher.Run();
                            return;
                        case "-BUILD":
                            buildAndQuit = true;
                            //todo: actually implement this
                            break;
                    }
                }
                var cecil = Assembly.GetExecutingAssembly().GetManifestResourceStream( "GnomoriaModUI.Cecil.Mono.Cecil.dll");
                var cecilRocks = Assembly.GetExecutingAssembly().GetManifestResourceStream( "GnomoriaModUI.Cecil.Mono.Cecil.Rocks.dll");
                AppDomain.CurrentDomain.Load(new System.IO.BinaryReader(cecil).ReadBytes((int)cecil.Length));
                AppDomain.CurrentDomain.Load(new System.IO.BinaryReader(cecilRocks).ReadBytes((int)cecilRocks.Length));
                AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
                switch (2)
                {
                    case 0:
                        GnomoriaModUI.App.Main();
                        break;
                    case 1:
                        //Application.EnableVisualStyles();
                        //Application.SetCompatibleTextRenderingDefault(false);
                        //Application.Run(new NewUI());
                        break;
                    case 2:
                        Application.EnableVisualStyles();
                        Application.SetCompatibleTextRenderingDefault(false);
                        Application.Run(new Form1());
                        break;
                }
            }
            else
            {
                MessageBox.Show("Gnomoria.exe could not be found in the current directory!\n\n"
                    + "Please copy GnomoriaModUI.exe and GnomoriaModController.dll into\n"
                    + "the folder you installed gnomoria to. At that location should\n"
                    + "also be a folder named modsMod dlls go into the sub-folder 'mods',\n"
                    + "where the dll file of mods go into.", "Error starting GnomoriaModUI", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        static System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            return AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName == args.Name);
        }
    }
}
