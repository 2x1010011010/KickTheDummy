using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddTireForce : MonoBehaviour
{
    [SerializeField] private Vector3 m_force;
	[SerializeField] private float m_bound;


	private Rigidbody _rigidbody;


	private void Awake()
	{
		_rigidbody = GetComponent<Rigidbody>();
		_rigidbody.maxAngularVelocity = Mathf.Infinity;
	}


	private void FixedUpdate()
	{
		_rigidbody.AddRelativeTorque( m_force );

		if( _rigidbody.position.x > m_bound )
			_rigidbody.position = new Vector3( -m_bound, _rigidbody.position.y, _rigidbody.position.z );
		else if( _rigidbody.position.x < -m_bound )
			_rigidbody.position = new Vector3( m_bound, _rigidbody.position.y, _rigidbody.position.z );
	}
}
