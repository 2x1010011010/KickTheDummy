using Game.ResourceSystem;
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "SalableEntity", menuName = "StaticData/SalableEntity", order = 1)]
public class SalableEntity : Resource
{
    public event Action StateChanged;

    [field: SerializeField] public PropEntity SalableResourceEntity { get; private set; }
    [field: SerializeField] public int Price { get; private set; }
    [SerializeField] public int CurentState { get { return curentState; } set { curentState = value; StateChanged?.Invoke();} }
    [SerializeField] public bool Purchased { get { return CurentState >= Price; } }
    [field: SerializeField] public Sprite PromoImage { get; private set; }

    [SerializeField, ReadyOnly] private int curentState;
}
