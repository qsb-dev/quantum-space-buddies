using QSB.Events;
using QSB.Messaging;
using QSB.Player;
using QSB.Player.TransformSync;
using QSB.WorldSync;

namespace QSB.EyeOfTheUniverse.EyeStateSync.Messages
{
	internal class EyeStateMessage : QSBEnumMessage<EyeState>
	{
		static EyeStateMessage() => GlobalMessenger<EyeState>.AddListener(EventNames.EyeStateChanged, Handler);

		private static void Handler(EyeState state)
		{
			if (PlayerTransformSync.LocalInstance != null)
			{
				new EyeStateMessage(state).Send();
			}
		}


		private EyeStateMessage(EyeState state) => Value = state;

		public EyeStateMessage() { }

		public override bool ShouldReceive => WorldObjectManager.AllObjectsReady;

		public override void OnReceiveLocal()
		{
			QSBPlayerManager.LocalPlayer.EyeState = Value;
		}

		public override void OnReceiveRemote()
		{
			var player = QSBPlayerManager.GetPlayer(From);
			player.EyeState = Value;
		}
	}
}