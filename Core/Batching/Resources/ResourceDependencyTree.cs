using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ScapeCore.Core.Batching.Resources
{
    public readonly record struct ResourceDependencyTree
    {
        private readonly Dictionary<string, ResourceWrapper> _resourcesPerName = new();
        private readonly List<KeyValuePair<Type, ResourceInfo>> _dependenciesPerType = new();
        private readonly Dictionary<ResourceInfo, ResourceWrapper> _dependencies = new();

        public ReadOnlyDictionary<string, ResourceWrapper> ResourcesPerName => new(_resourcesPerName);

        public ReadOnlyCollection<KeyValuePair<Type, ResourceInfo>> DependenciesPerType => new(_dependenciesPerType);

        public ReadOnlyDictionary<ResourceInfo, ResourceWrapper> Dependencies => new(_dependencies);

        public void Add(ResourceInfo info, Type dependency, dynamic resource)
        {
            _dependenciesPerType.Add(new(dependency, new(info.ResourceName, info.TargetType)));
            if (_dependencies.ContainsKey(info))
                _dependencies[info].dependencies.Add(dependency);
            else
            {
                var wrapper = new ResourceWrapper(dependency)
                {
                    resource = resource
                };
                _dependencies.Add(info, wrapper);
                if (_resourcesPerName.ContainsKey(info.ResourceName))
                    _resourcesPerName[info.ResourceName] = wrapper;
                else
                    _resourcesPerName.Add(info.ResourceName, wrapper);
            }

        }

        public bool ContainsResource(ResourceInfo info) => _dependencies.ContainsKey(info);
        public bool ContainsResource(string name) => _resourcesPerName.ContainsKey(name);
        public bool IsTypeDependent(Type type) => !_dependenciesPerType.Find(t => t.Key == type).Equals(default(KeyValuePair<Type, ResourceInfo>));

        public ResourceWrapper GetResource(string resourceName) => _resourcesPerName[resourceName];
        public ResourceWrapper GetResource(Type type) => _dependencies[_dependenciesPerType.Find(x => x.Key == type).Value];
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