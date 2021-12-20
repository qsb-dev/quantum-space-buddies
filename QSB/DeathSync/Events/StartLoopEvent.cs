using QSB.ClientServerStateSync;
using QSB.Events;
using QSB.Messaging;
using QSB.Utility;

namespace QSB.DeathSync.Events
{
	internal class StartLoopEvent : QSBEvent<PlayerMessage>
	{
		public override bool RequireWorldObjectsReady => false;

		public override void SetupListener() => GlobalMessenger.AddListener(EventNames.QSBStartLoop, Handler);
		public override void CloseListener() => GlobalMessenger.RemoveListener(EventNames.QSBStartLoop, Handler);

		private void Handler() => SendEvent(CreateMessage());

		private PlayerMessage CreateMessage() => new()
		{
			AboutId = LocalPlayerId
		};

		public override void OnReceiveLocal(bool server, PlayerMessage message)
			=> OnReceiveRemote(server, message);

		public override void OnReceiveRemote(bool server, PlayerMessage message)
		{
			DebugLog.DebugWrite($" ~~~ LOOP START ~~~");
			if (QSBSceneManager.CurrentScene == OWScene.SolarSystem)
			{
				ClientStateManager.Instance.FireChangeClientStateEvent(ClientState.AliveInSolarSystem);
			}
			else if (QSBSceneManager.CurrentScene == OWScene.EyeOfTheUniverse)
			{
				ClientStateManager.Instance.FireChangeClientStateEvent(ClientState.AliveInEye);
			}
			else
			{
				DebugLog.ToConsole($"Error - Got StartLoop event when not in universe!", OWML.Common.MessageType.Error);
				ClientStateManager.Instance.FireChangeClientStateEvent(ClientState.NotLoaded);
			}
		}
	}
}
