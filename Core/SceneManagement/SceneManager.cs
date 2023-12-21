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
 * SceneManager.cs
 * The SceneManager is responsible for managing a collection of
 * active Scene instances and provides methods to manipulate the
 * scenes. It is a crucial component used in the Scene Management
 * system facilitating the manipulation and organization of
 * scenes within the application.
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
        public static void Clear()
        {
            foreach (var scene in _scenes)
                scene.Value.Dispose();
            _scenes.Clear();
        }
    }
}