using QSB.ClientServerStateSync.Messages;
using QSB.Messaging;
using QSB.Player;
using QSB.Player.TransformSync;
using QSB.RichPresence;
using QSB.Utility;
using UnityEngine;

namespace QSB.ClientServerStateSync;

internal class ClientStateManager : MonoBehaviour
{
	public static ClientStateManager Instance { get; private set; }

	public delegate void ChangeStateEvent(ClientState newState);

	private void Awake()
		=> Instance = this;

	private void Start()
	{
		QSBSceneManager.OnPostSceneLoad += OnPostSceneLoad;
		Delay.RunWhen(() => PlayerTransformSync.LocalInstance != null,
			() => new ClientStateMessage(ForceGetCurrentState()).Send());
	}

	private void OnDestroy() =>
		QSBSceneManager.OnPostSceneLoad -= OnPostSceneLoad;

	public void ChangeClientState(ClientState newState)
	{
		if (PlayerTransformSync.LocalInstance == null || QSBPlayerManager.LocalPlayer.State == newState)
		{
			return;
		}

		QSBPlayerManager.LocalPlayer.State = newState;
		ActivityManager.Instance.OnChangeClientState(newState);
	}

	private static void OnPostSceneLoad(OWScene oldScene, OWScene newScene)
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
					if (oldScene == OWScene.SolarSystem)
					{
						// coming from solar system
						newState = ClientState.WaitingForOthersToBeReady;
					}
					else
					{
						// loading in from title screen
						newState = ClientState.AliveInEye;
					}

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
					if (oldScene == OWScene.SolarSystem)
					{
						// coming from solar system
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
							newState = ClientState.AliveInEye;
						}
					}

					break;
				default:
					newState = ClientState.NotLoaded;
					break;
			}
		}

		new ClientStateMessage(newState).Send();
	}

	public void OnDeath()
	{
		var currentScene = QSBSceneManager.CurrentScene;
		if (currentScene == OWScene.SolarSystem)
		{
			new ClientStateMessage(ClientState.DeadInSolarSystem).Send();
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
			new ClientStateMessage(ClientState.AliveInSolarSystem).Send();
		}
		else
		{
			DebugLog.ToConsole($"Error - Player tried to respawn in scene {currentScene}", OWML.Common.MessageType.Error);
		}
	}

	private static ClientState ForceGetCurrentState()
		=> QSBSceneManager.CurrentScene switch
		{
			OWScene.TitleScreen => ClientState.InTitleScreen,
			OWScene.Credits_Fast => ClientState.WatchingShortCredits,
			OWScene.Credits_Final or OWScene.PostCreditsScene => ClientState.WatchingLongCredits,
			OWScene.SolarSystem => ClientState.AliveInSolarSystem,
			OWScene.EyeOfTheUniverse => ClientState.AliveInEye,
			_ => ClientState.NotLoaded
		};
}
