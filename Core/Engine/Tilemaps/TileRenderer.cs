﻿/*
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

        public TileRenderer() : base((Texture2D)null)
        {
            rtransform = new();
            depth = 0f;
        }
        public TileRenderer(string texture) : base(texture)
        {
            var size = new Point(this.texture.Width, this.texture.Height);
            var center = Point.Zero;
            rtransform = new RectTransform(new(center, size), Vector2.Zero, Vector2.One);
            depth = 0f;
        }
        public TileRenderer(Texture2D texture) : base(texture)
        {
            var size = new Point(texture.Width, texture.Height);
            var center = Point.Zero;
            rtransform = new RectTransform(new(center, size), Vector2.Zero, Vector2.One);
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

        protected override void Render() => Game.SpriteBatch.Draw(texture,
                                                                gameObject.transform.position,
                                                                rtransform.rectangle,
                                                                Color.White,
                                                                rtransform.rotation.X,
                                                                new(texture.Width / 2, texture.Height / 2),
                                                                rtransform.scale,
                                                                spriteEffects,
                                                                depth);
    }
}