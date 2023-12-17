/*
.▄▄ ·  ▄▄·  ▄▄▄·  ▄▄▄·▄▄▄ .     ▄▄·       ▄▄▄  ▄▄▄ .
▐█ ▀. ▐█ ▌▪▐█ ▀█ ▐█ ▄█▀▄.▀·    ▐█ ▌▪▪     ▀▄ █·▀▄.▀·
▄▀▀▀█▄██ ▄▄▄█▀▀█  ██▀·▐▀▀▪▄    ██ ▄▄ ▄█▀▄ ▐▀▀▄ ▐▀▀▪▄
▐█▄▪▐█▐███▌▐█ ▪▐▌▐█▪·•▐█▄▄▌    ▐███▌▐█▌.▐▌▐█•█▌▐█▄▄▌
 ▀▀▀▀ ·▀▀▀  ▀  ▀ .▀    ▀▀▀     ·▀▀▀  ▀█▄▀▪.▀  ▀ ▀▀▀ 
https://github.com/Papishushi/ScapeCore

MIT License

Copyright (c) 2023 Daniel Molinero Lucas

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

Scene.cs
Represents an environment containing a collection of active behaviours, exposes
multiple methods to manipulate the scene. This class is mainly used in the
Sceme Management system.
*/

using ScapeCore.Core.Batching.Tools;
using ScapeCore.Core.Engine;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ScapeCore.Core.SceneManagement
{
    public class Scene
    {
        public string name = "Scene";
        public int sceneIndex = 0;

        public readonly List<MonoBehaviour> MonoBehaviours = new();
        public readonly List<GameObject> GameObjects = new();

        private readonly ConcurrentQueue<Func<DeeplyMutableType, bool>> _invocations = new();
        private readonly ConcurrentStack<TaskCompletionSource<DeeplyMutableType>> _instantiationCompletionSources = new();

        public Scene() => Task.Run(InstantiateInvocations);

        public Scene(int sceneIndex) : this() => this.sceneIndex = sceneIndex;
        public Scene(string name) : this() => this.name = name;
        public Scene(string name, int sceneIndex) : this()
        {
            this.name = name;
            this.sceneIndex = sceneIndex;
        }

        private void InstantiateInvocations()
        {
            while (true)
                if (!_invocations.IsEmpty && _invocations.TryDequeue(out var invocation))
                {
                    DeeplyMutableType deeplyMutable = new();
                    var b = invocation?.Invoke(deeplyMutable);
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

        public async Task<T?> AddToSceneAsync<T>() where T : MonoBehaviour
        {
            bool Instantiate(DeeplyMutableType value)
            {
                try
                {
                    DeeplyMutable<T> deeplyMutable = new((T?)Activator.CreateInstance(typeof(T)));
                    value.Value = deeplyMutable.Value;
                }
                catch (Exception ex)
                {
                    Log.Error("Scene {name} encountered a problem while instantiating object of type {t}\t:\t{ex}", name, typeof(T), ex.Message);
                    return false;
                }
                return true;
            }

            _invocations.Enqueue(Instantiate);
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
                    DeeplyMutableType deeplyMutable = new(Activator.CreateInstance(type));
                    value.Value = deeplyMutable.Value;
                }
                catch (Exception ex)
                {
                    Log.Error("Scene {name} encountered a problem while instantiating object of type {t}\t:\t{ex}", name, type, ex.Message);
                    return false;
                }
                return true;
            }

            _invocations.Enqueue(Instantiate);
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
                    DeeplyMutable<T> deeplyMutable = new((T?)Activator.CreateInstance(typeof(T)));
                    value.Value = deeplyMutable.Value;
                }
                catch (Exception ex)
                {
                    Log.Error("Scene {name} encountered a problem while instantiating object of type {t}\t:\t{ex}", name, typeof(T), ex.Message);
                    return false;
                }
                return true;
            }

            _invocations.Enqueue(Instantiate);
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
                    DeeplyMutableType deeplyMutable = new(Activator.CreateInstance(type));
                    value.Value = deeplyMutable.Value;
                }
                catch (Exception ex)
                {
                    Log.Error("Scene {name} encountered a problem while instantiating object of type {t}\t:\t{ex}", name, type, ex.Message);
                    return false;
                }
                return true;
            }

            _invocations.Enqueue(Instantiate);
            var tcs = new TaskCompletionSource<DeeplyMutableType>();
            _instantiationCompletionSources.Push(tcs);
            var result = tcs.Task;

            result.Wait();

            return result.Result;
        }

        public void RemoveFromScene(MonoBehaviour monoBehaviour)
        {
            if (MonoBehaviours.Contains(monoBehaviour))
            {

            }
            else
                Log.Warning("Cant remove a MonoBehaviour that is not contained on the scene.");
        }
        public void RemoveFromScene(GameObject gameObject)
        {
            if (GameObjects.Contains(gameObject))
            {

            }
            else
                Log.Warning("Cant remove a GameObject that is not contained on the scene.");
        }

        public void Find<T>(T monoBehaviour) where T : Behaviour
        {

        }

    }
}