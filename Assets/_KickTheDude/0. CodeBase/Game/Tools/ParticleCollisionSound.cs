using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleCollisionSound : MonoBehaviour
{
    [SerializeField, BoxGroup("SETUP")] private ParticleSystem _particleSystem;
    [SerializeField, BoxGroup("SETUP")] private AudioSource _source;
    [SerializeField, BoxGroup("SETUP")] private AudioClip _clip;

    private List<ParticleCollisionEvent> _collisionEvents = new List<ParticleCollisionEvent>();

    private void OnValidate()
    {
        if (_particleSystem == null) _particleSystem = GetComponent<ParticleSystem>();
    }

    /*
    private void OnParticleCollision(GameObject other)
    {

        var numCollisionEvents = _particleSystem.GetCollisionEvents(other, _collisionEvents);

        int i = 0;

        while (i < numCollisionEvents)
        {
            _source.PlayOneShot(_clip);
            i++;
        }
    }
    */
}
