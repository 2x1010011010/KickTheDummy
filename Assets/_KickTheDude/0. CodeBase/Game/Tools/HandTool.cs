using Game.InteractiveSystem;
using Sirenix.OdinInspector;
using UnityEngine;

public class HandTool : MonoBehaviour, ITool
{
    public string Name => "HandTool";

    [SerializeField, BoxGroup("PARAMETERS")] private float _hitRange = 10f;
    [SerializeField, BoxGroup("PARAMETERS")] private LayerMask _draggableMask = 1 << 0;
    [SerializeField, BoxGroup("PARAMETERS")] private ConfigurableJointParameters _dragParameters;

    [SerializeField, BoxGroup("AUDIO")] private AudioSource _source;
    [SerializeField, BoxGroup("AUDIO")] private AudioClip _grabClip;
    [SerializeField, BoxGroup("AUDIO")] private AudioClip _dropClip;

    private Vector3 _up;
    private Vector3 _forward;
    private Rigidbody _draggedBody;
    private ConfigurableJoint _joint;
    private Rigidbody _hand;
    private Camera _camera;
    private float _dragDistance;

    public IInteractable Interactable { get; private set; }

    public void Init(IInteractable initData)
    {
        Interactable = initData;
    }

    public void StopInteract()
    {
        StopUse();
    }

    private void OnEnable()
    {
        _camera = Camera.main;
        CreateHand();
    }

    public void StartUse(Vector2 screenPosition, Vector2 direction)
    {
        TryDrag(screenPosition, direction);
    }

    public void StopUse()
    {
        Drop();
    }

    public void FixedUpdate()
    {
        if (_joint)
        {
            var ray = _camera.ScreenPointToRay(Input.mousePosition);
            _hand.position = ray.origin + ray.direction * _dragDistance;
        }
    }

    private Rigidbody CreateHand()
    {
        GameObject handGO = new GameObject("hand");

        _hand = handGO.AddComponent<Rigidbody>();
        _hand.isKinematic = true;
        _hand.mass = 15f;

        handGO.transform.SetParent(transform);

        return _hand;
    }

    private bool TryDrag(Vector2 screenPosition, Vector2 direction)
    {
        var ray = _camera.ScreenPointToRay(screenPosition);

        if (!Physics.Raycast(ray, out RaycastHit hit, _hitRange, _draggableMask))
            return false;

        if (!hit.collider.attachedRigidbody)
            return false;

        _hand.transform.position = hit.point;

        _dragDistance = hit.distance;

        _draggedBody = hit.collider.attachedRigidbody;
        _joint = _hand.gameObject.AddComponent<ConfigurableJoint>();
        _joint.connectedBody = _draggedBody;
        _dragParameters.ApplyParametersToJoint(_joint);

        _source.PlayOneShot(_grabClip);

        return true;
    }

    private void Drop()
    {
        if (!_joint)
            return;

        Destroy(_joint);
        _joint = null;
        _draggedBody = null;

        _source.PlayOneShot(_dropClip);
    }


    private void OnDrawGizmos()
    {
        if (!_joint)
            return;

        Vector3 position = _draggedBody.transform.TransformPoint(_joint.connectedAnchor);
        Gizmos.DrawSphere(position, 0.1f);
    }

    public void Dispose()
    {
        
    }
}
