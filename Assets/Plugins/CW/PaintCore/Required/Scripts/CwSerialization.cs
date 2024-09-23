using UnityEngine;
using System.Collections.Generic;

namespace PaintCore
{
	/// <summary>This class handles the low level de/serialization of different paint objects to allow for things like networking.</summary>
	public static class CwSerialization
	{
		/// <summary>This stores an association between a <b>Material</b> hash code and the <b>Material</b> instance, so it can be de/serialized.</summary>
		public static Dictionary<int, Material> HashToMaterial = new Dictionary<int, Material>();

		/// <summary>This stores an association between a <b>Material</b> instance and the <b>Material</b> hash code, so it can be de/serialized.</summary>
		public static Dictionary<Material, int> MaterialToHash = new Dictionary<Material, int>();

		/// <summary>This stores an association between a <b>CwModel</b> hash code and the <b>CwModel</b> instance, so it can be de/serialized.</summary>
		public static Dictionary<CwHash, CwModel> HashToModel = new Dictionary<CwHash, CwModel>();

		/// <summary>This stores an association between a <b>CwModel</b> instance and the <b>CwModel</b> hash code, so it can be de/serialized.</summary>
		public static Dictionary<CwModel, CwHash> ModelToHash = new Dictionary<CwModel, CwHash>();

		/// <summary>This stores an association between a <b>Texture</b> hash code and the <b>Texture</b> instance, so it can be de/serialized.</summary>
		public static Dictionary<CwHash, Texture> HashToTexture = new Dictionary<CwHash, Texture>();

		/// <summary>This stores an association between a <b>Texture</b> instance and the <b>Texture</b> hash code, so it can be de/serialized.</summary>
		public static Dictionary<Texture, CwHash> TextureToHash = new Dictionary<Texture, CwHash>();

		/// <summary>This stores an association between a <b>CwModel</b> hash code and the <b>CwModel</b> instance, so it can be de/serialized.</summary>
		public static Dictionary<CwHash, CwPaintableTexture> HashToPaintableTexture = new Dictionary<CwHash, CwPaintableTexture>();

		/// <summary>This stores an association between a <b>CwModel</b> instance and the <b>CwModel</b> hash code, so it can be de/serialized.</summary>
		public static Dictionary<CwPaintableTexture, CwHash> PaintableTextureToHash = new Dictionary<CwPaintableTexture, CwHash>();

		public static void TryRegister(CwPaintableTexture paintableTexture, CwHash hash)
		{
			TryRegister(paintableTexture, hash, HashToPaintableTexture, PaintableTextureToHash);
		}

		public static void TryRegister(CwModel model, CwHash hash)
		{
			TryRegister(model, model.Hash, HashToModel, ModelToHash);
		}

		public static void TryRegister(Texture texture, CwHash hash)
		{
			TryRegister(texture, hash, HashToTexture, TextureToHash);
		}

		public static void TryRegister<T>(T obj, CwHash hash, Dictionary<CwHash, T> hashToObj, Dictionary<T, CwHash> objToHash)
			where T : Object
		{
			CwHash existingHash;

			if (objToHash.TryGetValue(obj, out existingHash) == true)
			{
				// Already up to date
				if (existingHash == hash)
				{
					return;
				}

				// Remove old hash
				objToHash.Remove(obj);
				hashToObj.Remove(existingHash);
			}

			// Register new
			if (hash != default(CwHash))
			{
				objToHash.Add(obj, hash);
				hashToObj.Add(hash, obj);
			}
		}

		public static int TryRegister(Material material)
		{
			var hash = GetStableStringHash(material.name);

			if (HashToMaterial.ContainsKey(hash) == true)
			{
				throw new System.Exception("You're trying to register the " + material + " Material, but you've already registered the " + HashToMaterial[hash] + " Material with the same hash.");
			}

			MaterialToHash.Add(material, hash);
			HashToMaterial.Add(hash, material);

			return hash;
		}

		private static int GetStableStringHash(string s)
		{
			var hash = 23;

			foreach (var c in s)
			{
				hash = hash * 31 + c;
			}

			return hash;
		}
	}
}