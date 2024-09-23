using UnityEngine;

namespace PaintCore
{
	/// <summary>This struct can be used to reference a <b>Texture</b> by instance or hash for de/serialization.
	/// NOTE: For the de/serialization to work you must call the <b>CwSerialization.TryRegister/TryUnregister</b> methods on your textures.</summary>
	[System.Serializable]
	public struct CwHashedTexture
	{
		[System.NonSerialized]
		private Texture instance;

		[SerializeField]
		private CwHash hash;

		public static implicit operator CwHashedTexture(Texture newInstance)
		{
			CwHashedTexture hashed;

			hashed.instance = newInstance;

			if (newInstance != null)
			{
				CwSerialization.TextureToHash.TryGetValue(newInstance, out hashed.hash);
			}
			else
			{
				hashed.hash = 0;
			}

			return hashed;
		}

		public static implicit operator Texture(CwHashedTexture hashed)
		{
			Texture texture;

			hashed.TryGetInstance(out texture);

			return texture;
		}

		public bool TryGetInstance(out Texture texture)
		{
			if (instance != null)
			{
				texture = instance;

				return true;
			}

			return CwSerialization.HashToTexture.TryGetValue(hash, out texture);
		}
	}
}