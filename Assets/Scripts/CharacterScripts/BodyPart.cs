using UnityEngine;

namespace CharacterScripts
{
  public class BodyPart : MonoBehaviour
  {
    [SerializeField] protected BodyPartSettings Settings;
    
    public virtual void TakeDamage()
    {
    }
  }
}