using System;

namespace Game.InteractiveSystem
{
    public interface IGrabber
    {
        event Action<GrabData> Grabbed;
        event Action<GrabData> Ungrabbed;

        IGrabbable CurentGrabbable { get; }
        bool HasGrabbable { get; }
        bool GrabbingBlocked { get; set; }

        void Grab();
        void Grab(IGrabbable grabbable);
        void Ungrab();
    }
}