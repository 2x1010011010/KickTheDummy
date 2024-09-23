using UnityEngine;

namespace FluidFlow
{
    public class SimpleProjectDecal : MonoBehaviour
    {
        public Camera Camera;
        public FFCanvas Canvas;
        public FFDecal Decal = new FFDecal(
            FFDecal.Mask.None(),
            FFDecal.Channel.Color(TextureChannelReference.Indirect("Color"), Color.blue));


        void Update()
        {
            if (Input.GetMouseButtonDown(0)) {
                // create a projector from the camera in direction of the mouse cursor
                var ray = Camera.ScreenPointToRay(Input.mousePosition);
                var projector = FFProjector.Orthogonal(ray, Vector3.up, .5f, .5f, .1f, 50);

                // project decal on the canvas with the given projector
                Canvas.ProjectDecal(Decal, projector);
            }
        }
    }
}

