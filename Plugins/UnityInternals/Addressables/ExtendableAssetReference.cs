using System;

namespace UnityEngine.AddressableAssets.KOOP
{
    /// <summary>
    /// <see cref="AssetReference"/> type that exposes functionality that is usually internal to the Addressables package
    /// so that we can create our own extended reference types.
    /// </summary>
    /// <owner>jonagill</owner>
    [Serializable]
    public abstract class ExtendableAssetReference : AssetReference
    {
        protected ExtendableAssetReference(string guid) : base(guid) { }

#if UNITY_EDITOR
        protected internal sealed override Type DerivedClassType => ExposedDerivedClassType;
        protected abstract Type ExposedDerivedClassType { get; }
#endif
    }   
}
