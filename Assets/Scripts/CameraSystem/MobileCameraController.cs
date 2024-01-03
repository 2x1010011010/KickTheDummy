using CharacterScripts;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CameraSystem
{
  public class MobileCameraController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
  {
    public bool OnPressed { get; private set; } = false;
    public int FingerID { get; private set; }

    public void OnPointerDown(PointerEventData eventData)
    {
      if (eventData.pointerCurrentRaycast.gameObject != gameObject) return;
      OnPressed = true;
      FingerID = eventData.pointerId;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
      if (eventData.pointerId == FingerID)
      {
        OnPressed = false;
      }
    }
  }
}