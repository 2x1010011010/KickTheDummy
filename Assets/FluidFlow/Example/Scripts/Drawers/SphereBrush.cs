using UnityEngine;

namespace FluidFlow
{
    public class SphereBrush : MonoBehaviour
    {
        [Header("Sphere Settings")]
        [Min(0)]
        public float Radius = .2f;

        [Header("Visualize")]
        public bool EnableVisualization = true;

        public void Draw(FFCanvas canvas, TextureChannel channel, FFBrush brush)
        {
            canvas.DrawSphere(channel, brush, transform.position, Radius);
        }

        private void Update()
        {
            if (EnableVisualization) {
                var mat = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one) * Matrix4x4.Scale(Vector3.one * Radius * 2);
                DrawerVisualizer.Draw(DrawerVisualizer.DrawerType.SPHERE, mat, Color.white);
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(transform.position, Radius);
        }
    }
}