// #define BETTERRESOURCES_PRE_BUILD

#if BETTERRESOURCES_PRE_BUILD
using System.Diagnostics.CodeAnalysis;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace HamerSoft.BetterResources.Editor
{
    /// <summary>
    /// This Pre-Build hook for BetterResources will generate a ResourceCache once a build is triggered (any platform).
    /// <remarks> Enable Scripting define: BETTERRESOURCES_PRE_BUILD to enable!</remarks>
    /// <remarks> Either by adding it to the UnityEngine PlayerSettings or at the top of some file write: #define BETTERRESOURCES_PRE_BUILD</remarks>
    /// </summary>
    [InitializeOnLoad, ExcludeFromCoverage, ExcludeFromCodeCoverage]
    internal static class PreBuildHook
    {
        static PreBuildHook()
        {
            BuildPlayerWindow.RegisterBuildPlayerHandler(OnClickBuildPlayer);
        }

        private static void OnClickBuildPlayer(BuildPlayerOptions options)
        {
            Debug.Log("<color=green>Generating BetterResources Cache!</color>");
            BetterResourcesEditor.GenerateCache();
            Debug.Log("<color=green>Generated BetterResources Cache Success!</color>");
            BuildPlayerWindow.DefaultBuildMethods.BuildPlayer(options);
        }
    }
}
#endif