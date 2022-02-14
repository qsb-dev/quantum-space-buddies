using OWML.Common;
using QSB.Animation.Player;
using QSB.Animation.Player.Thrusters;
using QSB.Audio;
using QSB.CampfireSync.WorldObjects;
using QSB.ClientServerStateSync;
using QSB.ItemSync.WorldObjects.Items;
using QSB.Messaging;
using QSB.Player.Messages;
using QSB.Player.TransformSync;
using QSB.QuantumSync.WorldObjects;
using QSB.RoastingSync;
using QSB.Tools;
using QSB.Tools.FlashlightTool;
using QSB.Tools.ProbeLauncherTool;
using QSB.Tools.ProbeTool;
using QSB.Utility;
using System.Linq;
using UnityEngine;

namespace QSB.Player
{
	public class PlayerInfo
	{
		/// <summary>
		/// the player transform sync's net id
		/// </summary>
		public uint PlayerId { get; }
		public string Name { get; set; }
		public PlayerHUDMarker HudMarker { get; set; }
		public PlayerTransformSync TransformSync { get; }
		public AnimationSync AnimationSync { get; }
		public ClientState State { get; set; }
		public EyeState EyeState { get; set; }
		public bool IsDead { get; set; }
		public bool Visible => IsLocalPlayer || !_ditheringAnimator || _ditheringAnimator._visible;
		public bool IsReady { get; set; }
		public bool IsInMoon { get; set; }
		public bool IsInShrine { get; set; }
		public bool IsInEyeShuttle { get; set; }
		public IQSBQuantumObject EntangledObject { get; set; }
		public QSBPlayerAudioController AudioController { get; set; }
		internal DitheringAnimator _ditheringAnimator;

		public bool IsLocalPlayer => TransformSync.isLocalPlayer;

		// Body Objects
		public OWCamera Camera
		{
			get
			{
				if (_camera == null && IsReady)
				{
					DebugLog.ToConsole($"Warning - {PlayerId}.Camera is null!", MessageType.Warning);
				}

				return _camera;
			}
			set
			{
				if (value == null)
				{
					DebugLog.ToConsole($"Warning - Setting {PlayerId}.Camera to null.", MessageType.Warning);
				}

				_camera = value;
			}
		}
		private OWCamera _camera;

		public GameObject CameraBody { get; set; }

		public GameObject Body
		{
			get
			{
				if (_body == null && IsReady)
				{
					DebugLog.ToConsole($"Warning - {PlayerId}.Body is null!", MessageType.Warning);
				}

				return _body;
			}
			set
			{
				if (value == null)
				{
					DebugLog.ToConsole($"Warning - Setting {PlayerId}.Body to null.", MessageType.Warning);
				}

				_body = value;
			}
		}
		private GameObject _body;

		public GameObject RoastingStick { get; set; }

		// Tools
		public GameObject ProbeBody { get; set; }
		public QSBProbe Probe { get; set; }
		public QSBFlashlight FlashLight => CameraBody == null ? null : CameraBody.GetComponentInChildren<QSBFlashlight>();
		public QSBTool Signalscope => GetToolByType(ToolType.Signalscope);
		public QSBTool Translator => GetToolByType(ToolType.Translator);
		public QSBProbeLauncherTool ProbeLauncher => (QSBProbeLauncherTool)GetToolByType(ToolType.ProbeLauncher);
		private Transform _handPivot;
		public Transform HandPivot
		{
			get
			{
				if (_handPivot == null)
				{
					_handPivot = Body.transform.Find("REMOTE_Traveller_HEA_Player_v2/" +
						"Traveller_Rig_v01:Traveller_Trajectory_Jnt/" +
						"Traveller_Rig_v01:Traveller_ROOT_Jnt/" +
						"Traveller_Rig_v01:Traveller_Spine_01_Jnt/" +
						"Traveller_Rig_v01:Traveller_Spine_02_Jnt/" +
						"Traveller_Rig_v01:Traveller_Spine_Top_Jnt/" +
						"Traveller_Rig_v01:Traveller_RT_Arm_Clavicle_Jnt/" +
						"Traveller_Rig_v01:Traveller_RT_Arm_Shoulder_Jnt/" +
						"Traveller_Rig_v01:Traveller_RT_Arm_Elbow_Jnt/" +
						"Traveller_Rig_v01:Traveller_RT_Arm_Wrist_Jnt/" +
						"REMOTE_ItemCarryTool/" +
						"HandPivot"); // TODO : kill me for my sins
				}

				return _handPivot;
			}
		}
		public Transform ItemSocket => HandPivot.Find("REMOTE_ItemSocket");
		public Transform ScrollSocket => HandPivot.Find("REMOTE_ScrollSocket");
		public Transform SharedStoneSocket => HandPivot.Find("REMOTE_SharedStoneSocket");
		public Transform WarpCoreSocket => HandPivot.Find("REMOTE_WarpCoreSocket");
		public Transform VesselCoreSocket => HandPivot.Find("REMOTE_VesselCoreSocket");
		public Transform SimpleLanternSocket => HandPivot.Find("REMOTE_SimpleLanternSocket");
		public Transform DreamLanternSocket => HandPivot.Find("REMOTE_DreamLanternSocket");
		public Transform SlideReelSocket => HandPivot.Find("REMOTE_SlideReelSocket");
		public Transform VisionTorchSocket => HandPivot.Find("REMOTE_VisionTorchSocket");
		public QSBMarshmallow Marshmallow { get; set; }
		public QSBCampfire Campfire { get; set; }
		public IQSBItem HeldItem { get; set; }
		public bool FlashlightActive { get; set; }
		public bool SuitedUp { get; set; }
		public bool ProbeLauncherEquipped { get; set; }
		public bool SignalscopeEquipped { get; set; }
		public bool TranslatorEquipped { get; set; }
		public bool ProbeActive { get; set; }

