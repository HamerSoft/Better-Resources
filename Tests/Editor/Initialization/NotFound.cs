using System.Collections;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace HamerSoft.BetterResources.Tests.Initialization
{
    public class NotFound : InitializationTest
    {
        [Test]
        public void When_ResourceCache_Is_Not_Found_BetterResources_Fires_Event_Invalid_During_Initialize()
        {
            bool isValid = false;

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
        public async Task When_ResourceCache_Is_Not_Found_BetterResources_Fires_Event_Invalid_During_InitializeAsync()
        {
            bool isValid = false;

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
        public IEnumerator
            When_ResourceCache_Is_Not_Found_BetterResources_Fires_Event_Invalid_During_InitializeRoutine()
        {
            bool isValid = false;

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