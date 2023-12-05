﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Xna.Framework;
using ScapeCore.Core.Batching;
using ScapeCore.Core.Engine.Components;
using Baksteen.Extensions.DeepCopy;
using ScapeCore.Core.Batching.Events;
using Serilog;
using ProtoBuf;
using ProtoBuf.Meta;

namespace ScapeCore.Core.Engine
{
    public abstract class MonoBehaviour : Behaviour
    {
        private bool _started = false;
        public GameObject gameObject;
        private GameTime _time;
        public GameTime Time { get => _time; }

        [SuppressMessage("Style", "IDE1006:Naming Styles",
                         Justification = "<In this way it does not match class name and keep it simple and descriptible.>")]
        public Transform transform { get => gameObject.transform; }

        ~MonoBehaviour() => OnDestroy();
        public MonoBehaviour() : base(nameof(MonoBehaviour)) => gameObject = new(this);

        public MonoBehaviour(params Behaviour[] behaviours) : base(nameof(MonoBehaviour))
        {
            var l = behaviours.ToList();
            l.Add(this);
            gameObject = new(l.ToArray());
        }

        public static T Clone<T>(T monoBehaviour) where T : MonoBehaviour => DeepCopyObjectExtensions.DeepCopy(monoBehaviour);

        protected override void OnCreate()
        {
            Game.MonoBehaviours.Add(this);
            Game.OnStart += StartWrapper;
            Game.OnUpdate += UpdateWrapper;
        }

        protected override void OnDestroy()
        {
            Game.MonoBehaviours.Remove(this);
            Game.OnUpdate -= UpdateWrapper;
            Game.OnStart -= StartWrapper;
        }

        protected abstract void Start();
        protected abstract void Update();

        private void StartWrapper(object source, StartBatchEventArgs args)
        {
            if (!isActive) return;
            Log.Verbose("{{{@source}}} {@args}", source.GetHashCode(), args.GetInfo());
            if (_started) return;
            Start();
            _started = true;
        }
        private void UpdateWrapper(object source, UpdateBatchEventArgs args)
        {
            if (!isActive) return;
            _time = args.GetTime();
            Log.Verbose("{{{@source}}} {@args}", source.GetHashCode(), args.GetInfo());
            Update();
        }
    }
}