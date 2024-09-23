using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleRotator : MonoBehaviour
{
    [SerializeField] private Transform _rotationRoot;
    [SerializeField] private float _fullRotateTime;

    private void OnEnable()
    {
        StartRotate();
    }

    public void StartRotate()
    {
        transform.DOKill();
        transform.DOLocalRotate(new Vector3(0, 360, 0), _fullRotateTime, RotateMode.FastBeyond360).SetRelative(true).SetEase(Ease.Linear).SetLoops(-1);
    }

    public void StopRotate()
    {
        transform.DOKill();
    }
}
