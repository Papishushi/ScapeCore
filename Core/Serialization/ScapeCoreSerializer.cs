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
 * ScapeCoreserializer.cs
 * This is the default serializer used in ScapeCore. Includes
 * all the logic used for serialize and compress objects at 
 * runtime.
 */

using MonoGame.Framework.Utilities.Deflate;
using ProtoBuf.Meta;
using ScapeCore.Core.Serialization.Tools;
using Serilog;
using System;
using System.IO;


namespace ScapeCore.Core.Serialization
{
    public sealed class ScapeCoreSerializer
    {
        private RuntimeTypeModel _model;
        private int _size;
        private string _binName;
        private string _compressedBinName;

        const string SERIALIZATION_ERROR_FORMAT = "Serialization to path {path} failed: {ex}";

        public ScapeCoreSerializer(RuntimeTypeModel model, int gzipBufferSize, string binName, string compressedBinName)
        {
            _size = gzipBufferSize;
            _model = model;
            _binName = binName;
            _compressedBinName = compressedBinName;
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
            NotSupported,
            Serilog,
            ModelNull
        }
        public readonly record struct SerializationOutput(SerializationError Error, byte[]? Data, long Size, string Path, bool Compressed);
        private string GetFileName(Type type, bool compress) => compress ? type.Name + _compressedBinName : type.Name + _binName;
        private SerializationError HandleSerializationError(string path, Exception ex)
        {
            Log.Error(SERIALIZATION_ERROR_FORMAT, path, ex.Message);
            SerializationError error = ex switch
            {
                UnauthorizedAccessException => SerializationError.UnauthorizedAccess,
                ArgumentNullException => SerializationError.NullPath,
                ArgumentException => SerializationError.PathNotValid,
                PathTooLongException => SerializationError.PathTooLong,
                DirectoryNotFoundException => SerializationError.DirectoryNotFound,
                NotSupportedException => SerializationError.NotSupported,
                _ => SerializationError.Serilog,
            };
            return error;
        }
        private SerializationOutput SerializeToPath(Type type, string path, bool compress, object obj, object? userState = null)
        {
            long size = 0;
            byte[]? data = null;
            SerializationOutput output;

            using (var writer = File.Open(Path.Combine(path, GetFileName(type, compress)), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read))
            {
                if (compress)
                {
                    using (var gzip = new GZipStream(writer, CompressionMode.Compress, false))
                    using (var bs = new BufferedStream(gzip, _size))
                    {
                        size = _model!.Serialize(bs, obj, userState);
                        data = bs.ToByteArray();
                    }
                }
                else
                {
                    size = _model!.Serialize(writer, obj, userState);
                    data = writer.ToByteArray();
                }
            }
            Log.Debug("Serialized {l} bytes from {type} into {path}", size, type, path);
            output = new() { Error = SerializationError.None, Data = data, Size = size, Path = path, Compressed = compress };
            return output;
        }
        private SerializationOutput SerializeToMemory(Type type, bool compress, object obj, object? userState = null)
        {
            long size = 0;
            byte[] data = new byte[_size];
            SerializationOutput output;
            using (MemoryStream ms = new MemoryStream(data, true))
            {
                if (compress)
                {
                    using (var gzip = new GZipStream(ms, CompressionMode.Compress, false))
                    using (var bs = new BufferedStream(gzip, _size))
                    {
                        size = _model!.Serialize(bs, obj, userState);
                    }
                }
                else
                {
                    size = _model!.Serialize(ms, obj, userState);
                }
            }
            Log.Debug("Serialized {l} bytes from {type}", size, type);
            output = new() { Error = SerializationError.None, Data = data, Size = size, Path = string.Empty, Compressed = compress };
            return output;
        }
        private bool CheckForSerializationErrors(Type type, string path, bool compress, out SerializationError? result)
        {
            if (_model == null)
            {
                Log.Warning(SERIALIZATION_ERROR_FORMAT, path, "Serialization model is null.");
                result = SerializationError.ModelNull;
                return true;
            }
            if (!_model!.CanSerialize(type))
            {
                Log.Error(SERIALIZATION_ERROR_FORMAT, $"Type {type.FullName} can't be serialized.");
                result = SerializationError.NotSerializable;
                return true;
            }
            result = null;
            return false;
        }
        public SerializationOutput Serialize<T>(T obj, string path, bool compress = false, object? userState = null)
        {
            if (CheckForSerializationErrors(typeof(T), path, compress, out var output)) return new() { Error = output ?? default, Data = default, Size = 0, Path = path, Compressed = compress };
            if (string.IsNullOrEmpty(path)) return new() { Error = SerializationError.NullPath, Data = default, Size = 0, Path = path, Compressed = compress };
            try
            {
                return SerializeToPath(typeof(T), path, compress, obj, userState);
            }
            catch (Exception ex)
            {
                return new() { Error = HandleSerializationError(path, ex), Data = default, Size = 0, Path = path, Compressed = compress };
            }
        }
        public SerializationOutput Serialize(Type type, object? obj, string path, bool compress = false, object? userState = null)
        {
            if (CheckForSerializationErrors(type, path, compress, out var output)) return new() { Error = output ?? default, Data = default, Size = 0, Path = path, Compressed = compress };
            if (string.IsNullOrEmpty(path)) return new() { Error = SerializationError.NullPath, Data = default, Size = 0, Path = path, Compressed = compress };
            try
            {
                return SerializeToPath(type, path, compress, obj, userState);
            }
            catch (Exception ex)
            {
                return new() { Error = HandleSerializationError(path, ex), Data = default, Size = 0, Path = path, Compressed = compress };
            }
        }
        public SerializationOutput Serialize<T>(T obj, bool compress = false, object? userState = null)
        {
            if (CheckForSerializationErrors(typeof(T), string.Empty, compress, out var output)) return new() { Error = output ?? default, Data = default, Size = 0, Path = string.Empty, Compressed = compress };
            try
            {
                return SerializeToMemory(typeof(T), compress, obj, userState);
            }
            catch (UnauthorizedAccessException ex)
            {
                return new() { Error = HandleSerializationError(string.Empty, ex), Data = default, Size = 0, Path = string.Empty, Compressed = compress };
            }
        }
        public SerializationOutput Serialize(Type type, object? obj, bool compress = false, object? userState = null)
        {
            if (CheckForSerializationErrors(type, string.Empty, compress, out var output)) return new() { Error = output ?? default, Data = default, Size = 0, Path = string.Empty, Compressed = compress };
            try
            {
                return SerializeToMemory(type, compress, obj, userState);
            }
            catch (UnauthorizedAccessException ex)
            {
                return new() { Error = HandleSerializationError(string.Empty, ex), Data = default, Size = 0, Path = string.Empty, Compressed = compress };
            }
        }
    }
}