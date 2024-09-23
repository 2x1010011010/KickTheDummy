using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.ExplosionSystem
{
    public class DealDamage : ExplodeReactor
    {
        public override string Name => "DEAL DAMAGE";

        [SerializeField] private float _damageDistance;
        [SerializeField] private DamageParameters _damageParameters;

        public override void ReactOnExplode(ExplosionData explosionData)
        {
            foreach (ExplosionContact explosionContact in explosionData.ExplosionContacts)
            {
                if(explosionContact.Collider.TryGetComponent(out IDamageable damageable))
                {
                    if (explosionContact.DistanceToExplosionCenter < _damageDistance)
                        damageable.TakeDamage(new Damage(_damageParameters.DamageCount, null, explosionContact.Collider, _damageParameters.DamageType, explosionContact.ContactPosition, explosionContact.ContactNormal));
                }
            }
        }
    }
}
