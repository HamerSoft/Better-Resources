using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using HamerSoft.BetterResources.Editor;
using NUnit.Framework;

namespace HamerSoft.BetterResources.Tests
{
    [TestFixture]
    public class PostProcessorTests : ResourceTests
    {
        [Test]
        public void When_NonResources_Are_Imported_AssetProcessor_DoesNot_Run()
        {
            var shouldRefresh = AssetPostProcessor.ShouldRefresh(
                new[] { Path.Combine(BetterResources.ASSETS, "RandomFolder", "someAsset.asset") },
                Array.Empty<string>(),
                Array.Empty<string>());
            Assert.That(shouldRefresh, Is.False);
        }

        [Test]
        public void When_NonResources_Are_Deleted_AssetProcessor_DoesNot_Run()
        {
            var shouldRefresh = AssetPostProcessor.ShouldRefresh(
                Array.Empty<string>(),
                new[] { Path.Combine(BetterResources.ASSETS, "RandomFolder", "someAsset.asset") },
                Array.Empty<string>());
            Assert.That(shouldRefresh, Is.False);
        }

        [Test]
        public void When_NonResources_Are_Moved_AssetProcessor_DoesNot_Run()
        {
            var shouldRefresh = AssetPostProcessor.ShouldRefresh(
                Array.Empty<string>(),
                Array.Empty<string>(),
                new[] { Path.Combine(BetterResources.ASSETS, "RandomFolder", "someAsset.asset") }
            );
            Assert.That(shouldRefresh, Is.False);
        }

        [Test]
        public void When_Cache_IsImported_AssetProcessor_DoesNot_Run()
        {
            var shouldRefresh = AssetPostProcessor.ShouldRefresh(
                new[]
                {
                    Path.Combine(BetterResources.ASSETS, BetterResources.RESOURCES,
                        $"{BetterResources.RESOURCES_CACHE}.asset")
                },
                Array.Empty<string>(),
                Array.Empty<string>());
            Assert.That(shouldRefresh, Is.False);
        }

        [Test]
        public void When_Resources_Are_Imported_AssetProcessor_Does_Run()
        {
            var shouldRefresh = AssetPostProcessor.ShouldRefresh(
                new[] { Path.Combine(BetterResources.ASSETS, BetterResources.RESOURCES, "someAsset.asset") },
                Array.Empty<string>(),
                Array.Empty<string>());
            Assert.That(shouldRefresh, Is.True);
        }

        [Test]
        public void When_Resources_Are_Deleted_AssetProcessor_Does_Run()
        {
            var shouldRefresh = AssetPostProcessor.ShouldRefresh(
                Array.Empty<string>(),
                new[] { Path.Combine(BetterResources.ASSETS, BetterResources.RESOURCES, "someAsset.asset") },
                Array.Empty<string>());
            Assert.That(shouldRefresh, Is.True);
        }

        [Test]
        public void When_Resources_Are_Moved_AssetProcessor_Does_Run()
        {
            var shouldRefresh = AssetPostProcessor.ShouldRefresh(
                Array.Empty<string>(),
                Array.Empty<string>(),
                new[] { Path.Combine(BetterResources.ASSETS, BetterResources.RESOURCES, "someAsset.asset") }
            );
            Assert.That(shouldRefresh, Is.True);
        }

#if BETTERRESOURCES_AUTO_GENERATE
        [Test(Description = "Ugly integration test using reflection yet this should be able to test the PostProcessor until I find a better way.")]
        public async Task When_Resources_Are_Detected_A_New_Cache_Is_Generated()
        {
            typeof(BetterResources).GetProperty("IsInitialized", BindingFlags.Static | BindingFlags.Public)
                .SetValue(null, false);
            typeof(BetterResources).GetProperty("IsValid", BindingFlags.Static | BindingFlags.Public)
                .SetValue(null, false);
            bool isGenerated = false;
            bool isValid = false;
            bool isInitialized = false;

            void Generated()
            {
                BetterResourcesEditor.CacheGenerated -= Generated;
                isGenerated = true;
            }

            void Initialized(bool valid)
            {
                BetterResources.Initialized -= Initialized;
                isInitialized = true;
                isValid = valid;
            }

            BetterResourcesEditor.CacheGenerated += Generated;
            BetterResources.Initialized += Initialized;
            AssetPostProcessor.Enable();
            await AwaitCreateAsset(BetterResources.RESOURCES, "my-new-Asset.asset");
            await WaitUntil(() => isGenerated);
            await WaitUntil(() => isInitialized);

            BetterResourcesEditor.CacheGenerated -= Generated;
            BetterResources.Initialized -= Initialized;
            Assert.That(isGenerated, Is.True);
            Assert.That(isInitialized, Is.True);
            Assert.That(isValid, Is.True.Or.False);
        }
#endif
    }
}