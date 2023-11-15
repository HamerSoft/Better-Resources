using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace HamerSoft.BetterResources.Tests
{
    public abstract class ResourceTests : TestBaseInfrastructure
    {
        [UnitySetUp]
        public virtual IEnumerator SetUp()
        {
            yield return DoSetup();
        }

        [TearDown]
        public virtual void TearDown()
        {
            DoTearDown();
        }
    }
}