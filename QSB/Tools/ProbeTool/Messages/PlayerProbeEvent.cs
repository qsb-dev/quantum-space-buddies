using QSB.Events;
using QSB.Messaging;
using QSB.Player;

namespace QSB.Tools.ProbeTool.Messages
{
	internal class PlayerProbeEvent : QSBEvent<EnumMessage<ProbeEvent>>
	{
		public override bool RequireWorldObjectsReady => true;

		public override void SetupListener()
			=> GlobalMessenger<ProbeEvent>.AddListener(EventNames.QSBProbeEvent, Handler);

		public override void CloseListener()
			=> GlobalMessenger<ProbeEvent>.RemoveListener(EventNames.QSBProbeEvent, Handler);

		private void Handler(ProbeEvent probeEvent) => SendEvent(CreateMessage(probeEvent));

		private EnumMessage<ProbeEvent> CreateMessage(ProbeEvent probeEvent) => new()
		{
			AboutId = LocalPlayerId,
			EnumValue = probeEvent
		};

		public override void OnReceiveRemote(bool server, EnumMessage<ProbeEvent> message)
		{
			var player = QSBPlayerManager.GetPlayer(message.AboutId);
			if (!player.IsReady || player.Probe == null)
			{
				return;
			}

			var probe = player.Probe;

			probe.HandleEvent(message.EnumValue);
		}
	}
}
