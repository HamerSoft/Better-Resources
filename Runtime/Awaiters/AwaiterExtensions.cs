#if UNITY_EDITOR
using UnityEditor.PackageManager.Requests;
#endif
using UnityEngine;

namespace HamerSoft.BetterResources.Awaiters
{
    /// <summary>
    /// Extensions for custom awaiters to use with async keyword
    /// </summary>
    public static class AwaiterExtensions
    {
        /// <summary>
        /// Get an awaiter for a ResourceRequest
        /// </summary>
        /// <param name="asyncOp">the resource request</param>
        /// <returns>Awaitable object</returns>
        public static ResourceRequestAwaiter GetAwaiter(this ResourceRequest asyncOp)
        {
            return new ResourceRequestAwaiter(asyncOp);
        }

#if UNITY_EDITOR
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
#endif
    }
}