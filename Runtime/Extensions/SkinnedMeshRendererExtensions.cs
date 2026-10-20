using UnityEngine;

public static class SkinnedMeshRendererExtensions
{
    public static void MatchBones(this SkinnedMeshRenderer target, SkinnedMeshRenderer source, Transform rootBone = null)
    {
        target.bones = source.bones;

        if (rootBone == null)
        {
            rootBone = source.rootBone;
        }

        target.rootBone = rootBone;
    }

    public static void MatchBounds(this SkinnedMeshRenderer target, SkinnedMeshRenderer source)
    {
        target.localBounds = source.localBounds;
    }

    /// <summary>
    /// 
    /// </summary>
    public static void SetBlendShapeWeight(this SkinnedMeshRenderer target, string blendShapeName, float weight)
    {
        var blendShapeIndex = target.sharedMesh.GetBlendShapeIndex(blendShapeName);
        if (blendShapeIndex == -1)
        {
            Debug.LogWarning($"Attempted to set blend shape {blendShapeName} on SkinnedMeshRenderer {target.gameObject.name} but failed: Blend shape not found.", target);
            return;
        }

        target.SetBlendShapeWeight(blendShapeIndex, weight);
    }
}