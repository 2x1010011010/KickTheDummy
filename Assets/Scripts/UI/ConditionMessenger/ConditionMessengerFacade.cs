using UI.Enums;

namespace UI.ConditionMessenger
{
  public static class ConditionMessengerFacade
  {
    private static ConditionMessage _message;
    private static ConditionMessengerPlane _outPlane;

    public static void Initialize(ConditionMessengerPlane outPlane)
    {
      _outPlane = outPlane;
    }

    public static void SendMessage(Conditions condition, string message = null)
    {
      _message = new ConditionMessage(condition, message);
      _outPlane.AddMessage(_message.GetMessage());
    }
  }
}