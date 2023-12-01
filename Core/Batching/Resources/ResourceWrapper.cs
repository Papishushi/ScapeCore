using System;
using System.Collections.Generic;

namespace ScapeCore.Core.Batching.Resources
{
    public class ResourceWrapper
    {
        public dynamic resource = null;
        public readonly List<Type> dependencies = new();

        public ResourceWrapper(Type dependency)
        {
            resource = null;
            dependencies.Add(dependency);
        }
        public ResourceWrapper(dynamic resource, List<Type> dependencies)
        {
            this.resource=resource;
            this.dependencies=dependencies;
        }
    }
}