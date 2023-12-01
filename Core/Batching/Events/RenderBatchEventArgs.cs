using Microsoft.Xna.Framework;
using System;

namespace ScapeCore.Core.Batching.Events
{
    internal delegate void RenderBatchEventHandler(object source, RenderBatchEventArgs args);
    internal class RenderBatchEventArgs : EventArgs
    {
        private readonly string eventInfo;
        private readonly GameTime gameTime;
        public RenderBatchEventArgs(GameTime time, string Text)
        {
            gameTime = time;
            eventInfo = Text;
        }
        public string GetInfo() => eventInfo;
        public GameTime GetTime() => gameTime;
    }
}
