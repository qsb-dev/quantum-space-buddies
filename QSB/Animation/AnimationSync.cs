using OWML.ModHelper.Events;
using QSB.Events;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace QSB.Animation
{
    public class AnimationSync : PlayerSyncObject
    {
        protected override uint PlayerIdOffset => 0;

        private Animator _anim;
        private Animator _bodyAnim;
        private NetworkAnimator _netAnim;

        private RuntimeAnimatorController _suitedAnimController;
        private AnimatorOverrideController _unsuitedAnimController;
        private GameObject _suitedGraphics;
        private GameObject _unsuitedGraphics;
        private PlayerCharacterController _playerController;
        private CrouchSync _crouchSync;

        private void Awake()
        {
            _anim = gameObject.AddComponent<Animator>();
            _netAnim = gameObject.AddComponent<NetworkAnimator>();
            _netAnim.enabled = false;
            _netAnim.animator = _anim;
        }

        private void OnDestroy()
        {
            _netAnim.enabled = false;
            if (_playerController == null)
            {
                return;
            }
            _playerController.OnJump -= OnJump;
            _playerController.OnBecomeGrounded -= OnBecomeGrounded;
            _playerController.OnBecomeUngrounded -= OnBecomeUngrounded;
            GlobalMessenger.RemoveListener(EventNames.SuitUp, OnSuitUp);
            GlobalMessenger.RemoveListener(EventNames.RemoveSuit, OnSuitDown);
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

            PlayerRegistry.PlayerSyncObjects.Add(this);

            for (var i = 0; i < _anim.parameterCount; i++)
            {
                _netAnim.SetParameterAutoSend(i, true);
            }
        }

        public void InitLocal(Transform body)
        {
            InitCommon(body);

            _playerController = body.parent.GetComponent<PlayerCharacterController>();
            _playerController.OnJump += OnJump;
            _playerController.OnBecomeGrounded += OnBecomeGrounded;
            _playerController.OnBecomeUngrounded += OnBecomeUngrounded;

            GlobalMessenger.AddListener(EventNames.SuitUp, OnSuitUp);
            GlobalMessenger.AddListener(EventNames.RemoveSuit, OnSuitDown);

            InitCrouchSync();
        }

        public void InitRemote(Transform body)
        {
            InitCommon(body);

            var playerAnimController = body.GetComponent<PlayerAnimController>();
            playerAnimController.enabled = false;

            _suitedAnimController = AnimControllerPatch.SuitedAnimController;
            _unsuitedAnimController = playerAnimController.GetValue<AnimatorOverrideController>("_unsuitedAnimOverride");
            _suitedGraphics = playerAnimController.GetValue<GameObject>("_suitedGroup");
            _unsuitedGraphics = playerAnimController.GetValue<GameObject>("_unsuitedGroup");

            playerAnimController.SetValue("_suitedGroup", new GameObject());
            playerAnimController.SetValue("_unsuitedGroup", new GameObject());
            playerAnimController.SetValue("_baseAnimController", null);
            playerAnimController.SetValue("_unsuitedAnimOverride", null);
            playerAnimController.SetValue("_rightArmHidden", false);

            var rightArmObjects = playerAnimController.GetValue<GameObject[]>("_rightArmObjects").ToList();
            rightArmObjects.ForEach(rightArmObject => rightArmObject.layer = LayerMask.NameToLayer("Default"));

            body.Find("player_mesh_noSuit:Traveller_HEA_Player/player_mesh_noSuit:Player_Head").gameObject.layer = 0;
            body.Find("Traveller_Mesh_v01:Traveller_Geo/Traveller_Mesh_v01:PlayerSuit_Helmet").gameObject.layer = 0;

            InitCrouchSync();
        }

        private void InitCrouchSync()
        {
            _crouchSync = gameObject.AddComponent<CrouchSync>();
            _crouchSync.Init(this, _playerController, _bodyAnim);
        }

        private void OnJump() => SendTrigger(AnimTrigger.Jump);
        private void OnBecomeGrounded() => SendTrigger(AnimTrigger.Grounded);
        private void OnBecomeUngrounded() => SendTrigger(AnimTrigger.Ungrounded);

        private void OnSuitUp() => SendTrigger(AnimTrigger.SuitUp);
        private void OnSuitDown() => SendTrigger(AnimTrigger.SuitDown);

        public void SendTrigger(AnimTrigger trigger, float value = 0)
        {
            GlobalMessenger<short, float>.FireEvent(EventNames.QSBAnimTrigger, (short)trigger, value);
        }

        public void HandleTrigger(AnimTrigger trigger, float value)
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
                    _crouchSync.CrouchParam.Target = value;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(trigger), trigger, null);
            }
        }

        private void SuitUp()
        {
            _bodyAnim.runtimeAnimatorController = _suitedAnimController;
            _anim.runtimeAnimatorController = _suitedAnimController;
            _unsuitedGraphics.SetActive(false);
            _suitedGraphics.SetActive(true);
        }

        private void SuitDown()
        {
            _bodyAnim.runtimeAnimatorController = _unsuitedAnimController;
            _anim.runtimeAnimatorController = _unsuitedAnimController;
            _unsuitedGraphics.SetActive(true);
            _suitedGraphics.SetActive(false);
        }

        public void SetSuitState(bool state)
        {
            if (state)
            {
                SuitUp();
                return;
            }
            SuitDown();
        }

    }
}
