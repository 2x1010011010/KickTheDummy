using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.InteractiveSystem;
using System;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System.Linq;

namespace Game.WeaponSystem
{
    public class Firearm : IInteractive<IInteractable>, IShootable, IReloadeable
    {
        public string Name => "FIREARM";

        public event Action<IReloadeable> Reloaded;
        public event Action<ShotData> Shoted;
        public event Action StopShoting;

        [OdinSerialize, FoldoutGroup("SETUP")] private LayerMask _shotLayerMask;
        [OdinSerialize, FoldoutGroup("SETUP")] public Transform ShotPosition { get; private set; }

        [OdinSerialize, FoldoutGroup("SHOT REACTORS"), ListDrawerSettings(DraggableItems = false, Expanded = true, ListElementLabelName = "Name")]
        public List<IInteractorAction<IShootable>> Reactors { get; private set; } = new List<IInteractorAction<IShootable>>();

        public IInteractable Interactable { get; private set; }

        Transform _shotParent;
        Vector3 _shotTransformLocalPos;
        Vector3 _shotTransformLocalRot;

        public void Init(IInteractable interactable)
        {
            Interactable = interactable;

            foreach (var reactor in Reactors)
                reactor.Init(this);

            _shotParent = ShotPosition.parent;
            _shotTransformLocalPos = ShotPosition.localPosition;
            _shotTransformLocalRot = ShotPosition.localEulerAngles;
        }

        public void Dispose()
        {
            foreach (var reactor in Reactors)
                reactor.Dispose();
        }

        [Button(ButtonSizes.Large), FoldoutGroup("ACTIONS")]
        public void Reload()
        {
            Reloaded?.Invoke(this);
        }

        public void Shot(Vector3 position, Vector3 direction)
        {
            RaycastHit raycastHit;
            Physics.SphereCast(position, 0.05f, direction, out raycastHit, 100, _shotLayerMask, QueryTriggerInteraction.Collide);

            Shoted?.Invoke(new ShotData(raycastHit, position, direction, raycastHit.point, raycastHit.normal, raycastHit.collider, this));
        }

        private void GetSpread()
        {
            //Quaternion.AngleAxis(ShotPosition.right)   + FirearmParameters.Spread.x
        }

        [Button(ButtonSizes.Large), FoldoutGroup("ACTIONS")]
        public void Shot()
        {
            Shot(ShotPosition.position, ShotPosition.forward);
        }

        [Button(ButtonSizes.Large), FoldoutGroup("ACTIONS")]
        public void StopInteract()
        {
            StopShooting();
        }

        public void StopShooting()
        {
            StopShoting?.Invoke();
        }

        public void Restore()
        {
            ShotPosition.parent = _shotParent;
            ShotPosition.localPosition = _shotTransformLocalPos;
            ShotPosition.localEulerAngles = _shotTransformLocalRot;
        }
    }
}