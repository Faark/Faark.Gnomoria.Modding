using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Faark.Util
{
    public static class DirectoryExtensions
    {
        public static System.IO.FileInfo ContainingFile(this System.IO.DirectoryInfo self, string file_name)
        {
            return new System.IO.FileInfo(System.IO.Path.Combine(self.FullName, file_name));
        }
        public static System.IO.DirectoryInfo ContainingDirectory(this System.IO.DirectoryInfo self, string file_name)
        {
            return new System.IO.DirectoryInfo(System.IO.Path.Combine(self.FullName, file_name));
        }
    }
    public static class FileExtensions
    {
        public static string GenerateMD5Hash(this System.IO.FileInfo file)
        {
            if (file == null || !file.Exists)
                return null;
            var md5gen = System.Security.Cryptography.MD5.Create();
            using (var stream = new System.IO.BufferedStream(file.OpenRead(), 1200000))
            {
                return BitConverter.ToString(md5gen.ComputeHash(stream));
            }
        }
        public static bool GetRelativePath(String base_path, string path_to_remove_base, out string relative_path)
        {
            base_path = System.IO.Path.GetFullPath(base_path + "\\");
            relative_path = Uri.UnescapeDataString(
                new Uri(base_path)
                    .MakeRelativeUri(new Uri(path_to_remove_base))
                    .ToString()
                    .Replace('/', System.IO.Path.DirectorySeparatorChar)
                );
            return !relative_path.StartsWith("..") && !relative_path.StartsWith(base_path);
            //!(relative_path.Contains('/') || relative_path.Contains('\\'));
        }
    }
    public static class EnumerableExtensions
    {
        public static string Join(this IEnumerable<string> source, string seperator)
        {
            return String.Join(seperator, source);
        }

        public static T ElementAfterOrDefault<T>(this IEnumerable<T> source, T elementBeforeSearchResult)
        {
            var comp = EqualityComparer<T>.Default;
            bool foundIt = false;
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            foreach (var el in source)
            {
                if (foundIt)
                {
                    return el;
                }
                else if (comp.Equals(el, elementBeforeSearchResult))
                {
                    foundIt = true;
                }
            }
            return default(T);
        }
        public static T ElementBeforeOrDefault<T>(this IEnumerable<T> source, T elementAfterSearchResult)
        {
            var comp = EqualityComparer<T>.Default;
            T last = default(T);
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            foreach (var el in source)
            {
                if (comp.Equals(el, elementAfterSearchResult))
                {
                    return last;
                }
                else
                {
                    last = el;
                }
            }
            return default(T);
        }
        public static T ElementAfterOrDefault<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            bool foundIt = false;
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            foreach (var el in source)
            {
                if (foundIt)
                {
                    return el;
                }
                else if (predicate(el))
                {
                    foundIt = true;
                }
            }
            return default(T);
        }
        public static T ElementBeforeOrDefault<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            T last = default(T);
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            foreach (var el in source)
            {
                if (predicate(el))
                {
                    return last;
                }
                else
                {
                    last = el;
                }
            }
            return default(T);
        }
        public static T ElementAfter<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            bool foundIt = false;
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            foreach (var el in source)
            {
                if (foundIt)
                {
                    return el;
                }
                else if (predicate(el))
                {
                    foundIt = true;
                }
            }
            if (foundIt)
            {
                throw new InvalidOperationException("Found element is the last item of the collection");
            }
            else
            {
                throw new InvalidOperationException("No elements in collection match the condition");
            }
        }
        public static T ElementAfter<T>(this IEnumerable<T> source, T elementBeforeSearchResult)
        {
            var comp = EqualityComparer<T>.Default;
            bool foundIt = false;
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            foreach (var el in source)
            {
                if (foundIt)
                {
                    return el;
                }
                else if (comp.Equals(el, elementBeforeSearchResult))
                {
                    foundIt = true;
                }
            }
            if (foundIt)
            {
                throw new InvalidOperationException("Found element is the last item of the collection");
            }
            else
            {
                throw new InvalidOperationException("No elements in collection match the condition");
            }
        }
        public static int IndexOf<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            if (source == null) throw new ArgumentNullException("items");
            if (predicate == null) throw new ArgumentNullException("predicate");

            int retVal = 0;
            foreach (var item in source)
            {
                if (predicate(item)) return retVal;
                retVal++;
            }
            return -1;
        }
        public static int IndexOf<T>(this IEnumerable<T> source, T element, IEqualityComparer<T> comparer)
        {
            return IndexOf(source, el => comparer.Equals(el, element));
        }
        public static int IndexOf<T>(this IEnumerable<T> source, T element)
        {
            return IndexOf(source, element, EqualityComparer<T>.Default);
        }
        public static IEnumerable<T> Union<T>(this IEnumerable<T> source, T element)
        {
            return source.Union(new T[] { element });
        }
        public static IEnumerable<T> Concat<T>(this IEnumerable<T> source, T element)
        {
            return source.Concat(new T[] { element });
        }
        public static bool SequenceEqual<T>(this IEnumerable<T> first, IEnumerable<T> second, Func<T, T, bool> equals_comparer)
        {
            if (equals_comparer == null)
            {
                throw new ArgumentNullException("comparer");
            }
            if (first == null)
            {
                throw new ArgumentNullException("first");
            }
            if (second == null)
            {
                throw new ArgumentNullException("second");
            }
            using (var enumerator = first.GetEnumerator())
            {
                using (var enumerator2 = second.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        if (!enumerator2.MoveNext() || !equals_comparer(enumerator.Current, enumerator2.Current))
                        {
                            bool result = false;
                            return result;
                        }
                    }
                    if (enumerator2.MoveNext())
                    {
                        bool result = false;
                        return result;
                    }
                }
            }
            return true;
        }

        //http://stackoverflow.com/questions/1577822/passing-a-single-item-as-ienumerablet
        /// <summary>
        /// Wraps this object instance into an IEnumerable&lt;T&gt;
        /// consisting of a single item.
        /// </summary>
        /// <typeparam name="T"> Type of the wrapped object.</typeparam>
        /// <param name="item"> The object to wrap.</param>
        /// <returns>
        /// An IEnumerable&lt;T&gt; consisting of a single item.
        /// </returns>
        public static IEnumerable<T> Yield<T>(this T item)
        {
            yield return item;
        }
    }
    public static class EventHandlerExtensions
    {
        public static void TryRaise<T, T2, T3>(this EventHandler<EventArgs<T, T2, T3>> handler, object self, T arg1, T2 arg2, T3 arg3)
        {
            if (handler != null)
            {
                handler.Invoke(self, new EventArgs<T, T2, T3>(arg1, arg2, arg3));
            }
        }
        public static void TryRaise<T, T2>(this EventHandler<EventArgs<T, T2>> handler, object self, T arg1, T2 arg2)
        {
            if (handler != null)
            {
                handler.Invoke(self, new EventArgs<T, T2>(arg1, arg2));
            }
        }
        public static void TryRaise<T>(this EventHandler<EventArgs<T>> handler, object self, T args)
        {
            if (handler != null)
            {
                handler.Invoke(self, new EventArgs<T>(args));
            }
        }
        public static void TryRaise(this EventHandler handler, object self)
        {
            if (handler != null)
            {
                handler.Invoke(self, new EventArgs());
            }
        }
        public static void TryRaise<T>(this EventHandler<T> handler, object self, T args) where T : EventArgs
        {
            if (handler != null)
            {
                handler.Invoke(self, args);
            }
        }
    }
    public class EventArgs<T> : EventArgs
    {
        public T Argument;
        public EventArgs(T arg)
            : base()
        {
            Argument = arg;
        }
    }
    public class EventArgs<T, T2> : EventArgs<T>
    {
        public T2 Argument2;
        public EventArgs(T arg, T2 arg2)
            : base(arg)
        {
            Argument2 = arg2;
        }
    }
    public class EventArgs<T, T2, T3> : EventArgs<T, T2>
    {
        public T3 Argument3;
        public EventArgs(T arg, T2 arg2, T3 arg3)
            : base(arg, arg2)
        {
            Argument3 = arg3;
        }
    }


    namespace Neoforce
    {
        public static class Helpers
        {
            public static String BreakStringAccoringToLineLength(String input, float maxLineLength, Microsoft.Xna.Framework.Graphics.SpriteFont font)
            {
                var words = input.Split(' ');
                var lines = new List<string>();
                var spaceLen = font.MeasureString(" ").X;
                var currentLineLength = 0f;
                StringBuilder currentLine = new StringBuilder();
                foreach (var word in words)
                {
                    var wordLen = font.MeasureString(word).X;
                    if (currentLineLength > 0)
                    {
                        if ((currentLineLength + spaceLen + wordLen) > maxLineLength)
                        {
                            lines.Add(currentLine.ToString());
                            currentLine.Clear();
                            currentLine.Append(word);
                            currentLineLength = wordLen;
                        }
                        else
                        {
                            currentLine.Append(' ').Append(word);
                            currentLineLength += wordLen + spaceLen;
                        }
                    }
                    else
                    {
                        currentLine.Append(word);
                        currentLineLength += wordLen;
                    }
                }
                lines.Add(currentLine.ToString());
                return String.Join("\n", lines);
            }
            private static String AddSpacesAccordingToLength(string text, float spaceLength, float remainingLength)
            {
                var cntFloat = (int)Math.Floor(remainingLength / spaceLength);
                if( cntFloat <= 0 )
                    return text;
                var both = cntFloat >> 1;
                return new String(' ', both + (cntFloat % 2)) + text + new String(' ', both);
            }
            public static String BreakAndCenterStringAccoringToLineLength(String input, float maxLineLength, Microsoft.Xna.Framework.Graphics.SpriteFont font)
            {
                var words = input.Split(' ');
                var lines = new List<String>();
                var spaceLen = font.MeasureString(" ").X;
                var currentLineLength = 0f;
                StringBuilder currentLine = new StringBuilder();
                foreach (var word in words)
                {
                    var wordLen = font.MeasureString(word).X;
                    if (currentLineLength > 0)
                    {
                        if ((currentLineLength + spaceLen + wordLen) > maxLineLength)
                        {
                            lines.Add(AddSpacesAccordingToLength(currentLine.ToString(), spaceLen, maxLineLength - currentLineLength));
                            currentLine.Clear();
                            currentLine.Append(word);
                            currentLineLength = wordLen;
                        }
                        else
                        {
                            currentLine.Append(' ').Append(word);
                            currentLineLength += wordLen + spaceLen;
                        }
                    }
                    else
                    {
                        currentLine.Append(word);
                        currentLineLength += wordLen;
                    }
                }
                lines.Add(AddSpacesAccordingToLength(currentLine.ToString(), spaceLen, maxLineLength - currentLineLength));
                return String.Join("\n", lines);
            }
        }
    }
    namespace Serialization
    {
        public static class JSON
        {
            /*
            private static Dictionary<Type, System.Runtime.Serialization.Json.DataContractJsonSerializer> Serializers = new Dictionary<Type, System.Runtime.Serialization.Json.DataContractJsonSerializer>();
            private static System.Runtime.Serialization.Json.DataContractJsonSerializer GetSerializer<T>(params Type knownTypes)
            {
                var type = typeof(T);
                System.Runtime.Serialization.Json.DataContractJsonSerializer seri;
                if (Serializers.TryGetValue(type, out seri))
                {
                    return seri;
                }
                return Serializers[type] = new System.Runtime.Serialization.Json.DataContractJsonSerializer(type);
            }
            */

            public static void ToJSON<T>(T object_to_serialize, System.IO.Stream target_stream, params Type[] knownTypes)
            {
                var seri = new System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof(T), knownTypes);
                seri.WriteObject(target_stream, object_to_serialize);
            }
            public static string ToJSON<T>(T object_to_serialize, params Type[] knownTypes)
            {
                using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
                {
                    ToJSON(object_to_serialize, stream, knownTypes);
                    return Encoding.Default.GetString(stream.ToArray());
                }
            }
            public static T FromJSON<T>(System.IO.Stream source_stream, params Type[] knownTypes)
            {
                var seri = new System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof(T), knownTypes);
                return (T)seri.ReadObject(source_stream);
            }
            public static T FromJSON<T>(string json_text, params Type[] knownTypes)
            {
                return FromJSON<T>(new System.IO.MemoryStream(Encoding.UTF8.GetBytes(json_text)), knownTypes);
            }
        }
        [Serializable]
        public class SerializableDataBag<T> : System.Runtime.Serialization.ISerializable, IEnumerable<KeyValuePair<String, T>>
        {
            private Dictionary<string, T> data = new Dictionary<String, T>();

            public SerializableDataBag(IEnumerable<KeyValuePair<String, T>> initial_data)
            {
                if (initial_data == null)
                    throw new ArgumentNullException("initial_data");
                foreach (var el in initial_data)
                {
                    if (el.Key == null)
                    {
                        throw new ArgumentException("Cannot have an empty key!");
                    }
                    data.Add(el.Key, el.Value);
                }
            }
            public SerializableDataBag(IEnumerable<Tuple<String, T>> initial_data)
            {
                if (initial_data == null)
                    throw new ArgumentNullException("initial_data");
                foreach (var el in initial_data)
                {
                    if (el.Item1 == null)
                    {
                        throw new ArgumentException("Cannot have an empty key!");
                    }
                    data.Add(el.Item1, el.Item2);
                }
            }
            public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                foreach (KeyValuePair<string, T> kvp in data)
                {
                    info.AddValue(kvp.Key, kvp.Value);
                }
            }
            public IEnumerator<KeyValuePair<string, T>> GetEnumerator()
            {
                return data.GetEnumerator();
            }
            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
            protected SerializableDataBag(SerializationInfo info, StreamingContext context)
            {
                // TODO: validate inputs before deserializing. See http://msdn.microsoft.com/en-us/library/ty01x675(VS.80).aspx
                foreach (SerializationEntry entry in info)
                {
                    data.Add(entry.Name, (T)entry.Value);
                }
            }
            public string ToJSON()
            {
                return JSON.ToJSON(this, typeof(T));
            }
            public static SerializableDataBag<T> FromJSON(String data)
            {
                return JSON.FromJSON<SerializableDataBag<T>>(data, typeof(T));
            }
            public Dictionary<string, T> ToDictionary()
            {
                return new Dictionary<string,T>(data);
            }
            public Dictionary<string, TOut> ToDictionary<TOut>(Func<T, TOut> converter)
            {
                var dict = new Dictionary<string, TOut>();
                foreach (var el in data)
                {
                    dict.Add(el.Key, converter(el.Value));
                }
                return dict;
            }
            public Dictionary<string, TOut> ToDictionary<TOut>(Func<string, T, TOut> converter)
            {
                var dict = new Dictionary<string, TOut>();
                foreach (var el in data)
                {
                    dict.Add(el.Key, converter(el.Key, el.Value));
                }
                return dict;
            }
        }
        public static class SerializableDataBag
        {
            public static String ToJSON<T>(IEnumerable<Tuple<String, T>> data)
            {
                return data.ToBag().ToJSON();
            }
            public static String ToJSON<T>(IEnumerable<KeyValuePair<String, T>> data)
            {
                return data.ToBag().ToJSON();
            }
            public static SerializableDataBag<T> ToBag<T>(this IEnumerable<Tuple<String, T>> data)
            {
                return new SerializableDataBag<T>(data);
            }
            public static SerializableDataBag<T> ToBag<T>(this IEnumerable<KeyValuePair<String, T>> data)
            {
                return new SerializableDataBag<T>(data);
            }
            public static String ToBagJSON<T>(this IEnumerable<Tuple<String, T>> data)
            {
                return ToJSON(data);
            }
            public static String ToBagJSON<T>(this IEnumerable<KeyValuePair<String, T>> data)
            {
                return ToJSON(data);
            }
            public static SerializableDataBag<T> FromJSON<T>(String data)
            {
                return SerializableDataBag<T>.FromJSON(data);
            }
        }
    }


    public static class DependencySort
    {

        private class DependencySortElement<T>
        {
            public T Element;
            public HashSet<T> Dependencies;
            public DependencySortElement(Tuple<T, IEnumerable<T>> data)
            {
                Element = data.Item1;
                Dependencies = new HashSet<T>();
                foreach (var el in data.Item2)
                {
                    Dependencies.Add(el);
                }
            }
        }

        public static List<T> Sort<T>(IEnumerable<Tuple<T, IEnumerable<T>>> data)
        {
            var openNodes = new HashSet<DependencySortElement<T>>();
            var noDependencyNodes = new Queue<DependencySortElement<T>>();
            var processedNodes = new List<T>();
            foreach (var el in data)
            {
                var newEl = new DependencySortElement<T>(el);
                if (newEl.Dependencies.Count <= 0)
                {
                    noDependencyNodes.Enqueue(newEl);
                }
                else
                {
                    openNodes.Add(newEl);
                }
            }
            while (noDependencyNodes.Count > 0)
            {
                var node = noDependencyNodes.Dequeue();
                openNodes.RemoveWhere(el =>
                {
                    if (el.Dependencies.Contains(node.Element))
                    {
                        el.Dependencies.Remove(node.Element);
                        if (el.Dependencies.Count <= 0)
                        {
                            noDependencyNodes.Enqueue(el);
                            return true;
                        }
                    }
                    return false;
                });
                processedNodes.Add(node.Element);
            }
            if (openNodes.Count > 0)
            {
                throw new InvalidOperationException("Cannot sort by dependency (graph has at least one cycle or unlisted nodes)");
            }
            return processedNodes;
            //throw new NotImplementedException();
        }
    }

    // We are using pre4.5, so we have to user our own...
    // http://www.codeproject.com/Articles/22363/Generic-WeakReference & http://ondevelopment.blogspot.de/2008/01/generic-weak-reference.html
    /// <summary>
    /// Represents a weak reference, which references an object while still allowing
    /// that object to be reclaimed by garbage collection.
    /// </summary>
    /// <typeparam name="T">The type of the object that is referenced.</typeparam>
    [Serializable]
    public class WeakReference<T>
        : WeakReference where T : class
    {
        /// <summary>
        /// Initializes a new instance of the WeakReference{T} class, referencing
        /// the specified object.
        /// </summary>
        /// <param name="target">The object to reference.</param>
        public WeakReference(T target)
            : base(target)
        { }
        /// <summary>
        /// Initializes a new instance of the WeakReference{T} class, referencing
        /// the specified object and using the specified resurrection tracking.
        /// </summary>
        /// <param name="target">An object to track.</param>
        /// <param name="trackResurrection">Indicates when to stop tracking the object. 
        /// If true, the object is tracked
        /// after finalization; if false, the object is only tracked 
        /// until finalization.</param>
        public WeakReference(T target, bool trackResurrection)
            : base(target, trackResurrection)
        { }
        protected WeakReference(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
        /// <summary>
        /// Gets or sets the object (the target) referenced by the 
        /// current WeakReference{T} object.
        /// </summary>
        public new T Target
        {
            get
            {
                return (T)base.Target;
            }
            set
            {
                base.Target = value;
            }
        }

        /// <summary>
        /// Casts an object of the type T to a weak reference
        /// of T.
        /// </summary>
        public static implicit operator WeakReference<T>(T target)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }
            return new WeakReference<T>(target);
        }
        /// <summary>
        /// Casts a weak reference to an object of the type the
        /// reference represents.
        /// </summary>
        public static implicit operator T(WeakReference<T> reference)
        {
            if (reference != null)
            {
                return reference.Target;
            }
            else
            {
                return null;
            }
        } 
    }   
}
