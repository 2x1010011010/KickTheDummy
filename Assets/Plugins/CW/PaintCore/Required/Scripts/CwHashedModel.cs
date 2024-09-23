using UnityEngine;

namespace PaintCore
{
	/// <summary>This struct can be used to reference a <b>Material</b> by instance or hash for de/serialization.
	/// NOTE: To support networking you must modify the <b>CwSerialization.TryRegister(CwModel)</b> method to register the model using a hash/id specific to your networking solution.</summary>
	[System.Serializable]
	public struct CwHashedModel
	{
		[System.NonSerialized]
		private CwModel instance;

		[SerializeField]
		private CwHash hash;

		public static implicit operator CwHashedModel(CwModel newInstance)
		{
			CwHashedModel hashed;

			hashed.instance = newInstance;

			CwSerialization.ModelToHash.TryGetValue(newInstance, out hashed.hash);

			return hashed;
		}

		public bool TryGetInstance(out CwModel model)
		{
			if (instance != null)
			{
				model = instance;

				return true;
			}

			if (CwSerialization.HashToModel.TryGetValue(hash, out model) == true && model != null)
			{
				instance = model;

				return true;
			}

			return false;
		}
	}
}