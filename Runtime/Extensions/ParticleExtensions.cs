using UnityEngine;

namespace KOOP.Extensions
{
    public static class ParticleExtensions
    {
		/// <summary>
		/// Instantiates and plays ParticleSystem at given transform's position and rotation, then destroys after lifetime (in seconds)
		/// </summary>
		public static void PlayCopyOnce(this ParticleSystem particlePrefab, float lifetime)
		{
			if (particlePrefab != null)
				PlayCopyOnce(particlePrefab, particlePrefab.transform.position, particlePrefab.transform.rotation, lifetime);
		}

		/// <summary>
		/// Instantiates and plays ParticleSystem at given transform's position and rotation, then destroys after lifetime (in seconds)
		/// </summary>
		public static void PlayCopyOnce(this ParticleSystem particlePrefab, Transform positionRotationReference, float lifetime)
		{
			if (positionRotationReference != null)
				PlayCopyOnce(particlePrefab, positionRotationReference.position, positionRotationReference.rotation, lifetime);
		}

		/// <summary>
		/// Instantiates and plays ParticleSystem at given position, then destroys after lifetime (in seconds)
		/// </summary>
		public static void PlayCopyOnce(this ParticleSystem particlePrefab, Vector3 position, float lifetime)
		{
			PlayCopyOnce(particlePrefab, position, Quaternion.identity, lifetime);
		}

		/// <summary>
		/// Instantiates and plays ParticleSystem at given position and rotation, then destroys after lifetime (in seconds)
		/// </summary>
		/// NOTE: This function's use should remain limited to scenarios where we need to play an effect for disappearing / despawning objects
		/// Could be optimized by adding automatic pooling of referenced ParticleSystems
		public static void PlayCopyOnce(this ParticleSystem particlePrefab, Vector3 position, Quaternion rotation, float lifetime)
		{
			if (particlePrefab != null)
			{
				var particles = MonoBehaviour.Instantiate(particlePrefab, position, rotation);
				particles.Play();
				MonoBehaviour.Destroy(particles, lifetime);
			}
		}
    }
}