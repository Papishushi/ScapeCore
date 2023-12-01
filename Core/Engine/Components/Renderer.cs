using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ScapeCore.Core.Batching;
using ScapeCore.Core.Batching.Events;
using ScapeCore.Core.Batching.Resources;
using System;
using System.Drawing;
using System.Text;

namespace ScapeCore.Core.Engine.Components
{
    public abstract class Renderer : Component
    {
        public Texture2D texture;
        private GameTime _time;

        public GameTime Time { get => _time; }

        public Renderer() : base(nameof(Renderer)) => texture = null; 
        public Renderer(Texture2D texture) : base(nameof(Renderer)) => this.texture=texture;
        public Renderer(string textureName) : base(nameof(Renderer)) => texture = ResourceManager.GetResource<Texture2D>(textureName);
        protected Renderer(StringBuilder name) : base(name.ToString()) => texture = null;


        protected override void OnCreate() => game.OnRender += RenderWrapper;
        protected override void OnDestroy() => game.OnRender -= RenderWrapper;

        protected override string Serialize()
        {
            throw new System.NotImplementedException();
        }

        protected abstract void Render();
        private void RenderWrapper(object source, RenderBatchEventArgs args)
        {
            _time = args.GetTime();
            Console.WriteLine($"{source.GetHashCode()} {args.GetInfo()}");
            Render();
        }
    }
}