using UnityEngine;
using UnityEngine.Events;

namespace Character
{
  public abstract class BodyPart : MonoBehaviour
  {
    public event UnityAction OnBodyPartClick;  
    
    public abstract void  TakeDamage();
  }
}