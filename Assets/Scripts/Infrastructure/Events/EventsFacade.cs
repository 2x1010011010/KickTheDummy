namespace Infrastructure.Events
{
  public class EventsFacade
  {
    public GameEvents GameEvents { get; } = new();
    public HudEvents HudEvents { get; } = new();
  }
}