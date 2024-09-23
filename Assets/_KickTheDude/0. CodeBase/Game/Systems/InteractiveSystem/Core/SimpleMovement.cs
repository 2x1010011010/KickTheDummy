using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.InteractiveSystem
{
    public class SimpleMovement : IInteractive<IInteractable>, IMoveable
    {
        public string Name => "SIMPLE MOVEMENT";

        [SerializeField] private Transform _moveTransform;
        [SerializeField] private float _speed;

        public IInteractable Interactable { get; private set; }

        public void Init(IInteractable initData)
        {
            Interactable = initData;
        }

        public void Dispose()
        {
           
        }

        public void StopInteract()
        {

        }

        public void Move(Vector3 direction, bool local)
        {
            _moveTransform.position = _moveTransform.position + direction * _speed * Time.deltaTime / Time.timeScale;
        }

        public void MoveToPoint(Vector3 point)
        {
            
        }

        public void StopMoving()
        {
            
        }

        public void Move(Vector2 input)
        {
            _moveTransform.position = (_moveTransform.forward * input.y + _moveTransform.right * input.x) * _speed;
        }
    }
}
