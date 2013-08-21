using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Xml.Linq;
using System.Runtime.Serialization;
using System.IO;

namespace Faark.Gnomoria.Modding
{
    public class ModSaveData
    {
        /// <summary>
        /// Contains some serialization stuff. That is actually pretty tricky...
        /// </summary>
        private class DataItem
        {
            public string key;
            public object value;
            public bool isInvalid = false;
            /*
            public void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                var hs = new HashSet<Mod>();
                try
                {
                    info.AddValue("Key", key);
                    info.AddValue("Type", value.GetType().FullName);
                    info.AddValue("Value", value);
                }
                catch (SerializationException err)
                {
                    throw err;
                }
                catch (Exception err)
                {
                    throw err;
                }
            }
            public msd_dataItem(SerializationInfo info, StreamingContext context)
            {
                key = info.GetString("Key");
                var type = info.GetString("Type");
                value = info.GetValue("Value", Type.GetType(type));
            }
             */
            public DataItem(string k, object v)
            {
                key = k;
                value = v;
            }
            public DataItem(XElement el)
            {
                try
                {
                    // Todo: handle invalid data properly!!
                    key = el.Attribute("Key").Value;
                    var type = Type.GetType(el.Attribute("Type").Value, true);
                    //if( type.IsPrimitive )
                    // TODO: Complete member initialization#
                    var dcs = new DataContractSerializer(type, null, int.MaxValue, false, false, null, ModSaveFile.GetDataContractResolver());
                    var tmpReader = el.CreateReader();
                    tmpReader.MoveToContent();
                    value = dcs.ReadObject(tmpReader, false);
                }
                catch (Exception)
                {
                    isInvalid = true;
                }
                return;
            }
            public void WriteSaveData(System.Xml.XmlWriter writer)
            {
                //We simply skip null values;
                if (value == null)
                    return;

                writer.WriteStartElement("Data");
                writer.WriteAttributeString("Key", key);
                // Todo: does this create a security hole? Though mods can do everything anyway, this may abuse a mod to do sth harmful. Consider it!
                writer.WriteAttributeString("Type", value.GetType().AssemblyQualifiedName);

                var dcs = new DataContractSerializer(value.GetType(), "", "", null, Int32.MaxValue, false, false, null, ModSaveFile.GetDataContractResolver());
                dcs.WriteObjectContent(writer, value);

                //var dcjs = new System.Runtime.Serialization.Json.DataContractJsonSerializer(value.GetType()

                writer.WriteEndElement();
            }
        }

        Dictionary<String, DataItem> data = new Dictionary<string, DataItem>();
        bool hasData = false;
        private XElement unserializedData = null;
        /*//[DataMember(Name = "Data")]
        private msd_dataItem[] serializableData
        {
            get
            {
                return data.Select(kvp => new msd_dataItem(kvp.Key, kvp.Value)).ToArray();
            }
            set
            {
                data = new Dictionary<string, object>();
                foreach (var d in value)
                {
                    if (d.value != null && (d.key != null))
                    {
                        data.Add(d.key, d.value);
                    }
                }
            }
        }*/

        public bool HasAnyData
        {
            get
            {
                return hasData;
            }
        }
        public bool HasData(string key)
        {
            return data.ContainsKey(key) && !data[key].isInvalid;
        }
        public object this[string key]
        {
            get
            {
                DataItem di;
                if (data.TryGetValue(key, out di))
                {
                    return di.value;
                }
                else
                {
                    throw new ArgumentException("No data found for key [" + key + "]");
                }
            }
            set
            {
                var di = new DataItem(key, value);
                data[di.key] = di;
                hasData = true;
            }
        }
        public T GetData<T>(string key)
        {
            var val = this[key];
            if (val is T)
            {
                return (T)val;
            }
            else
            {
                throw new Exception("Data [" + key + "] is not of type [" + typeof(T).ToString() + "], not [" + ((val == null) ? "NULL" : val.GetType().ToString()) + "]");
            }
        }
        public T GetData<T>(string key, T def)
        {
            if (HasData(key))
            {
                return GetData<T>(key);
            }
            else
            {
                return def;
            }
        }
        public void GetData<T>(string key, Action<T> to, T def = default(T))
        {
            if (HasData(key))
            {
                to(GetData<T>(key));
            }
            else
            {
                to(def);
            }
        }
        public String GetString(string key)
        {
            return GetData<string>(key);
        }
        public String GetString(string key, String def)
        {
            return GetData<string>(key, def);
        }
        public void SetData<T>(string key, T data)
        {
            this[key] = data;
        }
        public void SetString(string key, string value)
        {
            SetData(key, value);
        }
        public void ClearData(string key)
        {
            data.Remove(key);
            hasData = data.Count > 0;
        }

