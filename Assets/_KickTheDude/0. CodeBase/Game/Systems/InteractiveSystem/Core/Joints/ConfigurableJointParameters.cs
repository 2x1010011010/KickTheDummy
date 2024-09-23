using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class QuaternionParameter
{
    public float X;
    public float Y;
    public float Z;
    public float W = 1;
}

[System.Serializable]
public class SoftJointLimitSpringParameter
{
    public float Spring;
    public float Damper;
}

[System.Serializable]
public class SoftJointLimitParameter
{
    public float Limit;
    public float Bounciness;
    public float ContactDistance;
}

[System.Serializable]
public class JointDriveParameter
{
    public float PositionSpring;
    public float PositionDumper;
    public float MaximumForce = 3.402823e+38f;
}

[CreateAssetMenu(fileName = "ConfigurableJointParameters", menuName = "StaticData/ConfigurableJointParameters", order = 1)]
public class ConfigurableJointParameters : ScriptableObject
{
    [SerializeField] private Rigidbody _connectedBody;
    [SerializeField] private ArticulationBody _connectedArticulationBody;
    [SerializeField] private Vector3 _anchor;
    [SerializeField] private Vector3 _axis;
    [SerializeField] private bool _autoconfigureConnectedAnchor = true;
    [SerializeField] private Vector3 _connectedAnchor;
    [SerializeField] private Vector3 _secondaryAxis; 
    [SerializeField] private ConfigurableJointMotion _xMotion; 
    [SerializeField] private ConfigurableJointMotion _yMotion; 
    [SerializeField] private ConfigurableJointMotion _zMotion;
    [SerializeField] private ConfigurableJointMotion _angularXMotion;
    [SerializeField] private ConfigurableJointMotion _angularYMotion;
    [SerializeField] private ConfigurableJointMotion _angularZMotion;
    [SerializeField] private SoftJointLimitSpringParameter _linearLimitSpring;
    [SerializeField] private SoftJointLimitParameter _linearLimit;
    [SerializeField] private SoftJointLimitSpringParameter _angularXLimitSpring;
    [SerializeField] private SoftJointLimitParameter _lowAngularXLimit;
    [SerializeField] private SoftJointLimitParameter _highAngularXLimit;
    [SerializeField] private SoftJointLimitSpringParameter _angularYZLimitSpring;
    [SerializeField] private SoftJointLimitParameter _angularYLimit;
    [SerializeField] private SoftJointLimitParameter _angularZLimit;
    [SerializeField] private Vector3 _targetPosition;
    [SerializeField] private Vector3 _targetVelocity;
    [SerializeField] private JointDriveParameter _xDrive;
    [SerializeField] private JointDriveParameter _yDrive;
    [SerializeField] private JointDriveParameter _zDrive;
    [SerializeField] private QuaternionParameter _targetRotation;
    [SerializeField] private Vector3 _targetAngularVelocity;
    [SerializeField] private RotationDriveMode _rotationDriveMode;
    [SerializeField] private JointDriveParameter _angularXDrive;
    [SerializeField] private JointDriveParameter _angularYZDrive;
    [SerializeField] private JointDriveParameter _slerpDrive;
    [SerializeField] private JointProjectionMode _projectionMode;
    [SerializeField] private float _projectionDistance = 0.1f;
    [SerializeField] private float _projectionAngle = 180;
    [SerializeField] private bool _configuredInWorldSpace;
    [SerializeField] private bool _swapBodies;
    [SerializeField] private float _breakForce = Mathf.Infinity;
    [SerializeField] private float _breakTorque = Mathf.Infinity;
    [SerializeField] private bool _enableCollision;
    [SerializeField] private bool _enablePreprocessing = true;
    [SerializeField] private float _massScale = 1f;
    [SerializeField] private float _connectedMassScale = 1f;

