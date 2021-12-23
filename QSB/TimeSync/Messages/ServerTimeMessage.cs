using QSB.Messaging;
using QuantumUNET.Transport;

namespace QSB.TimeSync.Messages
{
	public class ServerTimeMessage : QSBMessage
	{
		private float ServerTime;
		private int LoopCount;

		public ServerTimeMessage(float time, int count)
		{
			ServerTime = time;
			LoopCount = count;
		}

		public ServerTimeMessage() { }

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(ServerTime);
			writer.Write(LoopCount);
		}

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			ServerTime = reader.ReadSingle();
			LoopCount = reader.ReadInt16();
		}

		public override void OnReceiveRemote()
			=> WakeUpSync.LocalInstance.OnClientReceiveMessage(ServerTime, LoopCount);
	}
}