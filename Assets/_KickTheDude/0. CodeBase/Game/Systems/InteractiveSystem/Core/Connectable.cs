using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Game.InteractiveSystem
{
    public class PhysicalLink : IDestroyable
    {
        private Rigidbody _firstRigidbody;
        private Rigidbody _secondRigidbody;
        private ConfigurableJoint _joint;

        public PhysicalLink(Rigidbody firstRigidbody, Rigidbody secondRigidbody, ConfigurableJoint joint)
        {
            _firstRigidbody = firstRigidbody;
            _secondRigidbody = secondRigidbody;
            _joint = joint;
        }

        public void DestroySelf()
        {
            UnityEngine.Object.Destroy(_joint);
        }
    }

    public class Connectable : MonoBehaviour, IDisposable
    {
        [Header("SETUP")]
        [SerializeField] private Rigidbody _selfRigidbody;
        [SerializeField] private MeshRenderersContainer _attachedMeshRenderersContainer;

        [SerializeField] List<PhysicalLink> _physicalLinks = new List<PhysicalLink>();

        public MeshRenderersContainer AttachedMeshRenderersContainer => _attachedMeshRenderersContainer;
        public Rigidbody SelfRigidbody => _selfRigidbody;
        public IEnumerable<PhysicalLink> PhysicalLinks => _physicalLinks;

        private void OnValidate()
        {
            if (_selfRigidbody == null) _selfRigidbody = GetComponent<Rigidbody>();
        }

        public void CreatePhysicaConnection(Connectable connectable, ConfigurableJointParameters connectParameters)
        {
            var joint = _selfRigidbody.gameObject.AddComponent<ConfigurableJoint>();
            joint.connectedBody = connectable.SelfRigidbody;

            connectParameters.ApplyParametersToJoint(joint);

            _physicalLinks.Add(new PhysicalLink(_selfRigidbody, connectable.SelfRigidbody, joint));
        }

        public void Dispose()
        {
            foreach (var physicalLink in _physicalLinks)
                physicalLink.DestroySelf();

            _physicalLinks.Clear();
        }

        private void OnDestroy()
        {
            Dispose();
        }
    }
}
