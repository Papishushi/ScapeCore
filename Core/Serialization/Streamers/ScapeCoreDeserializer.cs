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

using ProtoBuf.Meta;
using ScapeCore.Core.Batching.Tools;
using ScapeCore.Core.Serialization.Tools;
using Serilog;
using System;
using System.IO;


namespace ScapeCore.Core.Serialization.Streamers
{
    public sealed class ScapeCoreDeserializer : ScapeCoreSeralizationStreamer
    {
        public ScapeCoreDeserializer(RuntimeTypeModel model, int gzipBufferSize, string binName, string compressedBinName) :
            base(binName, compressedBinName, model, gzipBufferSize)
        { }

        public readonly record struct DeserializationOutput(Type Type, SerializationError Error, DeeplyMutableType Output, string Path, bool Decompressed);

        private DeserializationOutput DeserializeFromPath(Type type, string path, bool decompress, object? obj)
        {
            DeserializationOutput output;
            object? deserialized = default;


            using (var reader = File.OpenRead(Path.Combine(path, GetFileName(type, decompress))))
            {
                byte[] decompressed = reader.ToByteArray();

                if (decompress)
                    decompressed = Decompress(decompressed);

                using (var ms = new MemoryStream(decompressed, 0, decompressed.Length, false))
                {
                    deserialized = _model!.DeserializeWithLengthPrefix(ms, obj, type, ProtoBuf.PrefixStyle.Base128, 0);
                }
            }

            Log.Verbose("Deserialized type {t} from {path}.", type.Name, path);
            output = new() { Error = SerializationError.None, Output = new(deserialized), Type = type, Path = path, Decompressed = decompress };
            return output;
        }

        private DeserializationOutput DeserializeFromMemory(Type type, byte[] serialized, bool decompress, object? obj)
        {
            DeserializationOutput output;
            object? deserialized = default;
            byte[] decompressed = serialized;

            if (decompress)
                decompressed = Decompress(serialized);

            using (var ms = new MemoryStream(decompressed, 0, decompressed.Length, false))
            {
                deserialized = _model!.DeserializeWithLengthPrefix(ms, obj, type, ProtoBuf.PrefixStyle.Base128, 0);
            }

            Log.Verbose("Deserialized type {t}.", type.Name);
            output = new() { Error = SerializationError.None, Output = new(deserialized), Type = type, Path = string.Empty, Decompressed = decompress };
            return output;
        }

        public DeserializationOutput Deserialize<T>(string path, T? obj = default, bool decompress = false)
        {
            if (CheckForSerializationErrors(DESERIALIZATION_ERROR_FORMAT, typeof(T), path, decompress, out var output)) return new() { Error = output!.Value, Output = new(), Type = typeof(T), Path = path, Decompressed = decompress };
            if (string.IsNullOrEmpty(path)) return new() { Error = SerializationError.NullPath, Output = new(), Type = typeof(T), Path = path, Decompressed = decompress };
            try
            {
                return DeserializeFromPath(typeof(T), path, decompress, obj);
            }
            catch (Exception ex)
            {
                return new() { Error = HandleSerializationError(DESERIALIZATION_ERROR_FORMAT, path, ex), Output = new(), Type = typeof(T), Path = path, Decompressed = decompress };
            }
        }
        public DeserializationOutput Deserialize(Type type, string path, object? obj = default, bool decompress = false)
        {
            if (CheckForSerializationErrors(DESERIALIZATION_ERROR_FORMAT, type, path, decompress, out var output)) return new() { Error =output!.Value, Output = new(), Type = type, Path = path, Decompressed = decompress };
            if (string.IsNullOrEmpty(path)) return new() { Error = SerializationError.NullPath, Output = new(), Type = type, Path = path, Decompressed = decompress };
            try
            {
                return DeserializeFromPath(type, path, decompress, obj);
            }
            catch (Exception ex)
            {
                return new() { Error = HandleSerializationError(DESERIALIZATION_ERROR_FORMAT, path, ex), Output = new(), Type = type, Path = path, Decompressed = decompress };
            }
        }
        public DeserializationOutput Deserialize<T>(byte[] serialized, T? obj = default, bool decompress = false)
        {
            if (CheckForSerializationErrors(DESERIALIZATION_ERROR_FORMAT, typeof(T), string.Empty, decompress, out var output)) return new() { Error = output!.Value, Output = new(), Type = typeof(T), Path = string.Empty, Decompressed = decompress };
            try
            {
                return DeserializeFromMemory(typeof(T), serialized, decompress, obj);
            }
            catch (Exception ex)
            {
                return new() { Error = HandleSerializationError(DESERIALIZATION_ERROR_FORMAT, string.Empty, ex), Output = new(), Type = typeof(T), Path = string.Empty, Decompressed = decompress };
            }
        }
        public DeserializationOutput Deserialize(Type type, byte[] serialized, object? obj = default, bool decompress = false)
        {
            if (CheckForSerializationErrors(DESERIALIZATION_ERROR_FORMAT, type, string.Empty, decompress, out var output)) return new() { Error = output!.Value, Output = new(), Type = type, Path = string.Empty, Decompressed = decompress };
            try
            {
                return DeserializeFromMemory(type, serialized, decompress, obj);
            }
            catch (Exception ex)
            {
                return new() { Error = HandleSerializationError(DESERIALIZATION_ERROR_FORMAT, string.Empty, ex), Output = new(), Type = type, Path = string.Empty, Decompressed = decompress };
            }
        }
    }
}