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
        internal class ResourceInfoDto
        {
            [JsonProperty] internal string Guid { get; set; }
            [JsonProperty] internal string FullPath { get; set; }
            [JsonProperty] internal string Package { get; set; }
            [JsonProperty] internal HashSet<string> Components { get; set; }

            [JsonConstructor]
            internal ResourceInfoDto()
            {
            }

            internal ResourceInfoDto(string guid, string path, string packageName,
                HashSet<Type> components)
            {
                Guid = guid;
                FullPath = path;
                Package = packageName;
                Components = new HashSet<string>(components.Select(c => c.AssemblyQualifiedName));
            }
        }

        [JsonProperty] internal string CreatedAt;
        [JsonProperty] internal List<ResourceInfoDto> Resources;
        [JsonProperty] internal char DirectorySeparator;

        [JsonConstructor]
        internal ResourceCacheDto()
        {
        }

        internal ResourceCacheDto(string createdAt, List<ResourceInfo> resources, char directorySeparator)
        {
            CreatedAt = createdAt;
            DirectorySeparator = directorySeparator;
            Resources = new List<ResourceInfoDto>();
            foreach (var resource in resources)
            {
                Resources.Add(new ResourceInfoDto(resource.Guid, resource.FullPath, resource.Package,
                    resource.Components));
            }
        }
    }
}