using ScapeCore.Core.Batching.Tools;
using System;
using System.Collections.Generic;

namespace ScapeCore.Core.Batching.Resources
{
    public class ResourceWrapper
    {
        public DeeplyMutableType resource = null;
        public readonly List<Type> dependencies = new();

        public ResourceWrapper(Type dependency)
        {
            resource = null;
            dependencies.Add(dependency);
        }
        public ResourceWrapper(dynamic resource, Type dependency)
        {
            var unknown = resource.GetType();
            var type = typeof(DeeplyMutableType<>).MakeGenericType(unknown);
            dynamic context = Activator.CreateInstance(type);
            context.value = resource;
            resource = context;
            this.resource=resource;
            dependencies.Add(dependency);
        }
        public ResourceWrapper(dynamic resource, List<Type> dependencies)
        {
            var unknown = resource.GetType();
            var type = typeof(DeeplyMutableType<>).MakeGenericType(unknown);
            dynamic context = Activator.CreateInstance(type);
            context.Value = resource;
            resource = context;
            this.resource=resource;
            this.dependencies=dependencies;
        }
    }
}