using QSB.Events;
using QSB.Messaging;
using QSB.Player;
using QSB.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QSB.ProbeSync.Events
{
	class PlayerProbeEvent : QSBEvent<EnumMessage<ProbeEvent>>
	{
		public override EventType Type => EventType.ProbeEvent;

		public override void SetupListener()
		{
			GlobalMessenger<ProbeEvent>.AddListener(EventNames.QSBProbeEvent, Handler);
		}

		public override void CloseListener()
		{
			GlobalMessenger<ProbeEvent>.RemoveListener(EventNames.QSBProbeEvent, Handler);
		}

		private void Handler(ProbeEvent probeEvent) => SendEvent(CreateMessage(probeEvent));

		private EnumMessage<ProbeEvent> CreateMessage(ProbeEvent probeEvent) => new EnumMessage<ProbeEvent>
		{
			AboutId = LocalPlayerId,
			EnumValue = probeEvent
		};

		public override void OnReceiveRemote(bool server, EnumMessage<ProbeEvent> message)
		{
			DebugLog.DebugWrite($"recieve probe event type:{message.EnumValue} from:{message.AboutId}");

			var player = QSBPlayerManager.GetPlayer(message.AboutId);
			var probe = player.Probe;

			switch (message.EnumValue)
			{
				case ProbeEvent.Anchor:
				case ProbeEvent.Unanchor:
				case ProbeEvent.Launch:
					player.PlayerStates.ProbeActive = true;
					probe.SetState(true);
					break;
				case ProbeEvent.Destroy:
				case ProbeEvent.Retrieve:
					player.PlayerStates.ProbeActive = false;
					probe.SetState(false);
					break;
			}

			probe.HandleEvent(message.EnumValue);
		}
	}
}
