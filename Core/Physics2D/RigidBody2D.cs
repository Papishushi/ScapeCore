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
 * RigidBody2D.cs
 * This class represents a physics body in a simulation. This type
 * uses Colliders 2D as the boundary for the body.
 */


// THIS CLASS IS ABSOLUTELY IN WIP AND IT'S NOT FUNCTIONAL,
// THIS IS A DIRECT ADVERTENCY DO NOT EXPECT NOTHING.

using Microsoft.Xna.Framework;
using ScapeCore.Core.Engine;
using ScapeCore.Core.Engine.Components;
using Serilog;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace ScapeCore.Core.Physics2D
{
    public sealed class RigidBody2D : Component
    {
        public bool isStatic = false;
        public bool isKinematic = false;
        private readonly List<Collider2D> _attachedColliders = new();
        private List<Collider2D> _colliding = new();

        public Vector2 force = Vector2.Zero;
        public Vector2 angularVelocity = Vector2.Zero;
        public Vector2 gravity = new(0, 20.80665f); //20.80665f
        public float mass = 1;
        public Vector2 Speed { get => new(_speed.X, _speed.Y); }

        private float _bounceFactor = 0.72f;
        private float _frictionCoefficient = 0.2f;
        private Vector2 _acceleration;
        private Vector2 _angularAcceleration;
        private Vector2 _speed;
        private Vector2 _angularSpeed;
        private float _verticalDampingFactor = 0.98f;
        private float _horizontalDampingFactor = 0.95f;
        private float _speedThreshold = 0.01f; // Adjust the threshold as needed

        public  bool IsColliding { get => _colliding.Count > 0; }
        public Collider2D MainCollider { get => _attachedColliders.First(); }

        public void InvertSpeedHorizontal() => _speed.X *= -_bounceFactor;
        public void InvertSpeedVertical() => _speed.Y *= -_bounceFactor;

        protected void SetSpeed(Vector2 value) => _speed = value;

        public void Initialize()
        {
            if (_attachedColliders.Count == 0)
            {
                _attachedColliders.Add(gameObject?.AddBehaviour<Collider2D>());
                _attachedColliders.First().attachedRigidbody = this;
            }
        }

        public void FixedUpdateWrapper(GameTime time)
        {
            float deltaTime;
            double seconds, milliseconds;
            seconds = time.ElapsedGameTime.TotalSeconds;
            milliseconds = time.ElapsedGameTime.TotalMilliseconds;
            deltaTime = (float)seconds + ((float)milliseconds / 1000);
            deltaTime *= 0.1f;

            // Assuming _force is a Vector2 representing external forces
            _acceleration = force / mass + gravity;

            Vector2 dampingEffect = _speed - gravity;
            dampingEffect.X *= _horizontalDampingFactor;
            dampingEffect.Y *= _verticalDampingFactor;

            // Update velocity
            _speed = dampingEffect + gravity;
            _speed += _acceleration * deltaTime;

            // Apply speed threshold
            if (Math.Abs(_speed.X) < _speedThreshold)
                _speed.X = 0;

            if (Math.Abs(_speed.Y) < _speedThreshold)
                _speed.Y = 0;

            // Store the old position
            var oldPosition = transform.Position;
            // Update position
            transform.Position += _speed;

            float totalOverlapX = 0f;
            float totalOverlapY = 0f;

            // Update position after collision resolution
            _colliding = _attachedColliders.First().IsIntersecting();
            //Log.Debug("{cool}", _colliding.Count);
            foreach (var collider in _colliding)
            {
                var collidedRb = collider.attachedRigidbody;
                if (collidedRb == null || collidedRb.transform == null)
                    continue;

                var relativeVelocity = _speed - collidedRb.Speed;
                var relativePosition = transform.Position - collidedRb.transform.Position;

                // Adjust positions after collision
                float overlapX = Math.Abs(transform.Position.X - collidedRb.transform.Position.X) - (_attachedColliders.First().size.X + collider.size.X) * 0.5f;
                float overlapY = Math.Abs(transform.Position.Y - collidedRb.transform.Position.Y) - (_attachedColliders.First().size.Y + collider.size.Y) * 0.5f;

                // Accumulate adjustments
                totalOverlapX += overlapX;
                totalOverlapY += overlapY;

                // Calculate impulse based on the relative velocity and position
                var impulse = 2 * Vector2.Dot(relativeVelocity, relativePosition) / (relativePosition.LengthSquared() * (mass + collidedRb.mass));

                // Calculate the impulse force vector
                var impulseForce = impulse * relativePosition / relativePosition.Length();

                // Update velocities based on the impulse
                _speed -= impulseForce * collidedRb.mass;
                collidedRb.SetSpeed(collidedRb.Speed + impulseForce * mass);
            }

            // Apply position adjustments after resolving overlaps
            transform.Position = new Vector2(transform.Position.X - totalOverlapX, transform.Position.Y - totalOverlapY);

            // Optional: Check for large adjustments and log a warning
            Vector2 adjustedPosition = transform.Position;
            float adjustmentMagnitude = Vector2.Distance(oldPosition, adjustedPosition);
            if (adjustmentMagnitude > 1000f)
            {
                Log.Warning("Large position adjustment: {AdjustmentMagnitude}", adjustmentMagnitude);
            }
        }

    }
}