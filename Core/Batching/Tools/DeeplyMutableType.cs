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
            result = null;
            string name = binder.Name.ToLower();
            var v = name == nameof(Value).ToLower() || name == nameof(_value).ToLower();
            if (v)
            {
                result = _value;
                return true;
            }
            var f = false;
            foreach (var field in _value.GetType().GetFields())
                if (name == field.Name)
                {
                    result = field.GetValue(Value);
                    f = true;
                    break;
                }
            return f;
        }
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            string name = binder.Name.ToLower();
            var v = name == nameof(Value).ToLower() || name == nameof(_value).ToLower();
            if (v)
            {
                _value = value;
                return true;
            }
            var f = false;
            foreach (var field in _value.GetType().GetFields())
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
