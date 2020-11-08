using OWML.ModHelper.Events;
using QSB.EventsCore;
using QSB.Player;
using QSB.Utility;
using System;
using System.Linq;
using UnityEngine;

namespace QSB.Animation
{
    public class AnimationSync : PlayerSyncObject
    {
        private Animator _anim;
        private Animator _bodyAnim;
        private QSBNetworkAnimator _netAnim;
        private AnimatorMirror _mirror;

        private RuntimeAnimatorController _suitedAnimController;
        private AnimatorOverrideController _unsuitedAnimController;
        private GameObject _suitedGraphics;
        private GameObject _unsuitedGraphics;
        private PlayerCharacterController _playerController;
        private CrouchSync _crouchSync;

        private RuntimeAnimatorController _riebeckController;
        private RuntimeAnimatorController _chertController;
        private RuntimeAnimatorController _gabbroController;
        private RuntimeAnimatorController _feldsparController;

        public AnimationType CurrentType = AnimationType.PlayerUnsuited;

        private void Awake()
        {
            _anim = gameObject.AddComponent<Animator>();
            _netAnim = gameObject.AddComponent<QSBNetworkAnimator>();
            _netAnim.enabled = false;
            _netAnim.animator = _anim;

            QSBSceneManager.OnSceneLoaded += OnSceneLoaded;
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

            QSBSceneManager.OnSceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(OWScene scene, bool inUniverse)
        {
            var reibeckRoot = GameObject.Find("Traveller_HEA_Riebeck_ANIM_Talking");
            _riebeckController = reibeckRoot.GetComponent<Animator>().runtimeAnimatorController;
            var chertRoot = GameObject.Find("Traveller_HEA_Chert_ANIM_Chatter_Chipper");
            _chertController = chertRoot.GetComponent<Animator>().runtimeAnimatorController;
            var gabbroRoot = GameObject.Find("Traveller_HEA_Gabbro_ANIM_IdleFlute");
            _gabbroController = gabbroRoot.GetComponent<Animator>().runtimeAnimatorController;
            var feldsparRoot = GameObject.Find("Traveller_HEA_Feldspar_ANIM_Talking");
            _feldsparController = feldsparRoot.GetComponent<Animator>().runtimeAnimatorController;
        }

        private void InitCommon(Transform body)
        {
            _netAnim.enabled = true;
            _bodyAnim = body.GetComponent<Animator>();
            _mirror = body.gameObject.AddComponent<AnimatorMirror>();
            if (isLocalPlayer)
            {
                _mirror.Init(_bodyAnim, _anim);
            }
            else
            {
                _mirror.Init(_anim, _bodyAnim);
            }

            QSBPlayerManager.PlayerSyncObjects.Add(this);

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

        private void OnJump() => _netAnim.SetTrigger("Jump");
        private void OnBecomeGrounded() => _netAnim.SetTrigger("Grounded");
        private void OnBecomeUngrounded() => _netAnim.SetTrigger("Ungrounded");

        public void SendCrouch(float value = 0)
        {
            GlobalMessenger<float>.FireEvent(EventNames.QSBCrouch, value);
        }

        public void HandleCrouch(float value)
        {
            _crouchSync.CrouchParam.Target = value;
        }

        private void SuitUp()
        {
            SetAnimationType(AnimationType.PlayerSuited);
            _unsuitedGraphics.SetActive(false);
            _suitedGraphics.SetActive(true);
        }

        private void SuitDown()
        {
            SetAnimationType(AnimationType.PlayerUnsuited);
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

        public void SetAnimationType(AnimationType type)
        {
            if (CurrentType == type)
            {
                return;
            }
            DebugLog.DebugWrite($"{_bodyAnim.name} Changing animtype to {Enum.GetName(typeof(AnimationType), type)}");
            GlobalMessenger<AnimationType>.FireEvent(EventNames.QSBChangeAnimType, type);
            CurrentType = type;
            _netAnim.enabled = false;
            _netAnim.animator = null;
            switch (type)
            {
                case AnimationType.PlayerSuited:
                    _bodyAnim.runtimeAnimatorController = _suitedAnimController;
                    _anim.runtimeAnimatorController = _suitedAnimController;
                    break;
                case AnimationType.PlayerUnsuited:
                    _bodyAnim.runtimeAnimatorController = _unsuitedAnimController;
                    _anim.runtimeAnimatorController = _unsuitedAnimController;
                    break;
                case AnimationType.Chert:
                    _bodyAnim.runtimeAnimatorController = _chertController;
                    _bodyAnim.SetTrigger("Playing");
                    _anim.runtimeAnimatorController = _chertController;
                    _anim.SetTrigger("Playing");
                    break;
            }
            _mirror.RebuildFloatParams();
            _netAnim.animator = _anim;
            _netAnim.enabled = true;
            for (var i = 0; i < _anim.parameterCount; i++)
            {
                _netAnim.SetParameterAutoSend(i, true);
            }
        }
    }
}
