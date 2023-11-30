using System;
using ScapeCore.Targets;


namespace ScapeCore.Core.Engine
{
    public abstract class Behaviour
    {
        private readonly Guid _id = new();
        public LLAM game;
        public string name;
        public bool isActive;

        public Guid Id { get => _id; }

        ~Behaviour() => OnDestroy();
        public Behaviour()
        {
            game =  LLAM.Instance;
            name = nameof(Behaviour);
            isActive = true;
            OnCreate();
        }
        protected Behaviour(string name)
        {
            game =  LLAM.Instance;
            this.name = name;
            isActive = true;
            OnCreate();
        }
        public T To<T>() where T : Behaviour => (T)this;

        public override string ToString() => name;

        protected abstract void OnCreate();
        protected abstract void OnDestroy();
        protected virtual string Serialize() => $"{{\n" +
                                                $"\"{nameof(_id)}\": \"{_id}\",\n" +
                                                $"\"{nameof(game)}\": \"{game}\",\n" +
                                                $"\"{nameof(name)}\": \"{name}\",\n" +
                                                $"\"{nameof(isActive)}\": {isActive},\n" +
                                                $"}}";

    }
}
