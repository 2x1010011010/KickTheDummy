using UnityEngine;

namespace FluidFlow
{
    public class CapsuleBrush : MonoBehaviour
    {
        [Header("Capsule Settings")]
        [Min(0)]
        public float Height = .5f;

        public float Radius = .2f;

        [Header("Visualize")]
        public bool EnableVisualization = true;

        public void Draw(FFCanvas canvas, TextureChannel channel, FFBrush brush)
        {
            var offset = transform.up * Height * .5f;
            canvas.DrawCapsule(channel, brush, transform.position - offset, transform.position + offset, Radius);
        }

        private void Update()
        {
            if (EnableVisualization) {
                // scaling the capsule mesh in the y direction, does not 100% respresent the capsule shape, as the half-spheres at the caps are distorted
                var mat = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one) * Matrix4x4.Scale(new Vector3(Radius * 2, Height * .5f + Radius, Radius * 2));
                DrawerVisualizer.Draw(DrawerVisualizer.DrawerType.CAPSULE, mat, Color.white);
            }
        }

        private void OnDrawGizmos()
        {
            var offset = transform.up * Height * .5f;
            DrawWireCapsule(transform.position - offset, transform.position + offset, Radius);
        }

        /// <summary>
        /// Thanks to Qriva
        /// https://forum.unity.com/threads/drawing-capsule-gizmo.354634/
        /// </summary>
        public static void DrawWireCapsule(Vector3 p1, Vector3 p2, float radius)
        {
#if UNITY_EDITOR
            // Special case when both points are in the same position
            if (p1 == p2) {
                Gizmos.DrawWireSphere(p1, radius);
                return;
            }
            using (new UnityEditor.Handles.DrawingScope(Gizmos.color, Gizmos.matrix)) {
                Quaternion p1Rotation = Quaternion.LookRotation(p1 - p2);
                Quaternion p2Rotation = Quaternion.LookRotation(p2 - p1);
                // Check if capsule direction is collinear to Vector.up
                float c = Vector3.Dot((p1 - p2).normalized, Vector3.up);
                if (c == 1f || c == -1f) {
                    // Fix rotation
                    p2Rotation = Quaternion.Euler(p2Rotation.eulerAngles.x, p2Rotation.eulerAngles.y + 180f, p2Rotation.eulerAngles.z);
                }
                // First side
                UnityEditor.Handles.DrawWireArc(p1, p1Rotation * Vector3.left, p1Rotation * Vector3.down, 180f, radius);
                UnityEditor.Handles.DrawWireArc(p1, p1Rotation * Vector3.up, p1Rotation * Vector3.left, 180f, radius);
                UnityEditor.Handles.DrawWireDisc(p1, (p2 - p1).normalized, radius);
                // Second side
                UnityEditor.Handles.DrawWireArc(p2, p2Rotation * Vector3.left, p2Rotation * Vector3.down, 180f, radius);
                UnityEditor.Handles.DrawWireArc(p2, p2Rotation * Vector3.up, p2Rotation * Vector3.left, 180f, radius);
                UnityEditor.Handles.DrawWireDisc(p2, (p1 - p2).normalized, radius);
                // Lines
                UnityEditor.Handles.DrawLine(p1 + p1Rotation * Vector3.down * radius, p2 + p2Rotation * Vector3.down * radius);
                UnityEditor.Handles.DrawLine(p1 + p1Rotation * Vector3.left * radius, p2 + p2Rotation * Vector3.right * radius);
                UnityEditor.Handles.DrawLine(p1 + p1Rotation * Vector3.up * radius, p2 + p2Rotation * Vector3.up * radius);
                UnityEditor.Handles.DrawLine(p1 + p1Rotation * Vector3.right * radius, p2 + p2Rotation * Vector3.left * radius);
            }
#endif
        }
    }
}