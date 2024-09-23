using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public abstract class UIScenesBootstraper : MonoBehaviour
{
    private void OnEnable()
    {
        Bootstrap();
    }

    public abstract void Bootstrap();
}
