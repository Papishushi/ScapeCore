using Serilog;
using System;
using System.Dynamic;
using System.Linq;
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

        protected virtual FieldInfo[]? DynamicFields => _value?.GetType().GetFields();
        protected static bool LogWarning(string message)
        {
            Log.Warning(ERROR_FORMAT, message);
            return false;
        }
        protected static bool IsValueMember(string name) => name.Equals(nameof(Value), StringComparison.OrdinalIgnoreCase) || name.Equals(nameof(_value), StringComparison.OrdinalIgnoreCase);

        protected bool TryFieldOperation(string name, Func<FieldInfo, object?, bool> operation, object? result) =>
            DynamicFields?.FirstOrDefault(field => name.Equals(field.Name, StringComparison.OrdinalIgnoreCase)) switch
            {
                { } field => operation(field, result),
                _ => false
            };

        protected bool TryGetValueFromField(FieldInfo field, object? result)
        {
            try
            {
                result = field.GetValue(_value);
                return true;
            }
            catch (Exception ex)
            {
                return LogWarning(ex.Message);
            }
        }
        protected bool TrySetValueToField(FieldInfo field, object? value)
        {
            try
            {
                field.SetValue(_value, value);
                return true;
            }
            catch (Exception ex)
            {
                return LogWarning(ex.Message);
            }
        }

        public override bool TryGetMember(GetMemberBinder binder, out object? result)
        {
            result = null;
            if (_value == null)
                return false;
            if (IsValueMember(binder.Name))
            {
                result = _value;
                return true;
            }
            return TryFieldOperation(binder.Name, TryGetValueFromField, result);
        }
        public override bool TrySetMember(SetMemberBinder binder, object? value)
        {
            if (IsValueMember(binder.Name))
            {
                _value = value;
                return true;
            }
            return TryFieldOperation(binder.Name, TrySetValueToField, value);
        }
    }
}