        //[DataMember(Name="ModType")]
        public String ModType { get; private set; }
        public IMod LoadedMod { get; private set; }
        public bool IsModLoaded
        {
            get { return LoadedMod != null; }
        }
        

        /*
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("ModType", ModType);
            info.AddValue("Data", data.Select(kvp => new msd_dataItem() { key = kvp.Key, value = kvp.Value }).ToArray());
            return;
        }
        */
        public ModSaveData(IMod mod)
        {
            ModType = mod.GetType().FullName;
            LoadedMod = mod;
        }
        


        /*
        public ModSaveData(SerializationInfo info, StreamingContext context)
        {
            ModType = (string)info.GetValue("ModType", typeof(string));
            LoadedMod = RuntimeModController.ActiveMods.SingleOrDefault(mod => mod.GetType().FullName == ModType);
            if (IsModLoaded)
            {
                data = new Dictionary<string, object>();
                var datas = (msd_dataItem[])info.GetValue("Data", typeof(msd_dataItem[]));
                foreach (var d in datas)
                {
                    data.Add(d.key, d.value);
                }
            }
        }
        */

        public ModSaveData(XElement md_el)
        {
            ModType = md_el.Attribute("ModType").Value;
            LoadedMod = RuntimeModController.ActiveMods.SingleOrDefault(mod => mod.GetType().FullName == ModType);
            if (IsModLoaded)
            {
                foreach (var el in md_el.Elements("Data"))
                {
                    var di = new DataItem(el);
                    if (!di.isInvalid)
                    {
                        data.Add(di.key, di);
                    }
                }
            }
            else
            {
                unserializedData = new XElement(md_el);
            }
        }
        public void WriteSaveData(System.Xml.XmlWriter writer)
        {
            if (IsModLoaded)
            {
                writer.WriteStartElement("Mod");
                writer.WriteAttributeString("ModType", ModType);
                foreach (var data_el in data)
                {
                    data_el.Value.WriteSaveData(writer);
                }
                writer.WriteEndElement();
            }
            else
            {
                unserializedData.WriteTo(writer);
            }

        }
    }
    /// <summary>
    /// Actually just a collection of ModSaveData. Also takes care of some serialization stuff.
    /// </summary>
    [DataContract]
    class ModSaveFile
    {
        Dictionary<IMod, ModSaveData> modSaveData;
        Dictionary<string, ModSaveData> unloadedSaveData;
        List<ModSaveData> allSaveData;

        [DataMember(Name = "SavedModData")]
        private ModSaveData[] allSavedDataAsArray
        {
            get
            {
                return allSaveData.Where(d => !d.IsModLoaded || d.HasAnyData).ToArray();
            }
            set
            {
                allSaveData = value.ToList();
                modSaveData = new Dictionary<IMod, ModSaveData>();
                unloadedSaveData = new Dictionary<string, ModSaveData>();
                foreach (var el in allSaveData)
                {
                    if (el.IsModLoaded)
                        modSaveData.Add(el.LoadedMod, el);
                    else
                        unloadedSaveData.Add(el.ModType, el);
                }
            }
        }
        public ModSaveData GetDataFor(IMod mod)
        {
            ModSaveData result;
            if (modSaveData.TryGetValue(mod, out result))
            {
                return result;
            }
            var textKey = mod.GetType().FullName;
            if (unloadedSaveData.TryGetValue(textKey, out result))
            {
                throw new Exception("This should never happen!");
                /*
                #warning this should actually never happen anymore, since we look up existing mods while loading anyway.
                unloadedSaveData.Remove(textKey);
                modSaveData.Add(mod, result);
                return result;
                */
            }
            result = new ModSaveData(mod);
            modSaveData.Add(mod, result);
            allSaveData.Add(result);
            return result;
        }

