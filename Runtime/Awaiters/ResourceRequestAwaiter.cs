using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;
using Object = UnityEngine.Object;

namespace HamerSoft.BetterResources.Awaiters
{
    /// <summary>
    /// Awaiter for <see cref="UnityEngine.ResourceRequest"/>
    /// </summary>
    [DebuggerNonUserCode]
    public readonly struct ResourceRequestAwaiter : INotifyCompletion
    {
        private readonly ResourceRequest _asyncOperation;
        /// <summary>
        /// a flag indicating the operation is complete
        /// </summary>
        public bool IsCompleted => _asyncOperation.isDone;
        /// <summary>
        /// Initializes a new instance of a ResourceRequestAwaiter
        /// </summary>
        /// <param name="asyncOperation">the resource request to wait for</param>
        public ResourceRequestAwaiter(ResourceRequest asyncOperation) => _asyncOperation = asyncOperation;
        
        /// <inheritdoc/>
        public void OnCompleted(Action continuation) => _asyncOperation.completed += _ => continuation();
        /// <summary>
        /// Get the result Object of the ResourceRequest
        /// </summary>
        /// <returns></returns>
        public Object GetResult() => _asyncOperation.asset;
    }
}