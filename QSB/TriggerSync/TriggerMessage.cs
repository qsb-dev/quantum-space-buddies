using QSB.Messaging;
using QSB.Player;
using QSB.WorldSync;
using QuantumUNET.Transport;

namespace QSB.TriggerSync
{
	public class TriggerMessage : QSBBoolMessage
	{
		private int _id;

		public TriggerMessage(int id, bool entered)
		{
			_id = id;
			Value = entered;
		}

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(_id);
		}

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			_id = reader.ReadInt32();
		}

		public override bool ShouldReceive => WorldObjectManager.AllObjectsReady;

		public override void OnReceiveLocal() => OnReceiveRemote();

		public override void OnReceiveRemote()
		{
			var player = QSBPlayerManager.GetPlayer(From);
			var triggerLink = TriggerManager.GetTriggerLink(_id);
			if (Value)
			{
				triggerLink.Enter(player);
			}
			else
			{
				triggerLink.Exit(player);
			}
		}
	}
}
