using System.Dynamic;
using System.Text;
using System.Text.Json;

namespace ScapeCore.Core.Batching.Tools
{
    public class DeeplyMutableType : DynamicObject
    {
        private byte[] _value;
        public byte[] Value { get => _value ??= ConvertObjectToJsonByteArray(default); set => _value = value; }
        public DeeplyMutableType() { _value = default; }
        public DeeplyMutableType(object value) => _value = ConvertObjectToJsonByteArray(value);
        protected DeeplyMutableType(DeeplyMutableType deeplyMutableType) => _value = ConvertObjectToJsonByteArray(deeplyMutableType.Value);
        protected static byte[] ConvertObjectToJsonByteArray(object obj) => Encoding.UTF8.GetBytes(JsonSerializer.Serialize(obj));
        protected static T ConvertJsonByteArrayToObject<T>(byte[] byteArray) => JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(byteArray));

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
            _value = b ? ConvertObjectToJsonByteArray(value) : _value;
            return b;
        }
    }

    public class DeeplyMutableType<T> : DeeplyMutableType
    {
        private T _value;
        public new T Value { get => _value ??= ConvertJsonByteArrayToObject<T>(base.Value); set => _value = value; }
        public DeeplyMutableType() : base() { _value = default; }
        public DeeplyMutableType(T value) : base(value) { _value = value; }
        protected DeeplyMutableType(DeeplyMutableType deeplyMutableType) => _value = ConvertJsonByteArrayToObject<T>(deeplyMutableType.Value);

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            string name = binder.Name.ToLower();
            var b = name == nameof(Value).ToLower() || name == nameof(_value).ToLower();
            _value = (T)(b ? value : _value);
            return b;
        }
    }
}
