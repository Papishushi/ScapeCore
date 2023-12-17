/*
 * -*- encoding: utf-8 with BOM -*-
 * .▄▄ ·  ▄▄·  ▄▄▄·  ▄▄▄·▄▄▄ .     ▄▄·       ▄▄▄  ▄▄▄ .
 * ▐█ ▀. ▐█ ▌▪▐█ ▀█ ▐█ ▄█▀▄.▀·    ▐█ ▌▪▪     ▀▄ █·▀▄.▀·
 * ▄▀▀▀█▄██ ▄▄▄█▀▀█  ██▀·▐▀▀▪▄    ██ ▄▄ ▄█▀▄ ▐▀▀▄ ▐▀▀▪▄
 * ▐█▄▪▐█▐███▌▐█ ▪▐▌▐█▪·•▐█▄▄▌    ▐███▌▐█▌.▐▌▐█•█▌▐█▄▄▌
 *  ▀▀▀▀ ·▀▀▀  ▀  ▀ .▀    ▀▀▀     ·▀▀▀  ▀█▄▀▪.▀  ▀ ▀▀▀ 
 * https://github.com/Papishushi/ScapeCore
 * 
 * MIT License
 *
 * Copyright (c) 2023 Daniel Molinero Lucas
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

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