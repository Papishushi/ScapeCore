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
 * Behaviour.cs
 * It is the base class for all attacheable behaviours used on ScapeCore.
 */

using ScapeCore.Targets;
using System;

namespace ScapeCore.Core.Engine
{
    public abstract class Behaviour
    {
        private readonly Guid _id = new();

        private LLAM? _game = null;

        private bool _isActive;
        private bool _isDestroyed;

        public string name = nameof(Behaviour);

        protected LLAM? Game { get => _game; }
        public Guid Id { get => _id; }
        public bool IsActive { get => _isActive; }
        public bool IsDestroyed { get => _isDestroyed; }

        ~Behaviour() => Destroy();
        public Behaviour()
        {
            _game =  LLAM.Instance.TryGetTarget(out var target) ? target : null;
            _isActive = true;
            _isDestroyed = false;
            OnCreate();
        }
        protected Behaviour(string name) : this() => this.name = name;

        public T? To<T>() where T : Behaviour => this as T;

        public void SetActive(bool isActive) => _isActive = isActive;

        public override string ToString() => name;

        protected abstract void OnCreate();
        protected abstract void OnDestroy();

        public void Destroy()
        {
            if (_isDestroyed) return;
            _isActive = false;
            OnDestroy();
            _game = null;
            _isDestroyed = true;
        }

    }
}