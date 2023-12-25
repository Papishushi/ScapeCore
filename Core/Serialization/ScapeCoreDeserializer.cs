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
 * ScapeCoreDeserializer.cs
 * This is the default deserializer used in ScapeCore. Includes
 * all the logic used for deserialize and compress objects at 
 * runtime.
 */

using MonoGame.Framework.Utilities.Deflate;
using ProtoBuf.Meta;
using Serilog;
using System;
using System.IO;


namespace ScapeCore.Core.Serialization
{
    public sealed class ScapeCoreDeserializer
    {
        private RuntimeTypeModel _model;
        private int _size;
        private string _binName;
        private string _compressedBinName;

        const string DESERIALIZATION_ERROR_FORMAT = "Deserialization to path {path} failed: {ex}";

        public ScapeCoreDeserializer(RuntimeTypeModel model, int gzipBufferSize, string binName, string compressedBinName)
        {
            _size = gzipBufferSize;
            _model = model;
            _binName = binName;
            _compressedBinName = compressedBinName;
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
            IO,
            Serilog,
            ModelNull
        }
        public readonly record struct DeserializationOutput(Type Type, DeserializationError Error, object? Output, string Path, bool Decompressed);
        private string GetFileName(Type type, bool compress) => compress ? type.Name + _compressedBinName : type.Name + _binName;
        private DeserializationError HandleDeserializationError(string path, Exception ex)
        {
            Log.Error(DESERIALIZATION_ERROR_FORMAT, path, ex.Message);
            DeserializationError error = ex switch
            {
                UnauthorizedAccessException => DeserializationError.UnauthorizedAccess,
                ArgumentNullException => DeserializationError.NullPath,
                ArgumentException => DeserializationError.PathNotValid,
                PathTooLongException => DeserializationError.PathTooLong,
                DirectoryNotFoundException => DeserializationError.DirectoryNotFound,
                FileNotFoundException => DeserializationError.FileNotFound,
                NotSupportedException => DeserializationError.NotSupported,
                IOException => DeserializationError.IO,
                _ => DeserializationError.Serilog,
            };
            return error;
        }
        private bool CheckForDeserializationErrors(Type type, string path, bool compress, out DeserializationError? result)
        {
            if (_model == null)
            {
                Log.Warning(DESERIALIZATION_ERROR_FORMAT, path, "Serialization model is null.");
                result = DeserializationError.ModelNull;
                return true;
            }
            if (!_model!.CanSerialize(type))
            {
                Log.Error(DESERIALIZATION_ERROR_FORMAT, $"Type {type.FullName} can't be serialized.");
                result = DeserializationError.NotSerializable;
                return true;
            }
            result = null;
            return false;
        }
        private DeserializationOutput DeserializeFromPath(Type type, string path, bool decompress, object obj, object? userState = null)
        {
            DeserializationOutput output;
            object? deserialized = default;
            using (var reader = File.OpenRead(Path.Combine(path, GetFileName(type, decompress))))
            {
                if (decompress)
                {
                    using (var gzip = new GZipStream(reader, CompressionMode.Decompress, false))
                    using (var bs = new BufferedStream(gzip, _size))
                    {
                        deserialized = _model!.Deserialize(bs, obj, userState);
                    }
                }
                else
                    deserialized = _model!.Deserialize(reader, obj, userState);
            }
            Log.Debug("Deserialized type {t} from {path}.", type.Name, path);
            output = new() { Error = DeserializationError.None, Output = deserialized, Type = type, Path = path, Decompressed = decompress };
            return output;
        }
        private DeserializationOutput DeserializeFromMemory(Type type, byte[] serialized, bool decompress, object obj)
        {
            DeserializationOutput output;
            object? deserialized = default;
            using (var ms = new MemoryStream(serialized, false))
            {
                if (decompress)
                {
                    using (var gzip = new GZipStream(ms, CompressionMode.Decompress, false))
                    using (var bs = new BufferedStream(gzip, _size))
                    {
                        deserialized = _model!.Deserialize(bs, obj, type);
                    }
                }
                else
                    deserialized = _model!.Deserialize(ms, obj, type);
            }

            Log.Debug("Deserialized type {t}.", type.Name);
            output = new() { Error = DeserializationError.None, Output = deserialized, Type = type, Path = string.Empty, Decompressed = decompress };
            return output;
        }
        public DeserializationOutput Deserialize<T>(string path, T? obj = default, bool decompress = false, object? userState = null)
        {
            if (CheckForDeserializationErrors(typeof(T), path, decompress, out var output)) return new() { Error = output!.Value, Output = default, Type = typeof(T), Path = path, Decompressed = decompress };
            if (string.IsNullOrEmpty(path)) return new() { Error = DeserializationError.NullPath, Output = default, Type = typeof(T), Path = path, Decompressed = decompress };
            try
            {
                return DeserializeFromPath(typeof(T), path, decompress, obj, userState);
            }
            catch (Exception ex)
            {
                return new() { Error = HandleDeserializationError(path, ex), Output = default, Type = typeof(T), Path = path, Decompressed = decompress };
            }
        }
        public DeserializationOutput Deserialize(Type type, string path, object? obj = default, bool decompress = false, object? userState = null)
        {
            if (CheckForDeserializationErrors(type, path, decompress, out var output)) return new() { Error =output!.Value, Output = default, Type = type, Path = path, Decompressed = decompress };
            if (string.IsNullOrEmpty(path)) return new() { Error = DeserializationError.NullPath, Output = default, Type = type, Path = path, Decompressed = decompress };
            try
            {
                return DeserializeFromPath(type, path, decompress, obj, userState);
            }
            catch (Exception ex)
            {
                return new() { Error = HandleDeserializationError(path, ex), Output = default, Type = type, Path = path, Decompressed = decompress };
            }
        }
        public DeserializationOutput Deserialize<T>(byte[] serialized, T? obj = default, bool decompress = false, object? userState = null)
        {
            if (CheckForDeserializationErrors(typeof(T), string.Empty, decompress, out var output)) return new() { Error = output!.Value, Output = default, Type = typeof(T), Path = string.Empty, Decompressed = decompress };
            try
            {
                return DeserializeFromMemory(typeof(T), serialized, decompress, obj);
            }
            catch (Exception ex)
            {
                return new() { Error = HandleDeserializationError(string.Empty, ex), Output = default, Type = typeof(T), Path = string.Empty, Decompressed = decompress };
            }
        }
        public DeserializationOutput Deserialize(Type type, byte[] serialized, object? obj = default, bool decompress = false, object? userState = null)
        {
            if (CheckForDeserializationErrors(type, string.Empty, decompress, out var output)) return new() { Error = output!.Value, Output = default, Type = type, Path = string.Empty, Decompressed = decompress };
            try
            {
                return DeserializeFromMemory(type, serialized, decompress, obj);
            }
            catch (Exception ex)
            {
                return new() { Error = HandleDeserializationError(string.Empty, ex), Output = default, Type = type, Path = string.Empty, Decompressed = decompress };
            }
        }
    }
}