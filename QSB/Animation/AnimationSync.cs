using System.Collections.Generic;
using QSB.Messaging;
using UnityEngine;
using UnityEngine.Networking;

namespace QSB.Animation
{
    public class AnimationSync : NetworkBehaviour
    {
        public Animator BodyAnim { get; private set; }

        private static readonly Dictionary<uint, AnimationSync> _playerAnimationSyncs = new Dictionary<uint, AnimationSync>();

        private Animator _anim;
        private NetworkAnimator _netAnim;
        private MessageHandler<AnimTriggerMessage> _triggerHandler;

        private void Awake()
        {
            _anim = gameObject.AddComponent<Animator>();
            _netAnim = gameObject.AddComponent<NetworkAnimator>();
            _netAnim.animator = _anim;
        }

        public void Init(Transform body)
        {
            BodyAnim = body.GetComponent<Animator>();
            var animMirror = body.gameObject.AddComponent<AnimatorMirror>();

            _playerAnimationSyncs.Add(netId.Value, this);

            if (isLocalPlayer)
            {
                animMirror.Init(BodyAnim, _anim);

                _triggerHandler = new MessageHandler<AnimTriggerMessage>();
                _triggerHandler.OnServerReceiveMessage += OnServerReceiveMessage;
                _triggerHandler.OnClientReceiveMessage += OnClientReceiveMessage;

                var playerController = body.parent.GetComponent<PlayerCharacterController>();
                playerController.OnJump += OnPlayerJump;
                playerController.OnBecomeGrounded += OnPlayerGrounded;
                playerController.OnBecomeUngrounded += OnPlayerUngrounded;
            }
            else
            {
                animMirror.Init(_anim, BodyAnim);
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
                SenderId = netId.Value,
                TriggerName = triggerName
            };
            DebugLog.Instance.Screen($"Sending trigger to server: " + message.TriggerName);
            _triggerHandler.SendToServer(message);
        }

        private void OnServerReceiveMessage(AnimTriggerMessage message)
        {
            DebugLog.Instance.Screen("Server received trigger: " + message.TriggerName);
            _triggerHandler.SendToAll(message);
        }

        private void OnClientReceiveMessage(AnimTriggerMessage message)
        {
            var animSync = _playerAnimationSyncs[message.SenderId];
            if (animSync != this)
            {
                DebugLog.Instance.Screen($"Client received trigger: {message.TriggerName}. SenderId: {message.SenderId} is NOT local");
                animSync.BodyAnim.SetTrigger(message.TriggerName);
            }
        }

    }
}
