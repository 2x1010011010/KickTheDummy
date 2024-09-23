using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.WeaponSystem
{
    public class ApplyForceAfterShot : ShotAction
    {
        public override string Name => "APPLY FORCE";

        [SerializeField] private ForceParameters _hitForce;
        [SerializeField] private LayerMask _applyForceMask;

        public override void ReactOnShot(ShotData shotData)
        {
            if (shotData.ShotCollider == null) return;
            if (shotData.ShotCollider.attachedRigidbody == null) return;
            if (((1 << shotData.ShotCollider.gameObject.layer) & _applyForceMask) == 0) return;

            shotData.ShotCollider.attachedRigidbody.AddForce(-shotData.ShotNormal * _hitForce.Force, _hitForce.ForceMode);
        }
    }
}
