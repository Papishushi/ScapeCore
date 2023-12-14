using System.Dynamic;

namespace ScapeCore.Core.Batching.Tools
{
    public sealed class DeeplyMutable<T> : DeeplyMutableType
    {
        public new T Value { get => _value; set => _value = value; }
        public DeeplyMutable() : base() { }
        public DeeplyMutable(T value) : base(value) { }
        public DeeplyMutable(DeeplyMutableType deeplyMutableType) => _value = deeplyMutableType.Value;

        public override bool TrySetMember(SetMemberBinder binder, dynamic value)
        {
            string name = binder.Name.ToLower();
            var v = name == nameof(Value).ToLower() || name == nameof(_value).ToLower();
            if (v) 
            {
                _value = value;
                return true;
            }
            var f = false;
            foreach(var field in typeof(T).GetFields())
                if (name  == field.Name)
                {
                    field.SetValue(Value, value);
                    f = true;
                    break;
                }
            return f;
        }
    }
}
