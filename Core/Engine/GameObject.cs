using ScapeCore.Core.Engine.Components;
using ScapeCore.Core.SceneManagement;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;

namespace ScapeCore.Core.Engine
{
    public sealed class GameObject : Behaviour
    {
        public Transform transform;
        public string tag;
        private readonly List<Behaviour> behaviours;

        private static readonly List<string> tagList = new();
        public static ImmutableList<string> TagList { get => tagList.ToImmutableList(); }

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
        public GameObject(params Behaviour[] behaviours) : base(nameof(GameObject))
        {
            transform=new();
            tag = string.Empty;
            this.behaviours=new()
            {
                transform
            };
            foreach (Behaviour behaviour in behaviours)
                this.behaviours.Add(behaviour);
        }
        public GameObject(string name, params Behaviour[] behaviours) : base(name)
        {
            transform=new();
            tag = string.Empty;
            this.behaviours=new()
            {
                transform
            };
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
            if (typeof(Component).IsAssignableFrom(temp.GetType()))
                temp.To<Component>().gameObject = this;
            if (typeof(MonoBehaviour).IsAssignableFrom(temp.GetType()))
                temp.To<MonoBehaviour>().gameObject = this;
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
            if (typeof(Component).IsAssignableFrom(behaviour.GetType()))
                behaviour.To<Component>().gameObject = this;
            if (typeof(MonoBehaviour).IsAssignableFrom(behaviour.GetType()))
                behaviour.To<MonoBehaviour>().gameObject = this;
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
                if (typeof(Component).IsAssignableFrom(behaviour.GetType()))
                    behaviour.To<Component>().gameObject = this;
                if (typeof(MonoBehaviour).IsAssignableFrom(behaviour.GetType()))
                    behaviour.To<MonoBehaviour>().gameObject = this;
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
            if (typeof(Component).IsAssignableFrom(temp.GetType()))
                temp.To<Component>().gameObject = null;
            if (typeof(MonoBehaviour).IsAssignableFrom(temp.GetType()))
                temp.To<MonoBehaviour>().gameObject = null;
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
            if (typeof(Component).IsAssignableFrom(behaviour.GetType()))
                behaviour.To<Component>().gameObject = null;
            if (typeof(MonoBehaviour).IsAssignableFrom(behaviour.GetType()))
                behaviour.To<MonoBehaviour>().gameObject = null;
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
                if (typeof(Component).IsAssignableFrom(behaviour.GetType()))
                    behaviour.To<Component>().gameObject = null;
                if (typeof(MonoBehaviour).IsAssignableFrom(behaviour.GetType()))
                    behaviour.To<MonoBehaviour>().gameObject = null;
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
                if (typeof(Component).IsAssignableFrom(behaviour.GetType()))
                    behaviour.To<Component>().gameObject = null;
                if (typeof(MonoBehaviour).IsAssignableFrom(behaviour.GetType()))
                    behaviour.To<MonoBehaviour>().gameObject = null;
                l.Add(behaviour);
            }
            return l.GetEnumerator();
        }

        protected override void OnCreate()
        {
            if (SceneManager.CurrentScene.TryGetTarget(out var target))
                target.GameObjects.Add(this);
            else
            {
                var i = SceneManager.AddScene(new Scene("Scene", 0));
                if (i == -1)
                {
                    Log.Warning("{Ga} wasn't correctly created. There was a problem adding it to current scene or to a new one.", nameof(GameObject));
                    return;
                }
                var currentScene = SceneManager.Get(i);
                currentScene!.GameObjects.Add(this);
            }
        }

        protected override void OnDestroy()
        {
            if (SceneManager.CurrentScene.TryGetTarget(out var target))
                target.GameObjects.Remove(this);
            else
                Log.Warning("{Ga} wasn't correctly destroyed. There was a problem removing it from current scene.", nameof(GameObject));
        }

        public static GameObject? FindGameObjectWithTag(string tag)
        {
            var b = SceneManager.CurrentScene.TryGetTarget(out var target);
            if (!b) return null;
            return target!.GameObjects.Find(x => x.tag == tag);
        }
        public static IEnumerator<GameObject>? FindGameObjectsWithTag(string tag)
        {
            var b = SceneManager.CurrentScene.TryGetTarget(out var target);
            if (!b) return null;
            return target!.GameObjects.FindAll(x => x.tag == tag).GetEnumerator();
        }

    }
}