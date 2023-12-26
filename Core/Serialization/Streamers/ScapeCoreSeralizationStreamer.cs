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
 * ScapeCoreSerializationStreamer.cs
 * This is the base class from the serializers
 * used by ScapeCore.
 */

using ProtoBuf.Meta;
using Serilog;
using System;
using System.IO;


namespace ScapeCore.Core.Serialization.Streamers
{
    public abstract class ScapeCoreSeralizationStreamer : GZipStreamer
    {
        protected readonly string _binName;
        protected readonly string _compressedBinName;
        protected readonly RuntimeTypeModel _model;

        protected const string DESERIALIZATION_ERROR_FORMAT = "Deserialization to path {path} failed: {ex}";
        protected const string SERIALIZATION_ERROR_FORMAT = "Serialization to path {path} failed: {ex}";

        public enum SerializationError
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

        protected ScapeCoreSeralizationStreamer(string binName, string compressedBinName, RuntimeTypeModel model, int size) : base(size)
        {
            _binName = binName;
            _compressedBinName = compressedBinName;
            _model = model;
        }
        protected string GetFileName(Type type, bool compress) => compress ? type.Name + _compressedBinName : type.Name + _binName;
        protected bool CheckForSerializationErrors(string errorFormat, Type type, string path, bool compress, out SerializationError? result)
        {
            if (_model == null)
            {
                Log.Warning(errorFormat, path, "Serialization model is null.");
                result = SerializationError.ModelNull;
                return true;
            }
            if (!_model!.CanSerialize(type))
            {
                Log.Error(errorFormat, $"Type {type.FullName} can't be serialized.");
                result = SerializationError.NotSerializable;
                return true;
            }
            result = null;
            return false;
        }
        protected static SerializationError HandleSerializationError(string errorFormat, string path, Exception ex)
        {
            Log.Error(errorFormat, path, ex.Message);
            SerializationError error = ex switch
            {
                UnauthorizedAccessException => SerializationError.UnauthorizedAccess,
                ArgumentNullException => SerializationError.NullPath,
                ArgumentException => SerializationError.PathNotValid,
                PathTooLongException => SerializationError.PathTooLong,
                DirectoryNotFoundException => SerializationError.DirectoryNotFound,
                FileNotFoundException => SerializationError.FileNotFound,
                NotSupportedException => SerializationError.NotSupported,
                IOException => SerializationError.IO,
                _ => SerializationError.Serilog,
            };
            return error;
        }
    }
}