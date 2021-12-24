using System;
using QSB.WorldSync;
using QuantumUNET.Transport;

namespace QSB.Messaging
{
	public abstract class QSBWorldObjectMessage<T> : QSBMessage where T : IWorldObject
	{
		/// <summary>
		/// set automatically by SendMessage
		/// </summary>
		internal int ObjectId;
		/// <summary>
		/// set automatically when receiving
		/// </summary>
		protected T WorldObject { get; private set; }

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(ObjectId);
		}

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			ObjectId = reader.ReadInt32();
		}

		public override bool ShouldReceive
		{
			get
			{
				if (!WorldObjectManager.AllObjectsReady)
				{
					return false;
				}

				WorldObject = QSBWorldSync.GetWorldFromId<T>(ObjectId);
				return true;
			}
		}
	}


	public abstract class QSBBoolWorldObjectMessage<T> : QSBWorldObjectMessage<T> where T : IWorldObject
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

	public abstract class QSBFloatWorldObjectMessage<T> : QSBWorldObjectMessage<T> where T : IWorldObject
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

	public abstract class QSBEnumWorldObjectMessage<T, E> : QSBWorldObjectMessage<T>
		where T : IWorldObject
		where E : Enum
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
