#if UNITY_EDITOR

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;

namespace HamerSoft.BetterResources.Awaiters
{
    /// <summary>
    /// Awaiter for <see cref="UnityEditor.PackageManager.Requests.ListRequest"/>
    /// </summary>
    [DebuggerNonUserCode]
    public struct ListRequestAwaiter : INotifyCompletion
    {
        private readonly ListRequest _asyncOperation;

        /// <summary>
        /// A flag indicating if the ListRequest is complete
        /// </summary>
        public bool IsCompleted => _asyncOperation.IsCompleted;

        private Action _complete;

        /// <summary>
        /// Initialized a new instance of ListRequestAwaiter
        /// </summary>
        /// <param name="asyncOperation">The ListRequest returned by the PackageManager</param>
        public ListRequestAwaiter(ListRequest asyncOperation)
        {
            _asyncOperation = asyncOperation;
            _complete = null;
        }

        /// <inheritdoc/>
        public void OnCompleted(Action continuation)
        {
            if (!IsCompleted)
            {
                if (_complete != null)
                    return;

                _complete = continuation;
                EditorApplication.update += WaitForResponse;
            }
            else
            {
                continuation?.Invoke();
            }
        }

        private void WaitForResponse()
        {
            if (!_asyncOperation.IsCompleted)
                return;

            EditorApplication.update -= WaitForResponse;
            _complete?.Invoke();
            _complete = null;
        }

        /// <summary>
        /// Get the return value from the ListRequest
        /// </summary>
        /// <returns>PackageCollection object</returns>
        public PackageCollection GetResult() => _asyncOperation.Result;
    }
}
#endif