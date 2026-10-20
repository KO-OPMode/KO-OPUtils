using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using System;

namespace KOOP.Extensions
{
    public static class AsyncExtensions
    {
        /// <summary>
        /// Simple wrapper around subscribing to <see cref="AsyncOperationHandle.Completed"/> 
        /// that pre-checks if the operation has already finished and fires <see cref="onLoaded"/> immediately if so.
        /// </summary>
        public static void FireWhenLoadedOrIfAlreadyLoaded(this AsyncOperationHandle handle, Action<AsyncOperationHandle> onLoaded)
        {
            if (handle.Result == null)
            {
                handle.Completed += onLoaded;
            }
            else
            {
                onLoaded?.Invoke(handle);
            }
        }
        
        /// <summary>
        /// Simple wrapper around subscribing to <see cref="AsyncOperationHandle.Completed"/> 
        /// that pre-checks if the operation has already finished and fires <see cref="onLoaded"/> immediately if so.
        /// </summary>
        public static void FireWhenLoadedOrIfAlreadyLoaded<T>(this AsyncOperationHandle<T> handle, Action<AsyncOperationHandle<T>> onLoaded)
        {
            if (handle.Result == null)
            {
                handle.Completed += onLoaded;
            }
            else
            {
                onLoaded?.Invoke(handle);
            }
        }
    }
}