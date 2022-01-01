using QSB.Messaging;
using QSB.Player;
using QSB.Player.TransformSync;
using QSB.TimeSync;
using QSB.WorldSync;

namespace QSB.EyeOfTheUniverse.EyeStateSync.Messages
{
	internal class EyeStateMessage : QSBEnumMessage<EyeState>
	{
		static EyeStateMessage() => GlobalMessenger<EyeState>.AddListener(OWEvents.EyeStateChanged, Handler);

		private static void Handler(EyeState state)
		{
			if (PlayerTransformSync.LocalInstance)
			{
				new EyeStateMessage(state).Send();
			}
		}


		private EyeStateMessage(EyeState state) => Value = state;

		public override bool ShouldReceive => WorldObjectManager.AllObjectsReady;

		public override void OnReceiveLocal()
		{
			QSBPlayerManager.LocalPlayer.EyeState = Value;

			if (Value >= EyeState.ForestIsDark)
			{
				WakeUpSync.LocalInstance.EyeDisable = true;
			}
		}

		public override void OnReceiveRemote()
		{
			var player = QSBPlayerManager.GetPlayer(From);
			player.EyeState = Value;

			if (Value >= EyeState.ForestIsDark)
			{
				WakeUpSync.LocalInstance.EyeDisable = true;
			}
		}
	}
}