using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ScapeCore.Core.Batching.Events;
using ScapeCore.Core.Batching.Resources;
using Serilog;
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
        public Renderer(string textureName) : base(nameof(Renderer)) => texture = ResourceManager.GetResource<Texture2D>(textureName).Value;
        protected Renderer(StringBuilder name) : base(name.ToString()) => texture = null;


        protected override void OnCreate() => Game.OnRender += RenderWrapper;
        protected override void OnDestroy() => Game.OnRender -= RenderWrapper;

        protected abstract void Render();
        private void RenderWrapper(object source, RenderBatchEventArgs args)
        {
            _time = args.GetTime();
            Log.Verbose("{{{@source}}} {@args}", source.GetHashCode(), args.GetInfo());
            Render();
        }
    }
}