using Mirror;
using QSB.WorldSync;
using System;

namespace QSB.Messaging
{
	public abstract class QSBWorldObjectMessage<T> : QSBMessage where T : IWorldObject
	{
		/// <summary>
		/// set automatically by SendMessage
		/// </summary>
		internal int ObjectId;
		/// <summary>
		/// set automatically by ShouldReceive
		/// </summary>
		protected T WorldObject { get; private set; }

		public override void Serialize(NetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(ObjectId);
		}

		public override void Deserialize(NetworkReader reader)
		{
			base.Deserialize(reader);
			ObjectId = reader.Read<int>();
		}

		public override bool ShouldReceive
		{
			get
			{
				if (!QSBWorldSync.AllObjectsReady)
				{
					return false;
				}

				WorldObject = ObjectId.GetWorldObject<T>();
				return true;
			}
		}
	}

	public abstract class QSBBoolWorldObjectMessage<T> : QSBWorldObjectMessage<T> where T : IWorldObject
	{
		protected bool Value;

		public override void Serialize(NetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(Value);
		}

		public override void Deserialize(NetworkReader reader)
		{
			base.Deserialize(reader);
			Value = reader.Read<bool>();
		}
	}

	public abstract class QSBFloatWorldObjectMessage<T> : QSBWorldObjectMessage<T> where T : IWorldObject
	{
		protected float Value;

		public override void Serialize(NetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(Value);
		}

		public override void Deserialize(NetworkReader reader)
		{
			base.Deserialize(reader);
			Value = reader.Read<float>();
		}
	}

	public abstract class QSBEnumWorldObjectMessage<T, E> : QSBWorldObjectMessage<T>
		where T : IWorldObject
		where E : Enum
	{
		protected E Value;

		public override void Serialize(NetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write((int)(object)Value);
		}

		public override void Deserialize(NetworkReader reader)
		{
			base.Deserialize(reader);
			Value = (E)(object)reader.Read<int>();
		}
	}
}
