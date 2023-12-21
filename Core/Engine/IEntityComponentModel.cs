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
 * IEntityComponentModdel.cs
 * Represents a common interface for behaviours that attach to
 * themselves a GameObject reference, like Components and MonoBehaviours.
 */

using ScapeCore.Core.Engine.Components;
using System.Diagnostics.CodeAnalysis;

namespace ScapeCore.Core.Engine
{
    internal interface IEntityComponentModel
    {
        [SuppressMessage("Style", "IDE1006:Naming Styles",
         Justification = "<In this way it does not match class name and keep it simple and descriptible.>")]
        public GameObject? gameObject { get; internal set; }
        [SuppressMessage("Style", "IDE1006:Naming Styles",
         Justification = "<In this way it does not match class name and keep it simple and descriptible.>")]
        public Transform? transform { get => gameObject?.transform; }
    }
}