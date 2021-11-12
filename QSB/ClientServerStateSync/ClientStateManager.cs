using QSB.Events;
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
			QSBCore.UnityEvents.RunWhen(() => PlayerTransformSync.LocalInstance != null, () => QSBEventManager.FireEvent(EventNames.QSBClientState, ForceGetCurrentState()));
		}

		public void ChangeClientState(ClientState newState)
		{
			if (QSBPlayerManager.LocalPlayer.State == newState)
			{
				return;
			}

			DebugLog.DebugWrite($"CHANGE CLIENT STATE FROM {QSBPlayerManager.LocalPlayer.State} to {newState}");
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
						DebugLog.DebugWrite($"SERVER LOAD TITLESCREEN");
						newState = ClientState.InTitleScreen;
						break;
					case OWScene.Credits_Fast:
						DebugLog.DebugWrite($"SERVER LOAD SHORT CREDITS");
						newState = ClientState.WatchingShortCredits;
						break;
					case OWScene.Credits_Final:
					case OWScene.PostCreditsScene:
						DebugLog.DebugWrite($"SERVER LOAD LONG CREDITS");
						newState = ClientState.WatchingLongCredits;
						break;
					case OWScene.SolarSystem:
						if (oldScene == OWScene.SolarSystem)
						{
							// reloading scene
							DebugLog.DebugWrite($"SERVER RELOAD SOLARSYSTEM");
							newState = ClientState.WaitingForOthersToReadyInSolarSystem;
						}
						else
						{
							// loading in from title screen
							DebugLog.DebugWrite($"SERVER LOAD SOLARSYSTEM");
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
						DebugLog.DebugWrite($"CLIENT LOAD TITLESCREEN");
						newState = ClientState.InTitleScreen;
						break;
					case OWScene.Credits_Fast:
						DebugLog.DebugWrite($"CLIENT LOAD SHORT CREDITS");
						newState = ClientState.WatchingShortCredits;
						break;
					case OWScene.Credits_Final:
					case OWScene.PostCreditsScene:
						DebugLog.DebugWrite($"CLIENT LOAD LONG CREDITS");
						newState = ClientState.WatchingLongCredits;
						break;
					case OWScene.SolarSystem:
						if (serverState == ServerState.WaitingForAllPlayersToDie)
						{
							DebugLog.DebugWrite($"SEVER IN DEATH PHASE - WAIT");
							newState = ClientState.WaitingForOthersToReadyInSolarSystem;
							break;
						}

						if (oldScene == OWScene.SolarSystem)
						{
							// reloading scene
							DebugLog.DebugWrite($"CLIENT RELOAD SOLARSYSTEM");
							newState = ClientState.WaitingForOthersToReadyInSolarSystem;
						}
						else
						{
							// loading in from title screen
							DebugLog.DebugWrite($"CLIENT LOAD SOLARSYSTEM");
							if (serverState == ServerState.WaitingForAllPlayersToReady)
							{
								newState = ClientState.WaitingForOthersToReadyInSolarSystem;
							}
							else
							{
								newState = ClientState.AliveInSolarSystem;
							}
						}
						break;
					case OWScene.EyeOfTheUniverse:
						newState = ClientState.WaitingForOthersToReadyInSolarSystem;
						break;
					default:
						newState = ClientState.NotLoaded;
						break;
				}
			}

			QSBEventManager.FireEvent(EventNames.QSBClientState, newState);
		}

		public void OnDeath()
		{
			var currentScene = QSBSceneManager.CurrentScene;
			if (currentScene == OWScene.SolarSystem)
			{
				QSBEventManager.FireEvent(EventNames.QSBClientState, ClientState.DeadInSolarSystem);
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
				QSBEventManager.FireEvent(EventNames.QSBClientState, ClientState.AliveInSolarSystem);
			}
			else
			{
				DebugLog.ToConsole($"Error - Player tried to respawn in scene {currentScene}", OWML.Common.MessageType.Error);
			}
		}

		private ClientState ForceGetCurrentState()
		{
			DebugLog.DebugWrite($"ForceGetCurrentState");
			var currentScene = LoadManager.GetCurrentScene();
			var lastScene = LoadManager.GetPreviousScene();

			switch (currentScene)
			{
				case OWScene.TitleScreen:
					DebugLog.DebugWrite($"- TitleScreen");
					return ClientState.InTitleScreen;
				case OWScene.Credits_Fast:
					DebugLog.DebugWrite($"- Short Credits");
					return ClientState.WatchingShortCredits;
				case OWScene.Credits_Final:
				case OWScene.PostCreditsScene:
					DebugLog.DebugWrite($"- Long Credits");
					return ClientState.WatchingLongCredits;
				case OWScene.SolarSystem:
					DebugLog.DebugWrite($"- SolarSystem");
					return ClientState.AliveInSolarSystem;
				case OWScene.EyeOfTheUniverse:
					DebugLog.DebugWrite($"- Eye");
					return ClientState.AliveInEye;
				default:
					DebugLog.DebugWrite($"- Not Loaded");
					return ClientState.NotLoaded;
			}
		}
	}
}
