using Microsoft.Xna.Framework.Graphics;
using ScapeCore.Core.Engine;
using ScapeCore.Targets;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace ScapeCore.Core.Batching
{
    public class RuntimeBool
    {
        public RuntimeBool(bool value)
        {
            _value=value;
        }

        public static implicit operator bool(RuntimeBool runtimeBool) => runtimeBool._value;
        public static implicit operator RuntimeBool(bool staticBool) => new(staticBool);

        public bool Value { get => _value; set => _value = value; }
        private bool _value;

        public override bool Equals(object obj)
        {
            if (obj is RuntimeBool temp)
            {
                if (temp._value == _value)
                    return true;
                else
                    return false;
            }
            if (obj == null)
                return true;
            else
                return false;
        }

        public override int GetHashCode() => _value.GetHashCode();

        public override string ToString() => _value.ToString();
    }
    public unsafe static class ResourceManager
    {
        private static readonly SortedDictionary<string, GraphicsResource> _content = new();
        public static ReadOnlyDictionary<string, GraphicsResource> Content { get => new(_content); }

        internal static void Ping() { return; }
        static ResourceManager() => LLAM.Instance.OnLoad += LoadAllReferencedResources;

        private static void LoadAllReferencedResources(object source, LoadBatchEventArgs args)
        {
            Dictionary<string, List<Type>> dependencies = new();

            Console.WriteLine($"{source.GetHashCode()} {args.GetInfo()}");

            foreach (var type in ReflectiveEnumerator.GetEnumerableOfType<MonoBehaviour>())
            {
                foreach (var rsrcLoadAttr in Attribute.GetCustomAttributes(type).Where(attr => attr is ResourceLoadAttribute).Cast<ResourceLoadAttribute>())
                {
                    foreach (var loadName in rsrcLoadAttr.loadNames)
                    {
                        if (dependencies.ContainsKey(loadName))
                            dependencies[loadName].Add(type);
                        else
                            dependencies.Add(loadName, new() { type });
                    }
                }
            }

            var strBuilder = new StringBuilder();
            foreach (var dependency in dependencies)
            {
                strBuilder.Clear();
                strBuilder.Append($"{string.Join(',', dependency.Value)} type loaded resource {{{dependency.Key}}}: {LoadResource(dependency.Key)}");
                Console.WriteLine(strBuilder);
            }
        }
        private static bool LoadResource(string reference) => _content.TryAdd(reference, LLAM.Instance.Content.Load<Texture2D>(reference));
    }



}