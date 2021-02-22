using QSB.Animation;
using QSB.Player.Markers;
using QSB.Player.Tools;
using QSB.QuantumSync;
using QSB.Utility;
using System.Linq;
using UnityEngine;

namespace QSB.Player
{
	public class PlayerInfo
	{
		public uint PlayerId { get; }
		public string Name { get; set; }
		public bool IsReady { get; set; }
		public PlayerHUDMarker HudMarker { get; set; }
		public State State { get; set; } // TODO : decide if this is worth it (instead of having seperate variables for each thing)

		// Body Objects
		public OWCamera Camera { get; set; }
		public GameObject CameraBody { get; set; }
		public GameObject Body { get; set; }

		// Tools
		public GameObject ProbeBody { get; set; }
		public QSBProbe Probe { get; set; }
		public QSBFlashlight FlashLight => CameraBody?.GetComponentInChildren<QSBFlashlight>();
		public QSBTool Signalscope => GetToolByType(ToolType.Signalscope);
		public QSBTool Translator => GetToolByType(ToolType.Translator);
		public QSBTool ProbeLauncher => GetToolByType(ToolType.ProbeLauncher);

		// Conversation
		public int CurrentDialogueID { get; set; }
		public GameObject CurrentDialogueBox { get; set; }

		// Animation
		public AnimationSync AnimationSync => PlayerManager.GetSyncObject<AnimationSync>(PlayerId);
		public bool PlayingInstrument => AnimationSync.CurrentType != AnimationType.PlayerSuited
			&& AnimationSync.CurrentType != AnimationType.PlayerUnsuited;

		// Misc
		public bool IsInMoon;
		public bool IsInShrine;
		public IQSBQuantumObject EntangledObject;

		public PlayerInfo(uint id)
		{
			PlayerId = id;
			CurrentDialogueID = -1;
		}

		public void UpdateState(State state, bool value)
		{
			var states = State;
			if (value)
			{
				FlagsHelper.Set(ref states, state);
			}
			else
			{
				FlagsHelper.Unset(ref states, state);
			}
			State = states;
		}

		public void UpdateStateObjects()
		{
			if (OWInput.GetInputMode() == InputMode.None)
			{
				return;
			}
			FlashLight?.UpdateState(FlagsHelper.IsSet(State, State.Flashlight));
			Translator?.ChangeEquipState(FlagsHelper.IsSet(State, State.Translator));
			ProbeLauncher?.ChangeEquipState(FlagsHelper.IsSet(State, State.ProbeLauncher));
			Signalscope?.ChangeEquipState(FlagsHelper.IsSet(State, State.Signalscope));
			QSBCore.Helper.Events.Unity.RunWhen(() => PlayerManager.GetSyncObject<AnimationSync>(PlayerId) != null,
				() => PlayerManager.GetSyncObject<AnimationSync>(PlayerId).SetSuitState(FlagsHelper.IsSet(State, State.Suit)));
		}

		public bool GetState(State state)
			=> FlagsHelper.IsSet(State, state);

		private QSBTool GetToolByType(ToolType type)
		{
			return CameraBody?.GetComponentsInChildren<QSBTool>()
				.FirstOrDefault(x => x.Type == type);
		}
	}
}