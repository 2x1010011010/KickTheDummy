using UnityEngine;


namespace SurfaceDataSystemOld
{
    public class Respawnable : MonoBehaviour
    {
        private Vector3 _position;
        private Rigidbody _rigidbody;
    
    
        void Start()
        {
            _position = transform.position;
            _rigidbody = GetComponent<Rigidbody>();
        }
    
    
        void Update()
        {
            if( transform.position.y < -50 )
                Respawn();
        }
    
    
        private void Respawn()
        {
            _rigidbody.velocity = new Vector3( 0, _rigidbody.velocity.y, 0 );
            transform.position = _position + Vector3.up * 50f;
        }
    }
}