using System;
using UI.ConditionMessenger;
using UnityEngine;

namespace Infrastructure
{
  public class SceneBootstrap : MonoBehaviour
  {
    [SerializeField] private ConditionMessengerPlane _messengerPlane;

    private void Awake()
    {
      ConditionMessengerFacade.Initialize(_messengerPlane);
    }
  }
}