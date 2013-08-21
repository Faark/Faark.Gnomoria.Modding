
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
    /// <summary>
    /// Serves as some kind of "mod registery". Since we have to make sure that every mod is instanciated only once,
    /// it will take care of creating them via Activator.
    /// 
    /// Mods can also 
    /// </summary>
    public static class ModEnvironment
    {
        public enum EnvironmentStatus { Unspecified, /*ModSelection,*/ InGame }
        public static EnvironmentStatus Status { get; internal set; }

        public class ModCollection : IEnumerable<IMod>
        {

            private Dictionary<Type, IMod> loadedMods = new Dictionary<Type, IMod>();

            public IMod Get(ModType type)
            {
                var sysType = type.Type;
                IMod mod;
                if (loadedMods.TryGetValue(sysType, out mod))
                {
                    return mod;
                }
                mod = loadedMods[sysType] = (IMod)Activator.CreateInstance(sysType);
                return mod;
            }
            public T1 Get<T1>() where T1: IMod, new()
            {
                IMod mod;
                if (loadedMods.TryGetValue(typeof(T1), out mod))
                {
                    return (T1)mod;
                }
                var newMod = new T1();
                loadedMods[typeof(T1)] = newMod;
                return newMod;
            }
            public bool Has(ModType type)
            {
                return loadedMods.ContainsKey(type.Type);
            }

            public IMod this[ModType type]
            {
                get
                {
                    return Get(type);
                }
            }
            public IMod this[Type type]
            {
                get
                {
                    return Get(new ModType(type));
                }
            }

            public IEnumerator<IMod> GetEnumerator()
            {
                foreach (var el in loadedMods)
                {
                    yield return el.Value;
                }
            }
            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }
        }

        private static ModCollection mMods = new ModCollection();
        public static ModCollection Mods
        {
            get
            {
                return mMods;
            }
        }

        public static event EventHandler ResetSetupData;
        public static void RequestSetupDataReset()
        {
            ResetSetupData.TryRaise(null);
        }

    }
}
