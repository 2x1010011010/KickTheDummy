using UnityEngine;

namespace FluidFlow
{
    public class SimpleDrawParticle : MonoBehaviour
    {
        public Camera ViewCamera;
        public FFSimulatorParticle ParticleSimulator;

        public Color ParticleColor = Color.red;

        void Update()
        {
            if (!ParticleSimulator.Initialized)
                return;

            var ray = ViewCamera.ScreenPointToRay(Input.mousePosition);

            // Raycast-based particle painting requires a MeshCollider, with the same mesh as used for simulation/rendering.
            // Therefore, it is only suitable for non-skinned low-complexity meshes
            if (Physics.Raycast(ray, out var rayHit, 10)) {
                // if (ParticleSimulator.GravityMap.Canvas.Surfaces.TryGetSurfaceInfo(rayHit, out var hitSurfaceInfo)) { }

                if (ParticleSimulator.GravityMap.Canvas.TryGetCanvasUV(rayHit, out var canvasUV)) {
                    Debug.Log("FluidFlow: Hit UV in FFCanvas texture atlas: " + canvasUV);
                }
                if (Input.GetMouseButton(1)) {
                    ParticleSimulator.AddParticle(rayHit, 4, 1, ParticleColor);
                }
            }

            // Projection-based particle painting also works for skinned meshes without any collider.
            // In general this is less performant though, as the complete mesh has to be rendered, even when only drawing a single particle.
            if (Input.GetMouseButton(0)) {
                var projector = FFProjector.Orthogonal(ray, Vector3.up, .05f, .05f, .1f, 10);
                Utility.DebugFrustum(projector);
                ParticleSimulator.ProjectParticles(projector, new Vector2(.8f, 1f), new Vector2(1, 4), ParticleColor, .5f, 4);
            }

            if (Input.GetKeyDown(KeyCode.Space)) {
                ParticleSimulator.ClearAllParticles();
                ParticleSimulator.GravityMap.Canvas.InitializeTextureChannels();
            }
        }
    }
}