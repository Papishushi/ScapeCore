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
 * GameObject.cs
 * Represents a fundamental entity within a scene that can have
 * behaviours attached to it.
 */

using ScapeCore.Core.Engine.Components;
using ScapeCore.Core.SceneManagement;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace ScapeCore.Core.Engine
{
    public sealed class GameObject : Behaviour
    {
        public Transform? transform;
        public string tag;
        private readonly List<Behaviour> behaviours;

        private static readonly List<string> tagList = new();
        public static ImmutableList<string> TagList { get => tagList.ToImmutableList(); }

        public GameObject? parent = null;
        public readonly List<GameObject> children = new();

        public GameObject() : base(nameof(GameObject))
        {
            transform = new();
            tag = string.Empty;
            behaviours = new()
            {
                transform
            };
        }
        public GameObject(string name) : base(name)
        {
            transform=new();
            tag = string.Empty;
            behaviours=new()
            {
                transform
            };
        }
        public GameObject(params Behaviour[] behaviours) : this()
        {
            foreach (Behaviour behaviour in behaviours)
                this.behaviours.Add(behaviour);
        }
        public GameObject(string name, params Behaviour[] behaviours) : this(name)
        {
            foreach (Behaviour behaviour in behaviours)
                this.behaviours.Add(behaviour);
        }
        /// <summary>
        /// The best contraceptive for old people is nudity.
        /// </summary>
        /// <exception cref="System.NullReferenceException"></exception>
        private void BehavioursNullException()
        {
            if (behaviours == null) throw new System.NullReferenceException($"{nameof(behaviours)} is null");
        }

        public T? GetBehaviour<T>() where T : Behaviour
        {
            try
            {
                BehavioursNullException();
            }
            catch (NullReferenceException nRE)
            {
                Log.Error($"Failed to get behaviour on GameObject {name} {{{Id}}}\t{nRE.Message}");
                throw;
            }
            foreach (Behaviour behaviour in behaviours)
                if (behaviour.GetType() == typeof(T)) return (T)behaviour;
            return null;
        }

        public IEnumerator<T> GetBehaviours<T>() where T : Behaviour
        {
            try
            {
                BehavioursNullException();
            }
            catch (NullReferenceException nRE)
            {
                Log.Error($"Failed to get behaviour on GameObject {name} {{{Id}}}\t{nRE.Message}");
                throw;
            }
            return behaviours.Where(x => x.GetType() == typeof(T)).Cast<T>().GetEnumerator();
        }

        public T AddBehaviour<T>() where T : Behaviour, new()
        {
            try
            {
                BehavioursNullException();
            }
            catch (NullReferenceException nRE)
            {
                Log.Error($"Failed to add behaviour on GameObject {name} {{{Id}}}\t{nRE.Message}");
                throw;
            }
            var temp = new T();
            behaviours.Add(temp);
            if (typeof(IEntityComponentModel).IsAssignableFrom(temp.GetType()))
                ((IEntityComponentModel)temp).gameObject = this;
            return temp;
        }
        public T? AddBehaviour<T>(T behaviour) where T : Behaviour
        {
            try
            {
                BehavioursNullException();
            }
            catch (NullReferenceException nRE)
            {
                Log.Error($"Failed to add behaviour on GameObject {name} {{{Id}}}\t{nRE.Message}");
                throw;
            }
            if (behaviour == null) return null;
            behaviours.Add(behaviour);
            if (typeof(IEntityComponentModel).IsAssignableFrom(behaviour.GetType()))
                ((IEntityComponentModel)behaviour).gameObject = this;
            return behaviour;
        }

        public IEnumerator<T>? AddBehaviours<T>(params T[] behaviours) where T : Behaviour
        {
            try
            {
                BehavioursNullException();
            }
            catch (NullReferenceException nRE)
            {
                Log.Error($"Failed to add behaviour on GameObject {name} {{{Id}}}\t{nRE.Message}");
                throw;
            }
            if (behaviours == null) return null;
            var l = new List<T>();
            foreach (T behaviour in behaviours.Cast<T>())
            {
                if (behaviour == null) continue;
                this.behaviours.Add(behaviour);
                if (typeof(IEntityComponentModel).IsAssignableFrom(behaviour.GetType()))
                    ((IEntityComponentModel)behaviour).gameObject = this;
                l.Add(behaviour);
            }
            return l.GetEnumerator();
        }

        public T? RemoveBehaviour<T>() where T : Behaviour
        {
            try
            {
                BehavioursNullException();
            }
            catch (NullReferenceException nRE)
            {
                Log.Error($"Failed to remove behaviour on GameObject {name} {{{Id}}}\t{nRE.Message}");
                throw;
            }
            T? temp = behaviours.Find(x => x.GetType() == typeof(T))?.To<T>();
            if (temp == null) return null;
            behaviours.Remove(temp);
            if (typeof(IEntityComponentModel).IsAssignableFrom(temp.GetType()))
                ((IEntityComponentModel)temp).gameObject = null;
            return temp;
        }

        public T? RemoveBehaviour<T>(T behaviour) where T : Behaviour
        {
            try
            {
                BehavioursNullException();
            }
            catch (NullReferenceException nRE)
            {
                Log.Error($"Failed to remove behaviour on GameObject {name} {{{Id}}}\t{nRE.Message}");
                throw;
            }
            if (behaviour == null) return null;
            behaviours.Remove(behaviour);
            if (typeof(IEntityComponentModel).IsAssignableFrom(behaviour.GetType()))
                ((IEntityComponentModel)behaviour).gameObject = null;
            return behaviour;
        }

        public IEnumerator<T> RemoveBehaviours<T>() where T : Behaviour
        {
            try
            {
                BehavioursNullException();
            }
            catch (NullReferenceException nRE)
            {
                Log.Error($"Failed to remove behaviour on GameObject {name} {{{Id}}}\t{nRE.Message}");
                throw;
            }
            var l = new List<T>();
            foreach (T behaviour in behaviours.Cast<T>())
            {
                if (behaviour == null) continue;
                behaviours.Remove(behaviour);
                if (typeof(IEntityComponentModel).IsAssignableFrom(behaviour.GetType()))
                    ((IEntityComponentModel)behaviour).gameObject = null;
                l.Add(behaviour);
            }
            return l.GetEnumerator();
        }

        public IEnumerator<T> RemoveBehaviours<T>(params T[] behaviours) where T : Behaviour
        {
            try
            {
                BehavioursNullException();
            }
            catch (NullReferenceException nRE)
            {
                Log.Error($"Failed to remove behaviour on GameObject {name} {{{Id}}}\t{nRE.Message}");
                throw;
            }
            var l = new List<T>();
            foreach (T behaviour in behaviours.Cast<T>())
            {
                if (behaviour == null) continue;
                if (!behaviours.Contains(behaviour)) continue;
                this.behaviours.Remove(behaviour);
                if (typeof(IEntityComponentModel).IsAssignableFrom(behaviour.GetType()))
                    ((IEntityComponentModel)behaviour).gameObject = null;
                l.Add(behaviour);
            }
            return l.GetEnumerator();
        }

        protected override void OnCreate()
        {

        }

        protected override void OnDestroy()
        {
            foreach (var behaviour in behaviours)
                behaviour.Destroy();
            transform = null;
        }

        public static GameObject? FindGameObjectWithTag(string tag)
        {
            var b = SceneManager.CurrentScene.TryGetTarget(out var scene);
            if (!b) return null;
            var castedList = scene?.GameObjects as List<GameObject>;
            return castedList?.Find(x => x.tag == tag);
        }
        public static IEnumerator<GameObject>? FindGameObjectsWithTag(string tag)
        {
            var b = SceneManager.CurrentScene.TryGetTarget(out var scene);
            if (!b) return null;
            var castedList = scene?.GameObjects as List<GameObject>;
            return castedList?.FindAll(x => x.tag == tag).GetEnumerator();
        }

    }
}