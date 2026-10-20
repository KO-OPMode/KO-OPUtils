using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace KOOP.Extensions
{
    public static class MaskExtensions
    {
        private static readonly FieldInfo Mask_m_MaskMaterial;
        private static readonly FieldInfo Mask_m_UnmaskMaterial;

        static MaskExtensions()
        {
            Mask_m_MaskMaterial = typeof(Mask).GetField("m_MaskMaterial", BindingFlags.Instance | BindingFlags.NonPublic);
            Mask_m_UnmaskMaterial = typeof(Mask).GetField("m_UnmaskMaterial", BindingFlags.Instance | BindingFlags.NonPublic);
        }
        
        public static Material GetMaskMaterial(this Mask mask)
        {
            if (mask == null) throw new ArgumentNullException(nameof(mask));
            
            return (Material)Mask_m_MaskMaterial.GetValue(mask);
        }
        
        public static Material GetUnmaskMaterial(this Mask mask)
        {
            if (mask == null) throw new ArgumentNullException(nameof(mask));

            return (Material)Mask_m_UnmaskMaterial.GetValue(mask);
        }
        
        public static void SetMaskMaterial(this Mask mask, Material maskMaterial)
        {
            if (mask == null) throw new ArgumentNullException(nameof(mask));

            Mask_m_MaskMaterial.SetValue(mask, maskMaterial);
        }
        
        public static void SetUnmaskMaterial(this Mask mask, Material unmaskMaterial)
        {
            if (mask == null) throw new ArgumentNullException(nameof(mask));

            Mask_m_UnmaskMaterial.SetValue(mask, unmaskMaterial);
        }

        /// <owner>Lazlo</owner>
        /// <summary>
        /// Clears the internal stenciled material cache for a given mask.
        /// This fixes oft reported issues in UI material modification via IMaterialModifier failing
        /// to update if the material instance of a previous modifier is reused across modifications,
        /// thus getting cached by the mask once and never updated hereafter.
        /// A similar result can be obtained by disabling and re-enabling the mask, but that is less efficient.
        /// An similar result is also obtained by not reusing modified material instances passed to the mask,
        /// but that results two material instantiations, because both the modifier and the mask will instantiate one.
        /// </summary>
        /// <remarks>
        /// See bug reports and forum threads:
        /// https://issuetracker-mig.prd.it.unity3d.com/issues/canvas-type-shadergraph-materials-are-not-updated-in-the-scene-when-they-use-the-mask-component
        /// https://discussions.unity.com/t/masked-ui-elements-shader-not-updating/607569
        /// https://discussions.unity.com/t/masked-material-shader-changes-not-working-correctly/694722
        /// https://discussions.unity.com/t/masked-ui-elements-shader-not-updating/607569/7
        /// https://www.reddit.com/r/Unity3D/comments/361lrr/making_masked_ui_sprites_work_with_custom/
        /// </remarks>
        public static void ClearStencilMaterialCache(this Mask mask)
        {
            if (mask == null) throw new ArgumentNullException(nameof(mask));

            if (!mask.enabled)
            {
                return;
            }
            
            var maskMaterial = mask.GetMaskMaterial();
            var unmaskMaterial = mask.GetUnmaskMaterial();

            if (maskMaterial != null)
            {
                StencilMaterial.Remove(maskMaterial);
                mask.SetMaskMaterial(null);
            }

            if (unmaskMaterial != null)
            {
                StencilMaterial.Remove(unmaskMaterial);
                mask.SetUnmaskMaterial(null);
            }
        }
    }
}