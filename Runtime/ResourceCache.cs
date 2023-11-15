using System;
using System.Collections.Generic;

namespace HamerSoft.BetterResources
{
    internal class ResourceCache
    {
        internal DateTime CreatedAt { get; private set; }
        internal List<ResourceInfo> Resources { get; private set; }
        internal char DirectorySeparatorChar { get; private set; }

        internal ResourceCache(List<ResourceInfo> resources, DateTime createdAt, char directorySeparatorChar)
        {
            Resources = resources;
            CreatedAt = createdAt;
            DirectorySeparatorChar = directorySeparatorChar;
        }
    }
}