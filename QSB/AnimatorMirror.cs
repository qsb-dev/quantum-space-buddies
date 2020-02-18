using UnityEngine;

namespace QSB
{
    public class AnimatorMirror : MonoBehaviour
    {
        private Animator _from;
        private Animator _to;
        private bool _isRunning;

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
            _isRunning = true;
        }

        private void Update()
        {
            if (!_isRunning)
            {
                return;
            }

            foreach (var fromParam in _from.parameters)
            {
                switch (fromParam.type)
                {
                    case AnimatorControllerParameterType.Float:
                        _to.SetFloat(fromParam.name, _from.GetFloat(fromParam.name));
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

    }
}
