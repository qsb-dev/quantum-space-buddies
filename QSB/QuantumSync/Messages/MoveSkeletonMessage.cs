using QSB.Messaging;
using QSB.QuantumSync.WorldObjects;
using QuantumUNET.Transport;

namespace QSB.QuantumSync.Messages
{
	internal class MoveSkeletonMessage : QSBWorldObjectMessage<QSBQuantumSkeletonTower>
	{
		private int _pointingIndex, _towerIndex;

		public MoveSkeletonMessage(int pointingIndex, int towerIndex)
		{
			_pointingIndex = pointingIndex;
			_towerIndex = towerIndex;
		}

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(_pointingIndex);
			writer.Write(_towerIndex);
		}

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			_pointingIndex = reader.ReadInt32();
			_towerIndex = reader.ReadInt32();
		}

		public override void OnReceiveRemote() => WorldObject.MoveSkeleton(_pointingIndex, _towerIndex);
	}
}
