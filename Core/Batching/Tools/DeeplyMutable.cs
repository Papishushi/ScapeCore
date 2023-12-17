using Serilog;
using System;
using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace ScapeCore.Core.Batching.Tools
{
    public sealed class DeeplyMutable<T> : DeeplyMutableType
    {
        public new T? Value { get => _value; set => _value = value; }
        public DeeplyMutable() : base() { }
        public DeeplyMutable(T? value) : base(value) { }
        public DeeplyMutable(DeeplyMutableType deeplyMutableType) => _value = deeplyMutableType.Value;

        protected override FieldInfo[]? DynamicFields => typeof(T).GetFields();
    }
}
