using QSB.Messaging;
using UnityEngine;
using UnityEngine.Networking;

namespace QSB.Animation
{
    public class AnimationSync : NetworkBehaviour
    {
        private Animator _anim;
        private NetworkAnimator _netAnim;
        private Animator _bodyAnim;
        private MessageHandler<AnimTriggerMessage> _triggerHandler;

        private void Awake()
        {
            _anim = gameObject.AddComponent<Animator>();
            _netAnim = gameObject.AddComponent<NetworkAnimator>();
            _netAnim.animator = _anim;
            _triggerHandler = new MessageHandler<AnimTriggerMessage>();
            _triggerHandler.OnServerReceiveMessage += OnServerReceiveMessage;
            _triggerHandler.OnClientReceiveMessage += OnClientReceiveMessage;
        }

        public void Init(Transform body)
        {
            _bodyAnim = body.GetComponent<Animator>();
            var animMirror = body.gameObject.AddComponent<AnimatorMirror>();

            if (isLocalPlayer)
            {
                animMirror.Init(_bodyAnim, _anim);

                var playerController = body.parent.GetComponent<PlayerCharacterController>();
                playerController.OnJump += OnPlayerJump;
                playerController.OnBecomeGrounded += OnPlayerGrounded;
                playerController.OnBecomeUngrounded += OnPlayerUngrounded;
            }
            else
            {
                animMirror.Init(_anim, _bodyAnim);
            }

            for (var i = 0; i < _anim.parameterCount; i++)
            {
                _netAnim.SetParameterAutoSend(i, true);
            }
        }

        private void OnPlayerJump() => SendTrigger("Jump");

        private void OnPlayerGrounded() => SendTrigger("Grounded");

        private void OnPlayerUngrounded() => SendTrigger("Ungrounded");

        private void SendTrigger(string triggerName)
        {
            var message = new AnimTriggerMessage
            {
                TriggerName = triggerName
            };
            _triggerHandler.SendToServer(message);
        }

        private void OnServerReceiveMessage(AnimTriggerMessage message)
        {
            _triggerHandler.SendToAll(message);
        }

        private void OnClientReceiveMessage(AnimTriggerMessage message)
        {
            if (!isLocalPlayer)
            {
                DebugLog.Instance.Screen(message.TriggerName);
                _bodyAnim.SetTrigger(message.TriggerName);
            }
        }

    }
}
