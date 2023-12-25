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
 * RuntimeModelFactory.cs
 * An abstraction for a ProtoBuffer RuntimeTypeModel used on the
 * Serialization Manager.
 */

using ProtoBuf.Meta;
using ScapeCore.Core.Batching.Tools;
using Serilog;
using System;
using System.Linq;
using System.Reflection;


namespace ScapeCore.Core.Serialization
{
    public sealed class RuntimeModelFactory
    {
        private const string SCAPE_CORE_NAME = "ScapeCore";
        private const int FIELD_PROTOBUF_INDEX = 1;
        private const int SUBTYPE_PROTOBUF_INDEX = 556;
        private const string FIELD_ERROR_MESSAGE = "Serialization Manager tried to configure an object/dynamic field named {field} from Type {type}," +
                    " serializer does not support deeply mutable types, try changing field type to {dmtName}.";
        private const string PROPERTY_ERROR_MESSAGE = "Serialization Manager tried to configure an object/dynamic property named {property} from Type {type}," +
                            " serializer does not support deeply mutable types, try changing property type to {dmtName}.";

        private RuntimeTypeModel? _model = null;
        public RuntimeTypeModel? Model { get => _model; }

        public RuntimeModelFactory(Type[] types)
        {
            var runtimeModel = CreateRuntimeModel();
            foreach (var type in types)
                ConfigureType(runtimeModel, type);
            runtimeModel.MakeDefault();
            _model = runtimeModel;
        }

        public void AddType(Type type)
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
            SetTypeFields(metaType, type, ref fieldIndex);
            SetSubType(type, _model);
            if (type == typeof(DeeplyMutableType) || type.BaseType == typeof(DeeplyMutableType)) return;
            SetTypeProperties(metaType, type, ref fieldIndex);
        }

        private RuntimeTypeModel CreateRuntimeModel()
        {
            var runtimeModel = RuntimeTypeModel.Create(SCAPE_CORE_NAME);
            runtimeModel.AllowParseableTypes = true;
            runtimeModel.AutoAddMissingTypes = true;
            runtimeModel.MaxDepth = 100;
            return runtimeModel;
        }

        private void ConfigureType(RuntimeTypeModel runtimeModel, Type type)
        {
            var fieldIndex = FIELD_PROTOBUF_INDEX;
            var metaType = runtimeModel.Add(type, false);
            Log.Debug("Type {type} was configured for [de]Serialization...", type.Name);
            if (type.IsEnum) return;
            SetFieldsSubTypesAndProperties(runtimeModel, type, metaType, ref fieldIndex);
        }

        private void SetFieldsSubTypesAndProperties(RuntimeTypeModel runtimeModel, Type type, MetaType metaType, ref int fieldIndex)
        {
            SetTypeFields(metaType, type, ref fieldIndex);
            SetSubType(type, runtimeModel);
            if (CheckForDeeplyMutableType(runtimeModel, type)) return;
            SetTypeProperties(metaType, type, ref fieldIndex);
        }

        private void SetTypeFields(MetaType metaType, Type type, ref int fieldIndex)
        {
            foreach (var field in type.GetFields())
            {
                try
                {
                    if (field.FieldType.Name == typeof(object).Name)
                    {
                        Log.Warning(FIELD_ERROR_MESSAGE, field.Name, type.Name, typeof(DeeplyMutableType).FullName);
                        continue;
                    }
                    AddField(metaType, field, type, ref fieldIndex);
                }
                catch (Exception ex)
                {
                    Log.Warning("Serialization Manager can not determine type of field {property} from {type}.", field.Name, type);
                    Log.Verbose("{ex}", ex.Message);
                }
            }
        }

        private void AddField(MetaType metaType, FieldInfo field, Type type, ref int fieldIndex)
        {
            metaType.Add(fieldIndex++, field.Name);
            Log.Verbose("\tField [{i}]{field}[{t}] from Type {type}", fieldIndex - 1, field.Name, field.FieldType, type.Name);
        }

        private void SetSubType(Type type, RuntimeTypeModel runtimeModel)
        {
            foreach (var runtimeType in runtimeModel.GetTypes().Cast<MetaType>())
            {
                if (runtimeType.Type != type.BaseType) continue;
                var subTypeIndex = runtimeType.GetSubtypes().Length + SUBTYPE_PROTOBUF_INDEX;
                runtimeType.AddSubType(subTypeIndex, type);
                break;
            }
        }

        private bool CheckForDeeplyMutableType(RuntimeTypeModel runtimeModel, Type type)
        {
            if (type == typeof(DeeplyMutableType) || type.BaseType == typeof(DeeplyMutableType))
            {
                runtimeModel.MakeDefault();
                _model = runtimeModel;
                return true;
            }
            return false;
        }

        private void SetTypeProperties(MetaType metaType, Type type, ref int fieldIndex)
        {
            foreach (var property in type.GetProperties())
            {
                try
                {
                    if (property.PropertyType.Name == typeof(object).Name)
                    {
                        Log.Warning(PROPERTY_ERROR_MESSAGE, property.Name, type.Name, typeof(DeeplyMutableType).FullName);
                        continue;
                    }
                    AddProperty(metaType, property, type, ref fieldIndex);
                }
                catch (Exception ex)
                {
                    Log.Warning("Serialization Manager can not determine type of property {property} from {type}.", property.Name, type);
                    Log.Verbose("{ex}", ex.Message);
                }
            }
        }

        private void AddProperty(MetaType metaType, PropertyInfo property, Type type, ref int fieldIndex)
        {
            metaType.Add(fieldIndex++, property.Name);
            Log.Verbose("\tProperty [{i}]{property}[{t}] from Type {type}", fieldIndex - 1, property.Name, property.PropertyType, type.Name);
        }

        #region Change Serialization Model
        public enum ChangeModelError
        {
            None,
            NullModel
        }
        public readonly record struct ChangeModelOutput(ChangeModelError Error);
        public ChangeModelOutput ChangeModel(RuntimeTypeModel model)
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
    }
}