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
using System;
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
        internal static TypeModel? Model { get => _modelFactory?.Model; }
        public static RuntimeTypeModel? GetRuntimeClone() => _modelFactory?.Model?.DeepCopy();
        public static ScapeCoreSerializer? Serializer { get => _serializer; }
        public static ScapeCoreDeserializer? Deserializer { get => _deserializer; }

        static SerializationManager()
        {
            _modelFactory = new RuntimeModelFactory(_types);
            ConfigureModelAndSerializers(_modelFactory.Model!);
        }

        private static void ConfigureModelAndSerializers(RuntimeTypeModel runtimeModel)
        {
            _serializer = new(runtimeModel, GZIP_BUFFER_SIZE, PROTOBUFFER_BINARY, PROTOBUFER_COMPRESSED_BINARY);
            _deserializer = new(runtimeModel, GZIP_BUFFER_SIZE, PROTOBUFFER_BINARY, PROTOBUFER_COMPRESSED_BINARY);
        }

        public static void AddType(Type type) => _modelFactory?.AddType(type);

        public static ChangeModelOutput ChangeModel(RuntimeTypeModel model) => (_modelFactory ??= new(_types)).ChangeModel(model);

    }
}