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
 * MonoBehaviour.cs
 * MonoBehaviour is an abstract class representing a custom
 * updateable behaviour in the ScapeCore game engine. It provides
 * functionality for handling the creation, destruction, start,
 * and update events of a game object.
 */

using Baksteen.Extensions.DeepCopy;
using Microsoft.Xna.Framework;
using ScapeCore.Core.Batching.Events;
using ScapeCore.Core.Engine.Components;
using ScapeCore.Core.SceneManagement;
using ScapeCore.Targets;
using Serilog;
using System.Linq;

namespace ScapeCore.Core.Engine
{
    public abstract class MonoBehaviour : Behaviour, IEntityComponentModel
    {
        private bool _started = false;
        private GameTime? _time;

        public GameTime? Time { get => _time; }
        public GameObject? gameObject { get; set; }
        public Transform? transform { get => gameObject?.transform; }

        public MonoBehaviour() : base(nameof(MonoBehaviour)) => gameObject = new(this);

        public MonoBehaviour(params Behaviour[] behaviours) : base(nameof(MonoBehaviour))
        {
            var l = behaviours.ToList();
            l.Add(this);
            gameObject = new(l.ToArray());
        }

        public static T? Clone<T>(T monoBehaviour) where T : MonoBehaviour => DeepCopyObjectExtensions.DeepCopy(monoBehaviour);

        protected override void OnCreate()
        {
            if (Game == null)
            {
                Log.Warning("{Mo} wasn't correctly created. {LLAM} instance is GCed.", nameof(MonoBehaviour), typeof(LLAM).FullName);
                return;
            }
            if (SceneManager.CurrentScene.TryGetTarget(out var scene))
                scene.MonoBehaviours.Add(this);
            else
            {
                var i = SceneManager.AddScene(new("Scene", 0));
                if (i == -1)
                {
                    Log.Warning("{Mo} wasn't correctly created. There was a problem adding it to current scene or creating a new one.", nameof(MonoBehaviour));
                    return;
                }
                var currentScene = SceneManager.Get(i);
                currentScene!.MonoBehaviours.Add(this);
            }
            Game.OnStart += StartWrapper;
            Game.OnUpdate += UpdateWrapper;
        }

        protected override void OnDestroy()
        {
            if (Game == null)
            {
                Log.Warning("{Mo} wasn't correctly destroyed. {LLAM} instance is GCed.", nameof(MonoBehaviour), typeof(LLAM).FullName);
                return;
            }
            gameObject = null;
            Game.OnUpdate -= UpdateWrapper;
            Game.OnStart -= StartWrapper;
        }

        protected abstract void Start();
        protected abstract void Update();

        private void StartWrapper(object source, StartBatchEventArgs args)
        {
            if (_started) return;
            if (gameObject == null) return;
            if (IsDestroyed || !IsActive || gameObject.IsDestroyed || !gameObject.IsActive) return;
            Start();
            _started = true;
        }
        private void UpdateWrapper(object source, UpdateBatchEventArgs args)
        {
            if (gameObject == null) return;
            if (IsDestroyed || !IsActive || gameObject.IsDestroyed || !gameObject.IsActive) return;
            _time = args.GetTime();
            Update();
        }
    }
}