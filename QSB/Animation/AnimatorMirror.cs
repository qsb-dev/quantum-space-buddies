using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace QSB.Animation
{
    public class AnimatorMirror : MonoBehaviour
    {
        private const float SmoothTime = 0.05f;

        private Animator _from;
        private Animator _to;
        private bool _isRunning;

        private readonly Dictionary<string, AnimFloatParam> _floatParams = new Dictionary<string, AnimFloatParam>();

        public void Init(Animator from, Animator to)
        {
            _from = from;
            _to = to;
            CopyRuntimeControllers();
            foreach (var param in _from.parameters.Where(p => p.type == AnimatorControllerParameterType.Float))
            {
                _floatParams.Add(param.name, new AnimFloatParam());
            }
            _isRunning = true;
        }

        private void CopyRuntimeControllers()
        {
            if (_from.runtimeAnimatorController == null)
            {
                _from.runtimeAnimatorController = _to.runtimeAnimatorController;
            }
            else if (_to.runtimeAnimatorController == null)
            {
                _to.runtimeAnimatorController = _from.runtimeAnimatorController;
            }
        }

        private void Update()
        {
            if (!_isRunning)
            {
                return;
            }
            CopyRuntimeControllers();
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
