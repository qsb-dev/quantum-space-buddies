using Mirror;
using OWML.Common;
using QSB.Animation.Player.Messages;
using QSB.Animation.Player.Thrusters;
using QSB.Messaging;
using QSB.Player;
using QSB.Utility;
using System.Linq;
using UnityEngine;

namespace QSB.Animation.Player
{

	public class AnimationSync : PlayerSyncObject
	{
		private RuntimeAnimatorController _suitedAnimController;
		private AnimatorOverrideController _unsuitedAnimController;
		private GameObject _suitedGraphics;
		private GameObject _unsuitedGraphics;
		private PlayerCharacterController _playerController;
		private CrouchSync _crouchSync;

		private RuntimeAnimatorController _chertController;
		//private readonly RuntimeAnimatorController _eskerController;
		//private readonly RuntimeAnimatorController _feldsparController;
		//private readonly RuntimeAnimatorController _gabbroController;
		private RuntimeAnimatorController _riebeckController;

		public AnimatorMirror Mirror { get; private set; }
		public AnimationType CurrentType { get; set; }
		public Animator VisibleAnimator { get; private set; }
		public Animator InvisibleAnimator { get; private set; }
		public NetworkAnimator NetworkAnimator { get; private set; }

		protected void Awake()
		{
			InvisibleAnimator = gameObject.GetRequiredComponent<Animator>();
			NetworkAnimator = gameObject.GetRequiredComponent<NetworkAnimator>();
			NetworkAnimator.enabled = false;

			QSBSceneManager.OnUniverseSceneLoaded += OnUniverseSceneLoaded;
		}

		protected void OnDestroy()
		{
			Destroy(InvisibleAnimator);
			Destroy(NetworkAnimator);
			QSBSceneManager.OnUniverseSceneLoaded -= OnUniverseSceneLoaded;
		}

		private void OnUniverseSceneLoaded(OWScene oldScene, OWScene newScene) => LoadControllers();

		private void LoadControllers()
		{
			var bundle = QSBCore.InstrumentAssetBundle;
			_chertController = bundle.LoadAsset("assets/Chert/Traveller_Chert.controller") as RuntimeAnimatorController;
			_riebeckController = bundle.LoadAsset("assets/Riebeck/Traveller_Riebeck.controller") as RuntimeAnimatorController;
		}

		private void InitCommon(Transform body)
		{
			if (QSBSceneManager.IsInUniverse)
			{
				LoadControllers();
			}

			NetworkAnimator.enabled = true;
			VisibleAnimator = body.GetComponent<Animator>();
			Mirror = body.gameObject.AddComponent<AnimatorMirror>();
			if (isLocalPlayer)
			{
				Mirror.Init(VisibleAnimator, InvisibleAnimator);
			}
			else
			{
				Mirror.Init(InvisibleAnimator, VisibleAnimator);
			}

			// for (var i = 0; i < InvisibleAnimator.parameterCount; i++)
			// {
			// 	NetworkAnimator.SetParameterAutoSend(i, true);
			// }

			var playerAnimController = body.GetComponent<PlayerAnimController>();
			_suitedAnimController = playerAnimController._baseAnimController;
			_unsuitedAnimController = playerAnimController._unsuitedAnimOverride;
			_suitedGraphics = playerAnimController._suitedGroup;
			_unsuitedGraphics = playerAnimController._unsuitedGroup;
		}

		public void InitLocal(Transform body)
		{
			InitCommon(body);

			_playerController = body.parent.GetComponent<PlayerCharacterController>();

			InitCrouchSync();
			InitAccelerationSync();
		}

		public void InitRemote(Transform body)
		{
			InitCommon(body);

			var playerAnimController = body.GetComponent<PlayerAnimController>();
			playerAnimController.enabled = false;

			playerAnimController._suitedGroup = new GameObject();
			playerAnimController._unsuitedGroup = new GameObject();
			playerAnimController._baseAnimController = null;
			playerAnimController._unsuitedAnimOverride = null;
			playerAnimController._rightArmHidden = false;

			var rightArmObjects = playerAnimController._rightArmObjects.ToList();
			rightArmObjects.ForEach(rightArmObject => rightArmObject.layer = LayerMask.NameToLayer("Default"));

			body.Find("player_mesh_noSuit:Traveller_HEA_Player/player_mesh_noSuit:Player_Head").gameObject.layer = 0;
			body.Find("Traveller_Mesh_v01:Traveller_Geo/Traveller_Mesh_v01:PlayerSuit_Helmet").gameObject.layer = 0;

			SetAnimationType(AnimationType.PlayerUnsuited);

			InitCrouchSync();
			InitAccelerationSync();
			ThrusterManager.CreateRemotePlayerVFX(Player);

			var ikSync = body.gameObject.AddComponent<PlayerHeadRotationSync>();
			QSBCore.UnityEvents.RunWhen(() => Player.CameraBody != null, () => ikSync.Init(Player.CameraBody.transform));
		}

