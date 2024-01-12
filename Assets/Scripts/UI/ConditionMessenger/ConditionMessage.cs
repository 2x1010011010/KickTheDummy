using UI.Enums;

namespace UI.ConditionMessenger
{
  public class ConditionMessage
  {
    private string _messageText;
    private Conditions _condition;

    public ConditionMessage(Conditions condition, string messageText = null)
    {
      _messageText = messageText;
      _condition = condition;
    }

    public string GetMessage() =>
      _condition + " " + _messageText;
  }
}