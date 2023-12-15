using Serilog;
using System;
using System.Dynamic;

namespace ScapeCore.Core.Batching.Tools
{
    public sealed class DeeplyMutable<T> : DeeplyMutableType
    {
        public new T? Value { get => _value; set => _value = value; }
        public DeeplyMutable() : base() { }
        public DeeplyMutable(T value) : base(value) { }
        public DeeplyMutable(DeeplyMutableType deeplyMutableType) => _value = deeplyMutableType.Value;

        public override bool TrySetMember(SetMemberBinder binder, dynamic? value)
        {
            if (_value == null) return false;
            string name = binder.Name.ToLower();
            var isValue = name == nameof(Value).ToLower() || name == nameof(_value).ToLower();
            if (isValue)
            {
                _value = value;
                return true;
            }
            var isField = false;
            foreach (var field in typeof(T).GetFields())
            {
                if (name  == field.Name)
                {
                    try
                    {
                        field.SetValue(Value, value);
                    }
                    catch (Exception ex)
                    {
                        Log.Warning(ERROR_FORMAT, ex.Message);
                        return false;
                    }
                    isField = true;
                    break;
                }
            }
            return isField;
        }
    }
}
