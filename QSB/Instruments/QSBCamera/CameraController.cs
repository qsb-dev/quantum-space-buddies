using UnityEngine;

namespace QSB.Instruments.QSBCamera
{
    class CameraController : MonoBehaviour
    {
        private float _degreesX;
        private float _degreesY;
        private Quaternion _rotationX;
        private Quaternion _rotationY;

        private const float PercentToMove = 0.75f;

        public GameObject CameraObject;

        void FixedUpdate()
        {
            if (CameraManager.Instance.Mode != CameraMode.ThirdPerson)
            {
                return;
            }
            UpdatePosition();
            UpdateInput();
            UpdateRotation();
        }

        private void UpdatePosition()
        {
            var origin = transform.position;
            var localDirection = CameraObject.transform.localPosition.normalized;
            Vector3 localTargetPoint;
            if (Physics.Raycast(origin, transform.TransformDirection(localDirection), out RaycastHit outRay, 10, LayerMask.GetMask("Default")))
            {
                localTargetPoint = transform.InverseTransformPoint(outRay.point) * PercentToMove;
            }
            else
            {
                localTargetPoint = localDirection * 10 * PercentToMove;
            }
            var targetDistance = Vector3.Distance(origin, transform.TransformPoint(localTargetPoint));
            var currentDistance = Vector3.Distance(origin, CameraObject.transform.position);
            Vector3 movement;
            if (targetDistance < currentDistance)
            {
                // Snap to target to avoid clipping
                movement = localTargetPoint;
            }
            else
            {
                // Move camera out slowly
                movement = Vector3.MoveTowards(CameraObject.transform.localPosition, localTargetPoint, Time.fixedDeltaTime * 2f);
            }
            CameraObject.transform.localPosition = movement;
        }

        private void UpdateInput()
        {
            var input = OWInput.GetValue(InputLibrary.look, false, InputMode.All);
            _degreesX += input.x * 180f * Time.fixedDeltaTime;
            _degreesY += input.y * 180f * Time.fixedDeltaTime;
        }

        private void UpdateRotation()
        {
            _degreesX %= 360f;
            _degreesY %= 360f;
            _degreesY = Mathf.Clamp(_degreesY, -80f, 80f);
            _rotationX = Quaternion.AngleAxis(_degreesX, Vector3.up);
            _rotationY = Quaternion.AngleAxis(_degreesY, -Vector3.right);
            var localRotation = _rotationX * _rotationY * Quaternion.identity;
            transform.localRotation = localRotation;
        }
    }
}