        #region Initialization
        public void Init()
        {
            modSaveData = new Dictionary<IMod, ModSaveData>();
            unloadedSaveData = new Dictionary<string, ModSaveData>();
            allSaveData = new List<ModSaveData>();
        }
        [OnDeserializing]
        public void Init(StreamingContext context)
        {
            Init();
        }
        internal ModSaveFile()
        {
            Init();
        }
        #endregion
        #region Actual serialization calls
        internal class ModDataContractResolver : DataContractResolver
        {
            private string typeToString(Type t)
            {
                return System.Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(t.AssemblyQualifiedName)).Replace("=", "");
            }
            private string stringToAQN(string text)
            {
                while (text.Length % 4 != 0)
                {
                    text = text + "=";
                }
                return System.Text.ASCIIEncoding.ASCII.GetString(System.Convert.FromBase64String(text));
            }
            System.Xml.XmlDictionary dict = new System.Xml.XmlDictionary();
            public override Type ResolveName(string typeName, string typeNamespace, Type declaredType, DataContractResolver knownTypeResolver)
            {
                try
                {
                    if (typeNamespace == (this.GetType().Namespace + ".Base64Encoded"))
                    {
                        var txt = stringToAQN(typeName);
                        var ttype = Type.GetType(txt, false);
                        if (ttype != null)
                            return ttype;
                    }
                }
                catch (Exception)
                {
                }
                return knownTypeResolver.ResolveName(typeName, typeNamespace, declaredType, null);// ?? AppDomain.CurrentDomain.GetAssemblies().SelectMany(ass => ass.GetTypes()).FirstOrDefault(t => t.Namespace == typeNamespace && t.Name == typeName);
                // Todo: does this create a security hole? Though mods can do everything anyway, this may abuse a mod to do sth harmful. Consider it!
                // for now i don't think so, since typeName has to be assignable to it. And object or whatsoever...
                // it still could change the mods function. What about just loading stuff that the mod does reference? (eighter by game + modconfig or assembly?)
                // type.IsAssignableFrom
                /*var matches = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes()).Where(t => t.Name == typeName && t.Namespace == typeNamespace).ToArray();
                matches.Count();
                throw new NotImplementedException();*/
            }
            public override bool TryResolveType(Type type, Type declaredType, DataContractResolver knownTypeResolver, out System.Xml.XmlDictionaryString typeName, out System.Xml.XmlDictionaryString typeNamespace)
            {
                typeName = dict.Add(typeToString(type));
                typeNamespace = dict.Add(this.GetType().Namespace + ".Base64Encoded");
                //typeNamespace = dict.Add(type.Namespace);
                return true;
                //throw new NotImplementedException();
            }
        }
        private static ModDataContractResolver saveFileCtrRes = null;
        internal static ModDataContractResolver GetDataContractResolver()
        {
            if (saveFileCtrRes == null)
            {
                saveFileCtrRes = new ModDataContractResolver();
            }
            return saveFileCtrRes;
        }
        private static DataContractSerializer saveFileDcs = null;
        internal static DataContractSerializer GetDataContractSerializer()
        {
            if (saveFileDcs == null)
            {
                saveFileDcs = new DataContractSerializer(typeof(ModSaveFile), null, Int32.MaxValue, false, false, null, GetDataContractResolver());
            }
            return saveFileDcs;
        }
        public static ModSaveFile LoadFrom(FileInfo fileName)
        {
            if (!fileName.Exists)
            {
                RuntimeModController.Log.Write("Mod save exists for this world, creating a new one", fileName.Name);
                return new ModSaveFile();
            }

            var doc = XDocument.Load(fileName.FullName);
            var msf = new ModSaveFile();
            msf.allSavedDataAsArray = doc.Element("ModSaveFile").Elements("Mod").Select(md => new ModSaveData(md)).ToArray();
            return msf;
            /*
            var dcserializer = GetDataContractSerializer();
            //var serializer = new System.Xml.Serialization.XmlSerializer(typeof(ModEnvironmentConfiguration));
            using (var fstream = fileName.OpenRead())
            {
                var msf = (ModSaveFile)dcserializer.ReadObject(fstream);
                return msf;
            }
            */
        }
        public void SaveTo(FileInfo filename)
        {
            //var dcs = GetDataContractSerializer();
            var tmpFile = new FileInfo (filename.FullName + ".temp");
            using (var wstream = tmpFile.Open(FileMode.Create))
            {
                using (var writer = System.Xml.XmlWriter.Create(wstream, new System.Xml.XmlWriterSettings() { Indent = true }))
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("ModSaveFile");
                    writer.WriteAttributeString("Version", "1"); 
                    foreach (var md in modSaveData)
                    {
                        if (md.Value.HasAnyData || !md.Value.IsModLoaded)
                        {
                            md.Value.WriteSaveData(writer);
                        }
                    }
                    //writer.WriteComment("DO NOT CHANGE ANYTHING IN THIS FILE WITHOUT DELETING GnomoriaModded.exe!");//\nThis will make the EXE be recreated, since otherwise the game will crash.
                    //serializer.Serialize(writer, self);
                    //dcs.WriteObject(writer, this);
                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                    writer.Flush();
                    writer.Close();
                }
            }
            /*using (var s = new StreamWriter(filename.FullName + ".temp"))
            {
                dcs.WriteObject(s.BaseStream, this);
            }*/
            if (filename.Exists)
            {
                filename.Delete();
            }
            System.IO.File.Move(filename.FullName + ".temp", filename.FullName);
        }
        #endregion
    }
}
