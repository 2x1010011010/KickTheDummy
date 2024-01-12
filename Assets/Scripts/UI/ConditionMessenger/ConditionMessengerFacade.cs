using UI.ConditionMessager;
using UI.Enums;
using UnityEngine;

namespace UI.ConditionMessenger
{
  public static class ConditionMessengerFacade
  {
    public static ConditionMessage Message { get; private set; }
    private static ConditionMessengerPlane _outPlane;

    public static void Initialize(ConditionMessengerPlane outPlane)
    {
      _outPlane = outPlane;
    }

    public static void SendMessage(Conditions condition, string message = null)
    {
      Message = new ConditionMessage(condition, message);
      _outPlane.AddMessage(BuildMessage(Message));
    }

    private static string BuildMessage(ConditionMessage message)
    {
      return message.Condition.ToString() + " " + message.MessageText;
    }
  }
}