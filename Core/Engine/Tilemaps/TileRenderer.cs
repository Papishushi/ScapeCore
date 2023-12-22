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
 * TileRenderer.cs
 */

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ScapeCore.Core.Engine.Components;

namespace ScapeCore.Core.Engine.Tilemaps
{
    public sealed class TileRenderer : Renderer
    {
        public RectTransform rtransform;
        public SpriteEffects spriteEffects;
        public float depth;

        public TileRenderer() : base()
        {
            rtransform = new();
            depth = 0f;
        }
        public TileRenderer(string texture) : base(texture)
        {
            var size = new Point(this.texture?.Width ?? 0, this.texture?.Height ?? 0);
            var center = Point.Zero;
            rtransform = new RectTransform(size, center, Vector2.Zero, Vector2.Zero, Vector2.One);
            depth = 0f;
        }
        public TileRenderer(Texture2D texture) : base(texture)
        {
            var size = new Point(texture.Width, texture.Height);
            var center = Point.Zero;
            rtransform = new RectTransform(size, center, Vector2.Zero, Vector2.Zero, Vector2.One);
            depth = 0f;
        }
        public TileRenderer(string texture, RectTransform rtransform, SpriteEffects spriteEffects, float depth) : base(texture)
        {
            this.rtransform=rtransform;
            this.spriteEffects=spriteEffects;
            this.depth=depth;
        }
        public TileRenderer(Texture2D texture, RectTransform rtransform, SpriteEffects spriteEffects, float depth) : base(texture)
        {
            this.rtransform=rtransform;
            this.spriteEffects=spriteEffects;
            this.depth=depth;
        }

        protected override void Render() => Game?.SpriteBatch?.Draw(texture,
                                                                gameObject?.transform?.Position ?? Vector2.Zero,
                                                                rtransform.Rectangle,
                                                                Color.White,
                                                                rtransform.Rotation.X,
                                                                new Vector2((texture?.Width ?? 0) * 0.5f, (texture?.Height ?? 0) * 0.5f),
                                                                rtransform.Scale,
                                                                spriteEffects,
                                                                depth);
    }
}