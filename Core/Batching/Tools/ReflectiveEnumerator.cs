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
 * ReflectiveEnumerator.cs
 * ReflectiveEnumerator provides methods for obtaining an enumerable of types that are subclasses of a specified type.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ScapeCore.Core.Batching.Tools
{
    public static class ReflectiveEnumerator
    {
        public static IEnumerable<Type> GetEnumerableOfType<T>()
        {
            List<Type> types = new();
            foreach (var subclassType in Assembly.GetAssembly(typeof(T)).GetTypes().Where(x => x.IsSubclassOf(typeof(T))))
                types.Add(subclassType);
            return types;
        }
        public static IEnumerable<Type> GetEnumerableOfType(Type type)
        {
            List<Type> types = new();
            foreach (var subclassType in Assembly.GetAssembly(type).GetTypes().Where(x => x.IsSubclassOf(type)))
                types.Add(subclassType);
            return types;
        }
    }
}