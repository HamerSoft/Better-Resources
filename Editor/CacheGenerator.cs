using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HamerSoft.BetterResources.Dto;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace HamerSoft.BetterResources
{
    internal class CacheGenerator : IDisposable
    {
        private const string PACKAGES = "Packages";
        private readonly DateTime _createdAt;
        private string[] _assetPaths;
        private string[] _packageNames;

        internal CacheGenerator(string[] assetPaths, string[] packageNames, DateTime createdAt)
        {
            _createdAt = createdAt;
            _assetPaths = assetPaths ?? Array.Empty<string>();
            _packageNames = packageNames ?? Array.Empty<string>();
        }

        internal async Task<ResourceCache> GenerateAsync(CancellationToken token = default)
        {
            var resources = new List<ResourceAsset>();
            foreach (var path in _assetPaths.AsParallel().Where(p =>
                         p.Contains(
                             $"{BetterResources.DirectorySeparator}{BetterResources.RESOURCES}{BetterResources.DirectorySeparator}")))
            {
                if (token.IsCancellationRequested)
                    break;

                var packageName = GetPackageName(_packageNames, path);
                var resourceDirs = GetResourcesDirs(path);

                if (string.IsNullOrWhiteSpace(Path.GetExtension(resourceDirs[0])))
                    continue;
                var guid = AssetDatabase.AssetPathToGUID(path);
                var asset = await BetterResources.LoadAsync(resourceDirs[0]
                    .Replace(Path.GetExtension(resourceDirs[0]), ""));
                var components = GetComponents(asset);

                for (int i = 0; i < resourceDirs.Count; i++)
                {
                    resources.Add(new ResourceAsset(guid, resourceDirs[i], packageName, components));
                }
            }

            if (!token.IsCancellationRequested)
                return new ResourceCache(resources, _createdAt, BetterResources.DirectorySeparator);

            resources = null;
            return null;
        }

        internal ResourceCache Generate()
        {
            var resources = new List<ResourceAsset>();
            foreach (var path in _assetPaths.AsParallel().Where(p =>
                         p.Contains(
                             $"{BetterResources.DirectorySeparator}{BetterResources.RESOURCES}{BetterResources.DirectorySeparator}")))
            {
                var packageName = GetPackageName(_packageNames, path);
                var resourceDirs = GetResourcesDirs(path);

                if (string.IsNullOrWhiteSpace(Path.GetExtension(resourceDirs[0])))
                    continue;
                var guid = AssetDatabase.AssetPathToGUID(path);
                var asset = BetterResources.Load(resourceDirs[0]
                    .Replace(Path.GetExtension(resourceDirs[0]), ""));
                var components = GetComponents(asset);

                for (int i = 0; i < resourceDirs.Count; i++)
                {
                    resources.Add(new ResourceAsset(guid, resourceDirs[i], packageName, components));
                }
            }

            return new ResourceCache(resources, _createdAt, BetterResources.DirectorySeparator);
        }

        private IEnumerable<Type> GetComponents(Object asset)
        {
            if (asset is GameObject go)
                return go.GetComponents<Component>().Select(c => c.GetType());
            return new[] { asset.GetType() };
        }

        private string GetPackageName(string[] packageNames, string path)
        {
            string packageName = null;
            if (path.StartsWith($"{PACKAGES}{BetterResources.DirectorySeparator}"))
            {
                var packagePath = path.Substring($"{PACKAGES}{BetterResources.DirectorySeparator}".Length);
                foreach (var name in packageNames)
                {
                    if (packagePath.StartsWith(name))
                    {
                        packageName = name;
                        break;
                    }
                }
            }

            return packageName;
        }

        private IReadOnlyList<string> GetResourcesDirs(string path)
        {
            var split = path.Split($"{BetterResources.RESOURCES}{BetterResources.DirectorySeparator}");
            List<string> dirs = new List<string>();

            for (int i = 1; i < split.Length; i++)
                dirs.Add(path.Split($"{BetterResources.RESOURCES}{BetterResources.DirectorySeparator}", i + 2).Last());

            return dirs;
        }

        public void Dispose()
        {
            _assetPaths = null;
            _packageNames = null;
        }

        internal static string ToJson(ResourceCache cache)
        {
            return JsonConvert.SerializeObject(cache, new ResourceCacheConverter());
        }
    }
}