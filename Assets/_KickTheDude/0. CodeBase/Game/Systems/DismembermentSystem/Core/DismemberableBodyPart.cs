using System;
using FluidFlow;
using Sirenix.OdinInspector;
using UnityEngine;

public class DismemberableBodyPart : BodyPart, IDismemberable
{
    public event Action<IDismemberable, DismemberType> Dismembered;

    [SerializeField, BoxGroup("SETUP")] private Transform _limbAnimatorRoot;
    [SerializeField, BoxGroup("SETUP")] private GameObject _fakeGore;
    [SerializeField, BoxGroup("SETUP")] private Transform _fakePhysicalLimb;
    [SerializeField, BoxGroup("SETUP")] private Rigidbody[] _rigidbodies;

    [SerializeField, BoxGroup("PAINT DATA")] private FFCanvas _canvas;
    [SerializeField, BoxGroup("PAINT DATA")] private TextureChannel _textureChannel;
    [SerializeField, BoxGroup("PAINT DATA")] private FFBrush _brush;
    [SerializeField, BoxGroup("PAINT DATA")] private float _radius;

    private bool _dismembered;

    public virtual void Dismember(DismemberType dismemberType)
    {
        if (_dismembered) return;

        _fakePhysicalLimb.gameObject.SetActive(true);
        _fakePhysicalLimb.SetParent(null);
        _limbAnimatorRoot.localScale = Vector3.zero;
        _fakeGore.SetActive(true);
        _canvas.DrawSphere(_textureChannel, _brush, _limbAnimatorRoot.position, _radius, ComponentMask.All);

        foreach (var rb in _rigidbodies)
            rb.detectCollisions = false;

        _dismembered = true;

        Dismembered?.Invoke(this, dismemberType);
        //_simulatorParticle?.ProjectParticles(GetProjection(paintData.Position, paintData.Normal, paintData.Size), new Vector2(.4f, 1f), new Vector2(.5f, 3f), paintData.Color, .6f, paintData.Amount);
    }
}
