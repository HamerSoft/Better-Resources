using HamerSoft.BetterResources.Awaiters;
using UnityEditor.PackageManager.Requests;

namespace HamerSoft.BetterResources.Editor
{
    /// <summary>
    /// Extensions for custom awaiters to use with async keyword for Editor namespace specific logic
    /// </summary>
    public static class AwaiterExtensionsEditor
    {
        /// <summary>
        /// Get an awaiter for a ListRequest from the <see cref="UnityEditor.PackageManager"/>
        /// </summary>
        /// <param name="asyncOp">The ListRequest</param>
        /// <returns>Awaitable object</returns>
        /// <remarks>Sometimes it takes a very long time for this to return!</remarks>
        public static ListRequestAwaiter GetAwaiter(this ListRequest asyncOp)
        {
            return new ListRequestAwaiter(asyncOp);
        }
    }
}