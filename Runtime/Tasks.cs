using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;

namespace KOOP
{
    /// <summary>
    /// Helper class that provides extended patterns for exception logging when working with tasks.
    /// </summary>
    /// <owner>jonagill</owner>
    public static class Tasks
    {
#if UNITY_EDITOR
        [InitializeOnLoadMethod]
#else
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
#endif
        private static void Initialize()
        {
            // Subscribe for notifications if an unobserved task (e.g. one not awaited or stored in a variable)
            // ends with throwing an exception
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;   
        }

        private static void OnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs args)
        {
            LogException(args.Exception);
        }

        private static void LogException(Exception exception)
        {
            if (exception is AggregateException aggregateException)
            {
                exception = aggregateException.InnerException;
            }
            
            if (exception is TaskCanceledException)
            {
                // This task was canceled intentionally -- not worth logging
                return;
            }
            
            Debug.LogException(exception);
        }
    }    
}

