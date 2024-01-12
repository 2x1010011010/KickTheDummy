using UI.Enums;

namespace UI.ConditionMessager
{
  public class ConditionMessage
  {
    public string MessageText { get; private set; }
    public Conditions Condition { get; private set; }

    public ConditionMessage(Conditions condition, string messageText = null)
    {
      MessageText = messageText;
      Condition = condition;
    }
  }
}