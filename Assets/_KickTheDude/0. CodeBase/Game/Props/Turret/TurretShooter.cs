using System;
using System.Collections;
using System.Collections.Generic;
using RootMotion.Dynamics;
using RootMotion.FinalIK;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Zenject;

public class TurretShooter : MonoBehaviour
{
    [SerializeField, BoxGroup("PHYSICS PARAMETERS")] private LayerMask _layerMask;
    [SerializeField, BoxGroup("PHYSICS PARAMETERS")] private float _raycastDistance;
    [SerializeField, BoxGroup("PHYSICS PARAMETERS")] private Transform _rayOrigin;

    [SerializeField, BoxGroup("PHYSICS PARAMETERS")] private float _objectForce = 50f;
    [SerializeField, BoxGroup("PHYSICS PARAMETERS")] private float unpinRagdoll = 5f;
    [SerializeField, BoxGroup("PHYSICS PARAMETERS")] private float unpinRagdollForce = 100f;

    [SerializeField, BoxGroup("GUN PARAMETERS")] private float _firerate = 50f;
    [SerializeField, BoxGroup("GUN PARAMETERS")] private float _damage = 1f;
    [SerializeField, BoxGroup("GUN PARAMETERS")] private bool _dismember;

    [SerializeField, BoxGroup("AUDIO PARAMETERS")] private AudioSource _audioSource;
    [SerializeField, BoxGroup("AUDIO PARAMETERS")] private AudioClip[] _clips;
    [SerializeField, BoxGroup("AUDIO PARAMETERS")] private Vector2 _pitchRandom = Vector2.one;

    [SerializeField, BoxGroup("BLOOD PAINT PARAMETERS")] private Color _color = Color.red;
    [SerializeField, BoxGroup("BLOOD PAINT PARAMETERS")] private int _amount = 1;
    [SerializeField, BoxGroup("BLOOD PAINT PARAMETERS")] private float _size = 0.1f;
    [SerializeField, BoxGroup("BLOOD PAINT PARAMETERS")] private Texture _bloodTexture;
    [SerializeField, BoxGroup("BLOOD PAINT PARAMETERS")] private Texture _bloodNormalTexture;
    [SerializeField, BoxGroup("BLOOD PAINT PARAMETERS")] private float _textureSize = 0.1f;

    [SerializeField, BoxGroup("OTHER PAINT PARAMETERS")] private Texture _bulletHoleTexture;
    [SerializeField, BoxGroup("OTHER PAINT PARAMETERS")] private float _bulletHoleTextureSize = 0.1f;

    private RaycastHit _hit = new RaycastHit();
    private Coroutine _shooting;
    
    private IEffectsFactory _effectsFactory;

    public event Action<ShotData> Shoted;
    
   /* [Inject]
    private void Construct(IEffectsFactory effectsFactory)
    {
        _effectsFactory = effectsFactory;
    }*/

   public void Init(IEffectsFactory effectsFactory)
   {
       _effectsFactory = effectsFactory;
   }


   public void StartUse(Transform target)
    {
        if (_shooting != null)
            StopUse();

        _shooting = StartCoroutine(Shooting(target));
    }

    private IEnumerator Shooting(Transform target)
    {
        while (true)
        {
            Shot(target);
            yield return new WaitForSeconds(60 / _firerate);
        }
    }

    private async void Shot(Transform target)
    {
        PlayAudio();

        var targetPosition = new Vector3(target.position.x, _rayOrigin.position.y, target.position.z);
        var rayDirection = targetPosition - _rayOrigin.position;
        var ray = new Ray(_rayOrigin.position, rayDirection);
        
        Physics.Raycast(ray, out _hit, _raycastDistance, _layerMask, QueryTriggerInteraction.Ignore);

        await _effectsFactory.CreateEffectByPhysicsMaterial(_hit.collider.sharedMaterial, _hit.point, Quaternion.LookRotation(_hit.normal));

        if (_hit.rigidbody != null)
        {
            if (_hit.collider.TryGetComponent(out IDamageable damageable))
                damageable.TakeDamage(new Damage(_damage));

            if (_hit.rigidbody.TryGetComponent(out MuscleCollisionBroadcaster muscleCollisionBroadcaster))
            {
               // muscleCollisionBroadcaster.Hit(unpinRagdoll, Quaternion.LookRotation(ray.direction) * new Vector3(0, 0, unpinRagdollForce), _hit.point);
                ((BehaviourPuppet)muscleCollisionBroadcaster.puppetMaster.behaviours[0]).Unpin();

                if (muscleCollisionBroadcaster.puppetMaster.muscles[muscleCollisionBroadcaster.muscleIndex].target.name == "head")
                {
                    muscleCollisionBroadcaster.puppetMaster.Kill();
                    muscleCollisionBroadcaster.puppetMaster.targetRoot.GetComponent<FullBodyBipedIK>().enabled = false;
                }

                if (_hit.collider.TryGetComponent(out IPaintable paintable))
                {
                    paintable.Paint(new TexturePaintData(_hit.point, _hit.normal, _bloodTexture, _textureSize, false));
                    paintable.Paint(new TexturePaintData(_hit.point, _hit.normal, _bloodNormalTexture, _textureSize, true));
                    paintable.Paint(new ParticlesPaintData(_hit.point, _hit.normal, _color, _size, _amount));
                }

                if(_dismember && _hit.rigidbody.TryGetComponent(out IDismemberable dismemberable))
                {
                    dismemberable.Dismember(DismemberType.Tear);
                }
            }
            else
            {
               ApplyToObject(ray);
            }
        }
        else
        {
            ApplyToObject(ray);
        }
    }

    private void ApplyToObject(Ray ray)
    {
        if (_hit.collider.TryGetComponent(out IPaintable paintable))
        {
            paintable.Paint(new TexturePaintData(_hit.point, _hit.normal, _bulletHoleTexture, _bulletHoleTextureSize, false));
        }

        if (_hit.rigidbody == null) return;

        _hit.rigidbody.AddForceAtPosition(Quaternion.LookRotation(ray.direction) * new Vector3(0, 0, _objectForce), _hit.point, ForceMode.Impulse);
        _hit.rigidbody.AddForce(Vector3.up * _objectForce / 3, ForceMode.Impulse);
    }

    private void PlayAudio()
    {
        var targetClip = _clips[UnityEngine.Random.Range(0, _clips.Length)];

        _audioSource.Stop();
        _audioSource.pitch = UnityEngine.Random.Range(_pitchRandom.x, _pitchRandom.y);
        _audioSource.PlayOneShot(targetClip);
    }

    public void StopUse()
    {
        if (_shooting != null)
        {
            StopCoroutine(_shooting);
        }
    }
}
