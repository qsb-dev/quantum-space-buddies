using QSB.ClientServerStateSync.Events;
using QSB.Messaging;
using QSB.Player;
using QSB.Player.TransformSync;
using QSB.Utility;
using UnityEngine;

namespace QSB.ClientServerStateSync
{
	internal class ClientStateManager : MonoBehaviour
	{
		public static ClientStateManager Instance { get; private set; }

		public event ChangeStateEvent OnChangeState;
		public delegate void ChangeStateEvent(ClientState newState);

		private void Awake()
			=> Instance = this;

		private void Start()
		{
			QSBSceneManager.OnSceneLoaded += OnSceneLoaded;
			QSBCore.UnityEvents.RunWhen(() => PlayerTransformSync.LocalInstance != null,
				() => new ClientStateMessage(ForceGetCurrentState()).Send());
		}

		public void FireChangeClientStateEvent(ClientState newState)
		{
			ChangeClientState(newState);
			new ClientStateMessage(newState).Send();
		}

		public void ChangeClientState(ClientState newState)
		{
			if (PlayerTransformSync.LocalInstance == null || QSBPlayerManager.LocalPlayer.State == newState)
			{
				return;
			}

			QSBPlayerManager.LocalPlayer.State = newState;
			OnChangeState?.Invoke(newState);
		}

		private void OnSceneLoaded(OWScene oldScene, OWScene newScene, bool inUniverse)
		{
			var serverState = ServerStateManager.Instance.GetServerState();

			ClientState newState;

			if (QSBCore.IsHost)
			{

				switch (newScene)
				{
					case OWScene.TitleScreen:
						newState = ClientState.InTitleScreen;
						break;
					case OWScene.Credits_Fast:
						newState = ClientState.WatchingShortCredits;
						break;
					case OWScene.Credits_Final:
					case OWScene.PostCreditsScene:
						newState = ClientState.WatchingLongCredits;
						break;
					case OWScene.SolarSystem:
						if (oldScene == OWScene.SolarSystem)
						{
							// reloading scene
							newState = ClientState.WaitingForOthersToBeReady;
						}
						else
						{
							// loading in from title screen
							newState = ClientState.AliveInSolarSystem;
						}

						break;
					case OWScene.EyeOfTheUniverse:
						newState = ClientState.AliveInEye;
						break;
					default:
						newState = ClientState.NotLoaded;
						break;
				}
			}
			else
			{
				switch (newScene)
				{
					case OWScene.TitleScreen:
						newState = ClientState.InTitleScreen;
						break;
					case OWScene.Credits_Fast:
						newState = ClientState.WatchingShortCredits;
						break;
					case OWScene.Credits_Final:
					case OWScene.PostCreditsScene:
						newState = ClientState.WatchingLongCredits;
						break;
					case OWScene.SolarSystem:
						if (serverState == ServerState.WaitingForAllPlayersToDie)
						{
							newState = ClientState.WaitingForOthersToBeReady;
							break;
						}

						if (oldScene == OWScene.SolarSystem)
						{
							// reloading scene
							newState = ClientState.WaitingForOthersToBeReady;
						}
						else
						{
							// loading in from title screen
							if (serverState == ServerState.WaitingForAllPlayersToReady)
							{
								newState = ClientState.WaitingForOthersToBeReady;
							}
							else
							{
								newState = ClientState.AliveInSolarSystem;
							}
						}

						break;
					case OWScene.EyeOfTheUniverse:
						if (serverState == ServerState.WaitingForAllPlayersToReady)
						{
							newState = ClientState.WaitingForOthersToBeReady;
						}
						else
						{
							newState = ClientState.AliveInEye;
						}
						break;
					default:
						newState = ClientState.NotLoaded;
						break;
				}
			}

			FireChangeClientStateEvent(newState);
		}

		public void OnDeath()
		{
			var currentScene = QSBSceneManager.CurrentScene;
			if (currentScene == OWScene.SolarSystem)
			{
				FireChangeClientStateEvent(ClientState.DeadInSolarSystem);
			}
			else if (currentScene == OWScene.EyeOfTheUniverse)
			{
				DebugLog.ToConsole($"Error - You died in the Eye? HOW DID YOU DO THAT?!", OWML.Common.MessageType.Error);
			}
			else
			{
				// whaaaaaaaaa
				DebugLog.ToConsole($"Error - You died... in a menu? In the credits? In any case, you should never see this. :P", OWML.Common.MessageType.Error);
			}
		}

		public void OnRespawn()
		{
			var currentScene = QSBSceneManager.CurrentScene;
			if (currentScene == OWScene.SolarSystem)
			{
				DebugLog.DebugWrite($"RESPAWN!");
				FireChangeClientStateEvent(ClientState.AliveInSolarSystem);
			}
			else
			{
				DebugLog.ToConsole($"Error - Player tried to respawn in scene {currentScene}", OWML.Common.MessageType.Error);
			}
		}

		private ClientState ForceGetCurrentState()
		{
			var currentScene = LoadManager.GetCurrentScene();

			return currentScene switch
			{
				OWScene.TitleScreen => ClientState.InTitleScreen,
				OWScene.Credits_Fast => ClientState.WatchingShortCredits,
				OWScene.Credits_Final or OWScene.PostCreditsScene => ClientState.WatchingLongCredits,
				OWScene.SolarSystem => ClientState.AliveInSolarSystem,
				OWScene.EyeOfTheUniverse => ClientState.AliveInEye,
				_ => ClientState.NotLoaded,
			};
		}
	}
}
