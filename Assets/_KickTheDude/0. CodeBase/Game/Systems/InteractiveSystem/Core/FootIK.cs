using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootIK : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private Transform _ikTarget;
    [SerializeField] private AvatarIKGoal _ikGoal;
    [SerializeField] private float _weight;

    private void OnAnimatorIK(int layerIndex)
    {
        _animator.SetIKPositionWeight(_ikGoal, _weight);
        _animator.SetIKRotationWeight(_ikGoal, _weight);

        _animator.SetIKPosition(_ikGoal, _ikTarget.position);
        _animator.SetIKRotation(_ikGoal, _ikTarget.rotation);
    }
}
