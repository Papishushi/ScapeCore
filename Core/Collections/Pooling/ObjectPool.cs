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

using ScapeCore.Core.Batching.Tools;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Linq;

namespace ScapeCore.Core.Collections.Pooling
{
    public sealed class ObjectPool : IDisposable
    {
        private readonly ConcurrentBag<DeeplyMutableType> _pooledObjects;
        private Func<DeeplyMutableType>? _objectGenerator;
        private bool _disposedValue;

        public ObjectPool(Func<DeeplyMutableType> objectGenerator)
        {
            _pooledObjects = new();
            _objectGenerator = objectGenerator;
        }

        private static DeeplyMutableType GetError()
        {
            Log.Warning("Object Pool item generator is null. Try setting up a generator.");
            return new(null);
        }

        public void ChangeGenerator(Func<DeeplyMutableType> generator) => _objectGenerator = generator;
        public DeeplyMutableType Get => _pooledObjects.TryTake(out var item) ? item : _objectGenerator?.Invoke() ?? GetError();
        public bool Contains(DeeplyMutableType item) => _pooledObjects.Contains(item);
        public void Return(DeeplyMutableType item) => _pooledObjects.Add(item);
        public void Clear()
        {
            _pooledObjects?.Clear();
            _objectGenerator = null;
        }

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
                _disposedValue=true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
