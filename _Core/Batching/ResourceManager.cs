using Microsoft.Xna.Framework.Graphics;
using ScapeCore.Core.Engine;
using ScapeCore.Targets;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ScapeCore.Core.Batching
{
    public unsafe static class ResourceManager
    {
        private static readonly SortedDictionary<string, GraphicsResource> _content = new();
        public static ReadOnlyDictionary<string, GraphicsResource> Content { get => new(_content); }

        public static void Ping() { return; }
        static ResourceManager() => LLAM.Instance.OnLoad += LoadAllReferencedResources;

        internal unsafe record struct ResourceLoadReference(string Name, bool* Loaded, List<Type> Dependencies);

        private unsafe static void LoadAllReferencedResources(object source, LoadBatchEventArgs args)
        {
            LLAM lLAM = (LLAM)source;
            List<bool> loadings = new List<bool>();
            HashSet<ResourceLoadReference> uniqueReferences = new();

            Console.WriteLine($"{source.GetHashCode()} {args.GetInfo()}");
            foreach (var type in ReflectiveEnumerator.GetEnumerableOfType<MonoBehaviour>())
            {
                foreach (var rsrcLoadAttr in Attribute.GetCustomAttributes(type).Where(attr => attr is ResourceLoadAttribute).Cast<ResourceLoadAttribute>())
                {
                    loadings.Add(false);
                    foreach (var loadName in rsrcLoadAttr.loadNames)
                    {
                        var loaded = loadings[^1];

                        loaded = uniqueReferences.Add(new(loadName, &loaded, new() { type }));
                        if (loaded) continue;
                        foreach (var reference in uniqueReferences.Where(refe => *refe.Loaded).Where(refe => refe.Name == loadName))
                            reference.Dependencies.Add(type);
                    }
                }
            }

            foreach (var reference in uniqueReferences.Where(refe => *refe.Loaded))
                Console.WriteLine($"{reference} type loaded resource {{{reference.Name}}}: {LoadResource(reference)}");
        }
        private static bool LoadResource(ResourceLoadReference reference) => _content.TryAdd(reference.Name, LLAM.Instance.Content.Load<Texture2D>(reference.Name));
    }



}