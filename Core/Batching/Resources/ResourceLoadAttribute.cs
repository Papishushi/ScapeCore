using System;

namespace ScapeCore.Core.Batching.Resources
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public sealed class ResourceLoadAttribute : Attribute
    {
        internal readonly Type loadType;
        internal readonly string[] names;

        public ResourceLoadAttribute(Type loadType, params string[] names)
        {
            this.loadType = loadType;
            this.names = names;
        }

    }
}