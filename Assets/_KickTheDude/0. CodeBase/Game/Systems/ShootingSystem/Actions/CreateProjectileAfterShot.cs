using Game.InteractiveSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.WeaponSystem
{
    public class CreateProjectileAfterShot : ShotAction
    {
        public override string Name => "CREATE PROJECTILE";

        [SerializeField] private GameObject _projectilePrefab;

        public override void ReactOnShot(ShotData shotData)
        {
            var projectile = Object.Instantiate(_projectilePrefab, shotData.InitialPosition, Quaternion.LookRotation(shotData.Direction)).GetComponent<InteractableObject>();
            projectile.IgnoreOtherInteractable((InteractableObject)(_shootable as IInteractive<IInteractable>).Interactable);
        }
    }
}
