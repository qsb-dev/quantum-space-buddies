using System;
using OWML.ModHelper.Events;
using QSB.Events;
using QSB.Messaging;
using UnityEngine;
using UnityEngine.Networking;

namespace QSB.Animation
{
    public class AnimationSync : NetworkBehaviour
    {
        private const float CrouchSendInterval = 0.1f;
        private const float CrouchChargeThreshold = 0.01f;
        private const float CrouchSmoothTime = 0.05f;
        private const int CrouchLayerIndex = 1;

        private Animator _anim;
        private Animator _bodyAnim;
        private NetworkAnimator _netAnim;
        private MessageHandler<AnimTriggerMessage> _triggerHandler;

        private RuntimeAnimatorController _suitedAnimController;
        private AnimatorOverrideController _unsuitedAnimController;
        private GameObject _suitedGraphics;
        private GameObject _unsuitedGraphics;
        private PlayerCharacterController _playerController;

        private readonly AnimFloatParam _crouchParam = new AnimFloatParam();
        private float _sendTimer;
        private float _lastSentJumpChargeFraction;

        private void Awake()
        {
            _anim = gameObject.AddComponent<Animator>();
            _netAnim = gameObject.AddComponent<NetworkAnimator>();
            _netAnim.enabled = false;
            _netAnim.animator = _anim;
        }

        private void InitCommon(Transform body)
        {
            _netAnim.enabled = true;
            _bodyAnim = body.GetComponent<Animator>();
            var mirror = body.gameObject.AddComponent<AnimatorMirror>();
            if (isLocalPlayer)
            {
                mirror.Init(_bodyAnim, _anim);
            }
            else
            {
                mirror.Init(_anim, _bodyAnim);
            }

            PlayerRegistry.AnimationSyncs.Add(this);

            for (var i = 0; i < _anim.parameterCount; i++)
            {
                _netAnim.SetParameterAutoSend(i, true);
            }
        }

        public void InitLocal(Transform body)
        {
            InitCommon(body);

            _triggerHandler = new MessageHandler<AnimTriggerMessage>(MessageType.AnimTrigger);
            _triggerHandler.OnServerReceiveMessage += OnServerReceiveMessage;
            _triggerHandler.OnClientReceiveMessage += OnClientReceiveMessage;

            _playerController = body.parent.GetComponent<PlayerCharacterController>();
            _playerController.OnJump += OnJump;
            _playerController.OnBecomeGrounded += OnBecomeGrounded;
            _playerController.OnBecomeUngrounded += OnBecomeUngrounded;

            GlobalMessenger.AddListener(EventNames.SuitUp, OnSuitUp);
            GlobalMessenger.AddListener(EventNames.RemoveSuit, OnSuitDown);
        }

        public void InitRemote(Transform body)
        {
            InitCommon(body);

            var playerAnimController = body.GetComponent<PlayerAnimController>();
            playerAnimController.enabled = false;

            _suitedAnimController = AnimControllerHack.SuitedAnimController;
            _unsuitedAnimController = playerAnimController.GetValue<AnimatorOverrideController>("_unsuitedAnimOverride");
            _suitedGraphics = playerAnimController.GetValue<GameObject>("_suitedGroup");
            _unsuitedGraphics = playerAnimController.GetValue<GameObject>("_unsuitedGroup");

            playerAnimController.SetValue("_suitedGroup", new GameObject());
            playerAnimController.SetValue("_unsuitedGroup", new GameObject());
            playerAnimController.SetValue("_baseAnimController", null);
            playerAnimController.SetValue("_unsuitedAnimOverride", null);

            body.Find("player_mesh_noSuit:Traveller_HEA_Player/player_mesh_noSuit:Player_Head").gameObject.layer = 0;
            body.Find("Traveller_Mesh_v01:Traveller_Geo/Traveller_Mesh_v01:PlayerSuit_Helmet").gameObject.layer = 0;
        }

