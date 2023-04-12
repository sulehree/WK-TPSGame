using System.Collections.Generic;
using System;
using System.Linq;

namespace Invector.vCharacterController.AI.FSMBehaviour
{
    public static class vFSMHelper
    {
        public static IEnumerable<Type> FindSubClasses(this Type type)
        {
            IEnumerable<Type> exporters = type
             .Assembly.GetTypes()
             .Where(t => t.IsSubclassOf(type) && !t.IsAbstract);
            return exporters;
        }
    }
}