using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAttacheable
{
    void AttachTo(Rigidbody rigidbodyToAttach);
    void AttachTo(Rigidbody rigidbodyToAttach, ConfigurableJointParameters attachParameters);
    void Detach();
}
