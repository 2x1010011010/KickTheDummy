using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FluidFlow
{
    public class Mover : MonoBehaviour
    {
        public float RotationSpeed = 25;
        public float MoveSpeed = 1;
        public float HeightDelta;
        private Vector3 startPos;

        private void Awake()
        {
            startPos = transform.position;
        }

        private void Update()
        {
            transform.Rotate(Vector3.up * RotationSpeed * Time.deltaTime);
            transform.position = startPos + Vector3.up * HeightDelta * Mathf.Sin(Time.time * MoveSpeed);
        }
    }
}