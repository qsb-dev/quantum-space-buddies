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

			if (QSBCore.IsHost)
			{
				if (newScene == OWScene.SolarSystem && oldScene != OWScene.SolarSystem)
				{
					DebugLog.DebugWrite($"Server is loading SolarSystem just after creating server.");
					QSBEventManager.FireEvent(EventNames.QSBClientState, ClientState.AliveInSolarSystem);
				}

				if (newScene == OWScene.SolarSystem && oldScene == OWScene.SolarSystem)
				{
					DebugLog.DebugWrite($"Server is reloading SolarSystem");
					QSBEventManager.FireEvent(EventNames.QSBClientState, ClientState.WaitingForOthersToReadyInSolarSystem);
				}

				if (newScene == OWScene.TitleScreen)
				{
					DebugLog.DebugWrite($"Server has gone back to title screen");
					QSBEventManager.FireEvent(EventNames.QSBClientState, ClientState.InTitleScreen);
				}
			}
			else
			{
				if (newScene == OWScene.SolarSystem && oldScene != OWScene.SolarSystem && serverState != ServerState.AwaitingPlayConfirmation)
				{
					DebugLog.DebugWrite($"Client is loading SolarSystem just after connecting.");
					QSBEventManager.FireEvent(EventNames.QSBClientState, ClientState.AliveInSolarSystem);
				}

				if (newScene == OWScene.SolarSystem && oldScene == OWScene.SolarSystem)
				{
					DebugLog.DebugWrite($"Client is reloading SolarSystem");
					QSBEventManager.FireEvent(EventNames.QSBClientState, ClientState.WaitingForOthersToReadyInSolarSystem);
				}

				if (serverState == ServerState.WaitingForDeath)
				{
					DebugLog.DebugWrite($"Client loaded new scene while server is waiting for all players to die");
					QSBEventManager.FireEvent(EventNames.QSBClientState, ClientState.WaitingForOthersToReadyInSolarSystem);
				}
			}
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

		private ClientState ForceGetCurrentState()
		{
			var currentScene = LoadManager.GetCurrentScene();
			var lastScene = LoadManager.GetPreviousScene();

			if (currentScene == OWScene.TitleScreen || currentScene == OWScene.Credits_Fast || currentScene == OWScene.Credits_Final)
			{
				return ClientState.InTitleScreen;
			}

			// cant join while dead...

			if (currentScene == OWScene.SolarSystem)
			{
				return ClientState.AliveInSolarSystem;
			}

			if (currentScene == OWScene.EyeOfTheUniverse)
			{
				return ClientState.AliveInEye;
			}

			return ClientState.NotLoaded;
		}
	}
}
