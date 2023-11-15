using System.Collections;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HamerSoft.BetterResources.Extensions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace HamerSoft.BetterResources.Tests.Initialization
{
    public class Invalid : InitializationTest
    {
        [UnitySetUp]
        public override IEnumerator SetUp()
        {
            yield return base.SetUp();
            yield return AwaitCreateAsset(RESOURCES, "ResourcesCache.asset", "some invalid json").ToCoroutine();
        }

        [Test]
        public void BetterResources_Fails_To_Initialize_When_ResourceCache_In_Invalid()
        {
            var isValid = false;

            void InitCallback(bool valid)
            {
                isValid = valid;
            }

            BetterResources.Initialized += InitCallback;
            BetterResources.Initialize();
            LogAssert.Expect(LogType.Error, new Regex(""));
            BetterResources.Initialized -= InitCallback;
            Assert.That(isValid, Is.False);
            Assert.That(BetterResources.IsInitialized, Is.True);
            Assert.That(BetterResources.IsValid, Is.False);
        }

        [Test]
        public async Task BetterResources_Fails_To_InitializeAsync_When_ResourceCache_In_Invalid()
        {
            var isValid = false;

            void InitCallback(bool valid)
            {
                isValid = valid;
            }

            BetterResources.Initialized += InitCallback;
            await BetterResources.InitializeAsync();
            LogAssert.Expect(LogType.Error, new Regex(""));
            BetterResources.Initialized -= InitCallback;
            Assert.That(isValid, Is.False);
            Assert.That(BetterResources.IsInitialized, Is.True);
            Assert.That(BetterResources.IsValid, Is.False);
        }

        [UnityTest]
        public IEnumerator BetterResources_Fails_To_InitializeRoutine_When_ResourceCache_In_Invalid()
        {
            var isValid = false;

            void InitCallback(bool valid)
            {
                isValid = valid;
            }

            BetterResources.Initialized += InitCallback;
            yield return BetterResources.InitializeRoutine();
            LogAssert.Expect(LogType.Error, new Regex(""));
            BetterResources.Initialized -= InitCallback;
            Assert.That(isValid, Is.False);
            Assert.That(BetterResources.IsInitialized, Is.True);
            Assert.That(BetterResources.IsValid, Is.False);
        }
    }
}