using System.Dynamic;

namespace ScapeCore.Core.Batching.Tools
{
    public class DeeplyMutableType : DynamicObject
    {
        protected dynamic _value;
        public dynamic Value { get => _value; set => _value = value; }
        public DeeplyMutableType() { _value = default; }
        public DeeplyMutableType(object value) => _value = value;

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            string name = binder.Name.ToLower();
            var b = name == nameof(Value).ToLower() || name == nameof(_value).ToLower();
            result = b ? _value : null;
            return b;
        }
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            string name = binder.Name.ToLower();
            var b = name == nameof(Value).ToLower() || name == nameof(_value).ToLower();
            _value = b ? value : _value;
            return b;
        }
    }

    public sealed class DeeplyMutable<T> : DeeplyMutableType
    {
        public new T Value { get => _value; set => _value = value; }
        public DeeplyMutable() : base() { }
        public DeeplyMutable(T value) : base(value) { }
        public DeeplyMutable(DeeplyMutableType deeplyMutableType) => _value = deeplyMutableType.Value;

        public override bool TrySetMember(SetMemberBinder binder, dynamic value)
        {
            string name = binder.Name.ToLower();
            var b = name == nameof(Value).ToLower() || name == nameof(_value).ToLower();
            _value = b ? value : _value;
            return b;
        }
    }
}
