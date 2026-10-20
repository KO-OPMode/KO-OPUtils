using UnityEngine;

namespace KOOP.Extensions
{
	public static class GameObjectExtensions
	{
		/// <summary>
		/// Unity's default GetComponentInParent() will return any matching components found on the initial target behaviour itself.
		/// This version will only return components found on objects further up the hierarchy than the initial target behaviour.
		/// </summary>
		public static T GetComponentInParentIgnoringSelf<T>(this Component behaviour, bool includeInactive = false)
		{
			if (behaviour.transform.parent == null)
			{
				return default;
			}

			return behaviour.transform.parent.GetComponentInParent<T>(includeInactive);
		}

		/// <summary>
		/// Unity's default GetComponentInParent() will return any matching components found on the initial target behaviour itself.
		/// This version will only return components found on objects further up the hierarchy than the initial target behaviour.
		/// </summary>
		public static bool TryGetComponentInParentIgnoringSelf<T>(this Component behaviour, out T component,
			bool includeInactive = false)
		{
			component = GetComponentInParentIgnoringSelf<T>(behaviour, includeInactive);
			return component != null;
		}

		public static bool TryGetComponentInParent<T>(this Component behaviour, out T component, bool includeInactive = false)
		{
			component = behaviour.GetComponentInParent<T>(includeInactive);
			return component != null;
		}

		public static bool TryGetComponentInChildren<T>(this Component behaviour, out T component, bool includeInactive = false)
		{
			component = behaviour.GetComponentInChildren<T>(includeInactive);
			return component != null;
		}

		public static T GetOrAddComponent<T>(this Component behaviour) where T : Component
		{
			return GetOrAddComponent<T>(behaviour.gameObject, out _);
		}

		public static T GetOrAddComponent<T>(this Component behaviour, out bool added) where T : Component
		{
			return GetOrAddComponent<T>(behaviour.gameObject, out added);
		}

		public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
		{
			return GetOrAddComponent<T>(gameObject, out _);
		}

		public static T GetOrAddComponent<T>(this GameObject gameObject, out bool added) where T : Component
		{
			if (gameObject.TryGetComponent(out T existingComponent))
			{
				added = false;
				return existingComponent;
			}

			added = true;
			return gameObject.AddComponent<T>();
		}

		/// <summary>
		/// Sets layers recursively on the object and all of its children.
		/// </summary>
		public static void SetLayerIncludingAllChildren(this GameObject gameObject, int layer)
		{
			gameObject.layer = layer;
			foreach (Transform child in gameObject.transform)
			{
				SetLayerIncludingAllChildren(child.gameObject, layer);
			}
		}

		/// <summary>
		/// Replaces layer recursively on the object and all of its children ONLY if it was set to the previous layer
		/// </summary>
		public static void ReplaceLayerIncludingAllChildren(this GameObject gameObject, int prevLayer, int newLayer)
		{
			if (gameObject.layer == prevLayer)
				gameObject.layer = newLayer;

			foreach (Transform child in gameObject.transform)
			{
				ReplaceLayerIncludingAllChildren(child.gameObject, prevLayer, newLayer);
			}
		}

		/// <summary>
		/// Find first object of type T in the same scene as gameObject.
		/// </summary>
		public static T FindObjectInScene<T>(this GameObject gameObject, FindObjectsSortMode objectsSortMode = FindObjectsSortMode.InstanceID) where T : Component
		{
			foreach (var t in GameObject.FindObjectsByType<T>(objectsSortMode))
			{
				if (t.gameObject.scene == gameObject.scene)
				{
					return t;
				}
			}

			return null;
		}

		/// <summary>
		/// Returns whether this object has been marked as DontDestroyOnLoad.
		/// </summary>
		public static bool IsDontDestroyOnLoad(this GameObject gameObject)
		{
			return gameObject.scene.name == "DontDestroyOnLoad";
		}

		/// <summary>
		/// Get the full hierarchy path of a given transform object in a Unity scene.
		/// </summary>
		/// <param name="transform">the object to examine</param>
		/// <param name="includeScene">whether to include the root scene name in the output</param>
		/// <returns>A '/'-separated string, showing the path from a scene's root object, to <paramref name="transform"/>'s location.</returns>
		public static string GetHierarchyPath(this Transform transform, bool includeScene = false)
		{
			if (ReferenceEquals(transform, null))
			{
				return "(null)";
			}
			else if (transform.IsDestroyed())
			{
				return "(Destroyed)";
			}

			var path = transform.name;

			while (transform.parent != null)
			{
				transform = transform.parent;
				path = transform.name + "/" + path;
			}

			if (includeScene)
			{
				path = transform.gameObject.scene.name + "/" + path;
			}

			return path;
		}

		/// <summary>
		/// Calculates the total world bounds of a game object hierarchy by encompassing its individual component bounds.
		/// </summary>
		public static Bounds GetWorldBounds(this GameObject gameObject,
											bool includeRenderers = true,
											bool includeColliders = false,
											bool includeReflectionProbes = false)
		{
			var bounds = new Bounds(gameObject.transform.position, Vector2.zero);
			var foundBounds = false;

			if (includeRenderers)
			{
				foreach (var renderer in gameObject.GetComponentsInChildren<Renderer>())
				{
					var rendererBounds = renderer.bounds;

					if (!foundBounds)
					{
						bounds = rendererBounds;
						foundBounds = true;
					}
					else
					{
						bounds.Encapsulate(rendererBounds);
					}
				}
			}

			if (includeColliders)
			{
				foreach (var collider in gameObject.GetComponentsInChildren<Collider>())
				{
					var colliderBounds = collider.bounds;

					if (!foundBounds)
					{
						bounds = colliderBounds;
						foundBounds = true;
					}
					else
					{
						bounds.Encapsulate(colliderBounds);
					}
				}
			}

			if (includeReflectionProbes)
			{
				foreach (var reflectionProbe in gameObject.GetComponentsInChildren<ReflectionProbe>())
				{
					var reflectionProbeBounds = reflectionProbe.bounds;

					if (!foundBounds)
					{
						bounds = reflectionProbeBounds;
						foundBounds = true;
					}
					else
					{
						bounds.Encapsulate(reflectionProbeBounds);
					}
				}
			}

			return bounds;
		}
	}
}