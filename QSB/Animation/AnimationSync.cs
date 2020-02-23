using System.Collections.Generic;
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

        private static readonly Dictionary<uint, AnimationSync> _playerAnimationSyncs = new Dictionary<uint, AnimationSync>();

        private void Awake()
        {
            _anim = gameObject.AddComponent<Animator>();
            _netAnim = gameObject.AddComponent<NetworkAnimator>();
            _netAnim.animator = _anim;
        }

        public void Init(Transform body)
        {
            _bodyAnim = body.GetComponent<Animator>();
            var animMirror = body.gameObject.AddComponent<AnimatorMirror>();

            _playerAnimationSyncs.Add(netId.Value, this);

            if (isLocalPlayer)
            {
                animMirror.Init(_bodyAnim, _anim);

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
                SenderId = netId.Value,
                TriggerName = triggerName
            };
            DebugLog.Instance.Screen($"Sending trigger from SenderId={netId.Value} to server: " + message.TriggerName);
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
                _bodyAnim.SetTrigger(message.TriggerName);
            }
            else
            {
                DebugLog.Instance.Screen($"Client received trigger: {message.TriggerName}. SenderId: {message.SenderId} is local");
            }
        }

    }
}
