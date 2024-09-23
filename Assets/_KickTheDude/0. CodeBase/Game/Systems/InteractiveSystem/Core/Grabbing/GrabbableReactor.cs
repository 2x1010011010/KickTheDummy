using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

namespace Game.InteractiveSystem
{
    public abstract class GrabbableReactor : IInteractorAction<IGrabbable>
    {
        private IGrabbable _grabbable;

        public abstract string Name { get; }

        public void Init(IGrabbable reactSource)
        {
            _grabbable = reactSource;

            _grabbable.Grabbed += ReactOnGrab;
            _grabbable.Ungrabbed += ReactOnUngrab;
        }

        public void Dispose()
        {
            _grabbable.Grabbed -= ReactOnGrab;
            _grabbable.Ungrabbed -= ReactOnUngrab;
        }

        protected abstract void ReactOnGrab(GrabData grabData);
        protected abstract void ReactOnUngrab(GrabData grabData);
    }
}
