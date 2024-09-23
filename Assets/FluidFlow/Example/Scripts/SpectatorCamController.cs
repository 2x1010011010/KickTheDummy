using UnityEngine;


namespace FluidFlow
{
    public class SpectatorCamController : MonoBehaviour
    {
        public Transform Target;

        [Header("Movement")]
        public float RotationSpeed = 3;

        public float ZoomSpeed = 2;
        public float Dampen = 10f;

        [Header("Current")]
        public float Distance = 1;

        public Vector2 MinMaxDistance = new Vector2(.1f, 5);

        public Vector2 Rotation = new Vector2(275f, 45f);

        private enum MouseEvent
        {
            NONE,
            LEFT,
            RIGHT
        }

        private MouseEvent currentMouseEvent = MouseEvent.NONE;

        private Vector2 deltaRot;
        private float deltaZoom;

        private void Update()
        {
            if (currentMouseEvent != MouseEvent.LEFT)
                deltaRot = Vector2.Lerp(deltaRot, Vector2.zero, Dampen * Time.deltaTime);

            if (currentMouseEvent != MouseEvent.RIGHT)
                deltaZoom = Mathf.Lerp(deltaZoom, 0, Dampen * Time.deltaTime);

            if (currentMouseEvent == MouseEvent.NONE) {
                if (Input.GetKeyDown(KeyCode.LeftControl))
                    currentMouseEvent = MouseEvent.LEFT;
                else if (Input.GetMouseButtonDown(1))
                    currentMouseEvent = MouseEvent.RIGHT;
            } else {
                if (currentMouseEvent == MouseEvent.LEFT)
                    deltaRot = new Vector2(Input.GetAxis("Mouse X"), -Input.GetAxis("Mouse Y"));
                else
                    deltaZoom = -Input.GetAxis("Mouse Y");

                if (Input.GetKeyUp(KeyCode.LeftControl) || Input.GetMouseButtonUp(1))
                    currentMouseEvent = MouseEvent.NONE;
            }
            Rotation.y = Mathf.Clamp(Rotation.y + deltaRot.y * RotationSpeed, -85f, 85f);
            Rotation.x = (Rotation.x + deltaRot.x * RotationSpeed) % 360f;
            Distance = Mathf.Clamp(Distance + deltaZoom * ZoomSpeed, MinMaxDistance.x, MinMaxDistance.y);

            transform.position = Target.position + Quaternion.Euler(0, Rotation.x, 0) * (Quaternion.Euler(0, 0, Rotation.y) * Vector3.right * Distance);
            transform.LookAt(Target);
        }
    }
}