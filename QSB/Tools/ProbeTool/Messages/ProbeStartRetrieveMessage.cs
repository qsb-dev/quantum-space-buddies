using QSB.Messaging;
using QSB.Player;
using QSB.WorldSync;

namespace QSB.Tools.ProbeTool.Messages
{
	internal class ProbeStartRetrieveMessage : QSBFloatMessage
	{
		public override bool ShouldReceive => WorldObjectManager.AllObjectsReady;

		public ProbeStartRetrieveMessage(float duration) => Value = duration;

		public ProbeStartRetrieveMessage() { }

		public override void OnReceiveRemote()
		{
			var player = QSBPlayerManager.GetPlayer(From);
			if (!player.IsReady || player.Probe == null)
			{
				return;
			}

			var probe = player.Probe;
			probe.OnStartRetrieve(Value);
		}
	}
}
