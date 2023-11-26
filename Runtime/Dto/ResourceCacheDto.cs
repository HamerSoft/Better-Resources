using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace HamerSoft.BetterResources.Dto
{
    [Serializable]
    internal class ResourceCacheDto
    {
        [Serializable]
        internal class ResourceAssetDto
        {
            [JsonProperty] internal string Guid { get; set; }
            [JsonProperty] internal string FullPath { get; set; }
            [JsonProperty] internal string Package { get; set; }
            [JsonProperty] internal HashSet<string> Components { get; set; }

            [JsonConstructor]
            internal ResourceAssetDto()
            {
            }

            internal ResourceAssetDto(string guid, string path, string packageName,
                HashSet<Type> components)
            {
                Guid = guid;
                FullPath = path;
                Package = packageName;
                Components = new HashSet<string>(components.Select(c => c.AssemblyQualifiedName));
            }
        }

        [JsonProperty] internal string CreatedAt;
        [JsonProperty] internal List<ResourceAssetDto> Resources;
        [JsonProperty] internal char DirectorySeparator;

        [JsonConstructor]
        internal ResourceCacheDto()
        {
        }

        internal ResourceCacheDto(string createdAt, List<ResourceAsset> resources, char directorySeparator)
        {
            CreatedAt = createdAt;
            DirectorySeparator = directorySeparator;
            Resources = new List<ResourceAssetDto>();
            foreach (var resource in resources)
            {
                Resources.Add(new ResourceAssetDto(resource.Guid, resource.FullPath, resource.Package,
                    resource.Components));
            }
        }
    }
}