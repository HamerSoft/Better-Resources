using System;
using System.Collections;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using HamerSoft.BetterResources.Extensions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace HamerSoft.BetterResources.Tests.Initialization
{
    public class Success : InitializationTest
    {
        private class TestMono : MonoBehaviour
        {
        }

        [UnitySetUp]
        public override IEnumerator SetUp()
        {
            yield return base.SetUp();
            string someAssetPath = "";
            yield return (AwaitCreateAssetInPackage(Path.Combine(RESOURCES, "myTestFolder"), "someAsset.asset"))
                .ToCoroutine(s => someAssetPath = s);
            yield return AwaitCreateAsset(RESOURCES, "topLevelAsset.asset").ToCoroutine();
            ResourceCache cache = null;

            yield return GenerateCache(
                new[]
                {
                    Path.Combine(ASSETS, RESOURCES, "topLevelAsset.asset"),
                    someAssetPath
                },
                new[] { COM_HAMERSOFT_BETTERRESOURCES },
                DateTime.Now).ToCoroutine(resourceCache => cache = resourceCache);

            var json = CacheGenerator.ToJson(cache);
            yield return AwaitCreateAsset(RESOURCES, "ResourcesCache.asset", json).ToCoroutine();
        }

        private async Task<ResourceCache> GenerateCache(string[] assetPaths, string[] packageNames, DateTime time)
        {
            return await new CacheGenerator(assetPaths, packageNames, time).GenerateAsync();
        }

        [Test]
        public void Initialize_Loads_ManifestFrom_Resources()
        {
            var isValid = false;

            void InitCallback(bool valid)
            {
                isValid = valid;
            }

            BetterResources.Initialized += InitCallback;
            BetterResources.Initialize();
            BetterResources.Initialized -= InitCallback;
            Assert.That(BetterResources.Query().ByName("topLevelAsset"), Is.Not.Null);
            Assert.That(isValid, Is.True);
            Assert.That(BetterResources.IsInitialized, Is.True);
            Assert.That(BetterResources.IsValid, Is.True);
        }

        [Test]
        public async Task InitializeAsync_Loads_ManifestFrom_Resources()
        {
            var isValid = false;

            void InitCallback(bool valid)
            {
                isValid = valid;
            }

            BetterResources.Initialized += InitCallback;
            await BetterResources.InitializeAsync();
            BetterResources.Initialized -= InitCallback;
            Assert.That(BetterResources.Query().ByName("topLevelAsset"), Is.Not.Null);
            Assert.That(isValid, Is.True);
            Assert.That(BetterResources.IsInitialized, Is.True);
            Assert.That(BetterResources.IsValid, Is.True);
        }

        [Test]
        public async Task InitializeAsync_Stops_Loading_ManifestFrom_Resources_WhenCancelled()
        {
            var tokenSource = new CancellationTokenSource();
            tokenSource.Cancel();
            await BetterResources.InitializeAsync(null, tokenSource.Token);
            Assert.That(BetterResources.IsInitialized, Is.False);
            Assert.That(BetterResources.IsValid, Is.False);
        }

        [UnityTest]
        public IEnumerator InitializeRoutine_Loads_ManifestFrom_Resources()
        {
            var isValid = false;

            void InitCallback(bool valid)
            {
                isValid = valid;
            }

            BetterResources.Initialized += InitCallback;
            yield return BetterResources.InitializeRoutine();
            BetterResources.Initialized -= InitCallback;
            Assert.That(BetterResources.Query().ByName("topLevelAsset"), Is.Not.Null);
            Assert.That(isValid, Is.True);
            Assert.That(BetterResources.IsInitialized, Is.True);
            Assert.That(BetterResources.IsValid, Is.True);
        }

        [UnityTest]
        public IEnumerator InitializeRoutine_Stops_Loading_ManifestFrom_Resources_WhenStopped()
        {
            var go = new GameObject().AddComponent<TestMono>();
            var tokenSource = new CancellationTokenSource();
            tokenSource.Cancel();
            var routine = go.StartCoroutine(BetterResources.InitializeRoutine(null, tokenSource.Token));
            go.StopCoroutine(routine);
            yield return Task.Delay(1000).ToCoroutine();
            Assert.That(BetterResources.IsInitialized, Is.False);
            Assert.That(BetterResources.IsValid, Is.False);
        }
    }
}