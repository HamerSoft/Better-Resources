using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using HamerSoft.BetterResources.Awaiters;
using HamerSoft.BetterResources.Extensions;
using System.IO;
using Unity.Collections;
using UnityEngine;
using Object = UnityEngine.Object;

[assembly: InternalsVisibleTo("com.hamersoft.betterresources.tests")]
[assembly: InternalsVisibleTo("com.hamersoft.betterresources.editor")]

namespace HamerSoft.BetterResources
{
    /// <summary>
    /// A Better API to load resources from the Unity3D Resources folders.
    /// <remarks>Make sure to <seealso cref="Initialize"/> | <seealso cref="InitializeAsync"/> | <seealso cref="InitializeRoutine"/> in the Editor / Pre-Build before using the <see cref="Query"/> API.</remarks>
    /// </summary>
    public static class BetterResources
    {
        internal const string RESOURCES_CACHE = "ResourcesCache";
        internal const string ASSETS = "Assets";
        internal const string RESOURCES = "Resources";
        internal static char DirectorySeparator;
        private static ResourceManifest _manifest;

        /// <summary>
        /// Event fired when BetterResources is Initialized!
        /// </summary>
        public static event Action<bool> Initialized;

        /// <summary>
        /// A flag indicating if BetterResources is Initialized
        /// </summary>
        public static bool IsInitialized { get; private set; }

        /// <summary>
        /// A flag indicating if BetterResources is successfully Initialized
        /// <remarks>Initialization might be invalid when there is no ResourceCache found at the initialization directory or when the ResourceCache contains an invalid JSON.</remarks>
        /// </summary>
        public static bool IsValid { get; private set; }

        internal static void InitConstants()
        {
            DirectorySeparator = Path.DirectorySeparatorChar;
        }

        /// <summary>
        /// Initialize BetterResources
        /// <seealso cref="InitializeAsync"/>
        /// <see cref="InitializeRoutine"/>
        /// </summary>
        /// <param name="directory">Optional directory (local to Resources) where the ResourcesCache exists, if null is used, the ResourceCache is loaded from root.</param>
        /// <remarks><see cref="GenerateCache"/> allows for an optional directory too, these must match!</remarks>
        public static void Initialize(string directory = null)
        {
            if (IsInitialized)
                return;
            InitConstants();
            IsInitialized = true;
            var resourceCacheJson = Load<TextAsset>(string.IsNullOrWhiteSpace(directory)
                ? RESOURCES_CACHE
                : Path.Combine(directory, RESOURCES_CACHE));
            if (resourceCacheJson)
            {
                try
                {
                    _manifest = ManifestGenerator.FromResourceCacheJson(resourceCacheJson.text);
                    IsValid = true;
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to convert ResourceCache json, to an object! {e} | {e.Message}");
                    IsValid = false;
                }
            }
            else
            {
                Debug.LogError(
                    $"No Resource cache TextAsset called \"{RESOURCES_CACHE}\" found at Resources root path.");
                IsValid = false;
            }

            Initialized?.Invoke(IsValid);
        }

        /// <summary>
        /// Initialize BetterResources Async
        /// <seealso cref="Initialize"/>
        /// <seealso cref="InitializeRoutine"/>
        /// </summary>
        /// <param name="directory">Optional directory (local to Resources) where the ResourcesCache exists, if null is used, the ResourceCache is loaded from root.</param>
        /// <param name="token">Optional token to cancel the ongoing initialization process.</param>
        /// <remarks><see cref="GenerateCache"/> allows for an optional directory too, these must match!</remarks>
        public static async Task InitializeAsync(string directory = null, CancellationToken token = default)
        {
            if (IsInitialized)
                return;
            InitConstants();
            var resourceCacheJson = await LoadAsync<TextAsset>(string.IsNullOrWhiteSpace(directory)
                ? RESOURCES_CACHE
                : Path.Combine(directory, RESOURCES_CACHE));
            if (resourceCacheJson && !token.IsCancellationRequested)
            {
                try
                {
                    _manifest = await ManifestGenerator.FromResourceCacheJsonAsync(resourceCacheJson.text, token);
                    IsInitialized = true;
                    IsValid = true;
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to convert ResourceCache json, to an object! {e} | {e.Message}");
                    IsValid = false;
                    IsInitialized = true;
                }
            }
            else if (!token.IsCancellationRequested)
            {
                Debug.LogError(
                    $"No Resource cache TextAsset called \"{RESOURCES_CACHE}\" found at Resources root path.");
                IsValid = false;
                IsInitialized = true;
            }

            Initialized?.Invoke(IsValid);
        }

