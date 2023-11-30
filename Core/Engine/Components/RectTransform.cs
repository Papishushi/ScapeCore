using Microsoft.Xna.Framework;

namespace ScapeCore.Core.Engine.Components
{
    public sealed class RectTransform : Component
    {
        public Rectangle rectangle = new Rectangle();
        public Vector2 rotation = Vector2.Zero;
        public Vector2 scale = Vector2.One;

        public RectTransform()
        {
            rectangle = new(Point.Zero, Point.Zero);
            rotation = Vector2.Zero;
            scale = Vector2.One;
        }
        public RectTransform(Rectangle rectangle, Vector2 rotation, Vector2 scale)
        {
            this.rectangle=rectangle;
            this.rotation=rotation;
            this.scale=scale;
        }

        protected override void OnCreate()
        {

        }

        protected override void OnDestroy()
        {

        }

        protected override string Serialize()
        {
            throw new System.NotImplementedException();
        }
    }
}