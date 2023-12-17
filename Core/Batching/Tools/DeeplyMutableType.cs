/*
 * -*- encoding: utf-8 with BOM -*-
 * .▄▄ ·  ▄▄·  ▄▄▄·  ▄▄▄·▄▄▄ .     ▄▄·       ▄▄▄  ▄▄▄ .
 * ▐█ ▀. ▐█ ▌▪▐█ ▀█ ▐█ ▄█▀▄.▀·    ▐█ ▌▪▪     ▀▄ █·▀▄.▀·
 * ▄▀▀▀█▄██ ▄▄▄█▀▀█  ██▀·▐▀▀▪▄    ██ ▄▄ ▄█▀▄ ▐▀▀▄ ▐▀▀▪▄
 * ▐█▄▪▐█▐███▌▐█ ▪▐▌▐█▪·•▐█▄▄▌    ▐███▌▐█▌.▐▌▐█•█▌▐█▄▄▌
 *  ▀▀▀▀ ·▀▀▀  ▀  ▀ .▀    ▀▀▀     ·▀▀▀  ▀█▄▀▪.▀  ▀ ▀▀▀ 
 * https://github.com/Papishushi/ScapeCore
 * 
 * MIT License
 *
 * Copyright (c) 2023 Daniel Molinero Lucas
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

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
