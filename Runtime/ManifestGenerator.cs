using System.Threading;
using System.Threading.Tasks;
using HamerSoft.BetterResources.Dto;
using Newtonsoft.Json;

namespace HamerSoft.BetterResources
{
    internal static class ManifestGenerator
    {
        internal static ResourceManifest FromResourceCacheJson(string resourceCacheJson)
        {
            var cache = JsonConvert.DeserializeObject<ResourceCache>(resourceCacheJson, new ResourceCacheConverter());
            return new ResourceManifest(cache.Resources, cache.CreatedAt);
        }

        internal static async Task<ResourceManifest> FromResourceCacheJsonAsync(string resourceCacheJson, CancellationToken token = default)
        {
            return await Task.Run(() =>
            {
                var cache = JsonConvert.DeserializeObject<ResourceCache>(resourceCacheJson,
                    new ResourceCacheConverter());
                return new ResourceManifest(cache.Resources, cache.CreatedAt);
            }, token);
        }
    }
}