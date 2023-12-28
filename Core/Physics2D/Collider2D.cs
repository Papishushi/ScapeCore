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
 * Collider2D.cs
 * Represent a 2D Collider with a size and an attached RigidBody.
 */

using Microsoft.Xna.Framework;
using ScapeCore.Core.Engine;
using ScapeCore.Core.Engine.Components;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ScapeCore.Core.Physics2D
{
    public class Collider2D : Component
    {
        protected static readonly List<Collider2D> _colliders = new();

        public Vector2 size;
        public RigidBody2D? attachedRigidbody;

        public Collider2D()
        {
            _colliders.Add(this);
            size = new Vector2(10,10);
            attachedRigidbody = null;
        }

        public Collider2D(Vector2 size, RigidBody2D? attachedRigidbody)
        {
            _colliders.Add(this);
            this.size = size;
            this.attachedRigidbody = attachedRigidbody;
        }

        internal List<Collider2D> IsIntersecting()
        {
            List<Collider2D> result = new List<Collider2D>();
            foreach (var collider in _colliders)
            {
                if (collider == this) continue;
                var rb = collider.attachedRigidbody;
                if (rb == null || rb.transform == null || transform == null)
                    continue;

                float deltaX = Math.Abs(transform.Position.X - rb.transform.Position.X);
                float deltaY = Math.Abs(transform.Position.Y - rb.transform.Position.Y);

                float intersectX = (size.X + collider.size.X) * 0.5f;
                float intersectY = (size.Y + collider.size.Y) * 0.5f;

                if (deltaX < intersectX && deltaY < intersectY)
                {
                    // There is an intersection
                    result.Add(collider);
                }
            }

            return result;
        }

        protected override void OnDestroy()
        {
            _colliders.Remove(this);
            base.OnDestroy();
        }
    }
}