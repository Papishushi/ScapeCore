﻿using System;

namespace ScapeCore.Core.Batching.Events
{
    internal delegate void LoadBatchEventHandler(object source, LoadBatchEventArgs args);
    internal sealed class LoadBatchEventArgs : EventArgs
    {
        private readonly string eventInfo;
        public LoadBatchEventArgs(string Text) => eventInfo = Text;
        public string GetInfo() => eventInfo;
    }
}
