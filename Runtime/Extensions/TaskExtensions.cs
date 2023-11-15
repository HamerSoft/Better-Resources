using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;

namespace HamerSoft.BetterResources.Extensions
{
    /// <summary>
    /// Extensions to use tasks as coroutines
    /// </summary>
    public static class TaskExtensions
    {
        /// <summary>
        /// Convert a task to coroutine
        /// </summary>
        /// <param name="task">the task</param>
        /// <param name="token">optional cancellation token</param>
        /// <returns>IEnumerator to yield in coroutines</returns>
        public static IEnumerator ToCoroutine(this Task task, CancellationToken token = default)
        {
            while (!task.IsCanceled && !task.IsFaulted && !task.IsCompleted && !token.IsCancellationRequested)
                yield return null;
        }

        /// <summary>
        /// Convert a task of Type T to a coroutine
        /// </summary>
        /// <param name="task">to task</param>
        /// <param name="callback">callback to catch the return value of task T</param>
        /// <param name="token">optional cancellation token</param>
        /// <typeparam name="T">Task Type param</typeparam>
        /// <returns>IEnumerator to yield in coroutines</returns>
        public static IEnumerator ToCoroutine<T>(this Task<T> task, Action<T> callback = null,
            CancellationToken token = default)
        {
            while (!task.IsCanceled && !task.IsFaulted && !task.IsCompleted && !token.IsCancellationRequested)
                yield return null;
            if (task.IsCompleted && !token.IsCancellationRequested)
                callback?.Invoke(task.Result);
        }
    }
}