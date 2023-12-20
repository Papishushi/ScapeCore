/*
 * -*- encoding: utf-8 with BOM -*-
 * .▄▄ ·  ▄▄·  ▄▄▄·  ▄▄▄·▄▄▄ .     ▄▄·       ▄▄▄  ▄▄▄ .
 * ▐█ ▀. ▐█ ▌▪▐█ ▀█ ▐█ ▄█▀▄.▀·    ▐█ ▌▪▪     ▀▄ █·▀▄.▀·
 * ▄▀▀▀█▄██ ▄▄▄█▀▀█  ██▀·▐▀▀▪▄    ██ ▄▄ ▄█▀▄ ▐▀▀▄ ▐▀▀▪▄
 * ▐█▄▪▐█▐███▌▐█ ▪▐▌▐█▪·•▐█▄▄▌    ▐███▌▐█▌.▐▌▐█•█▌▐█▄▄▌
 *  ▀▀▀▀ ·▀▀▀  ▀  ▀ .▀    ▀▀▀     ·▀▀▀  ▀█▄▀▪.▀  ▀ ▀▀▀ 
 * https://github.com/Papishushi/ScapeCore
 * 
 * Copyright (c) 2023 Daniel Molinero Lucas
 * This file is subject to the terms and conditions defined in
 * file 'LICENSE.txt', which is part of this source code package.
 * 
 * ObjectPool.cs
 * A typeless collection used for pooling objects and reusing them.
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Linq;

namespace ScapeCore.Core.Batching.Tools
{
    public sealed class ObjectPool : IDisposable
    {
        private readonly ConcurrentBag<DeeplyMutableType> _pooledObjects;
        private readonly Func<DeeplyMutableType> _objectGenerator;
        private bool _disposedValue;

        public ObjectPool(Func<DeeplyMutableType> objectGenerator)
        {
            _pooledObjects = new();
            _objectGenerator = objectGenerator;
        }

        public DeeplyMutableType Get => _pooledObjects.TryTake(out var item) ? item : _objectGenerator();

        public bool Contains(DeeplyMutableType item) => _pooledObjects.Contains(item);

        public void Return(DeeplyMutableType item) => _pooledObjects.Add(item);

        private void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    foreach (var item in _pooledObjects)
                    {
                        if (item.Value?.GetType() is IDisposable)
                            item.Value?.Dispose();
                        item.Value = null;
                    }
                    _pooledObjects.Clear();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                _disposedValue=true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~ObjectPool()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
