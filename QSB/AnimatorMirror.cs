using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace QSB
{
    public class AnimatorMirror : MonoBehaviour
    {
        private const float SmoothTime = 0.02f;

        private Animator _from;
        private Animator _to;
        private bool _isRunning;
        private float _smoothVelocity;
        private Dictionary<string, float> _floatParams;

        public void Init(Animator from, Animator to)
        {
            _from = from;
            _to = to;
            if (_from.runtimeAnimatorController == null)
            {
                _from.runtimeAnimatorController = _to.runtimeAnimatorController;
            }
            else if (_to.runtimeAnimatorController == null)
            {
                _to.runtimeAnimatorController = _from.runtimeAnimatorController;
            }
            foreach (var param in _from.parameters.Where(p => p.type == AnimatorControllerParameterType.Float))
            {
                _floatParams.Add(param.name, param.defaultFloat);
            }
            _floatParams = new Dictionary<string, float>();
            _isRunning = true;
        }

        private void Update()
        {
            if (!_isRunning)
            {
                return;
            }
            SyncParams();
            SmoothFloats();
        }

        private void SyncParams()
        {
            foreach (var fromParam in _from.parameters)
            {
                switch (fromParam.type)
                {
                    case AnimatorControllerParameterType.Float:
                        _floatParams[fromParam.name] = _from.GetFloat(fromParam.name);
                        break;
                    case AnimatorControllerParameterType.Int:
                        _to.SetInteger(fromParam.name, _from.GetInteger(fromParam.name));
                        break;
                    case AnimatorControllerParameterType.Bool:
                        _to.SetBool(fromParam.name, _from.GetBool(fromParam.name));
                        break;
                }
            }
        }

        private void SmoothFloats()
        {
            foreach (var floatParam in _floatParams)
            {
                var current = _to.GetFloat(floatParam.Key);
                var value = Mathf.SmoothDamp(current, floatParam.Value, ref _smoothVelocity, SmoothTime);
                _to.SetFloat(floatParam.Key, value);
            }
        }

    }
}
