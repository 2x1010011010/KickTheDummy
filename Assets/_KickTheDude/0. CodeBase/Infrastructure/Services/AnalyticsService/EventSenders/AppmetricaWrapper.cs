using System.Collections.Generic;

public class AppmetricaEventSender : IAnalyticsEventSender
{
    private IYandexAppMetrica _appMetrica;

    public void Initialize()
    {
        _appMetrica = AppMetrica.Instance;
    }

    public void SendEvent(string eventName, Dictionary<string, object> events)
    {
        _appMetrica.ReportEvent(eventName, events);
        _appMetrica.SendEventsBuffer();
    }
}
