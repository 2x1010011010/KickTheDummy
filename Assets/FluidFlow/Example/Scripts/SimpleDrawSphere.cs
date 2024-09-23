using UnityEngine;

namespace FluidFlow
{
    public class SimpleDrawSphere : MonoBehaviour
    {
        public FFCanvas Canvas;

        public TextureChannelReference TargetChannel;
        public FFBrushSO Brush;
        public float Radius = .2f;

        private void Update()
        {
            if (Input.GetMouseButtonDown(0)) {
                Canvas.DrawSphere(TargetChannel, Brush, transform.position, Radius);
            }
        }
    }
}