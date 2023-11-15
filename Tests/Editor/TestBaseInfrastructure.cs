using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace HamerSoft.BetterResources.Tests
{
    public class TestBaseInfrastructure
    {
        protected const string HAMERSOFT = "HamerSoft";
        protected const string RESOURCES = "Resources";
        protected const string PACKAGES = "Packages";
        protected const string ASSETS = "Assets";
        protected const string COM_HAMERSOFT_BETTERRESOURCES = "com.hamersoft.betterresources";
        private List<string> CreatedFiles = new();
        private List<string> DirectoriesToDelete = new();

        protected IEnumerator DoSetup()
        {
            BetterResources.InitConstants();
            CreatedFiles = new List<string>();
            DirectoriesToDelete = new List<string>();
            if (!Directory.Exists(Path.Combine(Application.dataPath, RESOURCES)))
                Directory.CreateDirectory(Path.Combine(Application.dataPath, RESOURCES));
            yield return new WaitUntil(
                () => Directory.Exists(Path.Combine(Application.dataPath, RESOURCES)));
        }

        protected void DoTearDown()
        {
            foreach (var file in CreatedFiles)
                DeleteFile(file);

            var pathRoots = DirectoriesToDelete;
            AssetDatabase.DeleteAssets(pathRoots.ToArray(), new List<string>());
            AssetDatabase.Refresh();
        }

        protected string CreateAsset(string path, string fileName, string assetContent = null)
        {
            path = Path.Combine("Assets", path, fileName);
            AssetDatabase.CreateAsset(new TextAsset(assetContent ?? HAMERSOFT), path);
            AssetDatabase.ImportAsset(path);
            CreatedFiles.Add(path);
            return path;
        }

        protected async Task AwaitCreateAsset(string path, string assetName, string assetContent = null)
        {
            var directoryPath = GetRootedPath(path, false);
            if (!Directory.Exists(directoryPath))
            {
                var unexistendDir = GetFirstDirectoryThatDoesNotExist(Application.dataPath, path);
                Directory.CreateDirectory(directoryPath);
            }

            string p = CreateAsset(path, assetName, assetContent);
            string rootedPath = GetRootedPath(p);
            await WaitUntil(() => File.Exists(rootedPath));
        }

        protected async Task<string> AwaitCreateAssetInPackage(string path, string assetName)
        {
            var combinedPath = Path.Combine(PACKAGES, COM_HAMERSOFT_BETTERRESOURCES, "Runtime", path);
            var directoryPath = Path.Combine(Application.dataPath.Replace($"{Path.DirectorySeparatorChar}{ASSETS}", ""),
                combinedPath);
            if (!Directory.Exists(directoryPath))
            {
                var root = Path.Combine(Application.dataPath.Replace($"{Path.DirectorySeparatorChar}{ASSETS}", ""),
                    PACKAGES,
                    COM_HAMERSOFT_BETTERRESOURCES, "Runtime");
                GetFirstDirectoryThatDoesNotExist(root, path);
                Directory.CreateDirectory(directoryPath);
            }

            var filePath = Path.Combine(combinedPath, assetName);
            AssetDatabase.CreateAsset(new TextAsset(HAMERSOFT), filePath);
            AssetDatabase.ImportAsset(filePath);
            CreatedFiles.Add(filePath);
            string rootedPath = Path.Combine(directoryPath, assetName);
            await WaitUntil(() => File.Exists(rootedPath));
            return filePath;
        }

        protected string CreateMaterial(string path, string fileName)
        {
            path = Path.Combine(ASSETS, path, fileName);
            Material material = new Material(Shader.Find("Specular")) { name = fileName };
            AssetDatabase.CreateAsset(material, path);
            AssetDatabase.ImportAsset(path);
            CreatedFiles.Add(path);
            return path;
        }

        protected async Task AwaitCreateMaterial(string path, string assetName)
        {
            var directoryPath = GetRootedPath(path, false);
            if (!Directory.Exists(directoryPath))
            {
                
                Directory.CreateDirectory(directoryPath);
            }

            string p = CreateMaterial(path, assetName);
            string rootedPath = GetRootedPath(p);
            await WaitUntil(() => File.Exists(rootedPath));
        }

        protected async Task AwaitCreatePrefab(string path, string assetName, params Type[] components)
        {
            var directoryPath = GetRootedPath(path, false);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            var p = Path.Combine(ASSETS, path, assetName);
            var gameObject = new GameObject(assetName);
            components.ToList().ForEach(c => gameObject.AddComponent(c));

            await WaitUntil(() => { return components.All(c => gameObject.GetComponent(c) != null); });

            PrefabUtility.SaveAsPrefabAsset(gameObject, p);
            AssetDatabase.ImportAsset(p);
            CreatedFiles.Add(p);

            string rootedPath = GetRootedPath(p);
            await WaitUntil(() => File.Exists(rootedPath));
        }

        protected async Task WaitUntil(Func<bool> predicate)
        {
            while (!predicate())
                await Task.Delay(100);
        }

        protected void DeleteFile(string path)
        {
            if (File.Exists(path))
                File.Delete(path);
        }

        protected string GetRootedPath(string path, bool removeAssets = true)
        {
            return Path.Combine(
                removeAssets
                    ? Application.dataPath.Replace($"{Path.DirectorySeparatorChar}{ASSETS}", "")
                    : Application.dataPath,
                path);
        }

        private string GetFirstDirectoryThatDoesNotExist(string root, string path)
        {
            var directories = path.Split(Path.DirectorySeparatorChar);
            int index = 0;
            var option = directories[index];
            while (Directory.Exists(Path.Combine(root, option)))
                option = Path.Combine(option, directories[++index]);

            string first = ASSETS;
            if (root.Contains($"{Path.DirectorySeparatorChar}{PACKAGES}{Path.DirectorySeparatorChar}"))
                first = root.Split(Application.dataPath.Replace(ASSETS, ""))[1];
            DirectoriesToDelete.Add(Path.Combine(first, option));
            return Path.Combine(root, option);
        }
    }
}