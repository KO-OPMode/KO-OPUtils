using UnityEngine.ResourceManagement.AsyncOperations;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.AddressableAssets.KOOP
{
    /// <summary>
    /// Custom helper methods for working with Addressable AssetReferences
    /// </summary>
    public static class AssetReferenceExtensions
    {
        public static bool IsNullOrEmpty(this AssetReference assetReference)
        {
            return assetReference == null || string.IsNullOrEmpty(assetReference.AssetGUID);
        }

        public static bool KeyMatches(this AssetReference assetReference, AssetReference other)
        {
            if (assetReference.IsNullOrEmpty() || other.IsNullOrEmpty())
            {
                return false;
            }

            return assetReference.RuntimeKey == other.RuntimeKey;
        }

        public static bool TryRelease(this AsyncOperationHandle handle)
        {
            if (handle.IsValid())
            {
                handle.Release();
                return true;
            }

            return false;
        }

        public static bool TryRelease(this AsyncOperationHandle? handle)
        {
            if (handle.HasValue)
            {
                return TryRelease(handle.Value);
            }

            return false;
        }
        
        public static bool TryRelease<T>(this AsyncOperationHandle<T> handle)
        {
            if (handle.IsValid())
            {
                handle.Release();
                return true;
            }

            return false;
        }

        public static bool TryRelease<T>(this AsyncOperationHandle<T>? handle)
        {
            if (handle.HasValue)
            {
                return TryRelease(handle.Value);
            }

            return false;
        }
        
        #if UNITY_EDITOR

        public static bool EditorIsNullOrEmptyOrMissingAsset(this AssetReference assetReference)
        {
            if (assetReference.IsNullOrEmpty())
            {
                return true;
            }

            // Check if our reference actually points to an asset on disk
            var assetGuid = assetReference.AssetGUID;
            return string.IsNullOrEmpty(AssetDatabase.GUIDToAssetPath(assetGuid));
        }
        
        #endif
    }
}
