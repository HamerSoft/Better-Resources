using System;
using System.Collections.Generic;

namespace HamerSoft.BetterResources
{
    internal class ResourceManifest
    {
        internal readonly DateTime CreatedAt;
        internal readonly IReadOnlyList<ResourceAsset> Resources;

        internal ResourceManifest(IEnumerable<ResourceAsset> resources, DateTime createdAt)
        {
            Resources = new List<ResourceAsset>(resources);
            CreatedAt = createdAt;
        }
    }
}