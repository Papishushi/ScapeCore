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
 * SpriteRenderer.cs
 * SpriteRenderer inherits from the abstract Renderer class and
 * provides functionality for rendering 2D sprites.
 */

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Text;

namespace ScapeCore.Core.Engine.Components
{
    public sealed class SpriteRenderer : Renderer
    {
        public RectTransform rtransform;
        public SpriteEffects spriteEffects;
        public float depth;

        public SpriteRenderer() : base(new StringBuilder(nameof(SpriteRenderer)))
        {
            rtransform = new();
            depth = 0f;
        }
        public SpriteRenderer(string texture) : base(texture)
        {
            var size = new Point(this.texture?.Width ?? 0, this.texture?.Height ?? 0);
            var center = Point.Zero;
            rtransform = new RectTransform(new(center, size), Vector2.Zero, Vector2.One);
            depth = 0f;
        }
        public SpriteRenderer(Texture2D texture) : base(texture)
        {
            var size = new Point(texture.Width, texture.Height);
            var center = Point.Zero;
            rtransform = new RectTransform(new(center, size), Vector2.Zero, Vector2.One);
            depth = 0f;
        }
        public SpriteRenderer(string texture, RectTransform rtransform, SpriteEffects spriteEffects, float depth) : base(texture)
        {
            this.rtransform=rtransform;
            this.spriteEffects=spriteEffects;
            this.depth=depth;
        }
        public SpriteRenderer(Texture2D texture, RectTransform rtransform, SpriteEffects spriteEffects, float depth) : base(texture)
        {
            this.rtransform=rtransform;
            this.spriteEffects=spriteEffects;
            this.depth=depth;
        }

        protected override void Render() => Game?.SpriteBatch?.Draw(texture ?? new(Game.GraphicsDevice, texture!.Width, texture!.Height),
                                                                transform?.position ?? Vector2.Zero,
                                                                rtransform?.rectangle ?? new Rectangle(Point.Zero, new(100, 100)),
                                                                Color.White,
                                                                rtransform?.rotation.X ?? 0f,
                                                                new(texture!.Width / 2, texture!.Height / 2),
                                                                rtransform?.scale ?? Vector2.One,
                                                                spriteEffects,
                                                                depth);
    }
}