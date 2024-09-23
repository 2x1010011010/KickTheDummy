using Game.InteractiveSystem;
using Sirenix.OdinInspector;
using UnityEngine;

public class PhysicalConnection : SerializedMonoBehaviour, IDestroyable
{
    [SerializeField, BoxGroup("SETUP")] private LineRenderer _lineRenderer;
    [SerializeField, BoxGroup("SETUP")] private CapsuleCollider _trigger;

    [SerializeField, BoxGroup("DEBUG"), ReadOnly] private ConnectPoint _firstConnectPoint;
    [SerializeField, BoxGroup("DEBUG"), ReadOnly] private ConnectPoint _secondConnectPoint;
    [SerializeField, BoxGroup("DEBUG"), ReadOnly] private PhysicalConnectionParameters _physicalConnectionParameters;

    [SerializeField, BoxGroup("DEBUG"), ReadOnly] private ConfigurableJoint _physicalConnection;

    public ConnectPoint FirstConnectPoint => _firstConnectPoint;
    public ConnectPoint SecondConnectPoint => _secondConnectPoint;

    public PhysicalConnectionParameters PhysicalConnectionParameters => _physicalConnectionParameters;

    public void Init(ConnectPoint firstConnectPoint, ConnectPoint secondConnectPoint, PhysicalConnectionParameters physicalConnectionParameters, bool autoCalculateLimit)
    {
        _firstConnectPoint = firstConnectPoint;
        _secondConnectPoint = secondConnectPoint;
        _physicalConnectionParameters = physicalConnectionParameters;
        _lineRenderer.material = physicalConnectionParameters.Material;

        CreateJoint(autoCalculateLimit);

        _firstConnectPoint.InteractableObject.Destroyed += InteractableObjectDestroyed;
        _secondConnectPoint.InteractableObject.Destroyed += InteractableObjectDestroyed;

        if(_firstConnectPoint.Rigidbody.TryGetComponent(out IDetachable firstDetachable))
            firstDetachable.DetachSignal += DetachSignal;

        if (_secondConnectPoint.Rigidbody.TryGetComponent(out IDetachable secondDetachable))
            secondDetachable.DetachSignal += DetachSignal;
    }

    private void DetachSignal(IDetachable detachable, DetachType detachType)
    {
        DestroySelf();
    }

    private void InteractableObjectDestroyed(InteractableObject interactableObject)
    {
        DestroySelf();
    }

    private void CreateJoint(bool autoCalculateLimit)
    {
        _physicalConnection = _firstConnectPoint.Rigidbody.gameObject.AddComponent<ConfigurableJoint>();

        _physicalConnectionParameters.ConfigurableJointParameters.ApplyParametersToJoint(_physicalConnection);

        _physicalConnection.connectedBody = _secondConnectPoint.Rigidbody;

        if(autoCalculateLimit)
            _physicalConnection.linearLimit = new SoftJointLimit() { limit = Vector3.Distance(_firstConnectPoint.WorldConnectPoint, _secondConnectPoint.WorldConnectPoint) };

        _physicalConnection.anchor = _firstConnectPoint.LocalConnectPoint;
        if(!_physicalConnection.autoConfigureConnectedAnchor) _physicalConnection.connectedAnchor = _secondConnectPoint.LocalConnectPoint;
    }

    public void DestroySelf()
    {
        _firstConnectPoint.InteractableObject.Destroyed -= InteractableObjectDestroyed;
        _secondConnectPoint.InteractableObject.Destroyed -= InteractableObjectDestroyed;

        Destroy(_physicalConnection);
        Destroy(gameObject);
    }

    private void Update()
    {
        if (_firstConnectPoint == null || _secondConnectPoint == null) return;

        _lineRenderer.SetPosition(0, _firstConnectPoint.WorldConnectPoint);
        _lineRenderer.SetPosition(1, _secondConnectPoint.WorldConnectPoint);
    }

    private void FixedUpdate()
    {
        if (_firstConnectPoint == null || _secondConnectPoint == null) return;

        transform.position = Vector3.Lerp(_firstConnectPoint.WorldConnectPoint, _secondConnectPoint.WorldConnectPoint, 0.5f);
        transform.rotation = Quaternion.LookRotation((_firstConnectPoint.WorldConnectPoint - _secondConnectPoint.WorldConnectPoint).normalized);
        _trigger.height = Vector3.Distance(_firstConnectPoint.WorldConnectPoint, _secondConnectPoint.WorldConnectPoint);
    }
}
