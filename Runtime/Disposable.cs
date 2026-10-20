using System;
using System.Runtime.CompilerServices;

namespace KOOP
{
    /// <summary>
    /// Static methods for interfacing with IDisposable objects
    /// </summary>
    public static class Disposable
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DisposeAndSetNull<T>(ref T disposable) where T : class, IDisposable
        {
            disposable?.Dispose();
            disposable = null;
        }
    }    
}

