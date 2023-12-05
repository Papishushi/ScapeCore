using ProtoBuf;
using ScapeCore;
using System;
using System.Diagnostics.CodeAnalysis;

namespace ScapeCore.Core.Engine
{
    public abstract class Component : Behaviour
    {
        [SuppressMessage("Style", "IDE1006:Naming Styles",
                 Justification = "<In this way it does not match class name and keep it simple and descriptible.>")]
        public GameObject gameObject { get; internal set; }
        public Component() : base(nameof(Component)) { }
        protected Component(string name) : base(name) { }

    }

}
