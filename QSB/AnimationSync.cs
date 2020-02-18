using UnityEngine;
using UnityEngine.Networking;

namespace QSB
{
    public class AnimationSync : MonoBehaviour
    {
        private Animator _anim;
        private NetworkAnimator _netAnim;

        private void Awake()
        {
            _anim = gameObject.AddComponent<Animator>();
            _netAnim = gameObject.AddComponent<NetworkAnimator>();
            _netAnim.animator = _anim;
        }

        public void Init(Transform body, bool isLocalPlayer)
        {
            var bodyAnim = body.GetComponent<Animator>();
            var animMirror = body.gameObject.AddComponent<AnimatorMirror>();

            if (isLocalPlayer)
            {
                animMirror.Init(bodyAnim, _anim);
            }
            else
            {
                animMirror.Init(_anim, bodyAnim);
            }

            for (var i = 0; i < _anim.parameterCount; i++)
            {
                _netAnim.SetParameterAutoSend(i, true);
            }
        }

    }
}
