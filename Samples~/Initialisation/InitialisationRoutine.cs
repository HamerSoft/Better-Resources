using System.Collections;
using UnityEngine;

namespace HamerSoft.BetterResources.Samples.CacheGeneration
{
    public class InitialisationRoutine : MonoBehaviour
    {
        private IEnumerator Start()
        {
            /* Assumption: a Resources cache is available in the root Resources folder
            / It can be generated through the `Tools/HamerSoft/BetterResources/Generate Cache` menu
            / or by some custom mechanism, see BetterResourcesEditor.cs for more information.
            /
            / Subscribe to initialized event to do additional processing
            / Note: the event subscription is not really needed since we are yielding init
            */
            BetterResources.Initialized += BetterResourcesOnInitialized_Handler;
            // Initialize BetterResources.
            yield return BetterResources.InitializeRoutine();
            //Queries are now enabled :)
            using var queryBuilder = BetterResources.Query();
            var myMaterialResource = queryBuilder.GetResult<Material>();
            var myMaterial = BetterResources.Load(myMaterialResource);
            // do something with myMaterial.
        }

        private void BetterResourcesOnInitialized_Handler(bool isValid)
        {
            BetterResources.Initialized -= BetterResourcesOnInitialized_Handler;
            Debug.Log($"Better Resources initialized {(isValid ? "successfully" : "failed")}.");
        }
    }
}