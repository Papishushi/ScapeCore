using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace ScapeCore.Core.Batching.Resources
{
    public readonly record struct ResourceDependencyTree
    {
        private readonly ConcurrentDictionary<string, ResourceWrapper> _resourcesPerName = new();
        private readonly ConcurrentBag<KeyValuePair<Type, ResourceInfo>> _dependenciesPerType = new();
        private readonly ConcurrentDictionary<ResourceInfo, ResourceWrapper> _dependencies = new();

        public ImmutableDictionary<string, ResourceWrapper> ResourcesPerName => _resourcesPerName.ToImmutableDictionary();

        public ImmutableList<KeyValuePair<Type, ResourceInfo>> DependenciesPerType => _dependenciesPerType.ToImmutableList();

        public ImmutableDictionary<ResourceInfo, ResourceWrapper> Dependencies => _dependencies.ToImmutableDictionary();

        public void Add(ResourceInfo info, Type dependency, dynamic resource)
        {
            _dependenciesPerType.Add(new(dependency, new(info.ResourceName, info.TargetType)));
            if (_dependencies.ContainsKey(info))
                _dependencies[info].dependencies.Add(dependency);
            else
            {
                var wrapper = new ResourceWrapper(resource, dependency);
                _dependencies.TryAdd(info, wrapper);
                if (_resourcesPerName.ContainsKey(info.ResourceName))
                    _resourcesPerName[info.ResourceName] = wrapper;
                else
                    _resourcesPerName.TryAdd(info.ResourceName, wrapper);
            }

        }

        public bool ContainsResource(ResourceInfo info) => _dependencies.ContainsKey(info);
        public bool ContainsResource(string name) => _resourcesPerName.ContainsKey(name);
        public bool IsTypeDependent(Type type) => !DependenciesPerType.Find(t => t.Key == type).Equals(default(KeyValuePair<Type, ResourceInfo>));

        public ResourceWrapper GetResource(string resourceName) => _resourcesPerName[resourceName];
        public ResourceWrapper GetResource(Type type) => _dependencies[DependenciesPerType.Find(x => x.Key == type).Value];
        public ResourceWrapper GetResource(ResourceInfo info) => _dependencies[info];

        public void Clear()
        {
            _resourcesPerName.Clear();
            _dependenciesPerType.Clear();
            _dependencies.Clear();
        }

        public ResourceDependencyTree() { }
    }
}