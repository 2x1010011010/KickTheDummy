using UnityEngine;

namespace FluidFlow
{
    public class RPMaterialStash : ScriptableObject
    {
        [System.Serializable]
        public struct RPMaterial
        {
            public Material Default;
            public Material URP;
            public Material HDRP;

            public bool Matches(Material mat)
            {
                return Default == mat || URP == mat || HDRP == mat;
            }
        }

        public RPMaterial[] RPMaterials;

        public bool TryFind(Material material, out RPMaterial rpMaterial)
        {
            for (var i = RPMaterials.Length - 1; i >= 0; i--) {
                if (RPMaterials[i].Matches(material)) {
                    rpMaterial = RPMaterials[i];
                    return true;
                }
            }
            rpMaterial = default;
            return false;
        }
    }

}
