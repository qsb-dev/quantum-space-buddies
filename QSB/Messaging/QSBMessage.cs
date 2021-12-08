using System;
using QuantumUNET.Transport;

namespace QSB.Messaging
{
	public abstract class QSBMessage
	{
		public abstract void Serialize(QNetworkWriter writer);
		public abstract void Deserialize(QNetworkReader reader);

		public virtual bool ShouldReceive => true;
		public virtual void OnReceiveLocal() { }
		public virtual void OnReceiveRemote() { }
	}


	public abstract class QSBBoolMessage : QSBMessage
	{
		public bool Value;

		public override void Serialize(QNetworkWriter writer)
		{
			writer.Write(Value);
		}

		public override void Deserialize(QNetworkReader reader)
		{
			Value = reader.ReadBoolean();
		}
	}

	public abstract class QSBFloatMessage : QSBMessage
	{
		public float Value;

		public override void Serialize(QNetworkWriter writer)
		{
			writer.Write(Value);
		}

		public override void Deserialize(QNetworkReader reader)
		{
			Value = reader.ReadSingle();
		}
	}

	public abstract class QSBEnumMessage<E> : QSBMessage where E : Enum
	{
		public E Value;

		public override void Serialize(QNetworkWriter writer)
		{
			writer.Write((int)(object)Value);
		}

		public override void Deserialize(QNetworkReader reader)
		{
			Value = (E)(object)reader.ReadInt32();
		}
	}
}
