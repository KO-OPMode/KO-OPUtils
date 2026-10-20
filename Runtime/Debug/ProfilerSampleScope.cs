using System;
using UnityEngine;
using UnityEngine.Profiling;

namespace KOOP.Utils
{
    /// <summary>
    /// Scoped class for automatically calling <see cref="Profiler.BeginSample(string)"/>
    /// and <see cref="Profiler.EndSample()"/> within the defined scope.
    /// </summary>
    public struct ProfilerSampleScope : IDisposable
    {
        private bool _isDisposed;

        public ProfilerSampleScope(string name, bool profilingEnabled = true)
        {
            if (profilingEnabled)
            {
                _isDisposed = false;
                Profiler.BeginSample(name);
            }
            else
            {
                _isDisposed = true;
            }
        }
        
        public ProfilerSampleScope(string name, UnityEngine.Object targetObject, bool profilingEnabled = true)
        {

            if (profilingEnabled)
            {
                _isDisposed = false;
                Profiler.BeginSample(name, targetObject);
            }
            else
            {
                _isDisposed = true;
            }
        }
        
        public void Dispose()
        {
            if (_isDisposed) return;
            Profiler.EndSample();
            _isDisposed = true;
        }
    }
}

