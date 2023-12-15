using Serilog;
using System;
using System.Dynamic;
using System.Reflection;

namespace ScapeCore.Core.Batching.Tools
{
    public class DeeplyMutableType : DynamicObject
    {
        protected const string ERROR_FORMAT = "There was an error while setting a member from a deeply mutable type. {ex}";
        protected dynamic? _value;
        public dynamic? Value { get => _value ??= default; set => _value = value; }
        public DeeplyMutableType() { _value = default; }
        public DeeplyMutableType(object? value) => _value = value;

        public override bool TryGetMember(GetMemberBinder binder, out object? result)
        {
            result = null;
            if (_value == null) return false;
            var name = binder.Name.ToLower();
            var isValue = name == nameof(Value).ToLower() || name == nameof(_value).ToLower();
            if (isValue)
            {
                result = _value;
                return true;
            }
            var isField = false;
            FieldInfo[] fields = _value.GetType().GetFields();
            foreach (var field in fields)
            {
                if (name == field.Name)
                {
                    try
                    {
                        field.GetValue(Value);
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
        public override bool TrySetMember(SetMemberBinder binder, object? value)
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
            FieldInfo[] fields = _value.GetType().GetFields();
            foreach (var field in fields)
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
