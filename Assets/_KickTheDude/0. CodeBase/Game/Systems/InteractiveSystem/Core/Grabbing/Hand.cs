using System;
using UnityEngine;

namespace Game.InteractiveSystem
{
    public class Hand : MonoBehaviour, IGrabber
    {
        public event Action<IGrabbable, IGrabber> Grabbed;
        public event Action<IGrabbable, IGrabber> Ungrabbed;

        [SerializeField] private CollidersContainer _puppetColliders;
        [SerializeField] private ConfigurableJointParameters _configurableJointParameters;
        [SerializeField] private Transform _physicalRoot;
        [SerializeField] private Transform _visualRoot;

        public Grabbable CurentGrabbable { get; private set; }

        IGrabbable IGrabber.CurentGrabbable => throw new NotImplementedException();

        public bool HasGrabbable => throw new NotImplementedException();

        public bool GrabbingBlocked => throw new NotImplementedException();

        bool IGrabber.GrabbingBlocked { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        event Action<GrabData> IGrabber.Grabbed
        {
            add
            {
                throw new NotImplementedException();
            }

            remove
            {
                throw new NotImplementedException();
            }
        }

        event Action<GrabData> IGrabber.Ungrabbed
        {
            add
            {
                throw new NotImplementedException();
            }

            remove
            {
                throw new NotImplementedException();
            }
        }

        public void Grab(IGrabbable grabbable)
        {
            if(CurentGrabbable != null) Ungrab();

            CurentGrabbable = (Grabbable)grabbable;
            if (CurentGrabbable == null) return;

            var targetGrabbable = CurentGrabbable;
            CurentGrabbable.Grab(this);
            //CurentGrabbable.CollidersContainer.DeactivateAllColliders();
            //if (targetGrabbable.IgnoreWhenGrabbed) _puppetColliders.ChangeColliderIgnoreStatusWithOtherContainer(targetGrabbable.CollidersContainer, true);
            SetGrabbable(targetGrabbable);

            Grabbed?.Invoke(CurentGrabbable, this);
        }

        public void Ungrab()
        {
            if (CurentGrabbable == null) return;

            //CurentGrabbable.Ungrab(this);

            var targetGrabbable = CurentGrabbable;
            var ungrabbedGrabbable = CurentGrabbable;

            var curentGrabbable = CurentGrabbable;
            curentGrabbable.Restore();

            CurentGrabbable = null;

            Ungrabbed?.Invoke(ungrabbedGrabbable, this);
        }

        ConfigurableJoint joint;
        private void SetGrabbable(Grabbable grabbable)
        {
            /*
            grabbable.Rigidbody.isKinematic = true;
            grabbable.transform.position = _physicalRoot.position;
            grabbable.transform.rotation = _physicalRoot.rotation * Quaternion.Inverse(grabbable.RightHandHandle.localRotation);
            grabbable.transform.position = _physicalRoot.position + (grabbable.RightHandHandle.position - _physicalRoot.position) * -1;
            grabbable.transform.parent = _physicalRoot;

            grabbable.VisualRoot.transform.parent = _visualRoot;
            grabbable.VisualRoot.transform.localPosition = _physicalRoot.InverseTransformPoint(grabbable.transform.position);
            grabbable.VisualRoot.transform.localRotation = grabbable.transform.localRotation;
            */
        }

        public void Grab()
        {
            throw new NotImplementedException();
        }
    }
}
