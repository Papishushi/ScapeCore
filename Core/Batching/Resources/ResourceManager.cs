/*
 * -*- encoding: utf-8 with BOM -*-
 * .▄▄ ·  ▄▄·  ▄▄▄·  ▄▄▄·▄▄▄ .     ▄▄·       ▄▄▄  ▄▄▄ .
 * ▐█ ▀. ▐█ ▌▪▐█ ▀█ ▐█ ▄█▀▄.▀·    ▐█ ▌▪▪     ▀▄ █·▀▄.▀·
 * ▄▀▀▀█▄██ ▄▄▄█▀▀█  ██▀·▐▀▀▪▄    ██ ▄▄ ▄█▀▄ ▐▀▀▄ ▐▀▀▪▄
 * ▐█▄▪▐█▐███▌▐█ ▪▐▌▐█▪·•▐█▄▄▌    ▐███▌▐█▌.▐▌▐█•█▌▐█▄▄▌
 *  ▀▀▀▀ ·▀▀▀  ▀  ▀ .▀    ▀▀▀     ·▀▀▀  ▀█▄▀▪.▀  ▀ ▀▀▀ 
 * https://github.com/Papishushi/ScapeCore
 * 
 * Copyright (c) 2023 Daniel Molinero Lucas
 * This file is subject to the terms and conditions defined in
 * file 'LICENSE.txt', which is part of this source code package.
 * 
 * ResourceManager.cs
 * This manager is responsible for managing resources,
 * their dependencies, and loading referenced resources.
 */

using Microsoft.Xna.Framework.Content;
using ScapeCore.Core.Batching.Events;
using ScapeCore.Core.Batching.Tools;
using ScapeCore.Core.Engine;
using ScapeCore.Targets;
using Serilog;
using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace ScapeCore.Core.Batching.Resources
{
    public static class ResourceManager
    {
        private static readonly ResourceDependencyTree _tree = new();
        public static ResourceDependencyTree Content { get => _tree; }
        private static readonly LLAM? _game = null;

        static ResourceManager()
        {
            if (!LLAM.Instance.TryGetTarget(out var target))
            {
                Log.Error("ResourceManager was unable to load referenced resources. {LLAM} instance is GCed.", typeof(LLAM).FullName);
                return;
            }
            target.OnLoad += LoadAllReferencedResources;
            _game = target;
        }

        public static StrongBox<T?> GetResource<T>(string key) => new(new DeeplyMutable<T>(_tree.Dependencies[new(key, typeof(T))].resource).Value);

        private static void LoadAllReferencedResources(object source, LoadBatchEventArgs args)
        {
            Log.Debug($"{source.GetHashCode()} {args.GetInfo()}");

            foreach (var type in ReflectiveEnumerator.GetEnumerableOfType<MonoBehaviour>())
            {
                foreach (var rsrcLoadAttr in Attribute.GetCustomAttributes(type).Where(attr => attr is ResourceLoadAttribute && attr != null).Cast<ResourceLoadAttribute>())
                {
                    foreach (var loadName in rsrcLoadAttr.names)
                    {
                        var info = new ResourceInfo(loadName, rsrcLoadAttr.loadType);
                        if (_tree.ContainsResource(info))
                            _tree.GetResource(info).dependencies.Add(type);
                        else
                        {
                            var method = typeof(ContentManager).GetMethod(nameof(_game.Content.Load));
                            method = method?.MakeGenericMethod(info.TargetType);
                            var result = method?.Invoke(_game?.Content, new object[1] { info.ResourceName });
                            if (result == null)
                            {
                                Log.Error("Resource Manager encountered an error while loading a resource. Resource load returned {null}.", null);
                                continue;
                            }
                            var obj = Convert.ChangeType(result, info.TargetType);
                            if (obj == null)
                            {
                                Log.Error("Resource Manager encountered an error while loading a resource. Loaded resource wasnt succesfully chaged to type {t} and returned null.", info.TargetType);
                                continue;
                            }
                            dynamic changedObject = obj;
                            _tree.Add(info, type, changedObject);
                        }
                    }
                }
            }

            foreach (var dependency in _tree.Dependencies)
                Log.Debug($"{string.Join(',', dependency.Value.dependencies)} types loaded resource {{{dependency.Key.ResourceName}}} of type {{{dependency.Key.TargetType}}}");
        }
    }
}