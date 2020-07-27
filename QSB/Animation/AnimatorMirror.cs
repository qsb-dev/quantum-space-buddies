using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace QSB.Animation
{
    public class AnimatorMirror : MonoBehaviour
    {
        private const float SmoothTime = 0.05f;
        private const int CrouchLayerIndex = 1;

        private Animator _from;
        private Animator _to;

        private readonly Dictionary<string, AnimFloatParam> _floatParams = new Dictionary<string, AnimFloatParam>();

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
                _floatParams.Add(param.name, new AnimFloatParam());
            }
        }

        private void Update()
        {
            if (_to == null || _from == null)
            {
                return;
            }
            if (_to.runtimeAnimatorController != _from.runtimeAnimatorController)
            {
                _to.runtimeAnimatorController = _from.runtimeAnimatorController;
            }
            SyncParams();
            SmoothFloats();
            SyncLayerWeight();
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

        private void SyncLayerWeight()
        {
            var layerWeight = _from.GetLayerWeight(CrouchLayerIndex);
            _to.SetLayerWeight(CrouchLayerIndex, layerWeight);
        }

    }
}
