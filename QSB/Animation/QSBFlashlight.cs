using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace QSB.Animation
{
    public class QSBFlashlight : MonoBehaviour
    {
        public OWLight2[] _lights;
        public OWLight2 _illuminationCheckLight;
        public Transform _root;
        public Transform _basePivot;
        public Transform _wobblePivot;
        private bool _flashlightOn;
        private Vector3 _baseForward;
        private Quaternion _baseRotation;

        private void Start()
        {
            _baseForward = _basePivot.forward;
            _baseRotation = _basePivot.rotation;
        }

        public void TurnOn()
        {
            if (!_flashlightOn)
            {
                for (int i = 0; i < _lights.Length; i++)
                {
                    _lights[i].GetLight().enabled = true;
                }
                _flashlightOn = true;
                Quaternion rotation = _root.rotation;
                _basePivot.rotation = rotation;
                _baseRotation = rotation;
                _baseForward = _basePivot.forward;
            }
        }

        public void TurnOff()
        {
            if (_flashlightOn)
            {
                for (int i = 0; i < _lights.Length; i++)
                {
                    _lights[i].GetLight().enabled = false;
                }
                _flashlightOn = false;
            }
        }

        private void FixedUpdate()
        {
            Quaternion lhs = Quaternion.FromToRotation(_basePivot.up, _root.up) * Quaternion.FromToRotation(_baseForward, _root.forward);
            Quaternion b = lhs * _baseRotation;
            _baseRotation = Quaternion.Slerp(_baseRotation, b, 6f * Time.deltaTime);
            _basePivot.rotation = _baseRotation;
            _baseForward = _basePivot.forward;
            _wobblePivot.localRotation = OWUtilities.GetWobbleRotation(0.3f, 0.15f) * Quaternion.identity;
        }
    }
}
