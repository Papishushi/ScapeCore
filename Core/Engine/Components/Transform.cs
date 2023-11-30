using Microsoft.Xna.Framework;

namespace ScapeCore.Core.Engine.Components
{
    public sealed class Transform : Component
    {
        public Vector2 position = Vector2.Zero;
        public Vector2 rotation = Vector2.Zero;
        public Vector2 scale = Vector2.Zero;

        public Transform() : base() { }
        public Transform(Vector2 position, Vector2 rotation, Vector2 scale) : base()
        {
            this.position=position;
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