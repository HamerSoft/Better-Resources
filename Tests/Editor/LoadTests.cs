using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NUnit.Framework;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace HamerSoft.BetterResources.Tests
{
    public class LoadTests : ResourceTests
    {
        [UnityTest]
        public IEnumerator Load_OfTypeT_Returns_Object_If_Found()
        {
            string path = CreateAsset(RESOURCES, "myTextAsset.asset");
            string rootedPath = GetRootedPath(path);
            yield return new WaitUntil(
                () => File.Exists(rootedPath));

            Assert.That(BetterResources.Load<TextAsset>("myTextAsset"),
                Is.TypeOf<TextAsset>());
        }

        [UnityTest]
        public IEnumerator InstanceIDToObject_Returns_Object_If_Found()
        {
            string path = CreateAsset(RESOURCES, "myTextAsset.asset");
            string rootedPath = GetRootedPath(path);
            yield return new WaitUntil(
                () => File.Exists(rootedPath));
            var asset = BetterResources.Load<TextAsset>("myTextAsset");
            Assert.That(asset,
                Is.SameAs(BetterResources.InstanceIdToObject(asset.GetInstanceID())));
            Assert.That(BetterResources.InstanceIdToObject(-1337992), Is.Null);
        }

        [Test]
        public async Task InstanceIDsToObjects_Returns_Objects_If_Found()
        {
            string path = CreateAsset(RESOURCES, "myTextAsset.asset");
            string path2 = CreateAsset(RESOURCES, "myTextAsset2.asset");
            string tex = CreateMaterial(RESOURCES, "mytex.mat");
            string rootedPath = GetRootedPath(path);
            await WaitUntil(() => File.Exists(rootedPath));

            var asset1 = BetterResources.Load<TextAsset>("myTextAsset");
            var asset2 = BetterResources.Load<TextAsset>("myTextAsset2");
            var asset3 = BetterResources.Load<Material>("mytex");
            var instanceIDs =
                new NativeArray<int>(new[] { asset1.GetInstanceID(), asset2.GetInstanceID(), asset3.GetInstanceID() },
                    Allocator.Temp);
            var objects = new List<Object>();
            BetterResources.InstanceIdToObjectList(instanceIDs, objects);
            Assert.That(3, Is.EqualTo(objects.Count));
            Assert.That(asset1, Is.SameAs(objects[0]));
            Assert.That(asset2, Is.SameAs(objects[1]));
            Assert.That(asset3, Is.SameAs(objects[2]));
        }

        [UnityTest]
        public IEnumerator Load_Returns_Object_If_Found()
        {
            string path = CreateAsset(RESOURCES, "myTextAsset.asset");
            string rootedPath = GetRootedPath(path);
            yield return new WaitUntil(
                () => File.Exists(rootedPath));

            Assert.That(BetterResources.Load("myTextAsset"),
                Is.TypeOf<TextAsset>());
        }

        [UnityTest]
        public IEnumerator Load_OfTypeT_Returns_Null_If_Types_Dont_Match()
        {
            string path = CreateAsset(RESOURCES, "myTextAsset.asset");
            string rootedPath = GetRootedPath(path);
            yield return new WaitUntil(
                () => File.Exists(rootedPath));

            Assert.That(BetterResources.Load<Image>("myTextAsset"),
                Is.Null);
        }

        [UnityTest]
        public IEnumerator Load_OfTypeT_Returns_Null_If_NotFound()
        {
            string path = CreateAsset(RESOURCES, "myTextAsset.asset");
            string rootedPath = GetRootedPath(path);
            yield return new WaitUntil(
                () => File.Exists(rootedPath));

            Assert.That(BetterResources.Load<Image>($"FaultyPath{Path.DirectorySeparatorChar}myTextAsset"),
                Is.Null);
        }

        [UnityTest]
        public IEnumerator Load_Returns_Null_If_NotFound()
        {
            string path = CreateAsset(RESOURCES, "myTextAsset.asset");
            string rootedPath = GetRootedPath(path);
            yield return new WaitUntil(
                () => File.Exists(rootedPath));

            Assert.That(BetterResources.Load($"FaultyPath{Path.DirectorySeparatorChar}myTextAsset"),
                Is.Null);
        }

        [Test]
        public async Task LoadAsync_OfTypeT_Returns_Object_If_Found()
        {
            string path = CreateAsset(RESOURCES, "myTextAsset.asset");
            string rootedPath = GetRootedPath(path);
            await WaitUntil(() => File.Exists(rootedPath));

            var result = await BetterResources.LoadAsync<TextAsset>("myTextAsset");
            Assert.That(result, Is.TypeOf<TextAsset>());
        }

        [Test]
        public async Task LoadAsync_Returns_Object_If_Found()
        {
            string path = CreateAsset(RESOURCES, "myTextAsset.asset");
            string rootedPath = GetRootedPath(path);
            await WaitUntil(() => File.Exists(rootedPath));

            var result = await BetterResources.LoadAsync("myTextAsset");
            Assert.That(result, Is.TypeOf(typeof(TextAsset)));
        }

        [Test]
        public async Task LoadAsync_OfTypeT_Returns_Null_If_Types_Dont_Match()
        {
            string path = CreateAsset(RESOURCES, "myTextAsset.asset");
            string rootedPath = GetRootedPath(path);
            await WaitUntil(() => File.Exists(rootedPath));

            var result = await BetterResources.LoadAsync<Image>("myTextAsset");
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task LoadAsync_OfTypeT_Returns_Null_If_NotFound()
        {
            string path = CreateAsset(RESOURCES, "myTextAsset.asset");
            string rootedPath = GetRootedPath(path);
            await WaitUntil(() => File.Exists(rootedPath));

            var result = await BetterResources.LoadAsync<Image>($"FaultyPath{Path.DirectorySeparatorChar}myTextAsset");
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task LoadAsync_Returns_Null_If_NotFound()
        {
            string path = CreateAsset(RESOURCES, "myTextAsset.asset");
            string rootedPath = GetRootedPath(path);
            await WaitUntil(() => File.Exists(rootedPath));

            var result = await BetterResources.LoadAsync($"FaultyPath{Path.DirectorySeparatorChar}myTextAsset");
            Assert.That(result, Is.Null);
        }

        [Test]
        public void GetBuiltinResources_Returns_Unity_Resource()
        {
            var result = BetterResources.GetBuiltinResource<Mesh>("Sphere.fbx");
            Assert.That(result, Is.TypeOf<Mesh>());
            result = BetterResources.GetBuiltinResource(typeof(Mesh), "Sphere.fbx") as Mesh;
            Assert.That(result, Is.TypeOf<Mesh>());
        }

        [Test]
        public void GetBuiltinResources_Returns_Null_When_TypesDontMatch()
        {
            var result = BetterResources.GetBuiltinResource<Material>("Sphere.fbx");
            LogAssert.Expect(LogType.Assert, new Regex(""));
            LogAssert.Expect(LogType.Error, new Regex(""));
            Assert.That(result, Is.Null);
            result = BetterResources.GetBuiltinResource(typeof(Material), "Sphere.fbx") as Material;
            LogAssert.Expect(LogType.Assert, new Regex(""));
            LogAssert.Expect(LogType.Error, new Regex(""));
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task FindObjectOfTypeAll_ReturnsAll_ObjectsOf_Type()
        {
            int previous = BetterResources.FindObjectsOfTypeAll(typeof(TextAsset)).Length;
            string path = CreateAsset(RESOURCES, "myTextAsset.asset");
            string path2 = CreateAsset(RESOURCES, "myTextAsset2.asset");
            string tex = CreateMaterial(RESOURCES, "mytex.mat");
            string rootedPath = GetRootedPath(path);
            await WaitUntil(() => File.Exists(rootedPath));

            var textAssets = BetterResources.FindObjectsOfTypeAll(typeof(TextAsset));
            Assert.That(textAssets.Length - previous, Is.EqualTo(2));
        }

        [Test]
        public async Task FindObjectOfTypeAll_T_ReturnsAll_ObjectsOf_Type()
        {
            int previous = BetterResources.FindObjectsOfTypeAll<TextAsset>().Length;
            string path = CreateAsset(RESOURCES, "myTextAsset.asset");
            string path2 = CreateAsset(RESOURCES, "myTextAsset2.asset");
            string tex = CreateMaterial(RESOURCES, "mytex.mat");
            string rootedPath = GetRootedPath(path);
            await WaitUntil(() => File.Exists(rootedPath));

            var textAssets = BetterResources.FindObjectsOfTypeAll<TextAsset>();
            Assert.That(textAssets.Length - previous, Is.EqualTo(2));
        }

        [Test]
        public async Task Resources_Directory_Does_Not_Have_To_Be_TopLevelDirectory()
        {
            string resourcePath = Path.Combine("DoesNot", "HaveToBe", RESOURCES);
            string rootedDir = GetRootedPath(resourcePath, false);
            Directory.CreateDirectory(rootedDir);
            await WaitUntil(() => Directory.Exists(rootedDir));

            var path = CreateAsset(resourcePath, "myTextAsset.asset");
            string rootedPath = GetRootedPath(path);
            await WaitUntil(() => File.Exists(rootedPath));
            var asset = await BetterResources.LoadAsync<TextAsset>("myTextAsset");
            Assert.That(asset, Is.TypeOf<TextAsset>());

            AssetDatabase.DeleteAsset(Path.Combine(ASSETS, "DoesNot"));
        }

        [Test]
        public async Task Nested_Resources_Folders_Are_Treated_As_Top_Level_And_Are_Identical()
        {
            string resourcePath = Path.Combine("SomeDir", RESOURCES, "AnotherDir", RESOURCES);
            string rootedDir = GetRootedPath(resourcePath, false);
            Directory.CreateDirectory(rootedDir);
            await WaitUntil(() => Directory.Exists(rootedDir));

            var path = CreateAsset(resourcePath, "myTextAsset.asset");
            string rootedPath = GetRootedPath(path);
            await WaitUntil(() => File.Exists(rootedPath));

            var loadedFromParent =
                await BetterResources.LoadAsync<TextAsset>(Path.Combine("AnotherDir", RESOURCES, "myTextAsset"));
            var asset = await BetterResources.LoadAsync<TextAsset>("myTextAsset");

            Assert.That(loadedFromParent, Is.TypeOf<TextAsset>());
            Assert.That(asset, Is.TypeOf<TextAsset>());
            Assert.That(asset.text, Is.EqualTo(loadedFromParent.text));
            Assert.That(asset.GetInstanceID(), Is.EqualTo(loadedFromParent.GetInstanceID()));
            Assert.That(asset, Is.EqualTo(loadedFromParent));
            AssetDatabase.DeleteAsset(Path.Combine(ASSETS, "SomeDir"));
        }

        [Test]
        public async Task Load_Finds_Assets_With_Duplicate_Names()
        {
            await AwaitCreateAsset(Path.Combine(RESOURCES, "myTestFolder"), "foo.asset");
            await AwaitCreateMaterial(Path.Combine(RESOURCES, "myTestFolder"), "foo.mat");

            Assert.That(BetterResources.Load<TextAsset>($"myTestFolder{Path.DirectorySeparatorChar}foo"),
                Is.TypeOf<TextAsset>());
            Assert.That(BetterResources.Load<Material>($"myTestFolder{Path.DirectorySeparatorChar}foo"),
                Is.TypeOf<Material>());
        }

        [UnityTest]
        public IEnumerator UnloadAsset_UnloadsAsset_FromMemory()
        {
            string path = CreateAsset(RESOURCES, "myTextAsset.asset");
            string rootedPath = Path.Combine(Application.dataPath.Replace($"{Path.DirectorySeparatorChar}{ASSETS}", ""),
                path);
            yield return new WaitUntil(
                () => File.Exists(rootedPath));

            var asset = BetterResources.Load("myTextAsset");
            var oldId = asset.GetInstanceID();
            BetterResources.UnloadAsset(asset);
            BetterResources.UnloadUnusedAssets();
            GC.Collect();
            Assert.That(
                typeof(UnityEngine.Object).GetField("m_CachedPtr", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(asset), Is.EqualTo(new IntPtr(0)));
            asset = BetterResources.Load("myTextAsset");
            Assert.That(oldId, Is.EqualTo(asset.GetInstanceID()));
        }
    }
}