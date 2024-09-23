using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

public class ParticleCollisionDrawer : MonoBehaviour
{
    [SerializeField, BoxGroup("SETUP")] private ParticleSystem _particleSystem;

    [SerializeField, BoxGroup("PARAMETERS")] private LayerMask _particlesMask;
    [SerializeField, Range(0, 5f), BoxGroup("PARAMETERS")] private float _size = 0.1f;
    [SerializeField, Range(0, 50), BoxGroup("PARAMETERS")] private int _amount = 2;
    [SerializeField, BoxGroup("PARAMETERS")] private Color _color = Color.red;

    [SerializeField, BoxGroup("TEXTURE PARAMETERS")] private bool _drawTexture;
    [SerializeField, BoxGroup("TEXTURE PARAMETERS")] private LayerMask _mask;
    [SerializeField, Range(0, 5f), BoxGroup("TEXTURE PARAMETERS")] private float _textureSize = 0.1f;
    [SerializeField, BoxGroup("TEXTURE PARAMETERS")] private Texture _texture;

    private List<ParticleCollisionEvent> _collisionEvents = new List<ParticleCollisionEvent>();

    private void OnValidate()
    {
        if (_particleSystem == null) _particleSystem = GetComponent<ParticleSystem>();
    }

    private void OnParticleCollision(GameObject other)
    {
        var numCollisionEvents = _particleSystem.GetCollisionEvents(other, _collisionEvents);

        int i = 0;

        while (i < numCollisionEvents)
        {
            if (_collisionEvents[i].colliderComponent.TryGetComponent(out IPaintable paintable))
            {
                if (_drawTexture && IsCollisionOnAllowedLayer(_collisionEvents[i].colliderComponent.gameObject, _mask))
                {
                    paintable.Paint(new TexturePaintData(_collisionEvents[i].intersection, _collisionEvents[i].normal, _texture, _textureSize, false));
                }

                if (IsCollisionOnAllowedLayer(_collisionEvents[i].colliderComponent.gameObject, _particlesMask))
                    paintable.Paint(new ParticlesPaintData(_collisionEvents[i].intersection, _collisionEvents[i].normal, _color, _size, _amount));

                i++;
                continue;
            }

            if (_collisionEvents[i].colliderComponent.transform.parent.GetComponent<Rigidbody>() != null)
                if (_collisionEvents[i].colliderComponent.TryGetComponent(out IPaintable paintableRb))
                    paintableRb.Paint(new ParticlesPaintData(_collisionEvents[i].intersection, _collisionEvents[i].normal, _color, _size, _amount));

            i++;
        }
    }

    private bool IsCollisionOnAllowedLayer(GameObject gameObj, LayerMask layer)
        => ((1 << gameObj.layer) & layer) != 0;
}
