using Mirror;
using QSB.Messaging;

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

		public override void Serialize(NetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(ServerTime);
			writer.Write(LoopCount);
		}

		public override void Deserialize(NetworkReader reader)
		{
			base.Deserialize(reader);
			ServerTime = reader.Read<float>();
			LoopCount = reader.Read<int>();
		}

		public override void OnReceiveRemote()
			=> WakeUpSync.Instance.OnClientReceiveMessage(ServerTime, LoopCount);
	}
}