		// Conversation
		public int CurrentCharacterDialogueTreeId { get; set; } = -1;
		public GameObject CurrentDialogueBox { get; set; }

		// Animation
		public JetpackAccelerationSync JetpackAcceleration { get; set; }

		// Local only
		public PlayerProbeLauncher LocalProbeLauncher
		{
			get
			{
				if (!IsLocalPlayer)
				{
					DebugLog.ToConsole($"Warning - Tried to access local-only property LocalProbeLauncher in PlayerInfo for non local player!", MessageType.Warning);
					return null;
				}

				return CameraBody?.transform.Find("ProbeLauncher").GetComponent<PlayerProbeLauncher>();
			}
		}

		public Flashlight LocalFlashlight
		{
			get
			{
				if (!IsLocalPlayer)
				{
					DebugLog.ToConsole($"Warning - Tried to access local-only property LocalFlashlight in PlayerInfo for non local player!", MessageType.Warning);
					return null;
				}

				return Locator.GetFlashlight();
			}
		}

		public Signalscope LocalSignalscope
		{
			get
			{
				if (!IsLocalPlayer)
				{
					DebugLog.ToConsole($"Warning - Tried to access local-only property LocalSignalscope in PlayerInfo for non local player!", MessageType.Warning);
					return null;
				}

				return CameraBody?.transform.Find("Signalscope").GetComponent<Signalscope>();
			}
		}

		public NomaiTranslator LocalTranslator
		{
			get
			{
				if (!IsLocalPlayer)
				{
					DebugLog.ToConsole($"Warning - Tried to access local-only property LocalTranslator in PlayerInfo for non local player!", MessageType.Warning);
					return null;
				}

				return CameraBody?.transform.Find("NomaiTranslatorProp").GetComponent<NomaiTranslator>();
			}
		}

		public PlayerInfo(PlayerTransformSync transformSync)
		{
			PlayerId = transformSync.netId;
			TransformSync = transformSync;
			AnimationSync = transformSync.GetComponent<AnimationSync>();
		}

		public void UpdateObjectsFromStates()
		{
			if (OWInput.GetInputMode() == InputMode.None)
			{
				// ? why is this here lmao
				return;
			}

			if (CameraBody == null)
			{
				return;
			}

			FlashLight?.UpdateState(FlashlightActive);
			Translator?.ChangeEquipState(TranslatorEquipped);
			ProbeLauncher?.ChangeEquipState(ProbeLauncherEquipped);
			Signalscope?.ChangeEquipState(SignalscopeEquipped);
			AnimationSync.SetSuitState(SuitedUp);
		}

		public void UpdateStatesFromObjects()
		{
			if (Locator.GetFlashlight() == null || Locator.GetPlayerBody() == null)
			{
				FlashlightActive = false;
				SuitedUp = false;
			}
			else
			{
				FlashlightActive = Locator.GetFlashlight()._flashlightOn;
				SuitedUp = Locator.GetPlayerBody().GetComponent<PlayerSpacesuit>().IsWearingSuit();
			}

			new PlayerInformationMessage().Send();
		}

		private QSBTool GetToolByType(ToolType type)
		{
			if (CameraBody == null)
			{
				DebugLog.ToConsole($"Warning - Tried to GetToolByType({type}) on player {PlayerId}, but CameraBody was null.", MessageType.Warning);
				return null;
			}

			var tools = CameraBody.GetComponentsInChildren<QSBTool>();

			if (tools == null || tools.Length == 0)
			{
				DebugLog.ToConsole($"Warning - Couldn't find any QSBTools for player {PlayerId}.", MessageType.Warning);
				return null;
			}

			var tool = tools.FirstOrDefault(x => x.Type == type);

			if (tool == null)
			{
				DebugLog.ToConsole($"Warning - No tool found on player {PlayerId} matching ToolType {type}.", MessageType.Warning);
			}

			return tool;
		}

		public void SetVisible(bool visible, float seconds = 0)
		{
			if (IsLocalPlayer)
			{
				return;
			}

			if (!_ditheringAnimator)
			{
				DebugLog.ToConsole($"Warning - {PlayerId}.DitheringAnimator is null!", MessageType.Warning);
				return;
			}

			if (seconds == 0)
			{
				_ditheringAnimator.SetVisibleImmediate(visible);
			}
			else
			{
				_ditheringAnimator.SetVisible(visible, 1 / seconds);
			}
		}
	}
}
