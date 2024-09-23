using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.InteractiveSystem
{
    public class IgnoreCollidersBetweenSelf : MonoBehaviour
    {
        [SerializeField] private CollidersContainer _collidersContainer;

        private void OnEnable()
        {
            _collidersContainer.ChangeColliderIgnoreStatusWithOtherContainer(_collidersContainer, true);
        }
    }
}
