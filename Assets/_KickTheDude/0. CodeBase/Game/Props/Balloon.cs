using Game.DamageSystem;
using Game.InteractiveSystem;
using Sirenix.OdinInspector;
using System;
using UnityEngine;

public class Balloon : IInteractive<IInteractable>, IAttacheable, IUpdateable
{
    public event Action AttachedToCharacter;

    public string Name => "BALLOON";

    public IInteractable Interactable { get; private set; }

    [SerializeField, BoxGroup("SETUP")] private Rigidbody _selfRigidbody;
    [SerializeField, BoxGroup("SETUP")] private HealthContainer _healthContainer;
    [SerializeField, BoxGroup("SETUP")] private Vector2 _randomLength;
    [SerializeField, BoxGroup("SETUP")] private Material[] _randomMaterials;
    [SerializeField, BoxGroup("SETUP")] private MeshRenderersContainer _meshRenderersContainer;
    [SerializeField, BoxGroup("SETUP")] private AudioSource _balloonSource;
    [SerializeField, BoxGroup("SETUP")] private AudioClip _inflateClip;
    [SerializeField, BoxGroup("SETUP")] private AudioClip _snapClip;
    [SerializeField, BoxGroup("SETUP")] private ConstantForce _heliumForce;
    [SerializeField, BoxGroup("SETUP")] private float _forceWhenFree;
    [SerializeField, BoxGroup("SETUP")] private float _forceWhenAttach;
    [SerializeField, BoxGroup("SETUP")] private float _destroyHeight = 100;
    [SerializeField, BoxGroup("SETUP")] private float _characterRagdollRelaxValue;
    [SerializeField, BoxGroup("SETUP")] private LineRenderer _ropeRenderer;
    [SerializeField, BoxGroup("SETUP")] private ConfigurableJointParameters _attachParameters;

    [SerializeField, BoxGroup("DEBUG"), ReadOnly] private Rigidbody _attachetBody;
    [SerializeField, BoxGroup("DEBUG"), ReadOnly] private ConfigurableJoint _attachJoint;

    public void Init(IInteractable initData)
    {
        Interactable = initData;

        _healthContainer.OnHealthEnded += OnHealthEnded;

        _balloonSource.PlayOneShot(_inflateClip);

        var material = _randomMaterials[UnityEngine.Random.Range(0, _randomMaterials.Length)];
        foreach (var renderer in _meshRenderersContainer.Renderers)
            renderer.material = material;
    }

    public void Dispose()
    {
        StopInteract();
    }

    public void StopInteract()
    {
        Detach();
    }

    private void OnHealthEnded(HealthContainer healthContainer, float healthCount)
    {
        Debug.Log("Ballon health ended");
        Interactable.Destroy();
    }

    public void AttachTo(Rigidbody rigidbodyToAttach)
    {
        Debug.Log("Ballon attached to " + rigidbodyToAttach);
        _attachetBody = rigidbodyToAttach;

        _attachJoint = _selfRigidbody.gameObject.AddComponent<ConfigurableJoint>();
        _attachJoint.connectedBody = _attachetBody;

        _attachParameters.ApplyParametersToJoint(_attachJoint);
        _attachJoint.linearLimit = new SoftJointLimit { bounciness = 0, contactDistance = 0f, limit = UnityEngine.Random.Range(_randomLength.x, _randomLength.y) };

        _heliumForce.force = new Vector3(0, _forceWhenAttach, 0);

        _ropeRenderer.gameObject.SetActive(true);

        _balloonSource.PlayOneShot(_snapClip);

        var interactableObject = _attachetBody.GetComponentInParent<InteractableObject>();

        if(interactableObject != null)
            interactableObject.Destroyed += AttachedObjectDestroyed;
    }

    private void AttachedObjectDestroyed(InteractableObject attachedObject)
    {
        attachedObject.Destroyed -= AttachedObjectDestroyed;

        Detach();
    }

    public void AttachTo(Rigidbody rigidbodyToAttach, ConfigurableJointParameters attachParameters)
    {
        
    }

    public void Update()
    {
        if (_attachJoint == null) return;
        if (_attachetBody == null) { Detach(); return; }
        if (_attachetBody.detectCollisions == false) { Detach(); return; }
        if (_selfRigidbody.position.y > _destroyHeight)
        {
            Debug.Log("Ballon destroyed by heigth");

            Interactable.Destroy();
        }

        if (_ropeRenderer.gameObject.activeSelf)
        {
            _ropeRenderer.SetPosition(0, _selfRigidbody.position);
            _ropeRenderer.SetPosition(1, _attachetBody.transform.TransformPoint(_attachJoint.connectedAnchor));
        }
    }

    public void Detach()
    {
        GameObject.Destroy(_attachJoint);
        _attachJoint = null;
        _ropeRenderer.gameObject.SetActive(false);

        _heliumForce.force = new Vector3(0, _forceWhenFree, 0);

        if (_attachetBody == null) return;

        var interactableObject = _attachetBody.GetComponentInParent<InteractableObject>();

        if (interactableObject != null)
            interactableObject.Destroyed -= AttachedObjectDestroyed;
    }
}
