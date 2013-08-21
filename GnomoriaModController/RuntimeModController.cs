using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Faark.Util;

namespace Faark.Gnomoria.Modding
{   


    public static class RuntimeModController
    {

        private static List<IMod> active_mods;
        private static ModSaveFile modSaveFile;
        public static void Initialize(string[] args)
        {
            ModEnvironment.Status = ModEnvironment.EnvironmentStatus.InGame;
            //System.Diagnostics.Debugger.Break();
            //System.Diagnostics.Debug.Assert(false);

            Log.WriteText(
                Environment.NewLine
                + "________________________________________________" + Environment.NewLine
                + "Gnomoria with mod support startet." + Environment.NewLine
                + "Game version: " + typeof(Game.GnomanEmpire).Assembly.GetName().Version + Environment.NewLine
                + Environment.NewLine,
                Log.LogLevel.Normal,
                Log.TargetModes.File
                );

            //AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            var loadAssemblys = !args.Contains("-noassemblyloading");
            try
            {
                active_mods = new List<IMod>();
                var loaded_modfiles = new List<string>();
                var config = ModEnvironmentConfiguration.Load(new System.IO.FileInfo("GnomoriaModConfig.xml"));
                foreach (var modRef in config.ModReferences)
                {
                    if (!loaded_modfiles.Contains(modRef.AssemblyFile.FullName))
                    {
                        if (loadAssemblys)
                        {
                            Assembly.LoadFrom(modRef.AssemblyFile.FullName);
                        }
                        else
                        {
                            Type.GetType(modRef.TypeName);
                        }
                        loaded_modfiles.Add(modRef.AssemblyFile.FullName);
                    }
                }
                foreach (var modRef in config.ModReferences)
                {
                    var mod = ModEnvironment.Mods[modRef];
                    if (modRef.SetupData != null)
                    {
                        mod.SetupData = modRef.SetupData;
                    }
                    mod.Initialize_PreGame();
                    active_mods.Add(mod);
                }
            }
            catch (Exception err)
            {
                Log.Write(err);
                throw;
            }


            //System.IO.File.WriteAllLines("c:\\programme\\gnomoria\\modding.xml", new string[] { "IT", "DOES", "FUCKING", "WORK", "", ":D" });

            //throw new Exception("injecting mod-init successful");
            /*
            RuntimeModsConfig modsConfig;
            var serializer = new System.Xml.Serialization.XmlSerializer(typeof(RuntimeModsConfig));
            using (var fstream = System.IO.File.OpenRead("ModRuntimeData.xml"))
            {
                modsConfig = (RuntimeModsConfig)serializer.Deserialize(fstream);
                fstream.Close();
            }
            */
            //LoadMod(new DemoMod().GetConfig());
        }
        public static IEnumerable<IMod> ActiveMods
        {
            get
            {
                return active_mods;
            }
        }

        public static void PreSaveHook(Game.GnomanEmpire self, bool fallenKingdom)
        {
            foreach (var mod in active_mods)
            {
                mod.PreGameSaved(modSaveFile.GetDataFor(mod));
            }
        }
        public static System.Threading.Tasks.Task PostSaveHook(System.Threading.Tasks.Task saveTask, Game.GnomanEmpire self, bool fallenKingdom)
        {
            return saveTask.ContinueWith((task) =>
            {
                foreach (var mod in active_mods)
                {
                    mod.AfterGameSaved(modSaveFile.GetDataFor(mod));
                }
                var path = fallenKingdom ? Game.GnomanEmpire.SaveFolderPath("OldWorlds\\") : Game.GnomanEmpire.SaveFolderPath("Worlds\\");
                // Todo: fallen kingdoms not considered with filename and so on!
                var file = System.IO.Path.Combine(path, self.CurrentWorld + ".msv");
                modSaveFile.SaveTo(new System.IO.FileInfo(file));
            });
        }
        public static void PreLoadHook(Game.GnomanEmpire self, string fileName, bool fallenKingdom)
        {
            var dir = fallenKingdom ? Game.GnomanEmpire.SaveFolderPath("OldWorlds\\") : Game.GnomanEmpire.SaveFolderPath("Worlds\\");
            var file = System.IO.Path.Combine(dir, fileName + ".msv");
            modSaveFile = ModSaveFile.LoadFrom(new System.IO.FileInfo(file));
            foreach (var mod in active_mods)
            {
                mod.PreGameLoaded(modSaveFile.GetDataFor(mod));
            }
        }
        public static void PostLoadHook(Game.GnomanEmpire self, string fileName, bool fallenKingdom)
        {
            foreach (var mod in active_mods)
            {
                mod.AfterGameLoaded(modSaveFile.GetDataFor(mod));
            }
        }
        public static void PreCreateHook(Game.Map self, Game.CreateWorldOptions options)
        {
            modSaveFile = new ModSaveFile();
            foreach (var mod in active_mods)
            {
                mod.PreWorldCreation(modSaveFile.GetDataFor(mod), self, options);
            }
        }
        public static void PostCreateHook(Game.Map self, Game.CreateWorldOptions options)
        {
            foreach (var mod in active_mods)
            {
                mod.PostWorldCreation(modSaveFile.GetDataFor(mod), self, options);
            }
        }

