using UnityEngine;
using Zenject;

public class GameStartEventReporter : MonoBehaviour
{
    private IAnalyticsService _analyticsService;

    [Inject]
    private void Construct(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    private void Start()
    {
        _analyticsService.SendGameStartEvent();
    }
}
