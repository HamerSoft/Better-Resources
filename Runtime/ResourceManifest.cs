using System;
using System.Collections.Generic;

namespace HamerSoft.BetterResources
{
    internal class ResourceManifest
    {
        internal readonly DateTime CreatedAt;
        internal readonly IReadOnlyList<ResourceInfo> Resources;

        internal ResourceManifest(IEnumerable<ResourceInfo> resources, DateTime createdAt)
        {
            Resources = new List<ResourceInfo>(resources);
            CreatedAt = createdAt;
        }
    }
}