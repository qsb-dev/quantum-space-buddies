using QSB.Events;
using QSB.Messaging;
using QSB.Player;

namespace QSB.Tools.Events
{
	public class PlayerProbeEvent : QSBEvent<ToggleMessage>
	{
		public override EventType Type => EventType.ProbeActiveChange;

		public override void SetupListener()
		{
			GlobalMessenger<SurveyorProbe>.AddListener(EventNames.LaunchProbe, HandleLaunch);
			GlobalMessenger<SurveyorProbe>.AddListener(EventNames.RetrieveProbe, HandleRetrieve);
		}

		public override void CloseListener()
		{
			GlobalMessenger<SurveyorProbe>.RemoveListener(EventNames.LaunchProbe, HandleLaunch);
			GlobalMessenger<SurveyorProbe>.RemoveListener(EventNames.RetrieveProbe, HandleRetrieve);
		}

		private void HandleLaunch(SurveyorProbe probe) => SendEvent(CreateMessage(true));
		private void HandleRetrieve(SurveyorProbe probe) => SendEvent(CreateMessage(false));

		private ToggleMessage CreateMessage(bool value) => new ToggleMessage
		{
			AboutId = LocalPlayerId,
			ToggleValue = value
		};

		public override void OnReceiveRemote(bool server, ToggleMessage message)
		{
			var player = PlayerManager.GetPlayer(message.AboutId);
			player.UpdateState(State.ProbeActive, message.ToggleValue);
			player.Probe?.SetState(message.ToggleValue);
		}

		public override void OnReceiveLocal(bool server, ToggleMessage message) =>
			PlayerManager.LocalPlayer.UpdateState(State.ProbeActive, message.ToggleValue);
	}
}