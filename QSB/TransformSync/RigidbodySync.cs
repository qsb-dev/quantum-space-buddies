using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace QSB.TransformSync
{
    class RigidbodySync : MonoBehaviour
    {
        public Transform target;
        private Rigidbody _rigidbody;

        void Start()
        {
            gameObject.AddComponent<OWRigidbody>();
            _rigidbody = gameObject.GetComponent<Rigidbody>();
            if (_rigidbody == null)
            {
                DebugLog.Screen("Could not add rigidbody");
            }
            else
            {
                DebugLog.Screen("Could yes add rigidbody");
            }
            _rigidbody.useGravity = false;
            _rigidbody.isKinematic = true;
        }

        void FixedUpdate()
        {
            _rigidbody.MovePosition(target.position);
            _rigidbody.MoveRotation(target.rotation);
        }
    }
}
