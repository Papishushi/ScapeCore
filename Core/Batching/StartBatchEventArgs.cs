using Microsoft.Xna.Framework;
using System;

namespace ScapeCore.Core.Batching
{
    internal delegate void StartBatchEventHandler(object source, StartBatchEventArgs args);
    internal sealed class StartBatchEventArgs : EventArgs
    {
        private readonly string eventInfo;
        public StartBatchEventArgs(string Text) => eventInfo = Text;
        public string GetInfo() => eventInfo;
    }
}
