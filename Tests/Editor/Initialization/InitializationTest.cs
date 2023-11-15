using System.Collections;
using System.Reflection;
using UnityEngine.TestTools;

namespace HamerSoft.BetterResources.Tests.Initialization
{
    public class InitializationTest : ResourceTests
    {
        [UnitySetUp]
        public override IEnumerator SetUp()
        {
            yield return base.SetUp();
            typeof(BetterResources).GetProperty("IsInitialized", BindingFlags.Static | BindingFlags.Public)
                .SetValue(null, false);
            typeof(BetterResources).GetProperty("IsValid", BindingFlags.Static | BindingFlags.Public)
                .SetValue(null, false);
        }
    }
}