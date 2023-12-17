/*
 * -*- encoding: utf-8 with BOM -*-
 * .▄▄ ·  ▄▄·  ▄▄▄·  ▄▄▄·▄▄▄ .     ▄▄·       ▄▄▄  ▄▄▄ .
 * ▐█ ▀. ▐█ ▌▪▐█ ▀█ ▐█ ▄█▀▄.▀·    ▐█ ▌▪▪     ▀▄ █·▀▄.▀·
 * ▄▀▀▀█▄██ ▄▄▄█▀▀█  ██▀·▐▀▀▪▄    ██ ▄▄ ▄█▀▄ ▐▀▀▄ ▐▀▀▪▄
 * ▐█▄▪▐█▐███▌▐█ ▪▐▌▐█▪·•▐█▄▄▌    ▐███▌▐█▌.▐▌▐█•█▌▐█▄▄▌
 *  ▀▀▀▀ ·▀▀▀  ▀  ▀ .▀    ▀▀▀     ·▀▀▀  ▀█▄▀▪.▀  ▀ ▀▀▀ 
 * https://github.com/Papishushi/ScapeCore
 * 
 * MIT License
 *
 * Copyright (c) 2023 Daniel Molinero Lucas
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace ScapeCore.Core.SceneManagement
{
    public static class SceneManager
    {
        private static readonly ConcurrentDictionary<int, Scene> _scenes = new();
        private static int _scenesCount = 0;
        private static int _currentSceneIndex = 0;
        public static WeakReference<Scene?> CurrentScene { get => new(_scenes.GetValueOrDefault(_currentSceneIndex)); }
        public static ImmutableList<Scene> Scenes { get => _scenes.Values.ToImmutableList(); }
        public static int Count { get => _scenesCount; }

        public static void SetCurrentScene(int sceneIndex) => _currentSceneIndex = sceneIndex;
        public static Scene? Get(int sceneId)
        {
            if (_scenes.TryGetValue(sceneId, out var scene))
                return scene;
            else
                Log.Error("Scene with ID {id} not found in the SceneManager", sceneId);
            return null;
        }
        public static int AddScene(Scene scene)
        {
            if (_scenesCount <= 0)
                _scenes.TryAdd(0, scene);
            else if (!_scenes.TryAdd(_scenes.Last().Key + 1, scene))
            {
                Log.Error("There was a problem whilst trying to add Scene {s} to the SceneManager", scene.name);
                return -1;
            }
            _scenesCount++;
            return _scenes.Last().Key;
        }
        public static int RemoveScene(int sceneId)
        {
            if (_scenesCount <= 0)
                return -1;
            if (!_scenes.TryRemove(sceneId, out var scene))
            {
                Log.Error("There was a problem whilst trying to remove scene {s} to the SceneManager", scene?.name);
                return -1;
            }
            _scenesCount--;
            return sceneId;
        }
    }
}