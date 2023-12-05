using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Framework.Utilities.Deflate;
using ProtoBuf.Meta;
using ScapeCore.Core.Batching.Events;
using ScapeCore.Core.Batching.Resources;
using ScapeCore.Core.Batching.Tools;
using ScapeCore.Core.Engine;
using ScapeCore.Core.Engine.Components;
using ScapeCore.Targets;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;


namespace ScapeCore.Core.Serialization
{
    public static class SerializationManager
    {
        private const int GZIP_BUFFER_SIZE = 64*1024;

        private static RuntimeTypeModel _model = null;

        private readonly static Type[] _types =
        {
                typeof(LLAM),

                typeof(Behaviour),
                typeof(Component),
                typeof(Transform),
                typeof(GameObject),
                typeof(MonoBehaviour),

                typeof(Texture2D),
                typeof(Renderer),
                typeof(RectTransform),
                typeof(SpriteRenderer),

                typeof(ResourceWrapper),
                typeof(ResourceInfo),
                typeof(ResourceDependencyTree),

                typeof(LoadBatchEventArgs),
                typeof(StartBatchEventArgs),
                typeof(UpdateBatchEventArgs),
                typeof(RenderBatchEventArgs),
                typeof(GameTime)
        };
        internal static TypeModel Model { get => _model; }

        static SerializationManager()
        {
            var runtimeModel = RuntimeTypeModel.Create("ScapeCore");

            runtimeModel.AllowParseableTypes = true;
            runtimeModel.AutoAddMissingTypes = true;
            runtimeModel.MaxDepth = 100;
            foreach (var type in _types)
            {
                var i = 1;

                var metaType = runtimeModel.Add(type, false);
                Log.Debug("Type {type} was configured for [de]Serialization...", type.Name);
                foreach (var field in type.GetFields())
                {
                    if (field.FieldType.Name == typeof(object).Name)
                    {
                        Log.Warning("Serialization Manager tried to configure an object/dynamic field named {field} from Type {type}, serializer does not support deeply mutable types.", field.Name, type.Name);
                        continue;
                    }
                    metaType.Add(i++, field.Name);
                    Log.Verbose("\tField [{i}]{field}[{t}] from Type {type}", i-1, field.Name, field.FieldType, type.Name);
                }
            }
            runtimeModel.MakeDefault();
            _model = runtimeModel;
        }

        public static void AddType(Type type)
        {
            var i = 1;
            var metaType = _model.Add(type, false);
            metaType.IgnoreUnknownSubTypes = false;
            Log.Debug("Type {type} was configured for [de]Serialization...", type.Name);
            foreach (var field in type.GetFields())
            {
                if (field.FieldType.Name == typeof(object).Name)
                {
                    Log.Warning("Serialization Manager tried to configure an object/dynamic field named {field} from Type {type}, serializer does not support deeply mutable types.", field.Name, type.Name);
                    continue;
                }
                metaType.Add(i++, field.Name);
                Log.Verbose("\tField [{i}]{field}[{t}] from Type {type}", i-1, field.Name, field.FieldType, type.Name);
            }
            foreach (MetaType rtType in _model.GetTypes())
            {
                var x = 556;
                if (rtType.Type == type.BaseType)
                {
                    rtType.AddSubType(x++, type);
                    break;
                }
            }
        }

