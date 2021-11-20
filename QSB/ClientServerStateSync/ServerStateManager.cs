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
		private bool _blockNextCheck;

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
					DebugLog.DebugWrite($"SERVER LOAD CREDITS");
					QSBEventManager.FireEvent(EventNames.QSBServerState, ServerState.Credits);
					break;

				case OWScene.TitleScreen:
					DebugLog.DebugWrite($"SERVER LOAD TITLE SCREEN");
					QSBEventManager.FireEvent(EventNames.QSBServerState, ServerState.NotLoaded);
					break;

				case OWScene.SolarSystem:
					DebugLog.DebugWrite($"SERVER LOAD SOLARSYSTEM");
					if (oldScene == OWScene.SolarSystem)
					{
						QSBEventManager.FireEvent(EventNames.QSBServerState, ServerState.WaitingForAllPlayersToReady);
					}
					else
					{
						QSBEventManager.FireEvent(EventNames.QSBServerState, ServerState.InSolarSystem);
					}

					break;

				case OWScene.EyeOfTheUniverse:
					DebugLog.DebugWrite($"EYE");
					QSBEventManager.FireEvent(EventNames.QSBServerState, ServerState.WaitingForAllPlayersToReady);
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
			QSBEventManager.FireEvent(EventNames.QSBServerState, ServerState.WaitingForAllPlayersToDie);
		}

		private ServerState ForceGetCurrentState()
		{
			DebugLog.DebugWrite($"ForceGetCurrentState");

			var currentScene = LoadManager.GetCurrentScene();
			var lastScene = LoadManager.GetPreviousScene();

			switch (currentScene)
			{
				case OWScene.SolarSystem:
					DebugLog.DebugWrite($"- SolarSystem");
					return ServerState.InSolarSystem;
				case OWScene.EyeOfTheUniverse:
					DebugLog.DebugWrite($"- Eye");
					return ServerState.InEye;
				default:
					DebugLog.DebugWrite($"- Not Loaded");
					return ServerState.NotLoaded;
			}
		}

		private void Update()
		{
			if (!QSBCore.IsHost)
			{
				return;
			}

			if (_blockNextCheck)
			{
				_blockNextCheck = false;
				return;
			}

			if (_currentState == ServerState.WaitingForAllPlayersToReady)
			{
				if (QSBPlayerManager.PlayerList.All(x
					=> x.State is ClientState.WaitingForOthersToReadyInSolarSystem
					or ClientState.AliveInSolarSystem
					or ClientState.AliveInEye))
				{
					DebugLog.DebugWrite($"All ready!!");
					QSBEventManager.FireEvent(EventNames.QSBStartLoop);
					if (QSBSceneManager.CurrentScene == OWScene.SolarSystem)
					{
						QSBEventManager.FireEvent(EventNames.QSBServerState, ServerState.InSolarSystem);
					}
					else if (QSBSceneManager.CurrentScene == OWScene.EyeOfTheUniverse)
					{
						QSBEventManager.FireEvent(EventNames.QSBServerState, ServerState.InEye);
					}
					else
					{
						DebugLog.ToConsole($"Error - All players were ready in non-universe scene!?", OWML.Common.MessageType.Error);
						QSBEventManager.FireEvent(EventNames.QSBServerState, ServerState.NotLoaded);
					}
					
					_blockNextCheck = true;
				}
			}
		}
	}
}
