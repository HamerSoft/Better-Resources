using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HamerSoft.BetterResources.Editor;
using HamerSoft.BetterResources.Extensions;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace HamerSoft.BetterResources.Tests
{
    [TestFixture]
    public class QueryBuilderTests : TestBaseInfrastructure
    {
        private ResourceManifest _manifest;

        [UnitySetUp]
        public IEnumerator SetSetup()
        {
            yield return DoSetup();
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
            yield return ManifestGenerator.FromResourceCacheJsonAsync(json)
                .ToCoroutine(manifest => _manifest = manifest);
        }

        private async Task<ResourceCache> GenerateCache(string[] assetPaths, string[] packageNames, DateTime time)
        {
            return await new CacheGenerator(assetPaths, packageNames, time).GenerateAsync();
        }

        [Test]
        public void When_QueryBuilder_Is_Empty_And_Type_IsNot_Specified_Nothing_Is_Found()
        {
            var query = new QueryBuilder(_manifest);
            Assert.That(query.GetResult(), Is.Null);
        }

        [Test]
        public void When_QueryBuilder_Is_Empty_And_Type_Is_Specified_Assets_Are_Found()
        {
            var query = new QueryBuilder(_manifest);
            var result = query.GetResults<TextAsset>();
            Assert.That(result.Count(), Is.EqualTo(2));
        }

        [Test]
        public void QueryBuilder_Finds_Object_By_Name()
        {
            var query = new QueryBuilder(_manifest).ByName("someAsset");
            Assert.That(query.GetResult(), Is.Not.Null);
            Assert.That(query.GetResult().Name, Is.EqualTo("someAsset"));
            Assert.That(query.GetResult<TextAsset>(), Is.Not.Null);
            Assert.That(query.GetResult<TextAsset>().Name, Is.EqualTo("someAsset"));
        }

        [Test]
        public void QueryBuilder_Finds_Object_By_Name_UsesComparer()
        {
            var query = new QueryBuilder(_manifest).ByName("SoMeAsSeT",
                StringComparison.InvariantCultureIgnoreCase);
            Assert.That(query.GetResult(), Is.Not.Null);
            Assert.That(query.GetResult().Name, Is.EqualTo("someAsset"));
            Assert.That(query.GetResult<TextAsset>(), Is.Not.Null);
            Assert.That(query.GetResult<TextAsset>().Name, Is.EqualTo("someAsset"));
        }

        [Test]
        public void QueryBuilder_Finds_Object_AtRoot()
        {
            Assert.That("foo".StartsWith(""), Is.True);
            var query = new QueryBuilder(_manifest).AtRoot();
            Assert.That(query.GetResult(), Is.Not.Null);
            Assert.That(query.GetResult().Name, Is.EqualTo("topLevelAsset"));
        }

        [Test]
        public void QueryBuilder_Finds_Object_ByPath()
        {
            var query = new QueryBuilder(_manifest).ByPath("myTestFolder");
            Assert.That(query.GetResult(), Is.Not.Null);
            Assert.That(query.GetResult().Name, Is.EqualTo("someAsset"));
        }

        [Test]
        public void QueryBuilder_Finds_Object_ByGuid()
        {
            var guid = AssetDatabase.GUIDFromAssetPath(Path.Combine(ASSETS, RESOURCES, "topLevelAsset.asset"));
            var query = new QueryBuilder(_manifest).ByGuid(new System.Guid(guid.ToString()));
            var queryString = new QueryBuilder(_manifest).ByGuid(guid.ToString());
            Assert.That(query.GetResult(), Is.Not.Null);
            Assert.That(queryString.GetResult(), Is.Not.Null);
        }

        [Test]
        public void QueryBuilder_Finds_Object_ByPath_UsesComparer()
        {
            var query = new QueryBuilder(_manifest).ByPath("MyTeStFoLdEr",
                StringComparison.InvariantCultureIgnoreCase);
            Assert.That(query.GetResult(), Is.Not.Null);
            Assert.That(query.GetResult().Name, Is.EqualTo("someAsset"));
        }

        [Test]
        public void QueryBuilder_Finds_Object_ByPackage()
        {
            var query = new QueryBuilder(_manifest).ByPackage(COM_HAMERSOFT_BETTERRESOURCES);
            Assert.That(query.GetResult(), Is.Not.Null);
            Assert.That(query.GetResult().Name, Is.EqualTo("someAsset"));
        }

        [Test]
        public void QueryBuilder_Finds_Object_ByPackage_UsesComparer()
        {
            var query = new QueryBuilder(_manifest).ByPackage(COM_HAMERSOFT_BETTERRESOURCES,
                StringComparison.CurrentCultureIgnoreCase);
            Assert.That(query.GetResult(), Is.Not.Null);
            Assert.That(query.GetResult().Name, Is.EqualTo("someAsset"));
        }

        [Test]
        public void QueryBuilder_Finds_Object_By_Name_SubString()
        {
            var query = new QueryBuilder(_manifest).ByNameSubString("some");
            Assert.That(query.GetResult(), Is.Not.Null);
            Assert.That(query.GetResult().Name, Is.EqualTo("someAsset"));
        }

        [Test]
        public void QueryBuilder_Will_Search_By_Last_Set_Name_Or_SubString()
        {
            var query = new QueryBuilder(_manifest).ByName("my cancelled out name").ByNameSubString("some");
            Assert.That(query.GetResult(), Is.Not.Null);
            Assert.That(query.GetResult().Name, Is.EqualTo("someAsset"));

            query = new QueryBuilder(_manifest).ByNameSubString("my cancelled out name").ByName("someAsset");
            Assert.That(query.GetResult(), Is.Not.Null);
            Assert.That(query.GetResult().Name, Is.EqualTo("someAsset"));
        }

        [Test]
        public void QueryBuilder_Will_Search_By_Last_Set_Path_Or_Root_Or_SubString()
        {
            var query = new QueryBuilder(_manifest)
                .ByPath("my cancelled out name")
                .AtRoot()
                .ByPathSubString("Folder");
            Assert.That(query.GetResult(), Is.Not.Null);
            Assert.That(query.GetResult().Name, Is.EqualTo("someAsset"));

            query = new QueryBuilder(_manifest)
                .ByPathSubString("my cancelled out name")
                .AtRoot()
                .ByPath("myTestFolder");
            Assert.That(query.GetResult(), Is.Not.Null);
            Assert.That(query.GetResult().Name, Is.EqualTo("someAsset"));
            
            query = new QueryBuilder(_manifest)
                .ByPathSubString("my cancelled out name")
                .ByPath("some random folder")
                .AtRoot();
            Assert.That(query.GetResult(), Is.Not.Null);
            Assert.That(query.GetResult().Name, Is.EqualTo("topLevelAsset"));
        }

        [Test]
        public void QueryBuilder_Will_Search_By_Last_Set_Package_Or_SubString()
        {
            var query = new QueryBuilder(_manifest).ByPackage("random package").ByPackageSubString("hamersoft");
            Assert.That(query.GetResult(), Is.Not.Null);
            Assert.That(query.GetResult().Name, Is.EqualTo("someAsset"));

            query = new QueryBuilder(_manifest).ByPackageSubString("some substring")
                .ByPackage(COM_HAMERSOFT_BETTERRESOURCES);
            Assert.That(query.GetResult(), Is.Not.Null);
            Assert.That(query.GetResult().Name, Is.EqualTo("someAsset"));
        }

        [Test]
        public void QueryBuilder_Filters_Objects_By_Package_If_InPackage_IsFlagged()
        {
            var query = new QueryBuilder(_manifest).InPackage(true);
            Assert.That(query.GetResult(), Is.Not.Null);
            Assert.That(query.GetResult().Name, Is.EqualTo("someAsset"));

            query = new QueryBuilder(_manifest).InPackage(false);
            Assert.That(query.GetResult(), Is.Not.Null);
            Assert.That(query.GetResult().Name, Is.EqualTo("topLevelAsset"));
        }
        
        [Test]
        public void QueryBuilder_Returns_Nothing_When_ALL_ComponentQuery_Is_Null()
        {
            var query = new QueryBuilder(_manifest).WithAllComponents();
            Assert.That(query.GetResult(), Is.Null);
        }

        [Test]
        public void QueryBuilder_Returns_Nothing_When_ANY_ComponentQuery_Is_Null()
        {
            var query = new QueryBuilder(_manifest).WithoutAnyComponents();
            Assert.That(query.GetResult(), Is.Null);
        }
        [Test]
        public void QueryBuilder_Returns_Filters_Out_NullArguments_IN_ALL_ComponentQuery()
        {
            var query = new QueryBuilder(_manifest).WithAllComponents(typeof(TextAsset), null);
            Assert.That(query.GetResult(), Is.Not.Null);
            Assert.That(query.GetResult().Name, Is.EqualTo("topLevelAsset").Or.EqualTo("someAsset"));
        }

        [Test]
        public void QueryBuilder_Returns_Filters_Out_NullArguments_IN_ANY_ComponentQuery()
        {
            var query = new QueryBuilder(_manifest).WithoutAnyComponents(typeof(TextAsset), null);
            Assert.That(query.GetResult(), Is.Null);
        }
        
        [Test]
        public void QueryBuilder_Filters_Objects_with_One_Component()
        {
            var query = new QueryBuilder(_manifest).WithAllComponents(typeof(TextAsset));
            Assert.That(query.GetResult(), Is.Not.Null);
            Assert.That(query.GetResult().Name, Is.EqualTo("topLevelAsset").Or.EqualTo("someAsset"));
        }

        [Test]
        public void QueryBuilder_Filters_Objects_without_One_Component()
        {
            var query = new QueryBuilder(_manifest).WithoutAnyComponents(typeof(TextAsset));
            Assert.That(query.GetResult(), Is.Null);
        }

        [Test]
        public void ResourceAsset_Found_By_Query_Returns_UnityResource_When_Loaded()
        {
            var queryResult = new QueryBuilder(_manifest).InPackage(true).GetResult<TextAsset>();
            var resource = BetterResources.Load(queryResult);
            var genericResource = BetterResources.Load<TextAsset>(queryResult);
            var typedResource = BetterResources.Load(queryResult, queryResult.Type);
            Assert.That(resource, Is.Not.Null);
            Assert.That(resource, Is.TypeOf(typeof(TextAsset)));
            Assert.That(genericResource, Is.Not.Null);
            Assert.That(genericResource, Is.TypeOf(typeof(TextAsset)));
            Assert.That(typedResource, Is.Not.Null);
            Assert.That(typedResource, Is.TypeOf(typeof(TextAsset)));
        }

        [Test]
        public void QueryBuilder_Returns_Null_After_Disposed()
        {
            var query = new QueryBuilder(_manifest).AtRoot();
            Assert.That(query.GetResult(), Is.Not.Null);
            query.Dispose();
            Assert.That(query.GetResult(), Is.Null);
        }

        [TearDown]
        public void OneTimeTearDown()
        {
           DoTearDown();
        }
    }
}