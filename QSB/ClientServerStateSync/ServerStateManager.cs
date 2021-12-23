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

		public void FireChangeServerStateEvent(ServerState newState)
		{
			ChangeServerState(newState);
			QSBEventManager.FireEvent(EventNames.QSBServerState, newState);
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
					FireChangeServerStateEvent(ServerState.Credits);
					break;

				case OWScene.TitleScreen:
					FireChangeServerStateEvent(ServerState.NotLoaded);
					break;

				case OWScene.SolarSystem:
					if (oldScene == OWScene.SolarSystem)
					{
						FireChangeServerStateEvent(ServerState.WaitingForAllPlayersToReady);
					}
					else
					{
						FireChangeServerStateEvent(ServerState.InSolarSystem);
					}

					break;

				case OWScene.EyeOfTheUniverse:
					FireChangeServerStateEvent(ServerState.WaitingForAllPlayersToReady);
					break;

				case OWScene.None:
				case OWScene.Undefined:
				default:
					DebugLog.ToConsole($"Warning - newScene is {newScene}!", OWML.Common.MessageType.Warning);
					FireChangeServerStateEvent(ServerState.NotLoaded);
					break;
			}
		}

		private void OnTriggerSupernova()
		{
			if (QSBSceneManager.CurrentScene == OWScene.SolarSystem)
			{
				FireChangeServerStateEvent(ServerState.WaitingForAllPlayersToDie);
			}
		}

		private static ServerState ForceGetCurrentState()
			=> QSBSceneManager.CurrentScene switch
			{
				OWScene.SolarSystem => ServerState.InSolarSystem,
				OWScene.EyeOfTheUniverse => ServerState.InEye,
				_ => ServerState.NotLoaded
			};

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
					QSBEventManager.FireEvent(EventNames.QSBStartLoop);
					if (QSBSceneManager.CurrentScene == OWScene.SolarSystem)
					{
						FireChangeServerStateEvent(ServerState.InSolarSystem);
					}
					else if (QSBSceneManager.CurrentScene == OWScene.EyeOfTheUniverse)
					{
						FireChangeServerStateEvent(ServerState.InEye);
					}
					else
					{
						DebugLog.ToConsole($"Error - All players were ready in non-universe scene!?", OWML.Common.MessageType.Error);
						FireChangeServerStateEvent(ServerState.NotLoaded);
					}

					_blockNextCheck = true;
				}
			}
		}
	}
}