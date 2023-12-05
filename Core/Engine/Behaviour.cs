using System;
using ProtoBuf;
using ScapeCore.Core.Serialization;
using ScapeCore.Targets;

namespace ScapeCore.Core.Engine
{
    public abstract class Behaviour
    {
        private readonly Guid _id = new();
        public LLAM Game { get; private set; }
        public string name;
        public bool isActive;

        public Guid Id { get => _id; }

        ~Behaviour() => OnDestroy();
        public Behaviour()
        {
            Game =  LLAM.Instance;
            name = nameof(Behaviour);
            isActive = true;
            OnCreate();
        }
        protected Behaviour(string name)
        {
            Game =  LLAM.Instance;
            this.name = name;
            isActive = true;
            OnCreate();
        }
        public T To<T>() where T : Behaviour => (T)this;

        public override string ToString() => name;

        protected abstract void OnCreate();
        protected abstract void OnDestroy();
    }
}