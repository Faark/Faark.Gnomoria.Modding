
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
    public class MethodRefHook : UnmutableMethodModification
    {
        public MethodRefHook(MethodBase intercepted, MethodInfo custom_method, MethodHookFlags hook_flags = MethodHookFlags.None)
            : base(intercepted, custom_method, MethodHookType.RunBefore, hook_flags)
        {
            if (hook_flags == MethodHookFlags.CanSkipOriginal)
            {
                throw new NotImplementedException("RefHook does not yet support Skipping. Want that feature? Leave me a note, so i may actually implement it :)");
            }
        }
        public override IEnumerable<CustomParameterInfo> GetRequiredParameterLayout()
        {
            if (!InterceptedMethod.IsStatic)
            {
                yield return InterceptedMethod.DeclaringType;
            }
            foreach (var el in InterceptedMethod.GetParameters())
            {
                if (el.ParameterType.IsByRef)
                {
                    yield return el;
                }
                else
                {
                    yield return el.ParameterType.MakeByRefType();
                }
            }
        }
    }
}
