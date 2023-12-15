namespace ScapeCore.Core.Engine
{
    public abstract class Component : Behaviour, IEntityComponentModel
    {
        public GameObject? gameObject { get; set; }
        public Component() : base(nameof(Component)) { }
        protected Component(string name) : base(name) { }
    }
}