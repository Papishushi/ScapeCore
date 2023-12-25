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
using ScapeCore.Core.Batching.Tools;
using Serilog;
using System;
using System.IO;


namespace ScapeCore.Core.Serialization
{
    public sealed class ScapeCoreDeserializer : ScapeCoreSeralizationStreamer
    {
        public ScapeCoreDeserializer(RuntimeTypeModel model, int gzipBufferSize, string binName, string compressedBinName) :
            base(binName, compressedBinName, model, gzipBufferSize)
        { }

        public readonly record struct DeserializationOutput(Type Type, SerializationError Error, DeeplyMutableType Output, string Path, bool Decompressed);

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
                    deserialized = _model!.Deserialize(reader, obj, type);
            }
            Log.Verbose("Deserialized type {t} from {path}.", type.Name, path);
            output = new() { Error = SerializationError.None, Output = new(deserialized), Type = type, Path = path, Decompressed = decompress };
            return output;
        }
        private static MemoryStream GetCorrectMemoryStream(bool decompress, byte[] serialized, int lenght)
        {
            if (decompress) return new MemoryStream(serialized, false);
            else return new MemoryStream(serialized, 0, lenght, false);
        }
        private DeserializationOutput DeserializeFromMemory(Type type, byte[] serialized, long lenght, bool decompress, object obj)
        {
            DeserializationOutput output;
            object? deserialized = default;
            using (var ms = GetCorrectMemoryStream(decompress, serialized, (int)lenght))
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

            Log.Verbose("Deserialized type {t}.", type.Name);
            output = new() { Error = SerializationError.None, Output = new(deserialized), Type = type, Path = string.Empty, Decompressed = decompress };
            return output;
        }

        public DeserializationOutput Deserialize<T>(string path, T? obj = default, bool decompress = false, object? userState = null)
        {
            if (CheckForSerializationErrors(DESERIALIZATION_ERROR_FORMAT, typeof(T), path, decompress, out var output)) return new() { Error = output!.Value, Output = default, Type = typeof(T), Path = path, Decompressed = decompress };
            if (string.IsNullOrEmpty(path)) return new() { Error = SerializationError.NullPath, Output = default, Type = typeof(T), Path = path, Decompressed = decompress };
            try
            {
                return DeserializeFromPath(typeof(T), path, decompress, obj, userState);
            }
            catch (Exception ex)
            {
                return new() { Error = HandleSerializationError(DESERIALIZATION_ERROR_FORMAT, path, ex), Output = default, Type = typeof(T), Path = path, Decompressed = decompress };
            }
        }
        public DeserializationOutput Deserialize(Type type, string path, object? obj = default, bool decompress = false, object? userState = null)
        {
            if (CheckForSerializationErrors(DESERIALIZATION_ERROR_FORMAT, type, path, decompress, out var output)) return new() { Error =output!.Value, Output = default, Type = type, Path = path, Decompressed = decompress };
            if (string.IsNullOrEmpty(path)) return new() { Error = SerializationError.NullPath, Output = default, Type = type, Path = path, Decompressed = decompress };
            try
            {
                return DeserializeFromPath(type, path, decompress, obj, userState);
            }
            catch (Exception ex)
            {
                return new() { Error = HandleSerializationError(DESERIALIZATION_ERROR_FORMAT, path, ex), Output = default, Type = type, Path = path, Decompressed = decompress };
            }
        }
        public DeserializationOutput Deserialize<T>(byte[] serialized, long lenght, T? obj = default, bool decompress = false, object? userState = null)
        {
            if (CheckForSerializationErrors(DESERIALIZATION_ERROR_FORMAT, typeof(T), string.Empty, decompress, out var output)) return new() { Error = output!.Value, Output = default, Type = typeof(T), Path = string.Empty, Decompressed = decompress };
            try
            {
                return DeserializeFromMemory(typeof(T), serialized, lenght, decompress, obj);
            }
            catch (Exception ex)
            {
                return new() { Error = HandleSerializationError(DESERIALIZATION_ERROR_FORMAT, string.Empty, ex), Output = default, Type = typeof(T), Path = string.Empty, Decompressed = decompress };
            }
        }
        public DeserializationOutput Deserialize(Type type, byte[] serialized, long lenght, object? obj = default, bool decompress = false, object? userState = null)
        {
            if (CheckForSerializationErrors(DESERIALIZATION_ERROR_FORMAT, type, string.Empty, decompress, out var output)) return new() { Error = output!.Value, Output = default, Type = type, Path = string.Empty, Decompressed = decompress };
            try
            {
                return DeserializeFromMemory(type, serialized, lenght, decompress, obj);
            }
            catch (Exception ex)
            {
                return new() { Error = HandleSerializationError(DESERIALIZATION_ERROR_FORMAT, string.Empty, ex), Output = default, Type = type, Path = string.Empty, Decompressed = decompress };
            }
        }
    }
}