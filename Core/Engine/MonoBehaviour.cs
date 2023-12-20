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
 */

using Baksteen.Extensions.DeepCopy;
using Microsoft.Xna.Framework;
using ScapeCore.Core.Batching.Events;
using ScapeCore.Core.Engine.Components;
using ScapeCore.Core.SceneManagement;
using ScapeCore.Targets;
using Serilog;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace ScapeCore.Core.Engine
{
    public abstract class MonoBehaviour : Behaviour, IEntityComponentModel
    {
        private bool _started = false;
        public GameObject? gameObject { get; set; }
        private GameTime? _time;

        public GameTime? Time { get => _time; }

        [SuppressMessage("Style", "IDE1006:Naming Styles",
                         Justification = "<In this way it does not match class name and keep it simple and descriptible.>")]
        public Transform? transform { get => gameObject?.transform; }

        ~MonoBehaviour() => OnDestroy();
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
            if (IsDestroyed || !IsActive || _started) return;
            Start();
            _started = true;
        }
        private void UpdateWrapper(object source, UpdateBatchEventArgs args)
        {
            if (IsDestroyed || !IsActive) return;
            _time = args.GetTime();
            Update();
        }
    }
}