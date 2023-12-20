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
 * Scene.cs
 * Represents an environment containing a collection of active behaviours, exposes
 * multiple methods to manipulate the scene. This class is mainly used in the
 * Sceme Management system.
 */

using ScapeCore.Core.Batching.Tools;
using ScapeCore.Core.Engine;
using Serilog;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ScapeCore.Core.SceneManagement
{
    public class Scene : IDisposable
    {
        public string name = "Scene";
        public int sceneIndex = 0;
        private bool disposedValue;
        private readonly ConcurrentDictionary<Type, ObjectPool> _typePools = new();

        private readonly List<MonoBehaviour> _monoBehaviours = new();
        private readonly List<GameObject> _gameObjects = new();

        public IList MonoBehaviours { get => ArrayList.Synchronized(_monoBehaviours); }
        public IList GameObjects { get => ArrayList.Synchronized(_gameObjects); }

        private readonly ConcurrentQueue<Func<DeeplyMutableType, bool>> _objectGenerators = new();
        private readonly ConcurrentStack<TaskCompletionSource<DeeplyMutableType>> _instantiationCompletionSources = new();

        private readonly Task _instantiateInvocations;
        private readonly CancellationTokenSource _cancellationTokenSource = new();

        public Scene() => _instantiateInvocations = Task.Run(InstantiateInvocations);
        public Scene(int sceneIndex) : this() => this.sceneIndex = sceneIndex;
        public Scene(string name) : this() => this.name = name;
        public Scene(string name, int sceneIndex) : this()
        {
            this.name = name;
            this.sceneIndex = sceneIndex;
        }

        private void InstantiateInvocations()
        {
            do
            {
                if (!_objectGenerators.IsEmpty && _objectGenerators.TryDequeue(out var generator))
                {
                    DeeplyMutableType deeplyMutable = new();
                    var b = generator?.Invoke(deeplyMutable);
                    if (b ?? false)
                    {
                        if (_instantiationCompletionSources?.TryPop(out var tcs) ?? false)
                            tcs.SetResult(deeplyMutable);
                        else
                            Log.Error("Scene {name} encountered a problem while instantiating an invocation." +
                                " Stack wasn't able to pop the {tcs} for the current instantiation, but item was correctly instantiated.", name, typeof(TaskCompletionSource<DeeplyMutableType>));
                    }
                }
            }
            while (!_cancellationTokenSource.IsCancellationRequested);
               
        }

        public async Task<T?> AddToSceneAsync<T>() where T : MonoBehaviour
        {
            bool Instantiate(DeeplyMutableType value)
            {
                try
                {
                    _typePools.TryAdd(typeof(T), new ObjectPool(() => new DeeplyMutable<T>((T?)Activator.CreateInstance(typeof(T)))));
                    value.Value = _typePools[typeof(T)].Get.Value;
                }
                catch (Exception ex)
                {
                    Log.Error("Scene {name} encountered a problem while instantiating object of type {t}\t:\t{ex}", name, typeof(T), ex.Message);
                    return false;
                }
                return true;
            }

            _objectGenerators.Enqueue(Instantiate);
            var tcs = new TaskCompletionSource<DeeplyMutableType>();
            _instantiationCompletionSources.Push(tcs);
            var result = await tcs.Task;

            return (T?)result.Value;
        }

        public async Task<DeeplyMutableType> AddToSceneAsync(Type type)
        {
            bool Instantiate(DeeplyMutableType value)
            {
                try
                {
                    _typePools.TryAdd(type, new ObjectPool(() => new(Activator.CreateInstance(type))));
                    value.Value = _typePools[type].Get.Value;
                }
                catch (Exception ex)
                {
                    Log.Error("Scene {name} encountered a problem while instantiating object of type {t}\t:\t{ex}", name, type, ex.Message);
                    return false;
                }
                return true;
            }

            _objectGenerators.Enqueue(Instantiate);
            var tcs = new TaskCompletionSource<DeeplyMutableType>();
            _instantiationCompletionSources.Push(tcs);
            var result = await tcs.Task;

            return result;
        }

        public T? AddToScene<T>() where T : MonoBehaviour
        {
            bool Instantiate(DeeplyMutableType value)
            {
                try
                {
                    _typePools.TryAdd(typeof(T), new ObjectPool(() => new DeeplyMutable<T>((T?)Activator.CreateInstance(typeof(T)))));
                    value.Value = _typePools[typeof(T)].Get.Value;
                }
                catch (Exception ex)
                {
                    Log.Error("Scene {name} encountered a problem while instantiating object of type {t}\t:\t{ex}", name, typeof(T), ex.Message);
                    return false;
                }
                return true;
            }

            _objectGenerators.Enqueue(Instantiate);
            var tcs = new TaskCompletionSource<DeeplyMutableType>();
            _instantiationCompletionSources.Push(tcs);
            var result = tcs.Task;

            result.Wait();

            return result.Result.Value;
        }

        public DeeplyMutableType AddToScene(Type type)
        {
            bool Instantiate(DeeplyMutableType value)
            {
                try
                {
                    _typePools.TryAdd(type, new ObjectPool(() => new(Activator.CreateInstance(type))));
                    value.Value = _typePools[type].Get.Value;
                }
                catch (Exception ex)
                {
                    Log.Error("Scene {name} encountered a problem while instantiating object of type {t}\t:\t{ex}", name, type, ex.Message);
                    return false;
                }
                return true;
            }

            _objectGenerators.Enqueue(Instantiate);
            var tcs = new TaskCompletionSource<DeeplyMutableType>();
            _instantiationCompletionSources.Push(tcs);
            var result = tcs.Task;

            result.Wait();

            return result.Result;
        }

        public void RemoveFromScene(MonoBehaviour monoBehaviour)
        {
            if (_monoBehaviours.Contains(monoBehaviour))
            {
                _monoBehaviours.Remove(monoBehaviour);
                monoBehaviour.Destroy();
                _typePools[monoBehaviour.GetType()].Return(new(monoBehaviour));
            }
            else
                Log.Warning("Cant remove a MonoBehaviour that is not contained on the scene.");
        }
        public void RemoveFromScene(GameObject gameObject)
        {
            if (_gameObjects.Contains(gameObject))
            {
                _gameObjects.Remove(gameObject);
                gameObject.Destroy();
                if (_typePools.ContainsKey(gameObject.GetType()))
                    _typePools[gameObject.GetType()].Return(new(gameObject));
            }
            else
                Log.Warning("Cant remove a GameObject that is not contained on the scene.");
        }

        public void Find<T>(T monoBehaviour) where T : Behaviour
        {

        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    foreach(var typePool in _typePools)
                        typePool.Value.Dispose();
                    _typePools.Clear();
                    _cancellationTokenSource.Cancel();
                    _instantiateInvocations.Wait();
                    _instantiateInvocations.Dispose();
                    _monoBehaviours.Clear();
                    _gameObjects.Clear();
                    _objectGenerators.Clear();
                    foreach(var iCS in _instantiationCompletionSources)
                        iCS.SetCanceled();
                    _instantiationCompletionSources.Clear();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue=true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~Scene()
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