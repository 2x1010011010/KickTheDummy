using Game.InteractiveSystem;
using UnityEngine;

namespace Game.WeaponSystem
{
    public class DealDamageAfterShot : ShotAction
    {
        public override string Name => "DEAL DAMAGE";

        [SerializeField] private DamageParameters _damageParameters;

        public override void ReactOnShot(ShotData reactData)
        {
            if(reactData.ShotCollider == null) return;

            if(reactData.ShotCollider.TryGetComponent(out IDamageable damageable))
            {
                damageable.TakeDamage(
                    new Damage(
                        _damageParameters.DamageCount,
                        (reactData.Shootable as IInteractive<IInteractable>).Interactable.Root, 
                        reactData.ShotCollider, 
                        _damageParameters.DamageType, 
                        reactData.ShotPoint, 
                        reactData.ShotNormal
                        )
                    );
            }
        }
    }
}
