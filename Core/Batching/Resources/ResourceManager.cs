using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using ScapeCore.Core.Batching.Events;
using ScapeCore.Core.Batching.Tools;
using ScapeCore.Core.Engine;
using ScapeCore.Targets;
using Serilog;
using Serilog.Core;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ScapeCore.Core.Batching.Resources
{
    public static class ResourceManager
    {
        private static readonly ResourceDependencyTree _tree = new();

        public static ResourceDependencyTree Content { get => _tree; }

        internal static void Ping() { return; }
        static ResourceManager() => LLAM.Instance.OnLoad += LoadAllReferencedResources;

        public static T GetResource<T>(string key) => (T)Content.Dependencies[new(key,typeof(T))].resource;

        private static void LoadAllReferencedResources(object source, LoadBatchEventArgs args)
        {
            Log.Debug($"{source.GetHashCode()} {args.GetInfo()}");

            foreach (var type in ReflectiveEnumerator.GetEnumerableOfType<MonoBehaviour>())
            {
                foreach (var rsrcLoadAttr in Attribute.GetCustomAttributes(type).Where(attr => attr is ResourceLoadAttribute).Cast<ResourceLoadAttribute>())
                {
                    foreach (var loadName in rsrcLoadAttr.names)
                    {
                        var info = new ResourceInfo(loadName, rsrcLoadAttr.loadType);
                        if (_tree.ContainsResource(info))
                        {
                            _tree.GetResource(info).dependencies.Add(type);
                        }
                        else
                        {
                            var method = typeof(ContentManager).GetMethod(nameof(LLAM.Instance.Content.Load));
                            method = method.MakeGenericMethod(info.TargetType);
                            var result = method.Invoke(LLAM.Instance.Content, new object[1] { info.ResourceName });
                            dynamic changedObject = Convert.ChangeType(result, info.TargetType);
                            _tree.Add(info, type, changedObject);
                        }
                    }
                }
            }

            foreach (var dependency in _tree.Dependencies)
                Log.Debug(($"{string.Join(',', dependency.Value.dependencies)} types loaded resource {{{dependency.Key.ResourceName}}} of type {{{dependency.Key.TargetType}}}"));
        }
    }
}