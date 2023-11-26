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

    }
}