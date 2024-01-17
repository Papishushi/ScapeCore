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
using static ScapeCore.Core.Physics2D.Collider2D;

namespace ScapeCore.Core.Physics2D
{
    public sealed class RigidBody2D : Component
    {
        public bool isStatic = false;
        public bool isKinematic = false;
        private readonly List<Collider2D> _attachedColliders = new();
        private List<Collider2D> _colliding = new();
        private List<Impact2D> _impacts = new();

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
        private Vector2 _oldPosition = Vector2.Zero;
        public float adjustmentThreshold = 1000f;

        public  bool IsColliding { get => _colliding.Count > 0; }
        public Collider2D MainCollider { get => _attachedColliders.First(); }

        public void InvertSpeedHorizontal() => _speed.X *= -_bounceFactor;
        public void InvertSpeedVertical() => _speed.Y *= -_bounceFactor;

        public void SetSpeed(Vector2 value) => _speed = value;

        public void Initialize()
        {
            if (_attachedColliders.Count == 0)
            {
                _attachedColliders.Add(gameObject?.AddBehaviour<Collider2D>());
                _attachedColliders.First().attachedRigidbody = this;
            }
        }

        public float GetDeltaTime(GameTime gameTime)
        {
            double seconds, milliseconds;
            seconds = gameTime.ElapsedGameTime.TotalSeconds;
            milliseconds = gameTime.ElapsedGameTime.TotalMilliseconds;
            return (float)seconds + ((float)milliseconds / 1000);
        }

        private void CalculateCollisionsLogic(float deltaTime)
        {
            _impacts = _attachedColliders.First().IsIntersecting();
            _impacts.ForEach(x => _colliding.Add(x.Collision));

            foreach (var _impact in _impacts)
            {
                var collidedRb = _impact.Collision.attachedRigidbody;
                if (collidedRb == null || collidedRb.transform == null)
                    continue;

                var relativePosition = transform.Position - collidedRb.transform.Position;
                var relativeVelocity = _speed - collidedRb.Speed;

                // Calculate impulse based on the relative velocity and position
                var impulse = 2 * Vector2.Dot(relativeVelocity, relativePosition) / (relativePosition.LengthSquared() * (mass + collidedRb.mass));

                // Calculate the impulse force vector
                var impulseForce = impulse * relativePosition;
                var normal = Vector2.Normalize(_impact.Normal);
                // Update velocities based on the impulse
                _speed += ((impulseForce * normal) / mass) * _bounceFactor;  // Use += instead of -= for the first object
                //collidedRb.SetSpeed(collidedRb.Speed + (impulseForce * mass / 10));
            }
        }

        public void CalculateCollisionsAndApplyForces(float deltaTime) => CalculateCollisionsLogic(deltaTime);
        public void CalculateCollisionsAndApplyForces(GameTime gameTime, float timeStepFactor) => CalculateCollisionsLogic(GetDeltaTime(gameTime) * timeStepFactor);

        public void ResolveCollisionsOverlap()
        {
            var _displacementMinThreshold = 0.85f;

            foreach (var impact in _impacts)
            {
                var _displacementMaxThreshold = impact.Normal.Length() * 0.5f;
                var displacement = new Vector2(-impact.Normal.X, impact.Normal.Y);

                if (Math.Abs(displacement.X) < _displacementMinThreshold)
                    displacement.X = 0;
                if (Math.Abs(displacement.Y) < _displacementMinThreshold)
                    displacement.Y = 0;
                transform.Position += displacement;
            }
        }

        private void ResolveSpeedLogic(float deltaTime)
        {
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

            transform.Position += _speed;
        }

        public void ResolveSpeedAndApplyForces(float deltaTime) => ResolveSpeedLogic(deltaTime);
        public void ResolveSpeedAndApplyForces(GameTime gameTime, float timeStepFactor) => ResolveSpeedLogic(GetDeltaTime(gameTime) * timeStepFactor);


        public void FixedUpdateWrapper(GameTime time, float timeStepFactor)
        {
            var deltaTime = GetDeltaTime(time) * timeStepFactor;

            Vector2 adjustedPosition = transform.Position;
            float adjustmentMagnitude = Vector2.Distance(_oldPosition, adjustedPosition);
            if (adjustmentMagnitude > adjustmentThreshold)
                Log.Warning("Large position adjustment: {AdjustmentMagnitude}", adjustmentMagnitude);

            ResolveSpeedAndApplyForces(deltaTime);

            // Store the old position
            _oldPosition = transform.Position;
            // Update position
            transform.Position += _speed;

            CalculateCollisionsAndApplyForces(deltaTime);

            ResolveCollisionsOverlap();
        }

    }
}