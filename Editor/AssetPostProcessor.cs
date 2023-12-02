// #define BETTERRESOURCES_AUTO_GENERATE
// #define BETTERRESOURCES_LOG

#if BETTERRESOURCES_AUTO_GENERATE
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace HamerSoft.BetterResources.Editor
{
    [InitializeOnLoad]
    public class AssetPostProcessor : AssetPostprocessor
    {
        private static bool _enabled = true;
        private const int TIME_SINCE_STARTUP_THRESHOLD = 10;

        static AssetPostProcessor()
        {
            if (EditorApplication.timeSinceStartup > TIME_SINCE_STARTUP_THRESHOLD || !_enabled)
                return;

#if BETTERRESOURCES_LOG
            Debug.Log("<color=green>[BetterResources]: Generating cache upon Editor start up!</color>");
#endif
            RefreshCache();
        }

        private static void RefreshCache()
        {
#if BETTERRESOURCES_LOG
            Debug.Log("<color=green>[BetterResources]: Refreshing cache!</color>");
#endif
            AssetDatabase.DeleteAsset(Path.Combine(BetterResources.ASSETS, BetterResources.RESOURCES,
                $"{BetterResources.RESOURCES_CACHE}.asset"));
            BetterResourcesEditor.CacheGenerated += Init;
            BetterResourcesEditor.GenerateCache();
        }

        private static void Init()
        {
            BetterResourcesEditor.CacheGenerated -= Init;
            BetterResources.ForceInitialize(AssetDatabase.LoadAssetAtPath<TextAsset>(Path.Combine(
                BetterResources.ASSETS, BetterResources.RESOURCES,
                $"{BetterResources.RESOURCES_CACHE}.asset")));
#if BETTERRESOURCES_LOG
            Debug.Log("<color=green>[BetterResources]: Initialized in Editor and ready for Query!</color>");
#endif
        }

        internal static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            if (!_enabled)
                return;
            if (!ShouldRefresh(importedAssets, deletedAssets, movedAssets))
                return;

#if BETTERRESOURCES_LOG
            Debug.Log("<color=green>[BetterResources]: Change in Resources Detected!</color>");
#endif
            RefreshCache();
        }

        internal static bool ShouldRefresh(string[] importedAssets, string[] deletedAssets, string[] movedAssets)
        {
            var resourcesCache = Path.Combine(BetterResources.RESOURCES, $"{BetterResources.RESOURCES_CACHE}.asset");
            var resourceCheck =
                $"{Path.DirectorySeparatorChar}{BetterResources.RESOURCES}{Path.DirectorySeparatorChar}";
            return !ContainsResources(importedAssets, resourcesCache) &&
                   (ContainsResources(importedAssets, resourceCheck) ||
                    ContainsResources(deletedAssets, resourceCheck) ||
                    ContainsResources(movedAssets, resourceCheck));
        }

        private static bool ContainsResources(IEnumerable<string> assets, string resourceCheck)
        {
            return assets != null && assets.Any(path => path.Contains(resourceCheck));
        }

        public static void Enable()
        {
            _enabled = true;
        }

        public static void Disable()
        {
            _enabled = false;
        }
    }
}
#endif