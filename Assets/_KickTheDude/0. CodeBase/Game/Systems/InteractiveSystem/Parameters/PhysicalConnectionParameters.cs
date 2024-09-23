using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PhysicalConnectionEntity", menuName = "StaticData/PhysicalConnectionEntity", order = 1)]
public class PhysicalConnectionParameters : Resource
{
    [field: SerializeField] public PhysicalConnection PhysicalConnectionPrefab { get; private set; }
    [field: SerializeField] public ConfigurableJointParameters ConfigurableJointParameters { get; private set; }
    [field: SerializeField] public Material Material { get; private set; }
}
