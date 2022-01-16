using QuantumUNET.Messages;
using QuantumUNET.Transport;
using System;

namespace QSB.Messaging
{
	public abstract class QSBMessage : QMessageBase
	{
		/// <summary>
		/// set automatically by Send
		/// </summary>
		internal uint From;
		/// <summary>
		/// (default) uint.MaxValue = send to everyone <br/>
		/// 0 = send to host
		/// </summary>
		public uint To = uint.MaxValue;

		/// <summary>
		/// call the base method when overriding
		/// </summary>
		public override void Serialize(QNetworkWriter writer)
		{
			writer.Write(From);
			writer.Write(To);
		}

		/// <summary>
		/// call the base method when overriding
		/// <para/>
		/// note: no constructor is called before this,
		/// so fields won't be initialized.
		/// </summary>
		public override void Deserialize(QNetworkReader reader)
		{
			From = reader.ReadUInt32();
			To = reader.ReadUInt32();
		}

		/// <summary>
		/// checked before calling either OnReceive
		/// </summary>
		public virtual bool ShouldReceive => true;
		public virtual void OnReceiveLocal() { }
		public virtual void OnReceiveRemote() { }

		public override string ToString() => GetType().Name;
	}

	public abstract class QSBBoolMessage : QSBMessage
	{
		protected bool Value;

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
		protected float Value;

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
		protected E Value;

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
