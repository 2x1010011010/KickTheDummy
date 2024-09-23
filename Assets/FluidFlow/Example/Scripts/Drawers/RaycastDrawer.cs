using UnityEngine;

namespace FluidFlow
{
    public class RaycastDrawer : MonoBehaviour
    {
        public FFCanvas Target;
        public Camera MainCamera;

        public float DrawDepth = .7f;
        public float DrawRadius = .05f;

        private void Update()
        {
            if (Input.GetMouseButtonDown(0)) {
                var ray = MainCamera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit rayHit, 100)) {
                    Target.DrawCapsule("_FluidTex", FFBrush.SolidColor(Color.red), rayHit.point, rayHit.point + ray.direction * DrawDepth, DrawRadius);
                }
            }
        }
    }
}