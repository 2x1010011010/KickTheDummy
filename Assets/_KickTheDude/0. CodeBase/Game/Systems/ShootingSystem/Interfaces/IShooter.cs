using System;
using UnityEngine;

namespace Game.WeaponSystem
{
    public interface IShooter
    {
        event Action<IShooter, IShootable> Shoted;

        IShootable CurentShootable { get; }
        bool ShotFromPlayer { get; set; }

        void Shot();
        void Shot(Vector3 point);
        void StopShooting();
    }
}
