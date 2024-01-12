using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI.ConditionMessenger
{
  public class MessageBlock : MonoBehaviour
  {
    [SerializeField] private TMP_Text _message;

    public void Show (string message, float lifetime) 
    {
      _message.text = message;
      Destroy(gameObject, lifetime);
    }
  }
}