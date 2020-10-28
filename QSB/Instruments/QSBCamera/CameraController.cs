using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace QSB.Instruments.QSBCamera
{
    class CameraController : MonoBehaviour
    {
        private float _degreesX;
        private float _degreesY;
        private Quaternion _rotationX;
        private Quaternion _rotationY;

        void FixedUpdate()
        {
            if (CameraManager.Instance.Mode != CameraMode.ThirdPerson)
            {
                return;
            }
            UpdateInput();
            UpdateRotation();
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
            gameObject.transform.parent.localRotation = localRotation;
        }
    }
}
