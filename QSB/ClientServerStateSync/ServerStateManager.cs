using QSB.Events;
using QSB.Player;
using QSB.Player.TransformSync;
using QSB.Utility;
using System.Linq;
using UnityEngine;

namespace QSB.ClientServerStateSync
{
	internal class ServerStateManager : MonoBehaviour
	{
		public static ServerStateManager Instance { get; private set; }

		public event ChangeStateEvent OnChangeState;
		public delegate void ChangeStateEvent(ServerState newState);

		private ServerState _currentState;

		private void Awake() 
			=> Instance = this;

		private void Start()
		{
			if (!QSBCore.IsHost)
			{
				return;
			}
			QSBSceneManager.OnSceneLoaded += OnSceneLoaded;
			GlobalMessenger.AddListener("TriggerSupernova", OnTriggerSupernova);

			QSBCore.UnityEvents.RunWhen(() => PlayerTransformSync.LocalInstance != null, () => QSBEventManager.FireEvent(EventNames.QSBServerState, ForceGetCurrentState()));
		}

		public void ChangeServerState(ServerState newState)
		{
			if (_currentState == newState)
			{
				return;
			}
			DebugLog.DebugWrite($"CHANGE SERVER STATE FROM {_currentState} to {newState}");
			_currentState = newState;
			OnChangeState?.Invoke(newState);
		}

		public ServerState GetServerState() 
			=> _currentState;

		private void OnSceneLoaded(OWScene oldScene, OWScene newScene, bool inUniverse)
		{
			switch (newScene)
			{
				case OWScene.Credits_Fast:
				case OWScene.Credits_Final:
				case OWScene.PostCreditsScene:
					QSBEventManager.FireEvent(EventNames.QSBServerState, ServerState.Credits);
					break;

				case OWScene.TitleScreen:
					QSBEventManager.FireEvent(EventNames.QSBServerState, ServerState.NotLoaded);
					break;

				case OWScene.SolarSystem:
					if (oldScene == OWScene.SolarSystem)
					{
						QSBEventManager.FireEvent(EventNames.QSBServerState, ServerState.AwaitingPlayConfirmation);
					}
					else
					{
						QSBEventManager.FireEvent(EventNames.QSBServerState, ServerState.InSolarSystem);
					}
					break;

				case OWScene.EyeOfTheUniverse:
					QSBEventManager.FireEvent(EventNames.QSBServerState, ServerState.AwaitingPlayConfirmation);
					break;

				case OWScene.None:
				case OWScene.Undefined:
				default:
					DebugLog.ToConsole($"Warning - newScene is {newScene}!", OWML.Common.MessageType.Warning);
					QSBEventManager.FireEvent(EventNames.QSBServerState, ServerState.NotLoaded);
					break;
			}
		}

		private void OnTriggerSupernova()
		{
			DebugLog.DebugWrite($"TriggerSupernova");
			QSBEventManager.FireEvent(EventNames.QSBServerState, ServerState.WaitingForDeath);
		}

		private ServerState ForceGetCurrentState()
		{
			var currentScene = LoadManager.GetCurrentScene();
			var lastScene = LoadManager.GetPreviousScene();

			switch (currentScene)
			{
				case OWScene.SolarSystem:
					return ServerState.InSolarSystem;
				case OWScene.EyeOfTheUniverse:
					return ServerState.InEye;
				default:
					return ServerState.NotLoaded;
			}
		}

		private void Update()
		{
			if (!QSBCore.IsHost)
			{
				return;
			}

			if (_currentState == ServerState.AwaitingPlayConfirmation)
			{
				if (QSBPlayerManager.PlayerList.All(x => x.State == ClientState.WaitingForOthersToReadyInSolarSystem))
				{
					DebugLog.DebugWrite($"All ready!!");
					QSBEventManager.FireEvent(EventNames.QSBStartLoop);
					QSBEventManager.FireEvent(EventNames.QSBServerState, ServerState.InSolarSystem);
				}
			}
		}
	}
}
