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

        protected override void Render() => game.SpriteBatch.Draw(texture,
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