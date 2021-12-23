using QSB.Audio;
using QSB.Events;
using QSB.Messaging;
using QSB.Player;
using QSB.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QSB.EyeOfTheUniverse.EyeStateSync.Events
{
	internal class EyeStateEvent : QSBEvent<EnumMessage<EyeState>>
	{
		public override bool RequireWorldObjectsReady => true;

		public override void SetupListener() => GlobalMessenger<EyeState>.AddListener(EventNames.EyeStateChanged, Handler);
		public override void CloseListener() => GlobalMessenger<EyeState>.RemoveListener(EventNames.EyeStateChanged, Handler);

		private void Handler(EyeState state) => SendEvent(CreateMessage(state));

		private EnumMessage<EyeState> CreateMessage(EyeState state) => new()
		{
			AboutId = LocalPlayerId,
			EnumValue = state
		};

		public override void OnReceiveLocal(bool isHost, EnumMessage<EyeState> message)
		{
			DebugLog.DebugWrite($"LOCAL set eye state to {message.EnumValue}");
			QSBPlayerManager.LocalPlayer.EyeState = message.EnumValue;
		}

		public override void OnReceiveRemote(bool isHost, EnumMessage<EyeState> message)
		{
			DebugLog.DebugWrite($"{message.AboutId} set eye state to {message.EnumValue}");
			var player = QSBPlayerManager.GetPlayer(message.AboutId);
			player.EyeState = message.EnumValue;
		}
	}
}
