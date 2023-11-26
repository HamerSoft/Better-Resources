using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

[assembly: InternalsVisibleTo("com.hamersoft.betterresources.tests")]

namespace HamerSoft.BetterResources.Editor
{
    /// <summary>
    /// This class exposes a couple of variants to generate the a ResourceCache.
    /// <remarks> <see cref="GenerateCacheAsync"/> is used by the HamerSoft/BetterResources/Generate Cache menu item in the editor</remarks>
    /// <remarks> <see cref="GenerateCache"/> is used by the Pre-Build hook.</remarks>
    /// </summary>
    public static class BetterResourcesEditor
    {
        /// <summary>
        /// Generate the Cache in order to <see cref="BetterResources.Query"/>
        /// <remarks> You can use the built-in load functions without cache.</remarks>
        /// <remarks> This function can be triggered through CI and a Menu in the Unity3D editor toolbar at HamerSoft/BetterResources/Generate Cache.</remarks>
        /// </summary>
        [MenuItem("HamerSoft/BetterResources/Generate Cache")]
        public static async void GenerateCacheAsync()
        {
            await GenerateCacheAsync(default);
        }

        /// <summary>
        /// Generate the Cache in order to <see cref="BetterResources.Query"/>
        /// <remarks> You can use the built-in load functions without cache.</remarks>
        /// <remarks> This function can be triggered through CI.</remarks>
        /// </summary>
        /// <param name="token">Optional token to cancel the cache generation process</param>
        // ReSharper disable once MemberCanBePrivate.Global
        public static async Task GenerateCacheAsync(CancellationToken token)
        {
            var cache = await GenerateCacheInternal(token);
            if (!token.IsCancellationRequested)
                SaveCache(cache);
        }

        /// <summary>
        /// Generate the Cache in order to <see cref="BetterResources.Query"/>
        /// <remarks> You can use the built-in load functions without cache.</remarks>
        /// <remarks> This function can be triggered through CI or in the UnityCloud build just like <see cref="PreBuildHook"/>.</remarks>
        /// </summary>
        public static void GenerateCache()
        {
            AssetDatabase.Refresh();
            GenerateCache(AssetDatabase.GetAllAssetPaths(), GetPackageNames());
        }

        /// <summary>
        /// Placeholder class for the package manifest
        /// </summary>
        private class PackagesManifest
        {
            // ReSharper disable once CollectionNeverUpdated.Local
            public Dictionary<string, string> Dependencies;
        }

        /// <summary>
        /// This Hack is needed since Pre-Build hooks cannot be async.
        /// An Async / Coroutine method is needed since the UnityEditor.PackageManager.Client.List is an async operation
        /// So, here we read in the package manifest and read the package names directly from that json file
        /// </summary>
        /// <returns>array of package names from the manifest.json in the packages folder of the unity project</returns>
        private static string[] GetPackageNames()
        {
            const string assets = "Assets";
            const string packages = "Packages";
            const string manifestJson = "manifest.json";

            var json = File.ReadAllText(Path.Combine(Application.dataPath.Replace(assets, ""),
                packages, manifestJson));
            var manifest = JsonConvert.DeserializeObject<PackagesManifest>(json);
            return manifest.Dependencies.Keys.ToArray();
        }

        /// <summary>
        /// Hack to get a pre-build hook to work in unity since async methods are not allowed
        /// </summary>
        /// <param name="assetPaths">all asset paths</param>
        /// <param name="packageNames">names of packages</param>
        private static void GenerateCache(string[] assetPaths, string[] packageNames)
        {
            BetterResources.InitConstants();
            var cache = new CacheGenerator(assetPaths, packageNames,
                    DateTime.UtcNow)
                .Generate();
            SaveCache(cache);
        }

        internal static void SaveCache(ResourceCache cache, string directory = BetterResources.RESOURCES)
        {
            var rootedPath = Path.Combine(Application.dataPath, directory);
            if (!Directory.Exists(rootedPath))
                Directory.CreateDirectory(rootedPath);
            var assetPath = Path.Combine(BetterResources.ASSETS, directory, $"{BetterResources.RESOURCES_CACHE}.asset");
            AssetDatabase.CreateAsset(
                new TextAsset(CacheGenerator.ToJson(cache)) { name = BetterResources.RESOURCES_CACHE },
                assetPath);
            AssetDatabase.ImportAsset(assetPath);
            AssetDatabase.Refresh();
        }

        private static async Task<ResourceCache> GenerateCacheInternal(CancellationToken token)
        {
            AssetDatabase.Refresh();
            BetterResources.InitConstants();
            var paths = AssetDatabase.GetAllAssetPaths();
            var packageCollection = await UnityEditor.PackageManager.Client.List(false);
            ResourceCache cache = null;
            if (!token.IsCancellationRequested)
                cache = await new CacheGenerator(paths, packageCollection.Select(p => p.name).ToArray(),
                        DateTime.UtcNow)
                    .GenerateAsync(token);

            return cache;
        }
    }
}