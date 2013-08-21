
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
    /// Allows you to call your own method instead of a specific object constructor in selected functions
    /// </summary>
    [Obsolete("Do not use this shit! It is very early wip and you will only break sth...")]
    public class ClassCreationHook: IModification
    {
        public Type ClassToInterceptCreation { get; private set; }
        public MethodBase InterceptCreationInMethod { get; private set; }
        public MethodBase CustomCreationMethod { get; private set; }

        Type IModification.TargetType { get { return InterceptCreationInMethod == null ? ClassToInterceptCreation : InterceptCreationInMethod.DeclaringType; } }

        // Todo: make it more "generic", might by specifiying a base class and letting all others pass to it. Also an "ANY" instead of doChangeIn would be nice.
        public ClassCreationHook(Type classToIncerceptCreation, MethodBase doChangeInMethod, MethodBase customCalledCreatingFunction)
        {
            ClassToInterceptCreation = classToIncerceptCreation;
            InterceptCreationInMethod = doChangeInMethod;
            CustomCreationMethod = customCalledCreatingFunction;
            // Todo: Find a new solution for arguments
            // Todo: NO CHECKS HERE, YET!
        }
       
    }
}