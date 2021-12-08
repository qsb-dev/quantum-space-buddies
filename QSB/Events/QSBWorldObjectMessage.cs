using System;
using QSB.WorldSync;
using QuantumUNET.Transport;

namespace QSB.Events
{
	public abstract class QSBWorldObjectMessage<T> : QSBMessage where T : IWorldObject
	{
		public int Id;
		public T WorldObject { get; private set; }

		public override void Serialize(QNetworkWriter writer) => writer.Write(Id);
		public override void Deserialize(QNetworkReader reader) => Id = reader.ReadInt32();

		public override bool ShouldReceive
		{
			get
			{
				if (!WorldObjectManager.AllObjectsReady)
				{
					return false;
				}

				WorldObject = QSBWorldSync.GetWorldFromId<T>(Id);
				return true;
			}
		}
	}


	public abstract class QSBBoolWorldObjectMessage<T> : QSBWorldObjectMessage<T> where T : IWorldObject
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

	public abstract class QSBEnumWorldObjectMessage<T, E> : QSBWorldObjectMessage<T>
		where T : IWorldObject
		where E : Enum
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
