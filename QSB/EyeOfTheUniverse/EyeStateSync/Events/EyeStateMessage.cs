using QSB.Events;
using QSB.Messaging;
using QSB.Player;
using QSB.WorldSync;

namespace QSB.EyeOfTheUniverse.EyeStateSync.Events
{
	internal class EyeStateMessage : QSBEnumMessage<EyeState>
	{
		static EyeStateMessage() => GlobalMessenger<EyeState>.AddListener(EventNames.EyeStateChanged,
			state => new EyeStateMessage(state).Send());


		public EyeStateMessage(EyeState state) => Value = state;

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