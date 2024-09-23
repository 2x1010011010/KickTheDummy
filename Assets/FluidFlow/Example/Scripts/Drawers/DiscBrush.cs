using UnityEngine;

namespace FluidFlow
{
    public class DiscBrush : MonoBehaviour
    {
        [Header("Disc Settings")]
        [Min(0)]
        public float Thickness = .04f;

        public float Radius = .2f;

        [Header("Visualize")]
        public bool EnableVisualization = true;

        public void Draw(FFCanvas canvas, TextureChannel channel, FFBrush brush)
        {
            canvas.DrawDisc(channel, brush, transform.position, transform.up, Radius, Thickness);
        }

        private void Update()
        {
            if (EnableVisualization) {
                // scaling the capsule mesh in the y direction, does not 100% respresent the capsule shape, as the half-spheres at the caps are distorted
                var mat = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one) * Matrix4x4.Scale(new Vector3(Radius * 2, Thickness, Radius * 2));
                DrawerVisualizer.Draw(DrawerVisualizer.DrawerType.DISC, mat, Color.white);
            }
        }

        private void OnDrawGizmos()
        {
            DrawWireDisc(transform, Radius, Thickness);
        }

        public static void DrawWireDisc(Transform transform, float radius, float thickness)
        {
#if UNITY_EDITOR
            using (new UnityEditor.Handles.DrawingScope(Gizmos.color, Gizmos.matrix)) {
                var position = transform.position;
                var normal = transform.up;
                var right = transform.right * radius;
                var fwd = transform.forward * radius;
                var offset = normal * thickness;
                UnityEditor.Handles.DrawWireDisc(position + offset, normal, radius);
                UnityEditor.Handles.DrawWireDisc(position - offset, normal, radius);
                // Lines
                UnityEditor.Handles.DrawLine(position - offset - right, position + offset - right);
                UnityEditor.Handles.DrawLine(position - offset + right, position + offset + right);
                UnityEditor.Handles.DrawLine(position - offset - fwd, position + offset - fwd);
                UnityEditor.Handles.DrawLine(position - offset + fwd, position + offset + fwd);
            }
#endif
        }
    }
}