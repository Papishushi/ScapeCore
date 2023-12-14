using ScapeCore.Core.Batching.Tools;
using Serilog;
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
            var type = typeof(DeeplyMutable<>).MakeGenericType(unknown);
            dynamic context = Activator.CreateInstance(type);
            context.Value = resource;
            this.resource = context;
            dependencies.Add(dependency);
        }
        public ResourceWrapper(dynamic resource, List<Type> dependencies)
        {
            var unknown = resource.GetType();
            var type = typeof(DeeplyMutable<>).MakeGenericType(unknown);
            dynamic context = Activator.CreateInstance(type);
            context.Value = resource;
            this.resource = context;
            this.dependencies=dependencies;
        }
    }
}