public interface IAnalyticsService
{
    void Initialize();
    void SendLevelStartEvent(LevelData levelData);
    void SendGameStartEvent();
    void SendToolCategorySelectedEvent(ToolCategory toolCategory);
    void SendToolSelectedEvent(ToolCategory toolCategory, string toolName);
}