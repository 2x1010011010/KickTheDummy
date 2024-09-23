using UnityEngine;

namespace PaintCore
{
	/// <summary>This struct can be used to reference a <b>Material</b> by instance or hash for de/serialization.</summary>
	[System.Serializable]
	public struct CwHashedMaterial
	{
		[System.NonSerialized]
		private Material instance;

		[SerializeField]
		private int hash;

		public CwHashedMaterial(Material newInstance, int newHash)
		{
			instance = newInstance;
			hash     = newHash;
		}

		public bool TryGetInstance(out Material model)
		{
			if (instance != null)
			{
				model = instance;

				return true;
			}

			return CwSerialization.HashToMaterial.TryGetValue(hash, out model);
		}
	}
}