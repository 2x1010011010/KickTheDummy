using System.Collections.Generic;
using UnityEngine;

public class AnalyticsService : IAnalyticsService
{
    private List<IAnalyticsEventSender> _analyticsEventSenders;

    public AnalyticsService(List<IAnalyticsEventSender> analyticsEventSender)
    {
        _analyticsEventSenders = analyticsEventSender;
    }

    public void Initialize()
    {
        foreach (var eventsSender in _analyticsEventSenders)
            eventsSender.Initialize();
    }

    public void SendLevelStartEvent(LevelData levelData)
    {
        var eventName = "location_start";
        var parameters = new Dictionary<string, object>() { { "location_name", levelData.LevelName } };

        SendEvent(eventName, parameters);
    }

    public void SendGameStartEvent()
    {
        var eventName = "game_start";

        SendEvent(eventName, null);
    }

    public void SendToolCategorySelectedEvent(ToolCategory toolCategory)
    {
        
    }

    public void SendToolSelectedEvent(ToolCategory toolCategory, string toolName)
    {
        
    }

    private void SendEvent(string eventName, Dictionary<string, object> fields)
    {
        foreach (var eventSender in _analyticsEventSenders)
            eventSender.SendEvent(eventName, fields);

        Debug.Log($"[ANALYTICS] Event {eventName} sent!");
    }
}