        /// <summary>
        /// Initialize BetterResources in a Coroutine
        /// <seealso cref="Initialize"/>
        /// <seealso cref="InitializeAsync"/>
        /// </summary>
        /// <param name="directory">Optional directory (local to Resources) where the ResourcesCache exists, if null is used, the ResourceCache is loaded from root.</param>
        /// <param name="token">Optional token to cancel the ongoing initialization process. (The token os used since there is no nice way of cancelling the IEnumerator in this particular case)</param>
        /// <remarks><see cref="GenerateCache"/> allows for an optional directory too, these must match!</remarks>
        public static IEnumerator InitializeRoutine(string directory = null, CancellationToken token = default)
        {
            if (IsInitialized)
                yield break;
            yield return InitializeAsync(directory, token).ToCoroutine();
        }

        /// <summary>
        /// Loads the asset of the requested type stored at path in a Resources folder using a parameter type filter of type.
        /// </summary>
        /// <param name="path">path local to Resources</param>
        /// <param name="type">optional type filter</param>
        /// <returns>An object at the requested path, of type, or null</returns>
        public static Object Load(string path, Type type = null)
        {
            return type == null
                ? Resources.Load(path)
                : Resources.Load(path, type);
        }

        /// <summary>
        /// Loads the asset of the requested type stored at path in a Resources folder using a parameter type filter of type.
        /// </summary>
        /// <param name="resourceAsset">ResourceAsset, found through <see cref="Query"/></param>
        /// <param name="type">Optional type filter</param>
        /// <remarks>Type can be of any in the <see cref="ResourceAsset"/>.Components</remarks>
        /// <returns>An object at the path of the ResourceAsset, by any type filter that matches</returns>
        public static Object Load(ResourceAsset resourceAsset, Type type = null)
        {
            return type == null
                ? Resources.Load(resourceAsset.ResourcesPath)
                : Resources.Load(resourceAsset.ResourcesPath, type);
        }

        /// <summary>
        /// Loads the asset of the requested type stored at path in a Resources folder using a generic parameter type filter of type T.
        /// </summary>
        /// <param name="path">path local to Resources</param>
        /// <typeparam name="T">Generic Type Filter</typeparam>
        /// <returns>An object of the requested generic parameter type</returns>
        public static T Load<T>(string path) where T : Object
        {
            return Resources.Load<T>(path);
        }

        /// <summary>
        ///  Loads the asset of the requested type stored at path in a Resources folder using a generic parameter type filter of type T.
        /// </summary>
        /// <param name="resourceAsset">ResourceAsset, found through <see cref="Query"/></param>
        /// <typeparam name="T">Generic Type filter</typeparam>
        /// <returns>An object of the requested generic parameter type</returns>
        public static T Load<T>(ResourceAsset resourceAsset) where T : Object
        {
            return Resources.Load<T>(resourceAsset.ResourcesPath);
        }

        /// <summary>
        /// Asynchronously loads an asset stored at path in a Resources folder.
        /// </summary>
        /// <param name="path">pathname of the target folder</param>
        /// <remarks>When using the empty string (i.e., ""), the function will load the entire contents of the Resources folder.</remarks>
        /// <typeparam name="T">Generic Type Filter</typeparam>
        /// <returns>An object of the requested generic parameter type</returns>
        public static async Task<T> LoadAsync<T>(string path) where T : Object
        {
            return await Resources.LoadAsync<T>(path) as T;
        }

        /// <summary>
        /// Asynchronously loads an asset stored at path in a Resources folder.
        /// </summary>
        /// <param name="resourceAsset">ResourceAsset, found through <see cref="Query"/></param>
        /// <typeparam name="T">Generic Type Filter</typeparam>
        /// <remarks>Type filter T can be of any Type in <see cref="ResourceAsset"/>.Components</remarks>
        /// <returns>An object of the requested generic parameter type</returns>
        public static async Task<T> LoadAsync<T>(ResourceAsset resourceAsset) where T : Object
        {
            return await Resources.LoadAsync<T>(resourceAsset.ResourcesPath) as T;
        }

        /// <summary>
        /// Asynchronously loads an asset stored at path in a Resources folder.
        /// </summary>
        /// <param name="path">pathname of the target folder</param>
        /// <remarks>When using the empty string (i.e., ""), the function will load the entire contents of the Resources folder.</remarks>
        /// <param name="type">Type Filter</param>
        /// <returns>An object of the requested type parameter type</returns>
        public static async Task<Object> LoadAsync(string path, Type type = null)
        {
            return type == null
                ? await Resources.LoadAsync(path)
                : await Resources.LoadAsync(path, type);
        }

