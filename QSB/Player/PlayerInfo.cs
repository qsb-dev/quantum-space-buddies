using OWML.Common;
using QSB.Animation.Player;
using QSB.Animation.Player.Thrusters;
using QSB.CampfireSync.WorldObjects;
using QSB.ClientServerStateSync;
using QSB.Events;
using QSB.ItemSync.WorldObjects.Items;
using QSB.Player.TransformSync;
using QSB.QuantumSync;
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
		public uint PlayerId { get; }
		public string Name { get; set; }
		public PlayerHUDMarker HudMarker { get; set; }
		public PlayerTransformSync TransformSync { get; set; }

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
		public bool Visible { get; set; } = true;

		// Tools
		public GameObject ProbeBody { get; set; }
		public QSBProbe Probe { get; set; }
		public QSBFlashlight FlashLight 
		{
			get
			{
				if (CameraBody == null)
				{
					return null;
				}

				return CameraBody.GetComponentInChildren<QSBFlashlight>();
			}
		}
		public QSBTool Signalscope => GetToolByType(ToolType.Signalscope);
		public QSBTool Translator => GetToolByType(ToolType.Translator);
		public QSBProbeLauncherTool ProbeLauncher => (QSBProbeLauncherTool)GetToolByType(ToolType.ProbeLauncher);
		public Transform ItemSocket => CameraBody.transform.Find("REMOTE_ItemSocket");
		public Transform ScrollSocket => CameraBody.transform.Find("REMOTE_ScrollSocket");
		public Transform SharedStoneSocket => CameraBody.transform.Find("REMOTE_SharedStoneSocket");
		public Transform WarpCoreSocket => CameraBody.transform.Find("REMOTE_WarpCoreSocket");
		public Transform VesselCoreSocket => CameraBody.transform.Find("REMOTE_VesselCoreSocket");
		public Transform SimpleLanternSocket => CameraBody.transform.Find("REMOTE_SimpleLanternSocket");
		public Transform DreamLanternSocket => CameraBody.transform.Find("REMOTE_DreamLanternSocket");
		public Transform SlideReelSocket => CameraBody.transform.Find("REMOTE_SlideReelSocket");
		public Transform VisionTorchSocket => CameraBody.transform.Find("REMOTE_VisionTorchSocket");
		public QSBMarshmallow Marshmallow { get; set; }
		public QSBCampfire Campfire { get; set; }
		public IQSBOWItem HeldItem { get; set; }

		// Conversation
		public int CurrentCharacterDialogueTreeId { get; set; }
		public GameObject CurrentDialogueBox { get; set; }

		// Animation
		public AnimationSync AnimationSync => QSBPlayerManager.GetSyncObject<AnimationSync>(PlayerId);
		public bool PlayingInstrument => AnimationSync.CurrentType is not AnimationType.PlayerSuited
			and not AnimationType.PlayerUnsuited;
		public JetpackAccelerationSync JetpackAcceleration { get; set; }

		// Misc
		public bool IsReady { get; set; }
		public bool IsInMoon; // MOVE : move into PlayerStates?
		public bool IsInShrine; // MOVE : move into PlayerStates?
		public IQSBQuantumObject EntangledObject;
		public bool IsDead { get; set; }
		public ClientState State { get; set; }
		public bool FlashlightActive { get; set; }
		public bool SuitedUp { get; set; }
		public bool ProbeLauncherEquipped { get; set; }
		public bool SignalscopeEquipped { get; set; }
		public bool TranslatorEquipped { get; set; }
		public bool ProbeActive { get; set; }

		// Local only
		public PlayerProbeLauncher LocalProbeLauncher
		{
			get
			{
				if (QSBPlayerManager.LocalPlayer != this)
				{
					DebugLog.ToConsole($"Warning - Tried to access local-only property LocalProbeLauncher in PlayerInfo for non local player!", MessageType.Warning);
					return null;
				}

				return CameraBody.transform.Find("ProbeLauncher").GetComponent<PlayerProbeLauncher>();
			}
		}

		public Flashlight LocalFlashlight
		{
			get
			{
				if (QSBPlayerManager.LocalPlayer != this)
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
				if (QSBPlayerManager.LocalPlayer != this)
				{
					DebugLog.ToConsole($"Warning - Tried to access local-only property LocalSignalscope in PlayerInfo for non local player!", MessageType.Warning);
					return null;
				}

				return CameraBody.transform.Find("Signalscope").GetComponent<Signalscope>();
			}
		}

		public NomaiTranslator LocalTranslator
		{
			get
			{
				if (QSBPlayerManager.LocalPlayer != this)
				{
					DebugLog.ToConsole($"Warning - Tried to access local-only property LocalTranslator in PlayerInfo for non local player!", MessageType.Warning);
					return null;
				}

				return CameraBody.transform.Find("NomaiTranslatorProp").GetComponent<NomaiTranslator>();
			}
		}

		public PlayerInfo(uint id)
		{
			PlayerId = id;
			CurrentCharacterDialogueTreeId = -1;
		}

		public void UpdateObjectsFromStates()
		{
			if (OWInput.GetInputMode() == InputMode.None)
			{
				return;
			}

			FlashLight?.UpdateState(FlashlightActive);
			Translator?.ChangeEquipState(TranslatorEquipped);
			ProbeLauncher?.ChangeEquipState(ProbeLauncherEquipped);
			Signalscope?.ChangeEquipState(SignalscopeEquipped);
			QSBCore.UnityEvents.RunWhen(() => QSBPlayerManager.GetSyncObject<AnimationSync>(PlayerId) != null,
				() => QSBPlayerManager.GetSyncObject<AnimationSync>(PlayerId).SetSuitState(SuitedUp));
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
				SuitedUp = Locator.GetPlayerBody().GetComponent<PlayerSpacesuit>().IsWearingSuit(true);
			}

			QSBEventManager.FireEvent(EventNames.QSBPlayerInformation);
		}

		private QSBTool GetToolByType(ToolType type) => CameraBody?.GetComponentsInChildren<QSBTool>()
				.FirstOrDefault(x => x.Type == type);
	}
}