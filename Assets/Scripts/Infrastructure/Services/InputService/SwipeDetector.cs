using UnityEngine;

namespace Infrastructure.Services.InputService
{
  public class SwipeDetector : MonoBehaviour
  {
    [SerializeField] private RectTransform _swipeZone;
    public bool IsSwipeActive { get; set; }

    private void Update()
    {
    }
  }
}
