using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDetector<T> : IActivatiable
{
    event Action<T> NearestObjectChanged;
    
    T NearestObject { get; }
}
