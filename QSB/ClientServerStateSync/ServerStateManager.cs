using QSB.ClientServerStateSync.Messages;
using QSB.DeathSync.Messages;
using QSB.Messaging;
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

			QSBCore.UnityEvents.RunWhen(() => PlayerTransformSync.LocalInstance != null,
				() => new ServerStateMessage(ForceGetCurrentState()).Send());
		}

		public void SendChangeServerStateMessage(ServerState newState)
		{
			ChangeServerState(newState);
			new ServerStateMessage(newState).Send();
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
					SendChangeServerStateMessage(ServerState.Credits);
					break;

				case OWScene.TitleScreen:
					SendChangeServerStateMessage(ServerState.NotLoaded);
					break;

				case OWScene.SolarSystem:
					if (oldScene == OWScene.SolarSystem)
					{
						SendChangeServerStateMessage(ServerState.WaitingForAllPlayersToReady);
					}
					else
					{
						SendChangeServerStateMessage(ServerState.InSolarSystem);
					}

					break;

				case OWScene.EyeOfTheUniverse:
					SendChangeServerStateMessage(ServerState.WaitingForAllPlayersToReady);
					break;

				case OWScene.None:
				case OWScene.Undefined:
				default:
					DebugLog.ToConsole($"Warning - newScene is {newScene}!", OWML.Common.MessageType.Warning);
					SendChangeServerStateMessage(ServerState.NotLoaded);
					break;
			}
		}

		private void OnTriggerSupernova()
		{
			if (QSBSceneManager.CurrentScene == OWScene.SolarSystem)
			{
				SendChangeServerStateMessage(ServerState.WaitingForAllPlayersToDie);
			}
		}

		private ServerState ForceGetCurrentState()
		{
			var currentScene = LoadManager.GetCurrentScene();

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

			if (_blockNextCheck)
			{
				_blockNextCheck = false;
				return;
			}

			if (_currentState == ServerState.WaitingForAllPlayersToReady)
			{
				if (QSBPlayerManager.PlayerList.All(x
					=> x.State is ClientState.WaitingForOthersToBeReady
						or ClientState.AliveInSolarSystem
						or ClientState.AliveInEye))
				{
					DebugLog.DebugWrite($"All ready!!");
					new StartLoopMessage().Send();
					if (QSBSceneManager.CurrentScene == OWScene.SolarSystem)
					{
						SendChangeServerStateMessage(ServerState.InSolarSystem);
					}
					else if (QSBSceneManager.CurrentScene == OWScene.EyeOfTheUniverse)
					{
						SendChangeServerStateMessage(ServerState.InEye);
					}
					else
					{
						DebugLog.ToConsole($"Error - All players were ready in non-universe scene!?", OWML.Common.MessageType.Error);
						SendChangeServerStateMessage(ServerState.NotLoaded);
					}

					_blockNextCheck = true;
				}
			}
		}
	}
}