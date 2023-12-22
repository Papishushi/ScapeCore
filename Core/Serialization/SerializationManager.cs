/*
 * -*- encoding: utf-8 with BOM -*-
 * .▄▄ ·  ▄▄·  ▄▄▄·  ▄▄▄·▄▄▄ .     ▄▄·       ▄▄▄  ▄▄▄ .
 * ▐█ ▀. ▐█ ▌▪▐█ ▀█ ▐█ ▄█▀▄.▀·    ▐█ ▌▪▪     ▀▄ █·▀▄.▀·
 * ▄▀▀▀█▄██ ▄▄▄█▀▀█  ██▀·▐▀▀▪▄    ██ ▄▄ ▄█▀▄ ▐▀▀▄ ▐▀▀▪▄
 * ▐█▄▪▐█▐███▌▐█ ▪▐▌▐█▪·•▐█▄▄▌    ▐███▌▐█▌.▐▌▐█•█▌▐█▄▄▌
 *  ▀▀▀▀ ·▀▀▀  ▀  ▀ .▀    ▀▀▀     ·▀▀▀  ▀█▄▀▪.▀  ▀ ▀▀▀ 
 * https://github.com/Papishushi/ScapeCore
 * 
 * Copyright (c) 2023 Daniel Molinero Lucas
 * This file is subject to the terms and conditions defined in
 * file 'LICENSE.txt', which is part of this source code package.
 * 
 * SerializationManager.cs
 * Provides static methods for serializing and deserializing
 * objects using ProtoBuf with optional compression.
 */

using Baksteen.Extensions.DeepCopy;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Framework.Utilities.Deflate;
using ProtoBuf.Meta;
using ScapeCore.Core.Batching.Events;
using ScapeCore.Core.Batching.Resources;
using ScapeCore.Core.Batching.Tools;
using ScapeCore.Core.Collections.Merkle;
using ScapeCore.Core.Engine;
using ScapeCore.Core.Engine.Components;
using ScapeCore.Targets;
using Serilog;
using System;
using System.IO;
using System.Reflection.PortableExecutable;


namespace ScapeCore.Core.Serialization
{
    public static class SerializationManager
    {
        private const int GZIP_BUFFER_SIZE = 64*1024;
        private const int FIELD_PROTOBUF_INDEX = 1;
        private const int SUBTYPE_PROTOBUF_INDEX = 556;
        private const string SCAPE_CORE_NAME = "ScapeCore";
        private const string PROTOBUFER_COMPRESSED_BINARY = ".pb.bin.gz";
        private const string PROTOBUFFER_BINARY = ".pb.bin";
        private const string FIELD_ERROR_MESSAGE = "Serialization Manager tried to configure an object/dynamic field named {field} from Type {type}," +
                            " serializer does not support deeply mutable types, try changing field type to {dmtName}.";
        private const string PROPERTY_ERROR_MESSAGE = "Serialization Manager tried to configure an object/dynamic property named {property} from Type {type}," +
                            " serializer does not support deeply mutable types, try changing property type to {dmtName}.";


        private static RuntimeTypeModel? _model = null;

