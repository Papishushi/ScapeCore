using ScapeCore.Targets;
using System;

namespace ScapeCore.Core.Engine
{
    public abstract class Behaviour
    {
        private readonly Guid _id = new();
        public LLAM? Game { get; private set; }
        public string name;
        public bool isActive;

        public Guid Id { get => _id; }

        ~Behaviour() => OnDestroy();
        public Behaviour()
        {
            var b = LLAM.Instance.TryGetTarget(out var target);
            Game =  b ? target : null;
            name = nameof(Behaviour);
            isActive = true;
            OnCreate();
        }
        protected Behaviour(string name)
        {
            var b = LLAM.Instance.TryGetTarget(out var target);
            Game =  b ? target : null;
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