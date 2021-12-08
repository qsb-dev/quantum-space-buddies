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
}
