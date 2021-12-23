using QSB.ClientServerStateSync;
using QSB.Messaging;
using QSB.Utility;

namespace QSB.DeathSync.Events
{
	internal class StartLoopMessage : QSBMessage
	{
		public override void OnReceiveLocal() => OnReceiveRemote();

		public override void OnReceiveRemote()
		{
			DebugLog.DebugWrite($" ~~~ LOOP START ~~~");
			if (QSBSceneManager.CurrentScene == OWScene.SolarSystem)
			{
				ClientStateManager.Instance.SendChangeClientStateMessage(ClientState.AliveInSolarSystem);
			}
			else if (QSBSceneManager.CurrentScene == OWScene.EyeOfTheUniverse)
			{
				ClientStateManager.Instance.SendChangeClientStateMessage(ClientState.AliveInEye);
			}
			else
			{
				DebugLog.ToConsole($"Error - Got StartLoop event when not in universe!", OWML.Common.MessageType.Error);
				ClientStateManager.Instance.SendChangeClientStateMessage(ClientState.NotLoaded);
			}
		}
	}
}