		private void InitAccelerationSync()
		{
			Player.JetpackAcceleration = GetComponent<JetpackAccelerationSync>();
			var thrusterModel = hasAuthority ? Locator.GetPlayerBody().GetComponent<ThrusterModel>() : null;
			Player.JetpackAcceleration.Init(thrusterModel);
		}

		private void InitCrouchSync()
		{
			_crouchSync = GetComponent<CrouchSync>();
			_crouchSync.Init(_playerController, VisibleAnimator);
		}

		private void SuitUp()
		{
			new ChangeAnimTypeMessage(PlayerId, AnimationType.PlayerSuited).Send();
			SetAnimationType(AnimationType.PlayerSuited);
		}

		private void SuitDown()
		{
			new ChangeAnimTypeMessage(PlayerId, AnimationType.PlayerUnsuited).Send();
			SetAnimationType(AnimationType.PlayerUnsuited);
		}

		public void SetSuitState(bool state)
		{
			if (!Player.IsReady)
			{
				return;
			}

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

			CurrentType = type;
			if (_unsuitedAnimController == null)
			{
				DebugLog.ToConsole($"Error - Unsuited controller is null. ({PlayerId})", MessageType.Error);
			}

			if (_suitedAnimController == null)
			{
				DebugLog.ToConsole($"Error - Suited controller is null. ({PlayerId})", MessageType.Error);
			}

			if (_unsuitedGraphics == null)
			{
				DebugLog.ToConsole($"Warning - _unsuitedGraphics is null! ({PlayerId})", MessageType.Warning);
			}

			if (_suitedGraphics == null)
			{
				DebugLog.ToConsole($"Warning - _suitedGraphics is null! ({PlayerId})", MessageType.Warning);
			}

			RuntimeAnimatorController controller = default;
			switch (type)
			{
				case AnimationType.PlayerSuited:
					controller = _suitedAnimController;
					if (_unsuitedGraphics != null)
					{
						_unsuitedGraphics?.SetActive(false);
					}

					if (_suitedGraphics != null)
					{
						_suitedGraphics?.SetActive(true);
					}

					break;

				case AnimationType.PlayerUnsuited:
					controller = _unsuitedAnimController;
					if (_unsuitedGraphics != null)
					{
						_unsuitedGraphics?.SetActive(true);
					}

					if (_suitedGraphics != null)
					{
						_suitedGraphics?.SetActive(false);
					}

					break;

				case AnimationType.Chert:
					controller = _chertController;
					break;

				case AnimationType.Esker:
					//controller = _eskerController;
					break;

				case AnimationType.Feldspar:
					//controller = _feldsparController;
					break;

				case AnimationType.Gabbro:
					//controller = _gabbroController;
					break;

				case AnimationType.Riebeck:
					controller = _riebeckController;
					break;
			}

			if (InvisibleAnimator == null)
			{
				DebugLog.ToConsole($"Error - InvisibleAnimator is null. ({PlayerId})", MessageType.Error);
			}
			else
			{
				InvisibleAnimator.runtimeAnimatorController = controller;
			}

			if (VisibleAnimator == null)
			{
				DebugLog.ToConsole($"Error - VisibleAnimator is null. ({PlayerId})", MessageType.Error);
			}
			else
			{
				VisibleAnimator.runtimeAnimatorController = controller;
			}

			if (type is not AnimationType.PlayerSuited and not AnimationType.PlayerUnsuited)
			{
				if (VisibleAnimator != null)
				{
					VisibleAnimator.SetTrigger("Playing");
				}

				if (InvisibleAnimator != null)
				{
					InvisibleAnimator.SetTrigger("Playing");
				}
			}
			else
			{
				// Avoids "jumping" when exiting instrument and putting on suit
				if (VisibleAnimator != null)
				{
					VisibleAnimator.SetTrigger("Grounded");
				}

				if (InvisibleAnimator != null)
				{
					InvisibleAnimator.SetTrigger("Grounded");
				}
			}

			if (NetworkAnimator == null)
			{
				DebugLog.ToConsole($"Error - NetworkAnimator is null. ({PlayerId})", MessageType.Error);
			}
			else if (Mirror == null)
			{
				DebugLog.ToConsole($"Error - Mirror is null. ({PlayerId})", MessageType.Error);
			}
			else if (InvisibleAnimator != null)
			{
				NetworkAnimator.animator = InvisibleAnimator; // Probably not needed.
				Mirror.RebuildFloatParams();
				// for (var i = 0; i < InvisibleAnimator.parameterCount; i++)
				// {
				// 	NetworkAnimator.SetParameterAutoSend(i, true);
				// }
			}
		}
	}
}