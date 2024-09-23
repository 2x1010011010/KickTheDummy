using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallController : MonoBehaviour
{
    [SerializeField] private float m_speed;

    private Rigidbody _rigidbody;


    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.maxAngularVelocity = Mathf.Infinity;
    }


    private void FixedUpdate()
    {
        Vector2 input = new( Input.GetAxis( "Horizontal" ), Input.GetAxis( "Vertical" ) );

        Vector3 force = Vector3.forward * -input.x;
        force += Vector3.right * input.y;
		force *= m_speed;


		_rigidbody.AddTorque( force, ForceMode.Acceleration );
    }
}
