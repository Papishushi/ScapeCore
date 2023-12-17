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
            if (SceneManager.CurrentScene.TryGetTarget(out var scene))
                scene.MonoBehaviours.Remove(this);
            else
                Log.Warning("{Mo} wasn't correctly destroyed. There was a problem removing it from current scene.", nameof(MonoBehaviour));
            Game.OnUpdate -= UpdateWrapper;
            Game.OnStart -= StartWrapper;
        }

        protected abstract void Start();
        protected abstract void Update();

        private void StartWrapper(object source, StartBatchEventArgs args)
        {
            if (!isActive || _started) return;
            Start();
            _started = true;
        }
        private void UpdateWrapper(object source, UpdateBatchEventArgs args)
        {
            if (!isActive) return;
            _time = args.GetTime();
            Update();
        }
    }
}