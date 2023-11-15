using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HamerSoft.BetterResources.Extensions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace HamerSoft.BetterResources.Tests
{
    [TestFixture]
    public class QueryBuilderComponentsTests : TestBaseInfrastructure
    {
        private ResourceManifest _manifest;

        [UnitySetUp]
        public IEnumerator SetSetup()
        {
            yield return DoSetup();
            ResourceCache cache = null;
            yield return (AwaitCreatePrefab(RESOURCES, "CameraAudioLight.prefab", typeof(Camera), typeof(AudioSource),
                typeof(Light)).ToCoroutine());
            yield return (AwaitCreatePrefab(RESOURCES, "CameraAudio.prefab", typeof(Camera), typeof(AudioSource))
                .ToCoroutine());
            yield return (AwaitCreatePrefab(RESOURCES, "Camera.prefab", typeof(Camera))
                .ToCoroutine());
            yield return (AwaitCreatePrefab(RESOURCES, "Light.prefab", typeof(Light))
                .ToCoroutine());
            yield return GenerateCache(
                new[]
                {
                    Path.Combine(ASSETS, RESOURCES, "CameraAudioLight.prefab"),
                    Path.Combine(ASSETS, RESOURCES, "CameraAudio.prefab"),
                    Path.Combine(ASSETS, RESOURCES, "Camera.prefab"),
                    Path.Combine(ASSETS, RESOURCES, "Light.prefab"),
                },
                new[] { COM_HAMERSOFT_BETTERRESOURCES },
                DateTime.Now).ToCoroutine(resourceCache => cache = resourceCache);

            var json = CacheGenerator.ToJson(cache);
            yield return ManifestGenerator.FromResourceCacheJsonAsync(json)
                .ToCoroutine(manifest => _manifest = manifest);
        }

        private async Task<ResourceCache> GenerateCache(string[] assetPaths, string[] packageNames, DateTime time)
        {
            return await new CacheGenerator(assetPaths, packageNames, time).GenerateAsync();
        }

        [Test]
        public void QueryBuilder_Filters_Objects_With_All_Multiple_Components()
        {
            var query = new QueryBuilder(_manifest).WithAllComponents(typeof(Camera), typeof(AudioSource),
                typeof(Light));
            Assert.That(query.GetResult(), Is.Not.Null);
            Assert.That(query.GetResult().Name, Is.EqualTo("CameraAudioLight"));
        }

        [Test]
        public void QueryBuilder_Filters_Objects_With_Some_Multiple_Components()
        {
            var query = new QueryBuilder(_manifest).WithSomeComponents(typeof(AudioSource),
                typeof(Light));
            var results = query.GetResults();
            Assert.That(results.Count(), Is.EqualTo(3));
            Assert.That(results.ToList().Exists(ri => ri.Name == "Camera"), Is.False);
        }

        [Test]
        public void QueryBuilder_Filters_Objects_Without_Any_Multiple_Components()
        {
            var query = new QueryBuilder(_manifest).WithoutAnyComponents(typeof(AudioSource), typeof(Light));
            Assert.That(query.GetResult(), Is.Not.Null);
            Assert.That(query.GetResult().Name, Is.EqualTo("Camera"));
        }

        [Test]
        public void QueryBuilder_Filters_Objects_Without_All_Multiple_Components()
        {
            var query = new QueryBuilder(_manifest).WithoutAllComponents(typeof(AudioSource), typeof(Light),
                typeof(Camera), typeof(Transform));
            var results = query.GetResults();
            Assert.That(results.Count(), Is.EqualTo(3));
            Assert.That(results.ToList().Exists(ri => ri.Name == "CameraAudioLight"), Is.False);
        }

        [Test]
        public void QueryBuilder_Filters_Objects_With_And_Without_Multiple_Components()
        {
            var query = new QueryBuilder(_manifest).WithAllComponents(typeof(Light)).WithoutAnyComponents(
                typeof(AudioSource), typeof(Camera),
                typeof(TextAsset));
            Assert.That(query.GetResult(), Is.Not.Null);
            Assert.That(query.GetResult().Name, Is.EqualTo("Light"));
        }

        [Test]
        public async Task ResourceInfo_Found_By_QueryBuilder_Load_Async()
        {
            var queryResult = new QueryBuilder(_manifest).ByName("CameraAudioLight").GetResult<Camera>();
            var resource = await BetterResources.LoadAsync(queryResult);
            var typedResource = await BetterResources.LoadAsync<Camera>(queryResult);
            Assert.That(resource, Is.Not.Null);
            Assert.That(resource.name, Is.EqualTo("CameraAudioLight"));
            Assert.That(typedResource, Is.Not.Null);
            Assert.That(typedResource.name, Is.EqualTo("CameraAudioLight"));
        }

        [Test]
        public async Task ResourceInfo_Found_By_QueryBuilder_Can_Be_Loaded_Through_All_ComponentTypes()
        {
            var queryResult = new QueryBuilder(_manifest).ByName("CameraAudioLight").GetResult<Camera>();
            var resource = await BetterResources.LoadAsync(queryResult);
            var genericResource = await BetterResources.LoadAsync<Camera>(queryResult);
            var typedResource = await BetterResources.LoadAsync(queryResult, typeof(Camera));
            Assert.That(resource, Is.Not.Null);
            Assert.That(resource.name, Is.EqualTo("CameraAudioLight"));
            Assert.That(genericResource, Is.Not.Null);
            Assert.That(genericResource.name, Is.EqualTo("CameraAudioLight"));
            Assert.That(typedResource, Is.Not.Null);
            Assert.That(typedResource.name, Is.EqualTo("CameraAudioLight"));
        }

        [TearDown]
        public void OneTimeTearDown()
        {
            DoTearDown();
        }
    }
}