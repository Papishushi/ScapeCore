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
 * Transform.cs
 * Represents a transformation.
 */

using Microsoft.Xna.Framework;

namespace ScapeCore.Core.Engine.Components
{
    public class Transform : Component
    {
        public Vector2 _position = Vector2.Zero;
        public Vector2 _rotation = Vector2.Zero;
        public Vector2 _scale = Vector2.One;

        public Vector2 LocalPosition { get => _position; set => _position = value; }
        public Vector2 LocalRotation { get => _rotation; set => _rotation = value; }
        public Vector2 LocalScale { get => _scale; set => _scale = value; }
        public Vector2 Position { get => (gameObject?.parent?.transform?.Position ?? Vector2.Zero) + _position; set => _position = value; }
        public Vector2 Rotation { get => (gameObject?.parent?.transform?.Rotation ?? Vector2.Zero) + _rotation; set => _rotation = value; }
        public Vector2 Scale { get => (gameObject?.parent?.transform?.Scale ?? Vector2.One) * _scale; set => _scale = value; }

        public Transform() : base(nameof(Transform)) { }
        public Transform(string name) : base(name) { }
        public Transform(Vector2 position, Vector2 rotation, Vector2 scale) : base(nameof(Transform))
        {
            _position = position;
            _rotation = rotation;
            _scale = scale;
        }
        public Transform(string name, Vector2 position, Vector2 rotation, Vector2 scale) : base(name)
        {
            _position = position;
            _rotation = rotation;
            _scale = scale;
        }
    }
}