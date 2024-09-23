using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DamageParameters", menuName = "Parameters/DamageParameters", order = 1)]
public class DamageParameters : ScriptableObject
{
    [field: SerializeField] public float DamageCount { get; private set; }
    [field: SerializeField] public DamageType DamageType { get; private set; }
}
