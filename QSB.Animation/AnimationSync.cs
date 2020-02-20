using UnityEngine;
using UnityEngine.Networking;

namespace QSB.Animation
{
    public class AnimationSync : NetworkBehaviour
    {
        private Animator _anim;
        private NetworkAnimator _netAnim;

        private void Awake()
        {
            _anim = gameObject.AddComponent<Animator>();
            _netAnim = gameObject.AddComponent<NetworkAnimator>();
            _netAnim.animator = _anim;
        }

        public void Init(Transform body)
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
