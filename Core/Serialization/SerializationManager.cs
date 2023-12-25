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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using static ScapeCore.Core.Serialization.RuntimeModelFactory;

namespace ScapeCore.Core.Serialization
{
    public static class SerializationManager
    {
        private const int GZIP_BUFFER_SIZE = 64*1024;

        private const string PROTOBUFER_COMPRESSED_BINARY = ".pb.bin.gz";
        private const string PROTOBUFFER_BINARY = ".pb.bin";

        private static RuntimeModelFactory? _modelFactory = null;
        private static ScapeCoreSerializer? _serializer = null;
        private static ScapeCoreDeserializer? _deserializer = null;

        private readonly static Type[] _buildInTypes;
        internal static TypeModel? Model { get => _modelFactory?.Model; }
        public static RuntimeTypeModel? GetRuntimeClone() => _modelFactory?.Model?.DeepCopy();
        public static ScapeCoreSerializer? Serializer { get => _serializer; }
        public static ScapeCoreDeserializer? Deserializer { get => _deserializer; }

        static SerializationManager()
        {
            var assembly = typeof(SerializationManager).Assembly;
            var path = Path.Combine(assembly.Location[..assembly.Location.LastIndexOf('\\')], @"BuildinTypes.xml");
            var types = ParseXmlToTypesArray(path);
            var l = new List<Type>();
            var assemblyTypes = assembly.GetTypes();
            foreach (var type in assemblyTypes)
                if(types.Contains(type.Name)) l.Add(type);
            _buildInTypes = l.ToArray();
            _modelFactory = new RuntimeModelFactory(_buildInTypes);
            ConfigureSerializers(_modelFactory.Model!);
        }

        private static string[]? ParseXmlToTypesArray(string xmlFilePath)
        {
            var xmlDoc = XDocument.Load(xmlFilePath);
            XNamespace ns = "http://schemas.microsoft.com/powershell/2004/04";
            var types = xmlDoc?.Root?.Elements(ns + "Type").Select(type => type.Value).ToArray();
            return types;
        }

        private static void ConfigureSerializers(RuntimeTypeModel runtimeModel)
        {
            _serializer = new(runtimeModel, GZIP_BUFFER_SIZE, PROTOBUFFER_BINARY, PROTOBUFER_COMPRESSED_BINARY);
            _deserializer = new(runtimeModel, GZIP_BUFFER_SIZE, PROTOBUFFER_BINARY, PROTOBUFER_COMPRESSED_BINARY);
        }

        public static void AddType(Type type) => _modelFactory?.AddType(type);

        public static ChangeModelOutput ChangeModel(RuntimeTypeModel model)
        {
            var output = (_modelFactory ??= new(_buildInTypes)).ChangeModel(model);
            ConfigureSerializers(model);
            return output;
        }

        public static void ResetFactory() => _modelFactory = new RuntimeModelFactory(_buildInTypes);

    }
}