        public enum SerializationError
        {
            None,
            NotSerializable,
            UnauthorizedAccess,
            PathNotValid,
            NullPath,
            PathTooLong,
            DirectoryNotFound,
            NotSupported
        }
        public readonly record struct SerializationOutput(SerializationError Error, byte[] Data, long Size, string Path, bool Compressed);
        public static SerializationOutput Serialize<T>(T obj, string path, bool compress = false, object userState = null)
        {
            long size = 0;
            byte[] data = null;
            string fileName = compress ? typeof(T).Name + ".pb.bin.gz" : typeof(T).Name + ".pb.bin";

            if (!_model.CanSerialize(typeof(T))) return new() { Error = SerializationError.NotSerializable, Data = default, Size = size, Path = path, Compressed = compress };
            if (string.IsNullOrEmpty(path)) return new() { Error = SerializationError.NullPath, Data = default, Size = size, Path = path, Compressed = compress };
            try
            {
                using (var writer = File.Open(Path.Combine(path, fileName), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read))
                {
                    if (compress)
                    {
                        using (var gzip = new GZipStream(writer, CompressionMode.Compress, false))
                        using (var bs = new BufferedStream(gzip, GZIP_BUFFER_SIZE))
                        {
                            size = _model.Serialize<T>(bs, obj, userState);
                            data = bs.ToByteArray();
                        }
                    }
                    else
                    {
                        size = _model.Serialize<T>(writer, obj, userState);
                        data = writer.ToByteArray();
                    }
                }
                Log.Debug("Serialized {l} bytes from {type} into {path}", size, typeof(T), path);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("Serialization to path {path} failed: {ex}", path, ex.Message);
                return new() { Error = SerializationError.UnauthorizedAccess, Data = data, Size = size, Path = path, Compressed = compress };
            }
            catch (ArgumentNullException ex)
            {
                Log.Error("Serialization to path {path} failed: {ex}", path, ex.Message);
                return new() { Error = SerializationError.NullPath, Data = data, Size = size, Path = path, Compressed = compress };
            }
            catch (ArgumentException ex)
            {
                Log.Error("Serialization to path {path} failed: {ex}", path, ex.Message);
                return new() { Error = SerializationError.PathNotValid, Data = data, Size = size, Path = path, Compressed = compress };
            }
            catch (PathTooLongException ex)
            {
                Log.Error("Serialization to path {path} failed: {ex}", path, ex.Message);
                return new() { Error = SerializationError.PathTooLong, Data = data, Size = size, Path = path, Compressed = compress };
            }
            catch (DirectoryNotFoundException ex)
            {
                Log.Error("Serialization to path {path} failed: {ex}", path, ex.Message);
                return new() { Error = SerializationError.DirectoryNotFound, Data = data, Size = size, Path = path, Compressed = compress };
            }
            catch (NotSupportedException ex)
            {
                Log.Error("Serialization to path {path} failed: {ex}", path, ex.Message);
                return new() { Error = SerializationError.NotSupported, Data = data, Size = size, Path = path, Compressed = compress };
            }
            return new() { Error = SerializationError.None, Data = data, Size = size, Path = path, Compressed = compress };
        }
        public enum DeserializationError
        {
            None,
            NotSerializable,
            UnauthorizedAccess,
            PathNotValid,
            NullPath,
            PathTooLong,
            DirectoryNotFound,
            FileNotFound,
            NotSupported,
            IO
        }
        public readonly record struct DeserializationOutput<T>(DeserializationError Error, T Output, string Path, bool Decompressed);
        public static DeserializationOutput<T> Deserialize<T>(string path, T obj = default, bool decompress = false, object userState = null)
        {
            T deserialized = default;
            string fileName = decompress ? typeof(T).Name+".pb.bin.gz" : typeof(T).Name+".pb.bin";

            if (!_model.CanSerialize(typeof(T))) return new() { Error = DeserializationError.NotSerializable, Output = deserialized, Path = path };
            if (string.IsNullOrEmpty(path)) return new() { Error = DeserializationError.NullPath, Output = deserialized, Path = path };
            try
            {
                using (var reader = File.OpenRead(Path.Combine(path, fileName)))
                {
                    if (decompress)
                    {
                        using (var gzip = new GZipStream(reader, CompressionMode.Decompress, true))
                        using (var bs = new BufferedStream(gzip, GZIP_BUFFER_SIZE))
                        {
                            deserialized = _model.Deserialize<T>(reader, obj, userState);
                        }
                    }
                    else
                        deserialized = _model.Deserialize<T>(reader, obj, userState);
                }

                Log.Debug("Deserialized type {t} from {path}.", typeof(T).Name, path);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("Deserialization to path {path} failed: {ex}", path, ex.Message);
                return new() { Error = DeserializationError.UnauthorizedAccess, Output = deserialized, Path = path };
            }
            catch (ArgumentNullException ex)
            {
                Log.Error("Deserialization to path {path} failed: {ex}", path, ex.Message);
                return new() { Error = DeserializationError.NullPath, Output = deserialized, Path = path };
            }
            catch (ArgumentException ex)
            {
                Log.Error("Deserialization to path {path} failed: {ex}", path, ex.Message);
                return new() { Error = DeserializationError.PathNotValid, Output = deserialized, Path = path };
            }
            catch (PathTooLongException ex)
            {
                Log.Error("Deserialization to path {path} failed: {ex}", path, ex.Message);
                return new() { Error = DeserializationError.PathTooLong, Output = deserialized, Path = path };
            }
            catch (DirectoryNotFoundException ex)
            {
                Log.Error("Deserialization to path {path} failed: {ex}", path, ex.Message);
                return new() { Error = DeserializationError.DirectoryNotFound, Output = deserialized, Path = path };
            }
            catch (FileNotFoundException ex)
            {
                Log.Error("Deserialization to path {path} failed: {ex}", path, ex.Message);
                return new() { Error = DeserializationError.FileNotFound, Output = deserialized, Path = path };
            }
            catch (NotSupportedException ex)
            {
                Log.Error("Deserialization to path {path} failed: {ex}", path, ex.Message);
                return new() { Error = DeserializationError.NotSupported, Output = deserialized, Path = path };
            }
            catch (IOException ex)
            {
                Log.Error("Deserialization to path {path} failed: {ex}", path, ex.Message);
                return new() { Error = DeserializationError.IO, Output = deserialized, Path = path };
            }
            return new() { Error = DeserializationError.None, Output = deserialized, Path = path };
        }

        public enum ChangeModelError
        {
            None,
            NullModel
        }
        public readonly record struct ChangeModelOutput(ChangeModelError Error);
        public static ChangeModelOutput ChangeModel(RuntimeTypeModel model)
        {
            if (model == null)
            {
                Log.Warning("Cannot change to a null serialization model. Serialization model remains the same.");
                return new() { Error = ChangeModelError.NullModel };
            }
            _model = model;
            _model.CompileInPlace();
            Log.Debug("Serialization model was succesfully updated.");
            return new() { Error = ChangeModelError.None };
        }

        public static RuntimeTypeModel GetRuntimeClone() => (RuntimeTypeModel)_model;
        public static byte[] ToByteArray(this Stream stream)
        {
            using (stream)
            using (MemoryStream memStream = new MemoryStream())
            {
                stream.CopyTo(memStream);
                return memStream.ToArray();
            }
        }
    }
}