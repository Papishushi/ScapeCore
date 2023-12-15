using System.Diagnostics.CodeAnalysis;

namespace ScapeCore.Core.Engine
{
    internal interface IEntityComponentModel
    {
        [SuppressMessage("Style", "IDE1006:Naming Styles",
         Justification = "<In this way it does not match class name and keep it simple and descriptible.>")]
        public GameObject? gameObject { get; internal set; }
    }
}
