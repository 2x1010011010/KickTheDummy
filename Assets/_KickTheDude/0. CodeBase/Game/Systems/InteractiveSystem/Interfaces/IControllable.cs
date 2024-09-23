using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IControllable
{
    Transform Root { get; set; }

    void Move(Vector3 direction);
    void Jump();
    void Hit();
    void Shot();
    void Show();
    void Hide();
    void Aim();
    void Relax();
    void Crouch();
    void Stand();
    void Grab();
    void Throw();
    void Dash(Vector3 direction);
}
