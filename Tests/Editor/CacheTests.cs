using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HamerSoft.BetterResources.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace HamerSoft.BetterResources.Tests
{
    [TestFixture]
    public class CacheTests : ResourceTests
    {
        [UnitySetUp]
        public override IEnumerator SetUp()
        {
            BetterResources.InitConstants();
            return base.SetUp();
        }

        [Test]
        public async Task When_There_Are_No_Resources_Cache_Is_Empty()
        {
            var cache = await GenerateCache(Array.Empty<string>(), Array.Empty<string>(), DateTime.UtcNow);
            Assert.That(cache.Resources, Is.Empty);
        }

        [Test]
        public async Task DirectorySeparatorChar_Is_Added_To_Cache()
        {
            var cache = await GenerateCache(Array.Empty<string>(), Array.Empty<string>(), DateTime.UtcNow);
            Assert.That(cache.DirectorySeparatorChar, Is.EqualTo(Path.DirectorySeparatorChar));
        }

        [Test]
        public async Task TopLevel_Resources_Are_Added_To_Cache()
        {
            await AwaitCreateAsset(RESOURCES, "topLevelAsset.asset");
            var cache =
                await GenerateCache(
                    new[] { Path.Combine(ASSETS, RESOURCES, "topLevelAsset.asset") }, Array.Empty<string>(),
                    DateTime.UtcNow);

            Assert.That(cache.Resources.First().Name, Is.EqualTo("topLevelAsset"));
        }

        [Test]
        public async Task TopLevel_Resources_InPackages_Are_Added_To_Cache()
        {
            var assetpath = await AwaitCreateAssetInPackage(RESOURCES, "topLevelAsset.asset");
            var cache =
                await GenerateCache(
                    new[] { assetpath }, new[] { COM_HAMERSOFT_BETTERRESOURCES }, DateTime.UtcNow);

            Assert.That(cache.Resources.First().Name, Is.EqualTo("topLevelAsset"));
            Assert.That(cache.Resources.First().IsInPackage, Is.True);
        }

        [Test]
        public async Task NonTopLevel_ResourcesDirectories_Are_Added_To_Cache()
        {
            await AwaitCreateAsset(Path.Combine("MyFancyNestedTestFolder", RESOURCES), "topLevelAsset.asset");
            var cache =
                await GenerateCache(
                    new[] { Path.Combine(ASSETS, "MyFancyNestedTestFolder", RESOURCES, "topLevelAsset.asset") },
                    Array.Empty<string>()
                    , DateTime.UtcNow);

            Assert.That(cache.Resources.First().Name, Is.EqualTo("topLevelAsset"));
        }

        [Test]
        public async Task NonTopLevel_ResourcesDirectories_InPackages_Are_Added_To_Cache()
        {
            var path = await AwaitCreateAssetInPackage(Path.Combine("MyFancyNestedTestFolder", RESOURCES),
                "topLevelAsset.asset");
            var cache =
                await GenerateCache(
                    new[] { path },
                    new[] { COM_HAMERSOFT_BETTERRESOURCES },
                    DateTime.UtcNow);

            Assert.That(cache.Resources.First().Name, Is.EqualTo("topLevelAsset"));
        }

        [Test]
        public async Task Resources_WithDirectoryStructure_Are_Added_To_Cache()
        {
            await AwaitCreateAsset(Path.Combine(RESOURCES, "myTestFolder"), "someAsset.asset");
            var cache =
                await GenerateCache(
                    new[] { Path.Combine(ASSETS, RESOURCES, "myTestFolder", "someAsset.asset") },
                    Array.Empty<string>(),
                    DateTime.UtcNow);

            Assert.That(cache.Resources.First().ResourcesPath,
                Is.EqualTo($"myTestFolder{Path.DirectorySeparatorChar}someAsset"));
            Assert.That(cache.Resources.First().FullPath,
                Is.EqualTo($"myTestFolder{Path.DirectorySeparatorChar}someAsset.asset"));
            Assert.That(cache.Resources.First().Name, Is.EqualTo("someAsset"));
        }

        [Test]
        public async Task Resources_WithDirectoryStructure_InPackages_Are_Added_To_Cache()
        {
            var path = await AwaitCreateAssetInPackage(Path.Combine(RESOURCES, "myTestFolder"), "someAsset.asset");
            var cache =
                await GenerateCache(
                    new[] { path },
                    new[] { COM_HAMERSOFT_BETTERRESOURCES },
                    DateTime.UtcNow);

            Assert.That(cache.Resources.First().ResourcesPath,
                Is.EqualTo($"myTestFolder{Path.DirectorySeparatorChar}someAsset"));
            Assert.That(cache.Resources.First().FullPath,
                Is.EqualTo($"myTestFolder{Path.DirectorySeparatorChar}someAsset.asset"));
            Assert.That(cache.Resources.First().Name, Is.EqualTo("someAsset"));
        }

        [Test]
        public async Task Resources_With_Nested_DirectoryStructures_Are_Added_To_Cache()
        {
            await AwaitCreateAsset(Path.Combine(RESOURCES, "myTestFolder"), "foo.asset");
            await AwaitCreateAsset(Path.Combine(RESOURCES, "myTestFolder", "nested"), "bar.asset");
            var cache =
                await GenerateCache(
                    new[]
                    {
                        Path.Combine(ASSETS, RESOURCES, "myTestFolder", "foo.asset"),
                        Path.Combine(ASSETS, RESOURCES, "myTestFolder", "nested", "bar.asset")
                    },
                    Array.Empty<string>(),
                    DateTime.UtcNow);

            Assert.That(cache.Resources.First().FullPath,
                Is.EqualTo($"myTestFolder{Path.DirectorySeparatorChar}foo.asset"));
            Assert.That(cache.Resources.First().ResourcesPath,
                Is.EqualTo($"myTestFolder{Path.DirectorySeparatorChar}foo"));
            Assert.That(cache.Resources.First().Name, Is.EqualTo("foo"));
            Assert.That(cache.Resources.ElementAt(1).FullPath,
                Is.EqualTo($"myTestFolder{Path.DirectorySeparatorChar}nested{Path.DirectorySeparatorChar}bar.asset"));
            Assert.That(cache.Resources.ElementAt(1).ResourcesPath,
                Is.EqualTo($"myTestFolder{Path.DirectorySeparatorChar}nested{Path.DirectorySeparatorChar}bar"));
            Assert.That(cache.Resources.ElementAt(1).Name, Is.EqualTo("bar"));
        }

        [Test]
        public async Task Components_On_RootAsset_Are_Added_To_ResourceInfo()
        {
            await AwaitCreateMaterial(RESOURCES, "FooTestBarMat.mat");
            var cache = await GenerateCache(new[] { Path.Combine(ASSETS, RESOURCES, "FooTestBarMat.mat") },
                Array.Empty<string>(),
                DateTime.UtcNow);

            Assert.That(cache.Resources.First().Type, Is.EqualTo(typeof(Material)));
            Assert.That(cache.Resources.First().Components.First(), Is.EqualTo(typeof(Material)));
        }

        [Test]
        public async Task All_Components_On_Prefab_Are_Added_To_ResourceInfo()
        {
            await AwaitCreatePrefab(RESOURCES, "SomeFinePrefab.prefab", typeof(Camera), typeof(AudioSource),
                typeof(Light));
            var cache = await GenerateCache(
                new[] { Path.Combine(ASSETS, RESOURCES, "SomeFinePrefab.prefab") },
                Array.Empty<string>(),
                DateTime.UtcNow);

            Assert.That(cache.Resources.First().Components.FirstOrDefault(c => c == typeof(Camera)),
                Is.Not.Null);
            Assert.That(cache.Resources.First().Components.FirstOrDefault(c => c == typeof(AudioSource)),
                Is.Not.Null);
            Assert.That(cache.Resources.First().Components.FirstOrDefault(c => c == typeof(Light)),
                Is.Not.Null);
        }

        [Test]
        public async Task Cache_ToJson_Contains_CreatedAt()
        {
            var createdAt = DateTime.UtcNow;
            await AwaitCreateAsset(RESOURCES, "topLevelAsset.asset");
            var cache =
                await GenerateCache(
                    new[] { Path.Combine(ASSETS, RESOURCES, "topLevelAsset.asset") }, Array.Empty<string>(),
                    createdAt);

            var json = CacheGenerator.ToJson(cache);
            Assert.That(json.Contains(createdAt.ToString()), Is.True);
        }

        [Test]
        public async Task Empty_Directories_In_Resources_Are_Skipped()
        {
            var createdAt = DateTime.UtcNow;
            var cache =
                await GenerateCache(
                    new[] { Path.Combine(ASSETS, RESOURCES, "Foo") }, Array.Empty<string>(),
                    createdAt);

            Assert.That(cache.Resources, Is.Empty);
        }

        [Test(Description =
            @"Ugly test yet, ¯\_(ツ)_/¯. It tests whether the contents are as expected without using complex regexes. This will do for now.")]
        public async Task Cache_ToJson_Contains_Assets()
        {
            var path = await AwaitCreateAssetInPackage(Path.Combine(RESOURCES, "myTestFolder"), "someAsset.asset");
            await AwaitCreateAsset(RESOURCES, "topLevelAsset.asset");
            var cache =
                await GenerateCache(
                    new[] { Path.Combine(ASSETS, RESOURCES, "topLevelAsset.asset"), path },
                    new[] { COM_HAMERSOFT_BETTERRESOURCES },
                    DateTime.Now);

            var json = CacheGenerator.ToJson(cache);
            Assert.That(json.Contains("topLevelAsset"), Is.True);
            Assert.That(json.Contains("someAsset"), Is.True);
            Assert.That(json.Contains("com.hamersoft.betterresources"), Is.True);
            Assert.That(json.Contains("TextAsset"), Is.True);
        }

        [Test]
        public async Task ResourceCache_FromJson_ToManifest_Contains_Assets()
        {
            var path = await AwaitCreateAssetInPackage(Path.Combine(RESOURCES, "myTestFolder"), "someAsset.asset");
            await AwaitCreateAsset(RESOURCES, "topLevelAsset.asset");
            var cache =
                await GenerateCache(
                    new[] { Path.Combine(ASSETS, RESOURCES, "topLevelAsset.asset"), path },
                    new[] { COM_HAMERSOFT_BETTERRESOURCES },
                    DateTime.Now);

            var json = CacheGenerator.ToJson(cache);
            var manifest = await ManifestGenerator.FromResourceCacheJsonAsync(json);
            var topLevelResource = manifest.Resources.FirstOrDefault(ri => ri.Name == "topLevelAsset");
            var someAssetResource = manifest.Resources.FirstOrDefault(ri => ri.Name == "someAsset");
            Assert.That(topLevelResource, Is.Not.Null);
            Assert.That(someAssetResource, Is.Not.Null);
            Assert.That(topLevelResource.Type, Is.EqualTo(typeof(TextAsset)));
            Assert.That(someAssetResource.IsInPackage, Is.True);
            Assert.That(someAssetResource.Package, Is.EqualTo(COM_HAMERSOFT_BETTERRESOURCES));
        }

        [Test]
        public void BetterResources_SaveCache_Creates_ResourcesCache_In_Folder()
        {
            var directory = Path.Combine(RESOURCES, "test-cache");
            BetterResourcesEditor.SaveCache(
                new ResourceCache(new List<ResourceInfo>(), DateTime.UtcNow, Path.DirectorySeparatorChar), directory);
            Assert.That(BetterResources.Load<TextAsset>(Path.Combine("test-cache", "ResourcesCache")), Is.Not.Null);
            AssetDatabase.DeleteAsset(Path.Combine(ASSETS, directory));
        }

        [Test]
        public async Task When_Cache_Is_Deserialized_And_DirectorySeparators_Dont_Match_They_Are_Replaced()
        {
            var cache = new ResourceCache(new List<ResourceInfo>()
            {
                new(Guid.NewGuid().ToString(), "foo|bar.asset", null, new[] { typeof(TextAsset) })
            }, DateTime.UtcNow, '|');

            var json = CacheGenerator.ToJson(cache);
            var manifest = await ManifestGenerator.FromResourceCacheJsonAsync(json);
            Assert.That(manifest.Resources.First().ResourcesPath.Contains(Path.DirectorySeparatorChar), Is.True);
            Assert.That(manifest.Resources.First().ResourcesPath.Contains('|'), Is.False);
        }

        [Test]
        public async Task When_Cache_Generation_Is_Cancelled_It_Returns_Null()
        {
            var tokenSource = new CancellationTokenSource();
            tokenSource.Cancel();
            await AwaitCreateAsset(RESOURCES, "topLevelAsset.asset");
            var cache =
                await GenerateCache(
                    new[] { Path.Combine(ASSETS, RESOURCES, "topLevelAsset.asset") },
                    Array.Empty<string>(),
                    DateTime.Now, tokenSource.Token);


            Assert.That(cache, Is.Null);
        }

        private async Task<ResourceCache> GenerateCache(string[] assetPaths, string[] packageNames, DateTime time,
            CancellationToken token = default)
        {
            return await new CacheGenerator(assetPaths, packageNames, time).GenerateAsync(token);
        }
    }
}