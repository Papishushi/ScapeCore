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
using ScapeCore.Core.Collections.Pooling;
using ScapeCore.Core.Engine;
using Serilog;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

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

        private readonly record struct GeneratorCompletionReference(Func<DeeplyMutableType, bool> Generator, TaskCompletionSource<DeeplyMutableType> CompletionReference);
        private readonly ConcurrentStack<GeneratorCompletionReference> _objectGeneratorCompletionReferences = new();

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
                if (!_objectGeneratorCompletionReferences.IsEmpty && _objectGeneratorCompletionReferences.TryPop(out var generatorCompletionReference))
                {
                    DeeplyMutableType deeplyMutable = new();
                    var b = generatorCompletionReference.Generator.Invoke(deeplyMutable);
                    if (b)
                        generatorCompletionReference.CompletionReference.SetResult(deeplyMutable);
                    else
                        Log.Error("Scene {name} encountered a problem while instantiating an invocation." +
                            " Stack wasn't able to pop the {tcs} for the current instantiation, but item was correctly instantiated.", name, typeof(TaskCompletionSource<DeeplyMutableType>));
                }
            }
            while (!_cancellationTokenSource.IsCancellationRequested);
        }

        private bool InstantiateTypeToDeeplyMutable(DeeplyMutableType value, Type? type)
        {
            if (type == null) return false;
            try
            {
                _typePools.TryAdd(type, new ObjectPool(() => new DeeplyMutableType(Activator.CreateInstance(type))));
                value.Value = _typePools[type].Get.Value;
            }
            catch (Exception ex)
            {
                Log.Error("Scene {name} encountered a problem while instantiating object of type {t}\t:\t{ex}", name, type, ex.Message);
                return false;
            }
            return true;
        }

        private async Task<DeeplyMutableType> PushInstantiation(Func<DeeplyMutableType, bool> generator)
        {
            var tcs = new TaskCompletionSource<DeeplyMutableType>();
            _objectGeneratorCompletionReferences.Push(new(generator, tcs));
            return await tcs.Task;
        }

        private void AddToTrackers(dynamic result)
        {
            if (result == null) return;
            _monoBehaviours.Add(result);
            _gameObjects.Add(result.gameObject!);
        }

        public async Task<T?> AddToSceneAsync<T>() where T : MonoBehaviour
        {
            bool Instantiate(DeeplyMutableType value) => InstantiateTypeToDeeplyMutable(value, typeof(T));
            var result = (await PushInstantiation(Instantiate)).Value;
            AddToTrackers(result);
            return result;
        }

        public async Task<List<T>> AddToSceneMultipleAsync<T>(int cuantity) where T : MonoBehaviour
        {
            bool Instantiate(DeeplyMutableType value) => InstantiateTypeToDeeplyMutable(value, typeof(T));
            List<T> results = new();
            await Task.Run(() =>
            {
                Parallel.For(0, cuantity, async (int i) =>
                {
                    var result = (await PushInstantiation(Instantiate)).Value;
                    AddToTrackers(result);
                    results.Add(result);
                });
            });
            return results;
        }

        public async Task<DeeplyMutableType> AddToSceneAsync(Type type)
        {
            bool Instantiate(DeeplyMutableType value) => InstantiateTypeToDeeplyMutable(value, type);
            var result = await PushInstantiation(Instantiate);
            AddToTrackers(result.Value);
            return result;
        }

        public T? AddToScene<T>() where T : MonoBehaviour
        {
            bool Instantiate(DeeplyMutableType value) => InstantiateTypeToDeeplyMutable(value, typeof(T));
            var t = PushInstantiation(Instantiate);
            t.Wait();
            var result = t.Result.Value;
            AddToTrackers(result);
            return result;
        }

        public DeeplyMutableType AddToScene(Type type)
        {
            bool Instantiate(DeeplyMutableType value) => InstantiateTypeToDeeplyMutable(value, type);
            var t = PushInstantiation(Instantiate);
            t.Wait();
            var result = t.Result;
            AddToTrackers(result.Value);
            return result;
        }

        public void RemoveFromScene(MonoBehaviour monoBehaviour)
        {
            if (_monoBehaviours.Contains(monoBehaviour))
            {
                _monoBehaviours.Remove(monoBehaviour);
                monoBehaviour.Destroy();
                if (_typePools.ContainsKey(monoBehaviour.GetType()))
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
                    foreach (var typePool in _typePools)
                        typePool.Value.Dispose();
                    _typePools.Clear();
                    _cancellationTokenSource.Cancel();
                    _instantiateInvocations.Wait();
                    _instantiateInvocations.Dispose();
                    _monoBehaviours.Clear();
                    _gameObjects.Clear();
                    foreach (var iCS in _objectGeneratorCompletionReferences)
                        iCS.CompletionReference.SetCanceled();
                    _objectGeneratorCompletionReferences.Clear();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue=true;
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