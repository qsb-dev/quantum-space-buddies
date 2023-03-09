using Mirror;
using QSB.Messaging;

namespace QSB.TimeSync.Messages;

public class ServerTimeMessage : QSBMessage
{
	private float ServerTime;
	private int LoopCount;
	private float SecondsRemaining;

	public ServerTimeMessage(float time, int count, float secondsRemaining)
	{
		ServerTime = time;
		LoopCount = count;
		SecondsRemaining = secondsRemaining;
	}

	public override void Serialize(NetworkWriter writer)
	{
		base.Serialize(writer);
		writer.Write(ServerTime);
		writer.Write(LoopCount);
		writer.Write(SecondsRemaining);
	}

	public override void Deserialize(NetworkReader reader)
	{
		base.Deserialize(reader);
		ServerTime = reader.Read<float>();
		LoopCount = reader.Read<int>();
		SecondsRemaining = reader.Read<float>();
	}

	public override void OnReceiveRemote()
		=> WakeUpSync.LocalInstance?.OnClientReceiveMessage(ServerTime, LoopCount, SecondsRemaining);
}
