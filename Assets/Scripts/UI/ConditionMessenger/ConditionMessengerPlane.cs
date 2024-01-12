using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace UI.ConditionMessenger
{
  public class ConditionMessengerPlane : MonoBehaviour
  {
    [SerializeField] private RectTransform _messageBlock;
    [SerializeField] private float _lifetime;
    [SerializeField] private int _linesAmount = 10;
    [SerializeField] private float _timeout = 1.5f;
    [SerializeField] private float _shift = 5;
    [SerializeField] private bool _isMoveUp = true;

    private float curTimeout;
    private RectTransform[] tmp;
    private RectTransform clone;

    private List<string> _messages = new List<string>();
    void Awake () 
    {
      _messageBlock.gameObject.SetActive(false);
      _messages = new List<string>();
      tmp = new RectTransform[_linesAmount];
      curTimeout = _timeout;
    }

    public void AddMessage(string message)
    {
      _messages.Add(message);
      ShowMessage(message);
    }

    private void ShowMessage(string message)
    {
      RectTransform block = Instantiate(_messageBlock) as RectTransform;
      block.gameObject.SetActive(true);
      block.SetParent(transform);
      block.anchoredPosition = _messageBlock.anchoredPosition;
      block.GetComponent<MessageBlock>().Show(message, _lifetime);
      
      if(_linesAmount > 1)
      {
        foreach (var t in tmp)
        {
          if (!t) continue;
          Vector3 move;
          move = _isMoveUp ? new Vector3(t.anchoredPosition.x, t.anchoredPosition.y + t.sizeDelta.y + _shift, 0) : new Vector3(t.anchoredPosition.x, t.anchoredPosition.y - t.sizeDelta.y - _shift, 0);
          t.anchoredPosition = move;
        }
        if(tmp[0]) Destroy(tmp[0].gameObject);
        for(int i = 0; i < tmp.Length; i++)
        {
          if(i < tmp.Length-1) tmp[i] = tmp[i+1];
        }
        tmp[_linesAmount-1] = block;
      }
      else
      {
        if(clone) Destroy(clone.gameObject);
        clone = block;
      }
    }

    private void ClearPlane()
    {
      bool active = false;
      foreach(string mess in _messages)
      {
        if(mess != string.Empty) active = true;
      }
      if(!active) _messages = new List<string>();
    }
  }
}