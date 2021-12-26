using QSB.Messaging;
using QSB.Player;
using QSB.WorldSync;

namespace QSB.Tools.ProbeLauncherTool.Messages
{
	internal class PlayerRetrieveProbeMessage : QSBBoolMessage
	{
		public PlayerRetrieveProbeMessage(bool playEffects) => Value = playEffects;

		public PlayerRetrieveProbeMessage() { }

		public override bool ShouldReceive => WorldObjectManager.AllObjectsReady;

		public override void OnReceiveRemote()
		{
			var player = QSBPlayerManager.GetPlayer(From);
			player.ProbeLauncher.RetrieveProbe(Value);
		}
	}
}
