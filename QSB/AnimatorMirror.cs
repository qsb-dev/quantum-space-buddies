using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace QSB
{
    public class AnimatorMirror : MonoBehaviour
    {
        private const float SmoothTime = 0.05f;

        private Animator _from;
        private Animator _to;
        private bool _isRunning;

        private readonly Dictionary<string, FloatAnimParam> _floatParams = new Dictionary<string, FloatAnimParam>();

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
                _floatParams.Add(param.name, new FloatAnimParam());
            }
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
                        _floatParams[fromParam.name].Target = _from.GetFloat(fromParam.name);
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
                var current = floatParam.Value.Smooth(SmoothTime);
                _to.SetFloat(floatParam.Key, current);
            }
        }

    }
}
