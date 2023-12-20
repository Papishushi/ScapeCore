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
 * DeeplyMutable.cs
 * DeeplyMutable<T> is a generic class that extends DeeplyMutableType
 * and provides type-safe access to deeply mutable types.
 */

using Serilog;
using System;
using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace ScapeCore.Core.Batching.Tools
{
    public sealed class DeeplyMutable<T> : DeeplyMutableType
    {
        public new T? Value { get => _value; set => _value = value; }
        public DeeplyMutable() : base() { }
        public DeeplyMutable(T? value) : base(value) { }
        public DeeplyMutable(DeeplyMutableType deeplyMutableType) => _value = deeplyMutableType.Value;

        protected override FieldInfo[]? DynamicFields => typeof(T).GetFields();
    }
}
