using Microsoft.Xna.Framework;
using System;

namespace ScapeCore.Core.Batching.Events
{
    internal delegate void UpdateBatchEventHandler(object source, UpdateBatchEventArgs args);
    internal sealed class UpdateBatchEventArgs : EventArgs
    {
        private readonly string eventInfo;
        private readonly GameTime gameTime;
        public UpdateBatchEventArgs(GameTime time, string Text)
        {
            gameTime = time;
            eventInfo = Text;
        }
        public string GetInfo() => eventInfo;
        public GameTime GetTime() => gameTime;
    }
}
