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
        public Type localColliderType;
        private Rigidbody _rigidbody;
        private Collider _collider;
        private Vector3 _prevPosition;
        private const float _collisionDisableMovementThreshold = 250;

        void Awake()
        {
            _collider = GetComponent<Collider>();
            _collider.isTrigger = true;
        }

        void Start()
        {
            gameObject.AddComponent<OWRigidbody>();
            _rigidbody = gameObject.GetComponent<Rigidbody>();
            _rigidbody.useGravity = false;
            _rigidbody.isKinematic = true;
            _prevPosition = transform.position;

            InvokeRepeating(nameof(TryEnableCollisions), 1, 1);
        }

        public void IgnoreCollision(GameObject colliderParent)
        {
            var colliders = colliderParent.GetComponentsInChildren<Collider>();
            foreach (var collider in colliders)
            {
                Physics.IgnoreCollision(collider, _collider);
            }
        }

        void FixedUpdate()
        {
            _rigidbody.MovePosition(target.position);
            _rigidbody.MoveRotation(target.rotation);
        }

        void TryEnableCollisions()
        {
            if (!_collider.isTrigger)
            {
                return;
            }
            var colliders = Physics.OverlapSphere(transform.position, 2);
            foreach (var collider in colliders)
            {
                if (collider.gameObject == gameObject)
                {
                    continue;
                }
                if (collider.GetComponentInParent(localColliderType) || collider.GetComponent<RigidbodySync>())
                {
                    DebugLog.Screen(gameObject.name, "occupied by", collider.name);
                    return;
                }
            }
            DebugLog.Screen("Enable collisions for", gameObject.name);
            _collider.isTrigger = false;
        }

        void Update()
        {
            if (!_collider.isTrigger && (_prevPosition - transform.position).sqrMagnitude > _collisionDisableMovementThreshold)
            {
                DebugLog.Screen("Disable collisions for", gameObject.name);
                _collider.isTrigger = true;
            }
            _prevPosition = transform.position;
        }
    }
}
