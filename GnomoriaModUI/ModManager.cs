using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GnomoriaModUI
{
    class ModManager
    {
        public const string config_file_name = "GnomoriaModConfig.xml";
        public const string OriginalExecutable = "Gnomoria.exe";
        public const string ModdedExecutable = "GnomoriaModded.dll";
        public const string OriginalLibrary = "gnomorialib.dll";
        public const string ModdedLibrary = "gnomorialibModded.dll";
        public const string ModController = "GnomoriaModController.dll";
        public static readonly string[] Dependencies = new string[] { /*"GnomoriaModController.dll", */"Gnomoria.exe", "gnomorialib.dll", "SevenZipSharp.dll" };
        public static System.IO.DirectoryInfo GameDirectory = new System.IO.DirectoryInfo(System.IO.Directory.GetCurrentDirectory());
    }
}
