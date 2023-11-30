using ScapeCore.Core.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ScapeCore.Core.Batching
{
    public static class ReflectiveEnumerator
    {
        public static IEnumerable<Type> GetEnumerableOfType<T>() where T : MonoBehaviour
        {
            List<Type> types = new();
            foreach (var subclassType in Assembly.GetAssembly(typeof(T)).GetTypes().Where(x => x.IsSubclassOf(typeof(T))))
                types.Add(subclassType);
            return types;
        }
    }
}