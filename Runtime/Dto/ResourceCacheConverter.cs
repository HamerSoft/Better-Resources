using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

namespace HamerSoft.BetterResources.Dto
{
    internal class ResourceCacheConverter : JsonConverter<ResourceCache>
    {
        public override void WriteJson(JsonWriter writer, ResourceCache value, JsonSerializer serializer)
        {
            if (value == null)
                return;

            var dto = new ResourceCacheDto(value.CreatedAt.ToString(), value.Resources, value.DirectorySeparatorChar);
            serializer.Serialize(writer, dto);
        }

        public override ResourceCache ReadJson(JsonReader reader, Type objectType, ResourceCache existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            var dto = serializer.Deserialize<ResourceCacheDto>(reader);
            bool isDirectorySeparatorEqual = Path.DirectorySeparatorChar == dto.DirectorySeparator;
            ConcurrentDictionary<string, Assembly> assemblies = new ConcurrentDictionary<string, Assembly>();
            ConcurrentDictionary<int, Type> typeCache = new ConcurrentDictionary<int, Type>();

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                assemblies.TryAdd(assembly.FullName, assembly);

            var resources = dto.Resources.AsParallel().Select(resource => new ResourceAsset(resource.Guid,
                isDirectorySeparatorEqual
                    ? resource.FullPath
                    : resource.FullPath.Replace(dto.DirectorySeparator, Path.DirectorySeparatorChar), resource.Package,
                resource.Components.Select(component =>
                {
                    if (!typeCache.TryGetValue(component.GetHashCode(), out var type))
                    {
                        type = Type.GetType(component);
                        typeCache.AddOrUpdate(component.GetHashCode(), type, (_, existingType) => existingType);
                    }

                    return type;
                }))).ToList();

            assemblies = null;
            typeCache = null;
            return new ResourceCache(resources, DateTime.Parse(dto.CreatedAt), Path.DirectorySeparatorChar);
        }
    }
}