using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

public class BruisersDrawSystem : MonoBehaviour
{
    [SerializeField, BoxGroup("SETUP")] private Texture _bruiseTexture;
    [SerializeField, BoxGroup("SETUP")] private float _size;
    [SerializeField, BoxGroup("SETUP")] private List<CollisionObserver> _collisionSenders;

    private void OnEnable()
    {
        foreach (var collisionSender in _collisionSenders)
            collisionSender.CollisionEnter += CollisionEnter;
    }

    private void OnDisable()
    {
        foreach (var collisionSender in _collisionSenders)
            collisionSender.CollisionEnter -= CollisionEnter;
    }

    private void CollisionEnter(Collision collision)
    {
        return;

        var collisionContact = collision.GetContact(0);

        Debug.Log(collisionContact.thisCollider.gameObject.name);

        if (collisionContact.thisCollider.attachedRigidbody == null) return;

        if (collisionContact.thisCollider.attachedRigidbody.TryGetComponent(out IPaintable paintable))
        {
            Debug.Log("Paintable " + paintable);

            paintable.Paint(new TexturePaintData(collisionContact.point, collisionContact.normal, _bruiseTexture, _size, false));
        }
    }
}