        private void OnJump() => SendTrigger(AnimTrigger.Jump);
        private void OnBecomeGrounded() => SendTrigger(AnimTrigger.Grounded);
        private void OnBecomeUngrounded() => SendTrigger(AnimTrigger.Ungrounded);

        private void OnSuitUp() => SendTrigger(AnimTrigger.SuitUp);
        private void OnSuitDown() => SendTrigger(AnimTrigger.SuitDown);

        public void Reset()
        {
            if (_playerController == null)
            {
                return;
            }
            _netAnim.enabled = false;
            _playerController.OnJump -= OnJump;
            _playerController.OnBecomeGrounded -= OnBecomeGrounded;
            _playerController.OnBecomeUngrounded -= OnBecomeUngrounded;
            GlobalMessenger.RemoveListener("SuitUp", OnSuitUp);
            GlobalMessenger.RemoveListener("RemoveSuit", OnSuitDown);
        }

        private void SendTrigger(AnimTrigger trigger, float value = 0)
        {
            var message = new AnimTriggerMessage
            {
                SenderId = netId.Value,
                TriggerId = (short)trigger,
                Value = value
            };
            if (isServer)
            {
                _triggerHandler.SendToAll(message);
            }
            else
            {
                _triggerHandler.SendToServer(message);
            }
        }

        private void OnServerReceiveMessage(AnimTriggerMessage message)
        {
            _triggerHandler.SendToAll(message);
        }

        private void OnClientReceiveMessage(AnimTriggerMessage message)
        {
            var animationSync = PlayerRegistry.GetAnimationSync(message.SenderId);
            if (animationSync == null || animationSync == this)
            {
                return;
            }
            animationSync.HandleTrigger((AnimTrigger)message.TriggerId, message.Value);
        }

        private void HandleTrigger(AnimTrigger trigger, float value)
        {
            switch (trigger)
            {
                case AnimTrigger.Jump:
                case AnimTrigger.Grounded:
                case AnimTrigger.Ungrounded:
                    _bodyAnim.SetTrigger(trigger.ToString());
                    break;
                case AnimTrigger.SuitUp:
                    SuitUp();
                    break;
                case AnimTrigger.SuitDown:
                    SuitDown();
                    break;
                case AnimTrigger.Crouch:
                    _crouchParam.Target = value;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(trigger), trigger, null);
            }
        }

        public void SuitUp()
        {
            _bodyAnim.runtimeAnimatorController = _suitedAnimController;
            _anim.runtimeAnimatorController = _suitedAnimController;
            _unsuitedGraphics.SetActive(false);
            _suitedGraphics.SetActive(true);
        }

        public void SuitDown()
        {
            _bodyAnim.runtimeAnimatorController = _unsuitedAnimController;
            _anim.runtimeAnimatorController = _unsuitedAnimController;
            _unsuitedGraphics.SetActive(true);
            _suitedGraphics.SetActive(false);
        }

        private void Update()
        {
            if (isLocalPlayer)
            {
                SyncLocalCrouch();
            }
            else
            {
                SyncRemoteCrouch();
            }
        }

        private void SyncLocalCrouch()
        {
            if (_playerController == null)
            {
                return;
            }
            _sendTimer += Time.unscaledDeltaTime;
            if (_sendTimer < CrouchSendInterval)
            {
                return;
            }
            var jumpChargeFraction = _playerController.GetJumpChargeFraction();
            if (Math.Abs(jumpChargeFraction - _lastSentJumpChargeFraction) < CrouchChargeThreshold)
            {
                return;
            }
            SendTrigger(AnimTrigger.Crouch, jumpChargeFraction);
            _lastSentJumpChargeFraction = jumpChargeFraction;
            _sendTimer = 0;
        }

        private void SyncRemoteCrouch()
        {
            if (_bodyAnim == null)
            {
                return;
            }
            _crouchParam.Smooth(CrouchSmoothTime);
            var jumpChargeFraction = _crouchParam.Current;
            _bodyAnim.SetLayerWeight(CrouchLayerIndex, jumpChargeFraction);
        }

    }
}
