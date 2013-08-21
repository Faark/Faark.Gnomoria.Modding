using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Faark.Util;

namespace Faark.Gnomoria.Modding
{
    public abstract class ClassModification : IModification
    {
        public Type ClassToChange { get; private set; }
        public ClassModification(Type class_to_change)
        {
            if (class_to_change == null)
            {
                throw new ArgumentException("No class type referenced.");
            }
            if (!class_to_change.IsClass)
            {
                throw new ArgumentException("Given type [" + class_to_change.FullName + "] is not a class.");
            }
            ClassToChange = class_to_change;
        }
        Type IModification.TargetType
        {
            get
            {
                return ClassToChange;
            }
        }
    }
    public class ClassChangeBase: ClassModification
    {
        public Type NewBaseClass { get; private set; }
        public ClassChangeBase(Type class_to_change, Type new_base_class):base(class_to_change)
        {
            if (new_base_class == null)
            {
                throw new ArgumentException("No new base type referenced.");
            }
            if (!new_base_class.IsClass)
            {
                throw new ArgumentException("Given new base type [" + new_base_class.FullName + "] is not a class.");
            }
            if (!class_to_change.BaseType.IsAssignableFrom(new_base_class))
            {
                throw new ArgumentException("The targets original base type [" + class_to_change.BaseType.FullName + "] is not assignable from the given new base type [" + new_base_class.FullName + "].");
            }
            NewBaseClass = new_base_class;
            // Todo: make sure the new class has all constructors. It should work without for now, but forcing it would / should result in more awareness
        }
    }
}
