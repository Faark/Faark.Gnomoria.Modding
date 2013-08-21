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
    public interface IModification
    {
        Type TargetType { get; }
    }
    public interface IModificationCollection : IModification, IEnumerable<IModification>
    {
        IEnumerator<IModification> GetModifications();
    }
    public abstract class ModificationCollection : IModificationCollection
    {
        public abstract IEnumerator<IModification> GetModifications();
        public abstract Type TargetType { get; }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetModifications();
        }
        IEnumerator<IModification> IEnumerable<IModification>.GetEnumerator()
        {
            return GetModifications();
        }
    }

    [Obsolete("Use IModification and Mod.Modifications instead of Mod.Hooks. Everything else should stay the same.")]
    public interface IMethodModification : IModification
    {
    }


    public enum MethodHookType { RunBefore, RunAfter, Replace }
    [Flags]
    public enum MethodHookFlags
    {
        None = 0x0,
        CanSkipOriginal = 0x1
    }

    public class CustomParameterInfo
    {
        public Type ParameterType { get; private set; }
        public bool IsOut { get; private set; }
        public CustomParameterInfo(Type type, bool is_out = false)
        {
            ParameterType = (is_out && !type.IsByRef) ? type.MakeByRefType() : type;
            IsOut = is_out;
        }

        public bool IsSimpleType(Type t)
        {
            return !IsOut && (ParameterType == t);
        }

        public static implicit operator CustomParameterInfo(Type t)
        {
            return new CustomParameterInfo(t);
        }
        public static implicit operator CustomParameterInfo(ParameterInfo pi)
        {
            return new CustomParameterInfo(pi.ParameterType, pi.IsOut);
        }

        public static bool IsSimilar(CustomParameterInfo a, CustomParameterInfo b)
        {
            // TODO: generics are missing
            if (a == null || b == null)
                return false;
            if (a == b)
                return true;
            if (a.IsOut != b.IsOut)
                return false;
            if (a.ParameterType.IsByRef)
            {
                if (!b.ParameterType.IsByRef)
                    return false;
                return a.ParameterType.GetElementType() == b.ParameterType.GetElementType();
            }
            else
            {
                return a.ParameterType == b.ParameterType;
            }
        }
        public bool IsSimilar(CustomParameterInfo trg)
        {
            return IsSimilar(this, trg);
        }
    }

    public abstract class UnmutableMethodModification : IMethodModification
    {
        public static bool IsCompilerGenerated(MethodInfo method)
        {
            return method.GetCustomAttributes(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), false).Length > 0;
        }
        public static void VerifyNestedPublicMethod(MethodInfo method)
        {
            var cur_type = method.DeclaringType;
            while (cur_type != null)
            {
                if (!cur_type.IsPublic && !(cur_type.IsNested && cur_type.IsNestedPublic))
                {
                    throw new ArgumentException("One of the Types (" + cur_type.FullName + ") declaring the custom method (" + method.Name + ") is not public, makeing this method not accessible!");
                }
                cur_type = cur_type.DeclaringType;
            }
        }
        private static string MethodReferenceToString(MethodBase mref)
        {
            return mref.Name + "; Type " + mref.DeclaringType.FullName;
        }

        public MethodHookType HookType { get; private set; }
        public MethodBase InterceptedMethod { get; private set; }
        public MethodInfo CustomMethod{get; private set;}
        public MethodHookFlags HookFlags{get; private set;}

        Type IModification.TargetType
        {
            get
            {
                return InterceptedMethod.DeclaringType;
            }
        }

        protected virtual void Validate_1_NoInformationMissing()
        {
            if ((InterceptedMethod == null) || (CustomMethod == null))
            {

                if (InterceptedMethod == CustomMethod)
                {
                    throw new ArgumentException("Custom and intercepted methods are null. Can't create a hook without them!");
                }
                else if (InterceptedMethod == null)
                {
                    throw new ArgumentException("Intercepted method is null. Can't create a hook without! Custom method is [" + MethodReferenceToString(CustomMethod) + "]");
                }
                else
                {
                    throw new ArgumentException("Custom method is null. Can't create a hook without! Intercepted method is [" + MethodReferenceToString(InterceptedMethod) + "]");
                }
            }
        }
        protected virtual void Validate_2_Accessibility()
        {
            if (!(CustomMethod.IsStatic && CustomMethod.IsPublic))
                throw new ArgumentException("Custom method has to be public & static! " + CustomMethod.Name + " is not.");
            if (UnmutableMethodModification.IsCompilerGenerated(CustomMethod))
                throw new ArgumentException("Custom method can not be compiler generated");

            VerifyNestedPublicMethod(CustomMethod);
        }
        protected virtual void Validate_3_SpecialCases()
        {
            if (InterceptedMethod.IsConstructor && (HookType != MethodHookType.RunAfter))
                throw new ArgumentException("Intercepted Method appears to be a constructor for a type (" + InterceptedMethod.DeclaringType.FullName + "). Only MethodHookType.RunAfter is currently supported for constructors.");
            if (HookFlags.HasFlag(MethodHookFlags.CanSkipOriginal))
            {
                if (HookType != MethodHookType.RunBefore)
                {
                    throw new ArgumentException("MethodHookFlags.CanSkipOriginal requires MethodHookType.RunBefore!");
                }
            }
        }
        protected virtual void Validate_4_ParameterLayout()
        {
            var requredParameterLayout = GetRequiredParameterLayout().ToList();
            var foundParameterLayout = CustomMethod.GetParameters();
            var valid = requredParameterLayout.Count == foundParameterLayout.Length;
            for (var i = 0; (i < foundParameterLayout.Length) && valid; i++)
            {
                if (requredParameterLayout[i].ParameterType.IsByRef)
                {
                    if (foundParameterLayout[i].ParameterType.GetElementType() != requredParameterLayout[i].ParameterType.GetElementType())
                    {
                        valid = false;
                    }
                    if ((HookType != MethodHookType.RunAfter) && (foundParameterLayout[i].IsOut != requredParameterLayout[i].IsOut))
                    {
                        valid = false;
                    }
                }
                else if (foundParameterLayout[i].ParameterType != requredParameterLayout[i].ParameterType)
                {
                    if (!foundParameterLayout[i].ParameterType.IsValueType && !requredParameterLayout[i].ParameterType.IsValueType && requredParameterLayout[i].ParameterType.IsSubclassOf(foundParameterLayout[i].ParameterType))
                    {
                        //should be simple ptr stuff & fine to "convert". We can allow this
                    }
                    else
                    {
                        valid = false;
                    }
                }
            }
            if (!valid)
            {
                Func<IEnumerable<CustomParameterInfo>, String> toString = (f) =>
                {
                    return f.Select(t => t == null ? "NULL" : ((t.IsOut ? "out " : (t.ParameterType.IsByRef ? "ref " : "")) + (t.ParameterType.IsByRef ? t.ParameterType.GetElementType() : t.ParameterType).FullName)).Aggregate((f1, f2) => f1 + ", " + f2);
                };
                throw new ArgumentException("Invalid parameter Layout for method [" + MethodReferenceToString(CustomMethod) + "]! Expected [" + toString(requredParameterLayout) + "], got [" + toString(foundParameterLayout.Select(el => (CustomParameterInfo)el)) + "].");
            }
        }
        private void Validate_5_ReturnType_WrongReturnType(Type got, Type excpected, MethodBase func)
        {
            throw new ArgumentException("Invalid return type! Got [" + got.FullName + "] while expecting [" + excpected.FullName + "] at func [" + MethodReferenceToString(func) + "]");
        }
        protected virtual void Validate_5_ReturnType()
        {

            //Return value checking
            if (HookFlags.HasFlag(MethodHookFlags.CanSkipOriginal))
            {
                if (CustomMethod.ReturnType != typeof(bool))
                {
                    throw new ArgumentException("Methods that can skip the original method have to return a bool, indicating the skipping state");
                }
            }
            else if ((HasResultAsFirstParameter || (HookType == MethodHookType.Replace)) && (InterceptedMethod is MethodInfo))
            {
                var expected_type = (InterceptedMethod as MethodInfo).ReturnType;
                if (expected_type != CustomMethod.ReturnType)
                {
                    Validate_5_ReturnType_WrongReturnType(CustomMethod.ReturnType, expected_type, CustomMethod);
                }
            }
            else if (CustomMethod.ReturnType != typeof(void))
            {
                Validate_5_ReturnType_WrongReturnType(CustomMethod.ReturnType, typeof(void), CustomMethod);
            }
        }

        public UnmutableMethodModification(MethodBase intercepted, MethodInfo custom_method, MethodHookType hook_type = MethodHookType.RunAfter, MethodHookFlags hook_flags = MethodHookFlags.None)
        {
            HookType = hook_type;
            InterceptedMethod = intercepted;
            CustomMethod = custom_method;
            HookFlags = hook_flags;

            Validate_1_NoInformationMissing();
            Validate_2_Accessibility();
            Validate_3_SpecialCases();
            Validate_4_ParameterLayout();
            Validate_5_ReturnType();
        }

        public virtual bool HasResultAsFirstParameter
        {
            get
            {
                var intmethAsMethInfo = InterceptedMethod as MethodInfo;
                return (intmethAsMethInfo != null) && (HookType == MethodHookType.RunAfter) && (intmethAsMethInfo.ReturnType != typeof(void));
            }
        }
        public virtual bool RequiresLocalToCacheOutResult
        {
            get
            {
                return HookFlags.HasFlag(MethodHookFlags.CanSkipOriginal) && (InterceptedMethod is MethodInfo) && ((InterceptedMethod as MethodInfo).ReturnType != typeof(void));
            }
        }
        public virtual IEnumerable<CustomParameterInfo> GetRequiredParameterLayout()
        {
            if (HasResultAsFirstParameter)
            {
                yield return (InterceptedMethod as MethodInfo).ReturnType;
            }
            if (!InterceptedMethod.IsStatic)
            {
                yield return InterceptedMethod.DeclaringType;
            }
            foreach (var param in InterceptedMethod.GetParameters())
            {
                yield return param;
            }
            if (RequiresLocalToCacheOutResult)
            {
                yield return new CustomParameterInfo((InterceptedMethod as MethodInfo).ReturnType, true);
            }
        }
    }


}
