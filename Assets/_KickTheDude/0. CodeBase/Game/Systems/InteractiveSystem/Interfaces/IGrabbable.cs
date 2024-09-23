using System;
using UnityEngine;

namespace Game.InteractiveSystem
{
    public struct GrabData
    {
        public readonly IGrabbable Grabbable;
        public readonly IGrabber Grabber;

        public GrabData(IGrabbable grabbable, IGrabber grabber)
        {
            Grabbable = grabbable;
            Grabber = grabber;
        }
    }

    public interface IGrabbable
    {
        event Action<GrabData> Grabbed;
        event Action<GrabData> Ungrabbed;

        Rigidbody GrabbableRigidbody { get; }

        Transform RightHandHandle { get; }
        Transform LeftHandHandle { get; }

        Transform PhysicalRoot { get; }
        Transform VisualRoot { get; }

        GrabbableParameters GrabbableParameters { get; }

        IGrabber CurentGrabber { get; }
        bool IsGrabbed { get; }

        void Grab(IGrabber grabber);
        void Ungrab();
    }
}
