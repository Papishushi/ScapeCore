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
 * Renderer.cs
 * This class provides an abstraction for handling textures
 * and rendering logic in derived classes. It can be attached
 * to a GameObject.
 */

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ScapeCore.Core.Batching.Events;
using ScapeCore.Core.Batching.Resources;
using ScapeCore.Targets;
using Serilog;
using System.Text;

namespace ScapeCore.Core.Engine.Components
{
    public abstract class Renderer : Component
    {
        public Texture2D? texture;
        private GameTime? _time;

        public GameTime? Time { get => _time; }

        public Renderer() : base(nameof(Renderer)) => texture = null;
        public Renderer(Texture2D texture) : base(nameof(Renderer)) => this.texture= texture;
        public Renderer(string textureName) : base(nameof(Renderer)) => texture = ResourceManager.GetResource<Texture2D>(textureName).Value;
        protected Renderer(StringBuilder name) : base(name.ToString()) => texture = null;


        protected override void OnCreate() => Game!.OnRender += RenderWrapper;
        protected override void OnDestroy()
        {
            base.OnDestroy();
            texture = null;
            _time = null;
            Game!.OnRender -= RenderWrapper;
        }

        protected abstract void Render();
        private void RenderWrapper(object source, RenderBatchEventArgs args)
        {
            if (gameObject == null) return;
            if (IsDestroyed || !IsActive || gameObject.IsDestroyed || !gameObject.IsActive) return;
            _time = args.GetTime();
            Render();
        }
    }
}