        /// <summary>
        /// Asynchronously loads an asset stored at path in a Resources folder.
        /// </summary>
        /// <param name="resourceAsset">ResourceAsset, found through <see cref="Query"/></param>
        /// <remarks>When using the empty string (i.e., ""), the function will load the entire contents of the Resources folder.</remarks>
        /// <param name="type">Type Filter</param>
        /// <returns>An object of the requested type parameter type</returns>
        /// <returns></returns>
        public static async Task<Object> LoadAsync(ResourceAsset resourceAsset, Type type = null)
        {
            return type == null
                ? await Resources.LoadAsync(resourceAsset.ResourcesPath)
                : await Resources.LoadAsync(resourceAsset.ResourcesPath, type);
        }

        /// <summary>
        /// Get a built-in Resource
        /// </summary>
        /// <param name="path">Path of the resource</param>
        /// <remarks>Some resources require file extensions like: Sphere.fbx</remarks>
        /// <typeparam name="T">Generic type filter</typeparam>
        /// <returns>Built-in resource of Type T</returns>
        public static T GetBuiltinResource<T>(string path) where T : Object
        {
            return Resources.GetBuiltinResource<T>(path);
        }

        /// <summary>
        /// Get a built-in Resource
        /// </summary>
        /// <param name="path">Path of the resource</param>
        /// <remarks>Some resources require file extensions like: Sphere.fbx</remarks>
        /// <param name="type">Type filter</param>
        /// <returns>Built-in resource of optional type</returns>
        public static Object GetBuiltinResource(Type type, string path)
        {
            return Resources.GetBuiltinResource(type, path);
        }

        /// <summary>
        /// Unload an asset from memory
        /// <remarks>This will also destroy all existing references!</remarks>
        /// </summary>
        /// <param name="asset">The object to destroy</param>
        public static void UnloadAsset(Object asset)
        {
            Resources.UnloadAsset(asset);
        }

        /// <summary>
        /// Unload all unused Assets
        /// </summary>
        public static void UnloadUnusedAssets()
        {
            Resources.UnloadUnusedAssets();
        }

        /// <summary>
        /// Returns a list of all objects of Type type.
        /// </summary>
        /// <param name="type">type to search for.</param>
        /// <remarks>Pro-Tip: use <see cref="Query"/></remarks>
        /// <returns>Array of objects that match the type</returns>
        public static Object[] FindObjectsOfTypeAll(Type type)
        {
            return Resources.FindObjectsOfTypeAll(type);
        }

        /// <summary>
        /// Returns a list of all objects of Type type.
        /// </summary>
        /// <typeparam name="T">type to search for.</typeparam>
        /// <remarks>Pro-Tip: use <see cref="Query"/></remarks>
        /// <returns>Array of objects that match the type</returns>
        public static T[] FindObjectsOfTypeAll<T>() where T : Object
        {
            return Resources.FindObjectsOfTypeAll<T>();
        }

        /// <summary>
        /// Translates an instance ID to an object reference
        /// </summary>
        /// <param name="instanceID">Instance ID of an Object.</param>
        /// <returns>Resolved reference or null if the instance ID didn't match anything.</returns>
        public static Object InstanceIdToObject(int instanceID) => Resources.InstanceIDToObject(instanceID);

        /// <summary>
        /// Translates instance IDs to object references
        /// </summary>
        /// <param name="instanceIDs">array of instance IDs</param>
        /// <param name="objects">List of Object</param>
        /// <remarks>A List can be used as an argument since it keeps references</remarks>
        public static void InstanceIdToObjectList(NativeArray<int> instanceIDs, List<Object> objects) =>
            Resources.InstanceIDToObjectList(instanceIDs, objects);

        /// <summary>
        /// Create a new instance of a query builder to search the available resources
        /// <remarks>QueryBuilder is disposable, so best to use it in a using statement</remarks>
        /// </summary>
        /// <returns><see cref="QueryBuilder"/> instance</returns>
        public static QueryBuilder Query()
        {
            switch (IsInitialized)
            {
                case false:
                    Debug.LogError(
                        "BetterResources is not Initialized! Make sure you call BetterResources.Initialize|Async|Routine before query!");
                    break;
                case true when !IsValid:
                    Debug.LogError(
                        $"BetterResources was initialized but the {RESOURCES_CACHE} was invalid! Query will not return any results! ");
                    break;
            }

            return new QueryBuilder(_manifest);
        }
    }
}