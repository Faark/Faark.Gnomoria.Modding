
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
    public class MethodHook : UnmutableMethodModification
    {
        public MethodHook(MethodBase intercepted, MethodInfo custom_method, MethodHookType hook_type = MethodHookType.RunAfter, MethodHookFlags hook_flags = MethodHookFlags.None)
            :base(intercepted, custom_method, hook_type, hook_flags)
        {
        }
    }
    public class BeforeAndAfterMethodHook : ModificationCollection
    {
        public override Type TargetType { get { return ((IModification)OnBeforeHook).TargetType; } }
        public MethodHook OnBeforeHook { get; protected set; }
        public MethodHook OnAfterHook { get; protected set; }
        public override IEnumerator<IModification> GetModifications()
        {
            yield return OnBeforeHook;
            yield return OnAfterHook;
        }
        public BeforeAndAfterMethodHook(MethodBase intercepted, MethodInfo custom_before_method, MethodInfo custom_after_method)
        {
            OnBeforeHook = new MethodHook(intercepted, custom_before_method, MethodHookType.RunBefore);
            OnAfterHook = new MethodHook(intercepted, custom_after_method, MethodHookType.RunAfter);
        }
    }
}