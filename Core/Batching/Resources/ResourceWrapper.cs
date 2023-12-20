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
 * ResourceWrapper.cs
 * Wraps a loaded resource in a DeeplyMutableType and provides a
 * list of all the known types that are a dependency of that
 * resource. This class do not automatically update the dependencies.
 */

using ScapeCore.Core.Batching.Tools;
using System;
using System.Collections.Generic;

namespace ScapeCore.Core.Batching.Resources
{
    public class ResourceWrapper
    {
        public DeeplyMutableType? resource = null;
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