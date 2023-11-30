using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ScapeCore;
using ScapeCore.Core.Engine.Components;

namespace ScapeCore.Core.Engine
{
    public sealed class GameObject : Behaviour
    {
        public Transform transform;
        public string tag;
        private readonly List<Behaviour> behaviours;

        private static readonly List<string> tagList = new();
        public static ReadOnlyCollection<string> TagList { get => tagList.AsReadOnly(); }

        public GameObject() : base(nameof(GameObject))
        {
            transform = new();
            behaviours = new()
            {
                transform
            };
        }
        public GameObject(string name) : base(name)
        {
            transform=new();
            behaviours=new()
            {
                transform
            };
        }
        public GameObject(params Behaviour[] behaviours) : base(nameof(GameObject))
        {
            transform=new();
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
            if(behaviours == null) throw new System.NullReferenceException($"{nameof(behaviours)} is null");
        }
       
        public T GetBehaviour<T>() where T : Behaviour
        {
            try
            {
                BehavioursNullException();
            }
            catch (NullReferenceException nRE)
            {
                Console.WriteLine($"Failed to get behaviour on GameObject {name} {{{Id}}}\t{nRE.Message}");
                throw;
            }
            foreach (Behaviour behaviour in behaviours)
                if (behaviour.GetType() == typeof(T)) return (T)behaviour;
            return null;
        }

        public T AddBehaviour<T>() where T : Behaviour, new()
        {
            try
            {
                BehavioursNullException();
            }
            catch (NullReferenceException nRE)
            {
                Console.WriteLine($"Failed to add behaviour on GameObject {name} {{{Id}}}\t{nRE.Message}");
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
        public T AddBehaviour<T>(T behaviour) where T : Behaviour
        {
            try
            {
                BehavioursNullException();
            }
            catch (NullReferenceException nRE)
            {
                Console.WriteLine($"Failed to add behaviour on GameObject {name} {{{Id}}}\t{nRE.Message}");
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
        public T RemoveBehaviour<T>() where T : Behaviour
        {
            try
            {
                BehavioursNullException();
            }
            catch (NullReferenceException nRE)
            {
                Console.WriteLine($"Failed to remove behaviour on GameObject {name} {{{Id}}}\t{nRE.Message}");
                throw;
            }
            T temp = behaviours.Find(x =>  x.GetType() == typeof(T)).To<T>();
            if (temp == null) return null;
            behaviours.Remove(temp);
            if (typeof(Component).IsAssignableFrom(temp.GetType()))
                temp.To<Component>().gameObject = null;
            if (typeof(MonoBehaviour).IsAssignableFrom(temp.GetType()))
                temp.To<MonoBehaviour>().gameObject = null;
            return temp;
        }

        public T RemoveBehaviour<T>(T behaviour) where T : Behaviour
        {
            try
            {
                BehavioursNullException();
            }
            catch (NullReferenceException nRE)
            {
                Console.WriteLine($"Failed to remove behaviour on GameObject {name} {{{Id}}}\t{nRE.Message}");
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

        protected override void OnCreate() => game.GameObjects.Add(this);

        protected override void OnDestroy() => game.GameObjects.Remove(this);

        protected override string Serialize()
        {
            throw new System.NotImplementedException();
        }
    }
}