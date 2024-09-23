using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct PlayerCurrency
{
    public readonly int SoftCurrency;

    public PlayerCurrency(int softCurrency)
    {
        SoftCurrency = Mathf.Clamp(softCurrency, 0, softCurrency);
    }
}
