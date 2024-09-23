using UnityEngine;

namespace FluidFlow
{
    public static class DrawerVisualizer
    {
        public enum DrawerType
        {
            SPHERE,
            CAPSULE,
            DISC,
            CUBE
        }

        public static void Draw(DrawerType type, Matrix4x4 transform, Color color, float alpha = .2f)
        {
            var prop = new MaterialPropertyBlock();
            color.a = alpha;
            prop.SetColor(colorPropertyID, color);
            Graphics.DrawMesh(getBrushMesh(type), transform, getCachedMaterial(), 0, null, 0, prop);
        }

        private static Material getCachedMaterial()
        {
            if (!brushMaterial) {
                brushMaterial = new Material(Shader.Find("Hidden/FluidFlow/Example/BrushVisualizer"));
                brushMaterial.hideFlags = HideFlags.HideAndDontSave;
            }

            return brushMaterial;
        }

        private static Mesh getBrushMesh(DrawerType type)
        {
            switch (type) {
                case DrawerType.CAPSULE:
                    return getCachedPrimitiveMesh(ref unityCapsuleMesh, type);

                case DrawerType.DISC:
                    return getCachedPrimitiveMesh(ref unityCylinderMesh, type);

                case DrawerType.SPHERE:
                    return getCachedPrimitiveMesh(ref unitySphereMesh, type);

                case DrawerType.CUBE:
                default:
                    return getCachedPrimitiveMesh(ref unityCubeMesh, type);
            }
        }

        private static Mesh getCachedPrimitiveMesh(ref Mesh mesh, DrawerType type)
        {
            if (!mesh) {
                mesh = Resources.GetBuiltinResource<Mesh>(getPrimitiveMeshPath(type));
                if (!mesh)
                    mesh = new Mesh();
            }
            return mesh;
        }

        private static string getPrimitiveMeshPath(DrawerType type)
        {
            switch (type) {
                case DrawerType.CAPSULE:
                    return "New-Capsule.fbx";

                case DrawerType.DISC:
                    return "New-Cylinder.fbx";

                case DrawerType.SPHERE:
                    return "New-Sphere.fbx";

                case DrawerType.CUBE:
                default:
                    return "Cube.fbx";
            }
        }

        private static Material brushMaterial = null;
        private static int colorPropertyID = Shader.PropertyToID("_Color");
        private static Mesh unityCapsuleMesh = null;
        private static Mesh unityCubeMesh = null;
        private static Mesh unityCylinderMesh = null;
        private static Mesh unitySphereMesh = null;
    }
}