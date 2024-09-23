using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateWithKey : MonoBehaviour
{
    [SerializeField] private float m_speed;

    private Rigidbody _rigidbody;


    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }


	private void FixedUpdate()
	{
        Vector3 rotationBefore = _rigidbody.rotation.eulerAngles;
        if ( Input.GetKey( KeyCode.UpArrow ) )
            rotationBefore.x += Time.fixedDeltaTime * m_speed;
        else if( Input.GetKey( KeyCode.DownArrow ) )
			rotationBefore.x -= Time.fixedDeltaTime * m_speed;
        _rigidbody.MoveRotation( Quaternion.Euler( rotationBefore ) );
	}
}