        public static Game.GnomanEmpire Debug_GetEmpire()
        {
            return Game.GnomanEmpire.Instance;
        }


        public static class Log
        {
            [Flags]
            public enum TargetModes
            {
                UseGlobalSetting = 0x0,
                File = 0x1,
                Screen = 0x2,
                Both = 0x3
            };
            private static TargetModes mTarget = TargetModes.Both;
            public static TargetModes Target
            {
                get
                {
                    return mTarget;
                }
                set
                {
                    if (value == TargetModes.UseGlobalSetting)
                        return;
                    mTarget = TargetModes.Both;
                }
            }
            /// <summary>
            /// This stuff is not yet implemented!
            /// </summary>
            public enum LogLevel { Normal, Info, Warning, Error, Always };
            public static bool SuppressDoubleExceptions = true;

            public const string LogfileName = "GnomoriaModded.log";
            public static System.IO.FileInfo GetLogfile()
            {
                return new System.IO.FileInfo(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), LogfileName));
            }
            public static System.IO.FileInfo GetGameLogfile()
            {
                var assembly = Assembly.GetAssembly(typeof(Game.GUI.Controls.Manager));
                string path = Game.GnomanEmpire.SaveFolderPath(null) + System.IO.Path.GetFileNameWithoutExtension(assembly.Location) + ".log";
                return new System.IO.FileInfo(path);
            }

            private static string ObjectContentToString(object obj)
            {
                return (obj == null) ? "-null-" : obj.ToString();
            }
            private static string ObjectContentToStringOrType<T>(T obj)
            {
                if (obj != null)
                {
                    return obj.ToString();
                }
                else
                {
                    return "-null- (" + typeof(T).FullName + ")";
                }
            }
            private static string ObjectToString(object obj)
            {
                if (obj == null)
                    return "-null-";
                else if (obj is string)
                    return obj.ToString();
                else if (obj.GetType().IsValueType)
                    return obj.ToString();
                else
                {
                    return obj.ToString() + "; Hash: " + obj.GetHashCode();
                }
            }