    public void ApplyParametersToJoint(ConfigurableJoint configurableJoint)
    {
        configurableJoint.anchor = _anchor;
        configurableJoint.axis = _axis;
        configurableJoint.autoConfigureConnectedAnchor = _autoconfigureConnectedAnchor;
        configurableJoint.connectedAnchor = _connectedAnchor;
        configurableJoint.secondaryAxis = _secondaryAxis;
        configurableJoint.xMotion = _xMotion;
        configurableJoint.yMotion = _yMotion;
        configurableJoint.zMotion = _zMotion;
        configurableJoint.angularXMotion = _angularXMotion;
        configurableJoint.angularYMotion = _angularYMotion;
        configurableJoint.angularZMotion = _angularZMotion;
        configurableJoint.linearLimitSpring = GetSoftJointLimitSpring(_linearLimitSpring);
        configurableJoint.linearLimit = GetSoftJointLimit(_linearLimit);
        configurableJoint.angularXLimitSpring = GetSoftJointLimitSpring(_angularXLimitSpring);
        configurableJoint.lowAngularXLimit = GetSoftJointLimit(_lowAngularXLimit);
        configurableJoint.highAngularXLimit = GetSoftJointLimit(_highAngularXLimit);
        configurableJoint.angularYZLimitSpring = GetSoftJointLimitSpring(_angularYZLimitSpring);
        configurableJoint.angularYLimit = GetSoftJointLimit(_angularYLimit);
        configurableJoint.angularZLimit = GetSoftJointLimit(_angularZLimit);
        configurableJoint.targetPosition = _targetPosition;
        configurableJoint.targetVelocity = _targetVelocity;
        configurableJoint.xDrive = GetJointDrive(_xDrive);
        configurableJoint.yDrive = GetJointDrive(_yDrive);
        configurableJoint.zDrive = GetJointDrive(_zDrive);
        configurableJoint.targetRotation = GetQuaternion(_targetRotation);
        configurableJoint.targetAngularVelocity = _targetAngularVelocity;
        configurableJoint.rotationDriveMode = _rotationDriveMode;
        configurableJoint.angularXDrive = GetJointDrive(_angularXDrive);
        configurableJoint.angularYZDrive = GetJointDrive(_angularYZDrive);
        configurableJoint.slerpDrive = GetJointDrive(_slerpDrive);
        configurableJoint.projectionDistance = _projectionDistance;
        configurableJoint.projectionAngle = _projectionAngle;
        configurableJoint.configuredInWorldSpace = _configuredInWorldSpace;
        configurableJoint.swapBodies = _swapBodies;
        configurableJoint.breakForce = _breakForce;
        configurableJoint.breakTorque = _breakTorque;
        configurableJoint.enableCollision = _enableCollision;
        configurableJoint.enablePreprocessing = _enablePreprocessing;
        configurableJoint.massScale = _massScale;
        configurableJoint.connectedMassScale = _connectedMassScale;
    }

    private JointDrive GetJointDrive(JointDriveParameter jointDrive)
    {
        return new JointDrive() { maximumForce = jointDrive.MaximumForce, positionDamper = jointDrive.PositionDumper, positionSpring = jointDrive.PositionSpring };
    }

    private SoftJointLimitSpring GetSoftJointLimitSpring(SoftJointLimitSpringParameter softJointLimitSpring)
    {
        return new SoftJointLimitSpring() { damper = softJointLimitSpring.Damper, spring = softJointLimitSpring.Spring };
    }

    private SoftJointLimit GetSoftJointLimit(SoftJointLimitParameter softJointLimit)
    {
        return new SoftJointLimit() { bounciness = softJointLimit.Bounciness, contactDistance = softJointLimit.ContactDistance, limit = softJointLimit.Limit };
    }

    private Quaternion GetQuaternion(QuaternionParameter quaternionParameter)
    {
        return new Quaternion() { w = quaternionParameter.W, x = quaternionParameter.X, y = quaternionParameter.Y, z = quaternionParameter.Z };
    }
}