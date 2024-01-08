using UnityEngine;

[CreateAssetMenu(fileName = "CameraSettings", menuName = "Camera/Settings", order = 51)]
public class CameraSettings : ScriptableObject
{
  [field: SerializeField] public float MovementSpeed { get; private set; }
  [field: SerializeField] public float RotationSpeed { get; private set; }
  [field: SerializeField] public Vector3 RotationOffset { get; private set; }
  [field: SerializeField] public Vector2 DeltaPosition { get; private set; }
}
