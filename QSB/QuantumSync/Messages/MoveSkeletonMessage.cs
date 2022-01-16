using QSB.Messaging;
using QSB.QuantumSync.WorldObjects;
using QuantumUNET.Transport;

namespace QSB.QuantumSync.Messages
{
	internal class MoveSkeletonMessage : QSBWorldObjectMessage<QSBQuantumSkeletonTower>
	{
		private int _index;

		public MoveSkeletonMessage(int index) => _index = index;

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(_index);
		}

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			_index = reader.ReadInt32();
		}

		public override void OnReceiveRemote() => WorldObject.MoveSkeleton(_index);
	}
}