        private readonly static Type[] _types =
        {
            typeof(MerkleNode<>),
            typeof(MerkleTree<>),
            typeof(DeeplyMutableType),
            typeof(DeeplyMutable<>),
            typeof(WeakReference<>),
            typeof(Game),
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
        internal static TypeModel? Model { get => _model; }
        public static RuntimeTypeModel? GetRuntimeClone() => _model?.DeepCopy();

        static SerializationManager()
        {
            var runtimeModel = RuntimeTypeModel.Create(SCAPE_CORE_NAME);
            runtimeModel.AllowParseableTypes = true;
            runtimeModel.AutoAddMissingTypes = true;
            runtimeModel.MaxDepth = 100;
            foreach (var type in _types)
            {
                var fieldIndex = FIELD_PROTOBUF_INDEX;
                var metaType = runtimeModel.Add(type, false);
                Log.Debug("Type {type} was configured for [de]Serialization...", type.Name);
                foreach (var field in type.GetFields())
                {
                    if (field.FieldType.Name == typeof(object).Name)
                    {
                        Log.Warning(FIELD_ERROR_MESSAGE, field.Name, type.Name, typeof(DeeplyMutableType).FullName);
                        continue;
                    }
                    metaType.Add(fieldIndex++, field.Name);
                    Log.Verbose("\tField [{i}]{field}[{t}] from Type {type}", fieldIndex - 1, field.Name, field.FieldType, type.Name);
                }
                foreach (MetaType rtType in runtimeModel.GetTypes())
                {
                    if (rtType.Type == type.BaseType)
                    {
                        var sub = rtType.GetSubtypes();
                        if (sub.Length == 0)
                            rtType.AddSubType(SUBTYPE_PROTOBUF_INDEX, type);
                        else
                            rtType.AddSubType(SUBTYPE_PROTOBUF_INDEX + sub.Length, type);
                        break;
                    }
                }
                if (type == typeof(DeeplyMutableType) || type.BaseType == typeof(DeeplyMutableType))
                {
                    runtimeModel.MakeDefault();
                    _model = runtimeModel;
                    continue;
                }
                foreach (var property in type.GetProperties())
                {
                    try
                    {
                        if (property.PropertyType.Name == typeof(object).Name)
                        {
                            Log.Warning(PROPERTY_ERROR_MESSAGE, property.Name, type.Name, typeof(DeeplyMutableType).FullName);
                            continue;
                        }
                        metaType.Add(fieldIndex++, property.Name);
                        Log.Verbose("\tProperty [{i}]{property}[{t}] from Type {type}", fieldIndex - 1, property.Name, property.PropertyType, type.Name);
                    }
                    catch (Exception ex)
                    {
                        Log.Warning("Serialization Manager can not determine type of property {property} from {type}.", property.Name, type);
                        Log.Verbose("{ex}", ex.Message);
                        continue;
                    }
                }
            }
            runtimeModel.MakeDefault();
            _model = runtimeModel;
        }

        public static void AddType(Type type)
        {
            if (_model == null)
            {
                Log.Warning("Serialization Manager can not add a type {t} becouse serialization model is null.", type.FullName);
                return;
            }
            var fieldIndex = FIELD_PROTOBUF_INDEX;
            var metaType = _model.Add(type, false);
            metaType.IgnoreUnknownSubTypes = false;
            Log.Debug("Type {type} was configured for [de]Serialization...", type.Name);
            foreach (var field in type.GetFields())
            {
                if (field.FieldType.Name == typeof(object).Name)
                {
                    Log.Warning(FIELD_ERROR_MESSAGE, field.Name, type.Name, typeof(DeeplyMutableType).FullName);
                    continue;
                }
                metaType.Add(fieldIndex++, field.Name);
                Log.Verbose("\tField [{i}]{field}[{t}] from Type {type}", fieldIndex - 1, field.Name, field.FieldType, type.Name);
            }
            foreach (MetaType rtType in _model.GetTypes())
            {
                if (rtType.Type == type.BaseType)
                {
                    var sub = rtType.GetSubtypes();
                    if (sub.Length == 0)
                        rtType.AddSubType(SUBTYPE_PROTOBUF_INDEX, type);
                    else
                        rtType.AddSubType(SUBTYPE_PROTOBUF_INDEX + sub.Length, type);
                    break;
                }
            }
            if (type == typeof(DeeplyMutableType) || type.BaseType == typeof(DeeplyMutableType)) return;
            foreach (var property in type.GetProperties())
            {
                try
                {
                    if (property.PropertyType.Name == typeof(object).Name)
                    {
                        Log.Warning(PROPERTY_ERROR_MESSAGE, property.Name, type.Name, typeof(DeeplyMutableType).FullName);
                        continue;
                    }
                    metaType.Add(fieldIndex++, property.Name);
                    Log.Verbose("\tField [{i}]{field}[{t}] from Type {type}", fieldIndex - 1, property.Name, property.PropertyType, type.Name);
                }
                catch (Exception ex)
                {
                    Log.Warning("Serialization Manager can not determine type of property {property} from {type}.", property.Name, type);
                    Log.Verbose("{ex}", ex.Message);
                    continue;
                }
            }
        }

        #region Serialization
        public enum SerializationError
        {
            None,
            NotSerializable,
            UnauthorizedAccess,
            PathNotValid,
            NullPath,
            PathTooLong,
            DirectoryNotFound,
            NotSupported,
            Serilog,
            ModelNull
        }
        public readonly record struct SerializationOutput(SerializationError Error, byte[]? Data, long Size, string Path, bool Compressed);
        public static SerializationOutput Serialize<T>(T obj, string path, bool compress = false, object? userState = null)
        {
            long size = 0;
            byte[]? data = null;
            string fileName = compress ? typeof(T).Name + PROTOBUFER_COMPRESSED_BINARY : typeof(T).Name + PROTOBUFFER_BINARY;
            const string ERROR_FORMAT = "Serialization to path {path} failed: {ex}";

            if (_model == null)
            {
                Log.Warning(ERROR_FORMAT, path, "Serialization model is null.");
                return new() { Error = SerializationError.ModelNull, Data = data, Size = size, Path = path, Compressed = compress };
            }
            if (!_model.CanSerialize(typeof(T)))
            {
                Log.Error(ERROR_FORMAT, $"Type {typeof(T).FullName} can't be serialized.");
                return new() { Error = SerializationError.NotSerializable, Data = default, Size = size, Path = string.Empty, Compressed = compress };
            }
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
                            size = _model.Serialize(bs, obj, userState);
                            data = bs.ToByteArray();
                        }
                    }
                    else
                    {
                        size = _model.Serialize(writer, obj, userState);
                        data = writer.ToByteArray();
                    }
                }
                Log.Debug("Serialized {l} bytes from {type} into {path}", size, typeof(T), path);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error(ERROR_FORMAT, path, ex.Message);
                return new() { Error = SerializationError.UnauthorizedAccess, Data = data, Size = size, Path = path, Compressed = compress };
            }
            catch (ArgumentNullException ex)
            {
                Log.Error(ERROR_FORMAT, path, ex.Message);
                return new() { Error = SerializationError.NullPath, Data = data, Size = size, Path = path, Compressed = compress };
            }
            catch (ArgumentException ex)
            {
                Log.Error(ERROR_FORMAT, path, ex.Message);
                return new() { Error = SerializationError.PathNotValid, Data = data, Size = size, Path = path, Compressed = compress };
            }
            catch (PathTooLongException ex)
            {
                Log.Error(ERROR_FORMAT, path, ex.Message);
                return new() { Error = SerializationError.PathTooLong, Data = data, Size = size, Path = path, Compressed = compress };
            }
            catch (DirectoryNotFoundException ex)
            {
                Log.Error(ERROR_FORMAT, path, ex.Message);
                return new() { Error = SerializationError.DirectoryNotFound, Data = data, Size = size, Path = path, Compressed = compress };
            }
            catch (NotSupportedException ex)
            {
                Log.Error(ERROR_FORMAT, path, ex.Message);
                return new() { Error = SerializationError.NotSupported, Data = data, Size = size, Path = path, Compressed = compress };
            }
            catch (Exception ex)
            {
                Log.Error(ERROR_FORMAT, path, ex.Message);
                return new() { Error = SerializationError.Serilog, Data = data, Size = size, Path = path, Compressed = compress };
            }
            return new() { Error = SerializationError.None, Data = data, Size = size, Path = path, Compressed = compress };
        }
        public static SerializationOutput Serialize(Type type, object? obj, string path, bool compress = false, object? userState = null)
        {
            long size = 0;
            byte[]? data = null;
            string fileName = compress ? type.Name + PROTOBUFER_COMPRESSED_BINARY : type.Name + PROTOBUFFER_BINARY;
            const string ERROR_FORMAT = "Serialization to path {path} failed: {ex}";

            if (_model == null)
            {
                Log.Warning(ERROR_FORMAT, path, "Serialization model is null.");
                return new() { Error = SerializationError.ModelNull, Data = data, Size = size, Path = path, Compressed = compress };
            }
            if (!_model.CanSerialize(type))
            {
                Log.Error(ERROR_FORMAT, $"Type {type.FullName} can't be serialized.");
                return new() { Error = SerializationError.NotSerializable, Data = default, Size = size, Path = string.Empty, Compressed = compress };
            }
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
                            size = _model.Serialize(bs, obj, userState);
                            data = bs.ToByteArray();
                        }
                    }
                    else
                    {
                        size = _model.Serialize(writer, obj, userState);
                        data = writer.ToByteArray();
                    }
                }
                Log.Debug("Serialized {l} bytes from {type} into {path}", size, type, path);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error(ERROR_FORMAT, path, ex.Message);
                return new() { Error = SerializationError.UnauthorizedAccess, Data = data, Size = size, Path = path, Compressed = compress };
            }
            catch (ArgumentNullException ex)
            {
                Log.Error(ERROR_FORMAT, path, ex.Message);
                return new() { Error = SerializationError.NullPath, Data = data, Size = size, Path = path, Compressed = compress };
            }
            catch (ArgumentException ex)
            {
                Log.Error(ERROR_FORMAT, path, ex.Message);
                return new() { Error = SerializationError.PathNotValid, Data = data, Size = size, Path = path, Compressed = compress };
            }
            catch (PathTooLongException ex)
            {
                Log.Error(ERROR_FORMAT, path, ex.Message);
                return new() { Error = SerializationError.PathTooLong, Data = data, Size = size, Path = path, Compressed = compress };
            }
            catch (DirectoryNotFoundException ex)
            {
                Log.Error(ERROR_FORMAT, path, ex.Message);
                return new() { Error = SerializationError.DirectoryNotFound, Data = data, Size = size, Path = path, Compressed = compress };
            }
            catch (NotSupportedException ex)
            {
                Log.Error(ERROR_FORMAT, path, ex.Message);
                return new() { Error = SerializationError.NotSupported, Data = data, Size = size, Path = path, Compressed = compress };
            }
            catch (Exception ex)
            {
                Log.Error(ERROR_FORMAT, path, ex.Message);
                return new() { Error = SerializationError.Serilog, Data = data, Size = size, Path = path, Compressed = compress };
            }
            return new() { Error = SerializationError.None, Data = data, Size = size, Path = path, Compressed = compress };
        }
        public static SerializationOutput Serialize<T>(T obj, bool compress = false, object? userState = null)
        {
            long size = 0;
            byte[] data = new byte[GZIP_BUFFER_SIZE]; ;
            const string ERROR_FORMAT = "Serialization failed: {ex}";

            if (_model == null)
            {
                Log.Warning(ERROR_FORMAT, "Serialization model is null.");
                return new() { Error = SerializationError.ModelNull, Data = data, Size = size, Path = string.Empty, Compressed = compress };
            }
            if (!_model.CanSerialize(typeof(T)))
            {
                Log.Error(ERROR_FORMAT, $"Type {typeof(T).FullName} can't be serialized.");
                return new() { Error = SerializationError.NotSerializable, Data = default, Size = size, Path = string.Empty, Compressed = compress };
            }
            try
            {
                using (MemoryStream ms = new MemoryStream(data, true))
                {
                    if (compress)
                    {
                        using (var gzip = new GZipStream(ms, CompressionMode.Compress, false))
                        using (var bs = new BufferedStream(gzip, GZIP_BUFFER_SIZE))
                        {
                            size = _model.Serialize(bs, obj, userState);
                        }
                    }
                    else
                    {
                        size = _model.Serialize(ms, obj, userState);
                    }
                }
                Log.Debug("Serialized {l} bytes from {type}", size, typeof(T));
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error(ERROR_FORMAT, ex.Message);
                return new() { Error = SerializationError.UnauthorizedAccess, Data = data, Size = size, Path = string.Empty, Compressed = compress };
            }
            catch (ArgumentNullException ex)
            {
                Log.Error(ERROR_FORMAT, ex.Message);
                return new() { Error = SerializationError.NullPath, Data = data, Size = size, Path = string.Empty, Compressed = compress };
            }
            catch (ArgumentException ex)
            {
                Log.Error(ERROR_FORMAT, ex.Message);
                return new() { Error = SerializationError.PathNotValid, Data = data, Size = size, Path = string.Empty, Compressed = compress };
            }
            catch (PathTooLongException ex)
            {
                Log.Error(ERROR_FORMAT, ex.Message);
                return new() { Error = SerializationError.PathTooLong, Data = data, Size = size, Path = string.Empty, Compressed = compress };
            }
            catch (DirectoryNotFoundException ex)
            {
                Log.Error(ERROR_FORMAT, ex.Message);
                return new() { Error = SerializationError.DirectoryNotFound, Data = data, Size = size, Path = string.Empty, Compressed = compress };
            }
            catch (NotSupportedException ex)
            {
                Log.Error(ERROR_FORMAT, ex.Message);
                return new() { Error = SerializationError.NotSupported, Data = data, Size = size, Path = string.Empty, Compressed = compress };
            }
            catch (Exception ex)
            {
                Log.Error(ERROR_FORMAT, ex.Message);
                return new() { Error = SerializationError.Serilog, Data = data, Size = size, Path = string.Empty, Compressed = compress };
            }
            return new() { Error = SerializationError.None, Data = data, Size = size, Path = string.Empty, Compressed = compress };
        }
        public static SerializationOutput Serialize(Type type, object? obj, bool compress = false, object? userState = null)
        {
            long size = 0;
            byte[] data = new byte[GZIP_BUFFER_SIZE]; ;
            const string ERROR_FORMAT = "Serialization failed: {ex}";

            if (_model == null)
            {
                Log.Warning(ERROR_FORMAT, "Serialization model is null.");
                return new() { Error = SerializationError.ModelNull, Data = data, Size = size, Path = string.Empty, Compressed = compress };
            }
            if (!_model.CanSerialize(type))
            {
                Log.Error(ERROR_FORMAT, $"Type {type.FullName} can't be serialized.");
                return new() { Error = SerializationError.NotSerializable, Data = default, Size = size, Path = string.Empty, Compressed = compress };
            }
            try
            {
                using (MemoryStream ms = new MemoryStream(data, true))
                {
                    if (compress)
                    {
                        using (var gzip = new GZipStream(ms, CompressionMode.Compress, false))
                        using (var bs = new BufferedStream(gzip, GZIP_BUFFER_SIZE))
                        {
                            size = _model.Serialize(bs, obj, userState);
                        }
                    }
                    else
                    {
                        size = _model.Serialize(ms, obj, userState);
                    }
                }
                Log.Debug("Serialized {l} bytes from {type}", size, type);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error(ERROR_FORMAT, ex.Message);
                return new() { Error = SerializationError.UnauthorizedAccess, Data = data, Size = size, Path = string.Empty, Compressed = compress };
            }
            catch (ArgumentNullException ex)
            {
                Log.Error(ERROR_FORMAT, ex.Message);
                return new() { Error = SerializationError.NullPath, Data = data, Size = size, Path = string.Empty, Compressed = compress };
            }
            catch (ArgumentException ex)
            {
                Log.Error(ERROR_FORMAT, ex.Message);
                return new() { Error = SerializationError.PathNotValid, Data = data, Size = size, Path = string.Empty, Compressed = compress };
            }
            catch (PathTooLongException ex)
            {
                Log.Error(ERROR_FORMAT, ex.Message);
                return new() { Error = SerializationError.PathTooLong, Data = data, Size = size, Path = string.Empty, Compressed = compress };
            }
            catch (DirectoryNotFoundException ex)
            {
                Log.Error(ERROR_FORMAT, ex.Message);
                return new() { Error = SerializationError.DirectoryNotFound, Data = data, Size = size, Path = string.Empty, Compressed = compress };
            }
            catch (NotSupportedException ex)
            {
                Log.Error(ERROR_FORMAT, ex.Message);
                return new() { Error = SerializationError.NotSupported, Data = data, Size = size, Path = string.Empty, Compressed = compress };
            }
            catch (Exception ex)
            {
                Log.Error(ERROR_FORMAT, ex.Message);
                return new() { Error = SerializationError.Serilog, Data = data, Size = size, Path = string.Empty, Compressed = compress };
            }
            return new() { Error = SerializationError.None, Data = data, Size = size, Path = string.Empty, Compressed = compress };
        }
        #endregion Serialization

        #region Deserialization
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
            IO,
            Serilog,
            ModelNull
        }
        public readonly record struct DeserializationOutput<T>(DeserializationError Error, T? Output, string Path, bool Decompressed);
        public readonly record struct DeserializationOutput(Type Type, DeserializationError Error, object? Output, string Path, bool Decompressed);
        public static DeserializationOutput<T> Deserialize<T>(string path, T? obj = default, bool decompress = false, object? userState = null)
        {
            T? deserialized = default;
            string fileName = decompress ? typeof(T).Name+PROTOBUFER_COMPRESSED_BINARY : typeof(T).Name+PROTOBUFFER_BINARY;
            const string ERROR_FORMAT = "Deserialization to path {path} failed: {ex}";

            if (_model == null)
            {
                Log.Warning(ERROR_FORMAT, path, "Serialization model is null.");
                return new() { Error = DeserializationError.ModelNull, Output = deserialized, Path = path };
            }
            if (!_model.CanSerialize(typeof(T)))
            {
                Log.Error(ERROR_FORMAT, $"Type {typeof(T).FullName} can't be deserialized.");
                return new() { Error = DeserializationError.NotSerializable, Output = deserialized, Path = path };
            }
            if (string.IsNullOrEmpty(path)) return new() { Error = DeserializationError.NullPath, Output = deserialized, Path = path };
            try
            {
                using (var reader = File.OpenRead(Path.Combine(path, fileName)))
                {
                    if (decompress)
                    {
                        using (var gzip = new GZipStream(reader, CompressionMode.Decompress, false))
                        using (var bs = new BufferedStream(gzip, GZIP_BUFFER_SIZE))
                        {
                            deserialized = _model.Deserialize(bs, obj, userState);
                        }
                    }
                    else
                        deserialized = _model.Deserialize(reader, obj, userState);
                }

                Log.Debug("Deserialized type {t} from {path}.", typeof(T).Name, path);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error(ERROR_FORMAT, path, ex.Message);
                return new() { Error = DeserializationError.UnauthorizedAccess, Output = deserialized, Path = path };
            }
            catch (ArgumentNullException ex)
            {
                Log.Error(ERROR_FORMAT, path, ex.Message);
                return new() { Error = DeserializationError.NullPath, Output = deserialized, Path = path };
            }
            catch (ArgumentException ex)
            {
                Log.Error(ERROR_FORMAT, path, ex.Message);
                return new() { Error = DeserializationError.PathNotValid, Output = deserialized, Path = path };
            }
            catch (PathTooLongException ex)
            {
                Log.Error(ERROR_FORMAT, path, ex.Message);
                return new() { Error = DeserializationError.PathTooLong, Output = deserialized, Path = path };
            }
            catch (DirectoryNotFoundException ex)
            {
                Log.Error(ERROR_FORMAT, path, ex.Message);
                return new() { Error = DeserializationError.DirectoryNotFound, Output = deserialized, Path = path };
            }
            catch (FileNotFoundException ex)
            {
                Log.Error(ERROR_FORMAT, path, ex.Message);
                return new() { Error = DeserializationError.FileNotFound, Output = deserialized, Path = path };
            }
            catch (NotSupportedException ex)
            {
                Log.Error(ERROR_FORMAT, path, ex.Message);
                return new() { Error = DeserializationError.NotSupported, Output = deserialized, Path = path };
            }
            catch (IOException ex)
            {
                Log.Error(ERROR_FORMAT, path, ex.Message);
                return new() { Error = DeserializationError.IO, Output = deserialized, Path = path };
            }
            catch (Exception ex)
            {
                Log.Error(ERROR_FORMAT, path, ex.Message);
                return new() { Error = DeserializationError.Serilog, Output = deserialized, Path = path };
            }
            return new() { Error = DeserializationError.None, Output = deserialized, Path = path };
        }
        public static DeserializationOutput Deserialize(Type type, string path, object? obj = default, bool decompress = false, object? userState = null)
        {
            object? deserialized = default;
            string fileName = decompress ? type.Name+PROTOBUFER_COMPRESSED_BINARY : type.Name+PROTOBUFFER_BINARY;
            const string ERROR_FORMAT = "Deserialization to path {path} failed: {ex}";

            if (_model == null)
            {
                Log.Warning(ERROR_FORMAT, path, "Serialization model is null.");
                return new() { Type = type, Error = DeserializationError.ModelNull, Output = deserialized, Path = path };
            }
            if (!_model.CanSerialize(type))
            {
                Log.Error(ERROR_FORMAT, $"Type {type.FullName} can't be deserialized.");
                return new() { Type = type, Error = DeserializationError.NotSerializable, Output = deserialized, Path = path };
            }
            if (string.IsNullOrEmpty(path)) return new() { Type = type, Error = DeserializationError.NullPath, Output = deserialized, Path = path };
            try
            {
                using (var reader = File.OpenRead(Path.Combine(path, fileName)))
                {
                    if (decompress)
                    {
                        using (var gzip = new GZipStream(reader, CompressionMode.Decompress, false))
                        using (var bs = new BufferedStream(gzip, GZIP_BUFFER_SIZE))
                        {
                            deserialized = _model.Deserialize(type, bs, obj, userState);
                        }
                    }
                    else
                        deserialized = _model.Deserialize(type, reader, obj, userState);
                }

                Log.Debug("Deserialized type {t} from {path}.", type.Name, path);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error(ERROR_FORMAT, path, ex.Message);
                return new() { Type = type, Error = DeserializationError.UnauthorizedAccess, Output = deserialized, Path = path };
            }
            catch (ArgumentNullException ex)
            {
                Log.Error(ERROR_FORMAT, path, ex.Message);
                return new() { Type = type, Error = DeserializationError.NullPath, Output = deserialized, Path = path };
            }
            catch (ArgumentException ex)
            {
                Log.Error(ERROR_FORMAT, path, ex.Message);
                return new() { Type = type, Error = DeserializationError.PathNotValid, Output = deserialized, Path = path };
            }
            catch (PathTooLongException ex)
            {
                Log.Error(ERROR_FORMAT, path, ex.Message);
                return new() { Type = type, Error = DeserializationError.PathTooLong, Output = deserialized, Path = path };
            }
            catch (DirectoryNotFoundException ex)
            {
                Log.Error(ERROR_FORMAT, path, ex.Message);
                return new() { Type = type, Error = DeserializationError.DirectoryNotFound, Output = deserialized, Path = path };
            }
            catch (FileNotFoundException ex)
            {
                Log.Error(ERROR_FORMAT, path, ex.Message);
                return new() { Type = type, Error = DeserializationError.FileNotFound, Output = deserialized, Path = path };
            }
            catch (NotSupportedException ex)
            {
                Log.Error(ERROR_FORMAT, path, ex.Message);
                return new() { Type = type, Error = DeserializationError.NotSupported, Output = deserialized, Path = path };
            }
            catch (IOException ex)
            {
                Log.Error(ERROR_FORMAT, path, ex.Message);
                return new() { Type = type, Error = DeserializationError.IO, Output = deserialized, Path = path };
            }
            catch (Exception ex)
            {
                Log.Error(ERROR_FORMAT, path, ex.Message);
                return new() { Type = type, Error = DeserializationError.Serilog, Output = deserialized, Path = path };
            }
            return new() { Type = type, Error = DeserializationError.None, Output = deserialized, Path = path };
        }
        public static DeserializationOutput<T> Deserialize<T>(byte[] serialized, bool decompress = false, object? userState = null)
        {
            T? deserialized = default;
            const string ERROR_FORMAT = "Deserialization failed: {ex}";

            if (_model == null)
            {
                Log.Warning(ERROR_FORMAT, "Serialization model is null.");
                return new() { Error = DeserializationError.ModelNull, Output = deserialized, Path = string.Empty };
            }
            if (!_model.CanSerialize(typeof(T)))
            {
                Log.Error(ERROR_FORMAT, $"Type {typeof(T).FullName} can't be deserialized.");
                return new() { Error = DeserializationError.NotSerializable, Output = deserialized, Path = string.Empty };
            }
            try
            {
                using (var ms = new MemoryStream(serialized, false))
                {
                    if (decompress)
                    {
                        using (var gzip = new GZipStream(ms, CompressionMode.Decompress, false))
                        using (var bs = new BufferedStream(gzip, GZIP_BUFFER_SIZE))
                        {
                            deserialized = _model.Deserialize<T>(bs, userState: userState);
                        }
                    }
                    else
                        deserialized = _model.Deserialize<T>(ms, userState: userState);
                }

                Log.Debug("Deserialized type {t}.", typeof(T).Name);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error(ERROR_FORMAT, ex.Message);
                return new() { Error = DeserializationError.UnauthorizedAccess, Output = deserialized, Path = string.Empty };
            }
            catch (ArgumentNullException ex)
            {
                Log.Error(ERROR_FORMAT, ex.Message);
                return new() { Error = DeserializationError.NullPath, Output = deserialized, Path = string.Empty };
            }
            catch (ArgumentException ex)
            {
                Log.Error(ERROR_FORMAT, ex.Message);
                return new() { Error = DeserializationError.PathNotValid, Output = deserialized, Path = string.Empty };
            }
            catch (PathTooLongException ex)
            {
                Log.Error(ERROR_FORMAT, ex.Message);
                return new() { Error = DeserializationError.PathTooLong, Output = deserialized, Path = string.Empty };
            }
            catch (DirectoryNotFoundException ex)
            {
                Log.Error(ERROR_FORMAT, ex.Message);
                return new() { Error = DeserializationError.DirectoryNotFound, Output = deserialized, Path = string.Empty };
            }
            catch (FileNotFoundException ex)
            {
                Log.Error(ERROR_FORMAT, ex.Message);
                return new() { Error = DeserializationError.FileNotFound, Output = deserialized, Path = string.Empty };
            }
            catch (NotSupportedException ex)
            {
                Log.Error(ERROR_FORMAT, ex.Message);
                return new() { Error = DeserializationError.NotSupported, Output = deserialized, Path = string.Empty };
            }
            catch (IOException ex)
            {
                Log.Error(ERROR_FORMAT, ex.Message);
                return new() { Error = DeserializationError.IO, Output = deserialized, Path = string.Empty };
            }
            catch (Exception ex)
            {
                Log.Error(ERROR_FORMAT, ex.Message);
                return new() { Error = DeserializationError.Serilog, Output = deserialized, Path = string.Empty };
            }
            return new() { Error = DeserializationError.None, Output = deserialized, Path = string.Empty };
        }
        public static DeserializationOutput Deserialize(Type type, byte[] serialized, bool decompress = false, object? userState = null)
        {
            object? deserialized = default;
            const string ERROR_FORMAT = "Deserialization failed: {ex}";

            if (_model == null)
            {
                Log.Warning(ERROR_FORMAT, "Serialization model is null.");
                return new() { Type = type, Error = DeserializationError.ModelNull, Output = deserialized, Path = string.Empty };
            }
            if (!_model.CanSerialize(type))
            {
                Log.Error(ERROR_FORMAT, $"Type {type.FullName} can't be deserialized.");
                return new() { Type = type, Error = DeserializationError.NotSerializable, Output = deserialized, Path = string.Empty };
            }
            try
            {
                using (var ms = new MemoryStream(serialized, false))
                {
                    if (decompress)
                    {
                        using (var gzip = new GZipStream(ms, CompressionMode.Decompress, false))
                        using (var bs = new BufferedStream(gzip, GZIP_BUFFER_SIZE))
                        {
                            deserialized = _model.Deserialize(type, bs, userState: userState);
                        }
                    }
                    else
                        deserialized = _model.Deserialize(type, ms, userState: userState);
                }

                Log.Debug("Deserialized type {t}.", type.Name);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error(ERROR_FORMAT, ex.Message);
                return new() {Type = type,
                    Error = DeserializationError.UnauthorizedAccess, Output = deserialized, Path = string.Empty };
            }
            catch (ArgumentNullException ex)
            {
                Log.Error(ERROR_FORMAT, ex.Message);
                return new() {Type = type,
                    Error = DeserializationError.NullPath, Output = deserialized, Path = string.Empty };
            }
            catch (ArgumentException ex)
            {
                Log.Error(ERROR_FORMAT, ex.Message);
                return new() {Type = type,
                    Error = DeserializationError.PathNotValid, Output = deserialized, Path = string.Empty };
            }
            catch (PathTooLongException ex)
            {
                Log.Error(ERROR_FORMAT, ex.Message);
                return new() {Type = type,
                    Error = DeserializationError.PathTooLong, Output = deserialized, Path = string.Empty };
            }
            catch (DirectoryNotFoundException ex)
            {
                Log.Error(ERROR_FORMAT, ex.Message);
                return new() {Type = type,
                    Error = DeserializationError.DirectoryNotFound, Output = deserialized, Path = string.Empty };
            }
            catch (FileNotFoundException ex)
            {
                Log.Error(ERROR_FORMAT, ex.Message);
                return new() {Type = type,
                    Error = DeserializationError.FileNotFound, Output = deserialized, Path = string.Empty };
            }
            catch (NotSupportedException ex)
            {
                Log.Error(ERROR_FORMAT, ex.Message);
                return new() {Type = type,
                    Error = DeserializationError.NotSupported, Output = deserialized, Path = string.Empty };
            }
            catch (IOException ex)
            {
                Log.Error(ERROR_FORMAT, ex.Message);
                return new() {Type = type,
                    Error = DeserializationError.IO, Output = deserialized, Path = string.Empty };
            }
            catch (Exception ex)
            {
                Log.Error(ERROR_FORMAT, ex.Message);
                return new() {Type = type,
                    Error = DeserializationError.Serilog, Output = deserialized, Path = string.Empty };
            }
            return new() {Type = type,
                Error = DeserializationError.None, Output = deserialized, Path = string.Empty };
        }
        #endregion Deserialization

        #region Change Serialization Model
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
        #endregion Change Serialization Model

        private static byte[] ToByteArray(this Stream stream)
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