using Game.InteractiveSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastSensor : MonoBehaviour
{
    public event Action<IInteractive<IInteractable>> FoundedInteractiveChanged;

    [SerializeField] private float _checkPeriod = 0.1f;
    [SerializeField] private float _checkDistance = 4f;
    [SerializeField] private LayerMask _checkMask = ~0;

    private Coroutine _checking;
    private WaitForSecondsRealtime _waitingForCheck;

    IInteractive<IInteractable> LastCatchedInteractive;

    private void OnEnable()
    {
        _waitingForCheck = new WaitForSecondsRealtime(_checkPeriod);

        StopChecking();
        StartChecking();
    }

    private void OnDisable()
    {
        StopChecking();
    }

    private void StopChecking()
    {
        if(_checking != null) StopCoroutine(_checking);

        LastCatchedInteractive = null;
        FoundedInteractiveChanged?.Invoke(LastCatchedInteractive);
    }

    private void StartChecking()
    {
        _checking = StartCoroutine(Checking());
    }

    private IEnumerator Checking()
    {
        while (true)
        {
            CheckInteractive();
            yield return _waitingForCheck;
        }
    }

    private void CheckInteractive()
    {
        var raycastHit = new RaycastHit();
        var raycastPosition = transform.InverseTransformPoint(Camera.main.transform.position);
        raycastPosition.z = 0;

        if (Physics.Raycast(transform.TransformPoint(raycastPosition), Camera.main.transform.forward, out raycastHit, _checkDistance, _checkMask, QueryTriggerInteraction.Ignore))
        {
            var interactive = raycastHit.collider.GetComponentInParent<IInteractive<IInteractable>>();

            if (LastCatchedInteractive != interactive)
            {
                LastCatchedInteractive = interactive;

                FoundedInteractiveChanged?.Invoke(LastCatchedInteractive);
            }
        }
        else
        {
            if (LastCatchedInteractive == null) return;

            LastCatchedInteractive = null;

            FoundedInteractiveChanged?.Invoke(LastCatchedInteractive);
        }
    }
}
