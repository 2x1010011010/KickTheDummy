using System.Collections.Generic;

public interface IAnalyticsEventSender
{
    void Initialize();
    void SendEvent(string eventName, Dictionary<string, object> events);
}
