using System;
using System.IO;
using System.Threading;
using UnityEngine;

namespace HamerSoft.BetterResources.Samples.QueryBuilder
{
    public class QueryExamples : MonoBehaviour
    {
        private CancellationTokenSource _tokenSource;

        private async void Start()
        {
            _tokenSource = new CancellationTokenSource();
            await BetterResources.InitializeAsync(null, _tokenSource.Token);

            // The following are examples of queries but will (probably) not find results as such assets to not exist in this project
            // But these are perfectly valid queries you can run at run-time and design (editor) time.

            // Find a resource in a package (not project folder) in a sub-folder of the resources that contains the string "cameras" with a Camera component
            var myCameraResource = BetterResources.Query()
                .InPackage(true)
                .ByPackageSubString("cameras", StringComparison.OrdinalIgnoreCase)
                .WithSomeComponents(typeof(Camera))
                .GetResult();
            var camera = await BetterResources.LoadAsync(myCameraResource);

            // Find all resources with an AudioSource, MeshRenderer and Collider,
            // without an Animator that have the string "practice" in their name
            // in the Unity Project (not any packages) in a folder called "Targets".
            var myPrefabs = BetterResources.Query()
                .WithAllComponents(typeof(AudioSource), typeof(MeshRenderer), typeof(Collider))
                .WithoutAllComponents(typeof(Animator))
                .ByNameSubString("practice", StringComparison.OrdinalIgnoreCase)
                .InPackage(false)
                .ByPath($"Targets{Path.DirectorySeparatorChar}")
                .GetResults();

            var myPracticeTargets = await BetterResources.LoadAsync(myPrefabs);
        }

        private void OnDestroy()
        {
            _tokenSource.Cancel();
        }
    }
}