using QSB.Messaging;
using QSB.QuantumSync.WorldObjects;

namespace QSB.QuantumSync.Messages;

public class MoveSkeletonMessage : QSBWorldObjectMessage<QSBQuantumSkeletonTower, int>
{
	public MoveSkeletonMessage(int index) : base(index) { }

	public override void OnReceiveRemote() => WorldObject.MoveSkeleton(Data);
}