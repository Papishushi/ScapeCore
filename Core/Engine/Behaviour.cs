/*
 * -*- encoding: utf-8 with BOM -*-
 * .▄▄ ·  ▄▄·  ▄▄▄·  ▄▄▄·▄▄▄ .     ▄▄·       ▄▄▄  ▄▄▄ .
 * ▐█ ▀. ▐█ ▌▪▐█ ▀█ ▐█ ▄█▀▄.▀·    ▐█ ▌▪▪     ▀▄ █·▀▄.▀·
 * ▄▀▀▀█▄██ ▄▄▄█▀▀█  ██▀·▐▀▀▪▄    ██ ▄▄ ▄█▀▄ ▐▀▀▄ ▐▀▀▪▄
 * ▐█▄▪▐█▐███▌▐█ ▪▐▌▐█▪·•▐█▄▄▌    ▐███▌▐█▌.▐▌▐█•█▌▐█▄▄▌
 *  ▀▀▀▀ ·▀▀▀  ▀  ▀ .▀    ▀▀▀     ·▀▀▀  ▀█▄▀▪.▀  ▀ ▀▀▀ 
 * https://github.com/Papishushi/ScapeCore
 * 
 * MIT License
 *
 * Copyright (c) 2023 Daniel Molinero Lucas
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
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

        public string name;

        protected LLAM? Game { get => _game; }
        public Guid Id { get => _id; }
        public bool IsActive { get => _isActive; }
        public bool IsDestroyed { get => _isDestroyed; }

        ~Behaviour() => OnDestroy();
        public Behaviour()
        {
            var b = LLAM.Instance.TryGetTarget(out var target);
            _game =  b ? target : null;
            name = nameof(Behaviour);
            _isActive = true;
            _isDestroyed = false;
            OnCreate();
        }
        protected Behaviour(string name)
        {
            var b = LLAM.Instance.TryGetTarget(out var target);
            _game =  b ? target : null;
            this.name = name;
            _isActive = true;
            _isDestroyed = false;
            OnCreate();
        }
        public T To<T>() where T : Behaviour => (T)this;

        public void SetActive(bool isActive) => _isActive = isActive;

        public override string ToString() => name;

        protected abstract void OnCreate();
        protected abstract void OnDestroy();

        public void Destroy()
        {
            _isActive = false;
            OnDestroy();
            _game = null;
            _isDestroyed = true;
        }

    }
}