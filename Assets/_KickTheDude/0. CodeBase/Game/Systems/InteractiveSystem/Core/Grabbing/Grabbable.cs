using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.InteractiveSystem
{
    [System.Serializable]
    public class Grabbable : IInteractive<IInteractable>, IGrabbable, IRestoreable
    {
        public string Name => "GRABBABLE";

        public event Action<GrabData> Grabbed;
        public event Action<GrabData> Ungrabbed;

        [OdinSerialize, FoldoutGroup("SETUP")] public Rigidbody GrabbableRigidbody { get; set; }
        [OdinSerialize, FoldoutGroup("SETUP")] public Transform RightHandHandle { get; set; }
        [OdinSerialize, FoldoutGroup("SETUP")] public Transform LeftHandHandle { get; set; }
        [OdinSerialize, FoldoutGroup("SETUP")] public Transform PhysicalRoot { get; set; }
        [OdinSerialize, FoldoutGroup("SETUP")] public Transform VisualRoot { get; set; }
        [OdinSerialize, FoldoutGroup("SETUP")] public GrabbableParameters GrabbableParameters { get; private set; }

        [OdinSerialize, FoldoutGroup("DEBUG"), ReadOnly] public bool IsGrabbed { get => CurentGrabber != null; }
        [OdinSerialize, FoldoutGroup("DEBUG"), ReadOnly] public IGrabber CurentGrabber { get; set; }

        [OdinSerialize, FoldoutGroup("REACTORS"), ListDrawerSettings(DraggableItems = false, Expanded = true, ListElementLabelName = "Name")]
        public List<IInteractorAction<IGrabbable>> Reactors { get; private set; } = new List<IInteractorAction<IGrabbable>>();

        public IInteractable Interactable { get; private set; }

        public void Init(IInteractable interactable)
        {
            Interactable = interactable;

            foreach (var reactor in Reactors)
                reactor.Init(this);
        }

        public void Dispose()
        {
            foreach (var reactor in Reactors)
                reactor.Dispose();
        }

        public void Grab(IGrabber grabber)
        {
            CurentGrabber = grabber;

            Grabbed?.Invoke(new GrabData(this, grabber));
        }

        public void Ungrab()
        {
            Ungrabbed?.Invoke(new GrabData(this, CurentGrabber));

            CurentGrabber = null;
        }

        public void Restore()
        {
            GrabbableRigidbody.isKinematic = false;

            PhysicalRoot.transform.parent = GrabbableRigidbody.transform;
            PhysicalRoot.transform.localPosition = Vector3.zero;
            PhysicalRoot.transform.localEulerAngles = Vector3.zero;

            VisualRoot.transform.parent = GrabbableRigidbody.transform;
            VisualRoot.transform.localPosition = Vector3.zero;
            VisualRoot.transform.localEulerAngles = Vector3.zero;
        }

        public void StopInteract()
        {
            Ungrab();
            Restore();
        }
    }
}
