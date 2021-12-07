using QuantumUNET.Messages;
using QuantumUNET.Transport;

namespace QSB.Messaging
{
	public class PlayerMessage : QMessageBase
	{
		/// <summary>
		/// The Player ID that is sending this message
		/// </summary>
		public uint FromId { get; set; }

		/// <summary>
		/// The Player ID that this message is about
		/// </summary>
		public uint AboutId { get; set; }

		/// <summary>
		/// If true, only send this message to the host of the current session
		/// /// (OnReceiveLocal/Remote is not called on any other client)
		/// </summary>
		public bool OnlySendToHost { get; set; }

		/// <summary>
		/// The Player ID that this message is for.
		/// By default, this is uint.MaxValue,
		/// which means this is ignored and the message is sent to all clients
		/// </summary>
		public uint ForId { get; set; } = uint.MaxValue;

		public override void Deserialize(QNetworkReader reader)
		{
			FromId = reader.ReadUInt32();
			AboutId = reader.ReadUInt32();
			OnlySendToHost = reader.ReadBoolean();
			if (!OnlySendToHost)
			{
				reader.ReadUInt32();
			}
		}

		public override void Serialize(QNetworkWriter writer)
		{
			writer.Write(FromId);
			writer.Write(AboutId);
			writer.Write(OnlySendToHost);
			if (!OnlySendToHost)
			{
				writer.Write(ForId);
			}
		}
	}
}
