
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
    [DataContract]
    public class ModType
    {

        [DataMember(Name = "ModType")]
        private String m_modTypeName;
        private Type m_modType;


        public Type TryGetType()
        {
            if (m_modType != null)
                return m_modType;
            if (m_modTypeName != null)
            {
                return m_modType = Type.GetType(
                    m_modTypeName,
                    an => AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(la => la.FullName == an.FullName),
                    (a, tn, casesense) => a.GetType(tn, false, true),
                    false
                    );
            }
            return null;
        }

        public Type Type
        {
            get
            {
                if (m_modType != null)
                    return m_modType;
                if (m_modTypeName != null)
                {
                    return m_modType = Type.GetType(
                        m_modTypeName,
                        an => AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(la => la.FullName == an.FullName),
                        (a, tn, casesense) => a.GetType(tn, false, true),
                        true
                        );
                }
                return null;
            }
        }
        public String TypeName
        {
            get
            {
                var tryType = TryGetType();
                return tryType != null ? tryType.AssemblyQualifiedName : m_modTypeName;
            }
        }

        public IMod GetInstance()
        {
            return ModEnvironment.Mods[this];
        }

        public ModType(IMod mod)
        {
            m_modType = mod.GetType();
            m_modTypeName = TypeName;
        }
        public ModType(string typeName)
        {
            m_modTypeName = typeName;
        }
        public ModType(Type sysType)
        {
            m_modType = sysType;
            m_modTypeName = TypeName;
        }
        protected ModType() { }

        public static implicit operator ModType(Mod toCreateFrom)
        {
            return new ModType(toCreateFrom);
        }
    }

    [DataContract]
    public class ModReference : ModType
    {
        [DataMember(Name = "Hash")]
        private String m_dllHash;
        [DataMember(Name = "AssemblyFileName")]
        private String m_assemblyFileName;
        [DataMember(Name = "SetupData", EmitDefaultValue = false)]
        private String m_setupData = null;
        public String Hash
        {
            get { return m_dllHash; }
        }
        public String AssemblyFileName
        {
            get { return m_assemblyFileName; }
        }
        public System.IO.FileInfo AssemblyFile
        {
            get
            {
                return new System.IO.FileInfo(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), m_assemblyFileName));
            }
        }
        public String SetupData
        {
            get
            {
                return m_setupData;
            }
        }

        protected ModReference() { }
        public ModReference(IMod mod)
            : base(mod)
        {
            m_setupData = mod.SetupData;
            m_dllHash = new System.IO.FileInfo(new System.Uri(Type.Assembly.CodeBase).LocalPath).GenerateMD5Hash();
            string refPath;
            if (Faark.Util.FileExtensions.GetRelativePath(System.IO.Directory.GetCurrentDirectory(), Type.Assembly.CodeBase, out refPath))
            {
                m_assemblyFileName = refPath;
            }
            else
            {
                m_assemblyFileName = Type.Assembly.CodeBase;
            }
        }

        
        public static implicit operator ModReference(Mod toCreateFrom)
        {
            return new ModReference(toCreateFrom);
        }
        
    }
}
