
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
    public class MethodAddVirtual : UnmutableMethodModification
    {
        public Type ModifyingType { get; private set; }
        public MethodAddVirtual(Type modifyingType, MethodInfo virtual_method, MethodInfo custom_method, MethodHookType type = MethodHookType.RunAfter, MethodHookFlags flags = MethodHookFlags.None)
            :base(virtual_method, custom_method, type, flags)
        {
            ModifyingType = modifyingType;
            if (ModifyingType == null)
                throw new ArgumentException("No target type specified!");
            if (!virtual_method.DeclaringType.IsAssignableFrom(modifyingType))
                throw new ArgumentException("Given Type [" + ModifyingType.FullName + "] is not a child of target [" + virtual_method.DeclaringType + "]!");
            base.Validate_4_ParameterLayout();

            if (modifyingType
                .GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .Select(m => m.GetBaseDefinition())
                .Contains(virtual_method.GetBaseDefinition()))
            {
                if (virtual_method.DeclaringType == modifyingType)
                {
                    throw new ArgumentException("Can't add a new virtual override method [" + virtual_method.Name + "]. Type [" + modifyingType.FullName + "] appears to already have such one. May just use a hook?");
                }
            }
            else
            {
                throw new NotImplementedException("This part hopefully isn't necessary ever :p");
                /*
                 * MAKE SURE THAT WE HAVE THE REF TO ACTUAL BASE FUNCTION WE MODIFY ... required to to base.ourfunc();
                 *
                var tMeth = modifyingType
                    .GetMethods();
                var virtParamTypes = virtual_method.GetParameters().Select(p => p.ParameterType);
                var similarMethods = modifyingType
                    .GetMethods()
                    .Where(f => f.Name == virtual_method.Name
                        && f.ReturnType == virtual_method.ReturnType
                        && f.IsStatic == virtual_method.IsStatic
                        && f.GetParameters().Select(p => p.ParameterType).SequenceEqual(virtParamTypes)
                    );
                if (similarMethods.Count() > 0)
                    throw new ArgumentException("Can't add a new virtual override method [" + virtual_method.Name + "]. Type [" + modifyingType.FullName + "] appears to already have such one. May just use a hook?");
                */
            }
        }
        protected override void Validate_4_ParameterLayout()
        {
            // we skip this for now, so we can set local vars. TODO: Make sure that we have some kind of check somewere!!!!!!!!!!!!!
            //base.Validate_3_ParameterLayout();
        }
        public override IEnumerable<CustomParameterInfo> GetRequiredParameterLayout()
        {
            var first = true;
            foreach (var el in base.GetRequiredParameterLayout())
            {
                if (first)
                {
                    if (!el.IsSimpleType(InterceptedMethod.DeclaringType))
                    {
                        throw new Exception();
                    }
                    yield return ModifyingType;
                    first = false;
                    continue;
                }
                yield return el;
            }
        } 
    }
}
