using System;

namespace ScapeCore.Core.Batching
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class ResourceLoadAttribute : Attribute
    {
        internal readonly string[] loadNames;

        public ResourceLoadAttribute(params string[] names) => loadNames = names;
    }
}