using System;
using QSB.Player;
using QuantumUNET.Messages;
using QuantumUNET.Transport;

namespace QSB.Messaging
{
	public abstract class QSBMessage : QMessageBase
	{
		/// set automatically when sending a message
		public uint From;
		/// <summary>
		/// (default) uint.MaxValue = send to everyone <br/>
		/// 0 = send to host
		/// </summary>
		public uint To = uint.MaxValue;

		/// call the base method when overriding
		public override void Serialize(QNetworkWriter writer)
		{
			writer.Write(From);
			writer.Write(To);
		}

		/// call the base method when overriding
		public override void Deserialize(QNetworkReader reader)
		{
			From = reader.ReadUInt32();
			To = reader.ReadUInt32();
		}

		/// checked before calling either OnReceive
		public virtual bool ShouldReceive => true;
		public virtual void OnReceiveRemote() { }
		public virtual void OnReceiveLocal() { }
	}


	public abstract class QSBBoolMessage : QSBMessage
	{
		public bool Value;

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(Value);
		}

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			Value = reader.ReadBoolean();
		}
	}

	public abstract class QSBFloatMessage : QSBMessage
	{
		public float Value;

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(Value);
		}

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			Value = reader.ReadSingle();
		}
	}

	public abstract class QSBEnumMessage<E> : QSBMessage where E : Enum
	{
		public E Value;

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write((int)(object)Value);
		}

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			Value = (E)(object)reader.ReadInt32();
		}
	}
}
