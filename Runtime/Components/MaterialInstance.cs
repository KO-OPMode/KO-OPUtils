using System.Collections.Generic;
using System.Linq;
using KOOP.Attributes;
using UnityCollections;
using UnityEngine;

namespace KOOP
{
    /// <summary>
    /// Component that creates a runtime instance of the given material and replaces all usages of it on the specified
    /// renderers. This makes it easy to modify a material and have it affect all of those
    /// renderers at once without writing to a persistent asset.
    /// </summary>
    /// <owner>jonagill</owner>
    public class MaterialInstance : MonoBehaviour
    {
        [SerializeField] private Material _sourceMaterial;
        [Button(
            "Cache Child Renderers", 
            nameof(CacheChildRenderers), 
            tooltip: "Assign all child renderers that use the source material to our list of target renderers",
            drawBelowProperty: true)]
        [SerializeField] private List<Renderer> _targetRenderers;
        
        private Material _materialInstance;
        
        // Caches if we've created our instance so we don't have to do expensive null checks constantly
        private bool _hasValidInstance;
        
        private bool _isDestroying = false;

        public Material Source => _sourceMaterial;
        
        public Material Instance
        {
            get
            {
                if (TryEnsureMaterialInstance())
                {
                    return _materialInstance;
                }

                return null;
            }
        }

        private void Awake()
        {
            AssignMaterialInstance();
        }

        private void OnDestroy()
        {
            _isDestroying = true;
            CleanupMaterialInstance();
        }

        private bool TryEnsureMaterialInstance()
        {
            if (_hasValidInstance) 
                return true; // We have a valid instance
            
            if (_isDestroying)
                return false; // Don't re-allocate when we're being destroyed

            if (!Application.isPlaying)
                return false; // Don't create instances in the editor
            
            AssignMaterialInstance();
            return _hasValidInstance;
        }

        private void AssignMaterialInstance()
        {
            CleanupMaterialInstance();
            
            if (_sourceMaterial == null) 
            {
                Debug.LogWarning($"{this}: No source material applied. Skipping creating a material instance.", this);
                return;
            }
            
            _materialInstance = Instantiate(_sourceMaterial);
            _hasValidInstance = true;
            
            foreach (var target in _targetRenderers)
            {
                if (target != null)
                {
                    // Swap our source material to our new instance
                    SwapMaterials(target, fromMaterial: _sourceMaterial, toMaterial: _materialInstance);
                }
            }
        }

        private void CleanupMaterialInstance()
        {
            if (_materialInstance == null)
            {
                return;
            }

            foreach (var target in _targetRenderers)
            {
                if (target != null)
                {
                    // Restore our source material to our target renderers
                    SwapMaterials(target, fromMaterial: _materialInstance, toMaterial: _sourceMaterial);
                }
            }
            
            DestroyImmediate(_materialInstance);
            _materialInstance = null;
            _hasValidInstance = false;
        }

        private void SwapMaterials(Renderer renderer, Material fromMaterial, Material toMaterial)
        {
            using PooledList<Material> sharedMaterials = new();
            renderer.GetSharedMaterials(sharedMaterials.BackingList);

            bool dirty = false;
            for (int i = 0; i < sharedMaterials.Count; i++)
            {
                if (sharedMaterials[i] == fromMaterial)
                {
                    sharedMaterials[i] = toMaterial;
                    dirty = true;
                }
            }

            if (dirty)
            {
                renderer.SetSharedMaterials(sharedMaterials.BackingList);
            }
        }
        
        private void CacheChildRenderers()
        {
            _targetRenderers.Clear();
            using PooledList<Renderer> childRenderers = new();
            
            GetComponentsInChildren(true, childRenderers.BackingList);
            
            foreach (var childRenderer in childRenderers)
            {
                if (childRenderer.sharedMaterials.Contains(_sourceMaterial))
                {
                    _targetRenderers.Add(childRenderer);
                }
            }
        }
    }
}