            private static object writeLock = new object();
            private static bool supressWrite = false;
            private static void DoWrite(string text, LogLevel level, TargetModes target)
            {
                lock (writeLock)
                {
                    if (supressWrite)
                        return;
                    supressWrite = true;
                    target = target == TargetModes.UseGlobalSetting ? Target : target;
                    if (target.HasFlag(TargetModes.File))
                    {
                        try
                        {
                            System.IO.File.AppendAllText(GetLogfile().FullName, text);
                        }
                        catch (Exception) { }
                    }
                    if (target.HasFlag(TargetModes.Screen))
                    {
                        try
                        {
                            var g = Game.GnomanEmpire.Instance;
                            if (g != null)
                            {
                                var w = g.World;
                                if (w != null)
                                {
                                    var nm = w.NotificationManager;
                                    if (nm != null)
                                    {
                                        if (g.Region != null)
                                        {
                                            var guiMgr = g.GuiManager;
                                            if (guiMgr != null)
                                            {
                                                if (guiMgr.HUD != null)
                                                {
                                                    nm.AddNotification(text, false);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception) { }
                    }
                    supressWrite = false;
                }
            }

            public static void Write(IEnumerable<String> lines, LogLevel level = LogLevel.Normal, TargetModes target = TargetModes.UseGlobalSetting )
            {
                DoWrite(
                    "Date: " + DateTime.Now.ToString() 
                    + Environment.NewLine
                    + String.Join(Environment.NewLine, lines) 
                    + Environment.NewLine
                    + Environment.NewLine,
                    level,
                    target
                    );
            }
            public static void Write(LogLevel level = LogLevel.Normal, TargetModes target = TargetModes.UseGlobalSetting, params string[] texts)
            {
                Write((IEnumerable<string>)texts, level, target);
            }
            public static void Write(params string[] texts)
            {
                Write((IEnumerable<string>)texts);
            }
            public static void Write(string text, LogLevel level = LogLevel.Normal, TargetModes target = TargetModes.UseGlobalSetting)
            {
                Write(text.Yield(), level, target);
            }
            private static Exception lastException;
            public static void Write(string preText, Exception err, LogLevel level = LogLevel.Normal, TargetModes target = TargetModes.UseGlobalSetting)
            {
                lastException = err;
                string errText;
                try
                {
                    errText = ObjectContentToStringOrType(err);
                }
                catch (Exception toTextEx)
                {
                    try
                    {
                        errText = "Failed to retrieve error message: "+toTextEx.ToString();
                    }
                    catch (Exception)
                    {
                        errText = "Fatal error retrieving error message.";
                    }
                }
                Write((preText == null ? "" : preText + " ") + errText, level, target);
            }
            public static void Write(Exception err, LogLevel level = LogLevel.Normal, TargetModes target = TargetModes.UseGlobalSetting)
            {
                if (SuppressDoubleExceptions && (err == lastException))
                {
                    return;
                }
                lastException = err;
                Write((string)null, err /*ObjectContentToStringOrType(err)*/, level, target);
            }
            public static void Write(params object[] what)
            {
                Write(what.Select(el => ObjectToString(el)));
            }
            public static void Write(LogLevel level = LogLevel.Normal, TargetModes target = TargetModes.UseGlobalSetting, params object[] what)
            {
                Write(what.Select(el => ObjectToString(el)), level, target);
            }
            public static void WriteList<T>(IEnumerable<T> collection, LogLevel level = LogLevel.Normal, TargetModes target = TargetModes.UseGlobalSetting)
            {
                var list = collection as IList<T>;
                var pre = new List<String>(1);
                if (list == null)
                {
                    pre.Add("Enumeration<" + typeof(T).FullName + ">: " + ObjectContentToString(collection));
                }
                else
                {
                    pre.Add("List<" + typeof(T).FullName + ">: " + ObjectContentToString(collection));
                }
                Write(collection == null ? pre : pre.Union(collection.Select(el => "> " + ObjectToString(el))), level, target);
            }
            public static void WriteText(string text, LogLevel level, TargetModes target)
            {
                DoWrite(text + Environment.NewLine, level, target);
            }


            /*
            public static void WriteLog(Exception err)
            {

                //short but localized version:
                WriteLog("\n\nERROR (" + DateTime.Now.ToString() + "):\n" + err.ToString());
                return;
                /*
                String error_text = null;
                var t = new System.Threading.Thread(() =>
                {
                    error_text = err.ToString();
                });
                t.CurrentUICulture = new System.Globalization.CultureInfo("en-US");
                t.Start();
                t.Join();
                WriteLog("\n\nERROR (" + DateTime.Now.ToString() + "):\n" + error_text);
                 *
                /*
                ExceptionLogger el = new ExceptionLogger(err);
                System.Threading.Thread t = new System.Threading.Thread(el.DoLog);
                t.CurrentUICulture = new System.Globalization.CultureInfo("en-US");
                t.Start();
                *
            }
            class ExceptionLogger
            {
                Exception _ex;

                public ExceptionLogger(Exception ex)
                {
                    _ex = ex;
                }

                public void DoLog()
                {
                    WriteLog("\n\nERROR (" + DateTime.Now.ToString() + "):\n" + _ex.ToString());
                    //Console.WriteLine(_ex.ToString()); //Will display en-US message
                }
            }*/

            /*private static List<Game.GUI.Controls.Label> screenLabels = new List<Game.GUI.Controls.Label>();
            public static void WriteScreen(params String[] lines)
            {
                for (var i = 0; i < lines.Length; i++)
                {
                    if (screenLabels.Count <= i)
                    {
                        var screenLabel = new Game.GUI.Controls.Label(Game.GnomanEmpire.Instance.GuiManager.Manager);
                        screenLabel.Init();
                        screenLabel.Left = 10;
                        screenLabel.Top = 80 + (i * 25);
                        screenLabel.Width = 800;
                        screenLabel.Height = 25;
                        Game.GnomanEmpire.Instance.GuiManager.Add(screenLabel);
                        screenLabels.Add(screenLabel);
                    }


                    if (lines[i] != null)
                    {
                        screenLabels[i].Text = lines[i];
                    }
                }
            }*/
        }
    }
}
