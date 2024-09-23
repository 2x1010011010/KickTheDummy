using UnityEngine;

namespace FluidFlow
{
    public class DecalProjector : MonoBehaviour
    {
        [Header("Projection Settings")]
        [Min(0.0f)]
        public float ProjectionSize = .4f;

        public float ProjectionNear = .04f;
        public float ProjectionFar = .8f;

        public bool SurfaceAngleBasedFade = true;
        public bool PaintBackfacingSurface = false;

        [Header("Visualize")]
        public bool EnableVisualization = true;

        public void Draw(FFCanvas canvas, FFDecal decal)
        {
            canvas.ProjectDecal(decal, FFProjector.Orthogonal(transform, ProjectionSize, ProjectionSize, ProjectionNear, ProjectionFar), SurfaceAngleBasedFade, PaintBackfacingSurface);
        }

        private void Update()
        {
            if (EnableVisualization) {
                var depth = ProjectionFar - ProjectionNear;
                var mat = transform.localToWorldMatrix * Matrix4x4.TRS(Vector3.forward * (depth * .5f + ProjectionNear), Quaternion.identity, new Vector3(ProjectionSize, ProjectionSize, depth));
                DrawerVisualizer.Draw(DrawerVisualizer.DrawerType.CUBE, mat, Color.white);
            }
        }

        private void OnDrawGizmos()
        {
            var inv = FFProjector.Orthogonal(transform, ProjectionSize, ProjectionSize, ProjectionNear, ProjectionFar).ViewProjection.inverse;
            var aaa = inv.MultiplyPoint(new Vector3(1, 1, 1));
            var aab = inv.MultiplyPoint(new Vector3(1, 1, -1));
            var aba = inv.MultiplyPoint(new Vector3(1, -1, 1));
            var abb = inv.MultiplyPoint(new Vector3(1, -1, -1));
            var baa = inv.MultiplyPoint(new Vector3(-1, 1, 1));
            var bab = inv.MultiplyPoint(new Vector3(-1, 1, -1));
            var bba = inv.MultiplyPoint(new Vector3(-1, -1, 1));
            var bbb = inv.MultiplyPoint(new Vector3(-1, -1, -1));

            Gizmos.DrawLine(aaa, aba);
            Gizmos.DrawLine(aba, bba);
            Gizmos.DrawLine(bba, baa);
            Gizmos.DrawLine(baa, aaa);
            Gizmos.DrawLine(aab, abb);
            Gizmos.DrawLine(abb, bbb);
            Gizmos.DrawLine(bbb, bab);
            Gizmos.DrawLine(bab, aab);
            Gizmos.DrawLine(aaa, aab);
            Gizmos.DrawLine(aba, abb);
            Gizmos.DrawLine(baa, bab);
            Gizmos.DrawLine(bba, bbb);
        }
    }
}