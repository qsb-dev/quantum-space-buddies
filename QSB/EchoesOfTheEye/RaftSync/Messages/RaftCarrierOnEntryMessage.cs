using QSB.EchoesOfTheEye.RaftSync.WorldObjects;
using QSB.Messaging;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.EchoesOfTheEye.RaftSync.Messages;

public class RaftCarrierOnEntryMessage : QSBWorldObjectMessage<IQSBRaftCarrier, int>
{
	public RaftCarrierOnEntryMessage(QSBRaft raft) : base(raft.ObjectId) { }

	public override void OnReceiveRemote()
	{
		// TODO : work out if we can just call RaftCarrier.OnEntry with a right gameobject? tried it with _fluidDetector.gameObject and it didn't work

		var qsbRaft = Data.GetWorldObject<QSBRaft>();
		var attachedObj = (RaftCarrier)WorldObject.AttachedObject;

		attachedObj._raft = qsbRaft.AttachedObject;
		attachedObj._raft.OnArriveAtTarget += attachedObj.OnArriveAtTarget;
		attachedObj.GetAlignDestination().localEulerAngles = Vector3.zero;

		var relativeDockForward = attachedObj.GetAlignDestination().InverseTransformDirection(attachedObj._raft.transform.forward);
		relativeDockForward.y = 0f;

		var targetRaftRotation = OWMath.Angle(Vector3.forward, relativeDockForward, Vector3.up);
		targetRaftRotation = OWMath.RoundToNearestMultiple(targetRaftRotation, 90f);
		attachedObj.GetAlignDestination().localEulerAngles = new Vector3(0f, targetRaftRotation, 0f);

		var raftMovementDirection = attachedObj.GetAlignDestination().position - attachedObj._raft.GetBody().GetPosition();
		raftMovementDirection = Vector3.Project(raftMovementDirection, attachedObj._raft.transform.up);

		var targetPosition = attachedObj.GetAlignDestination().position - attachedObj.GetAlignDestination().up * raftMovementDirection.magnitude;

		attachedObj._raft.MoveToTarget(targetPosition, attachedObj.GetAlignDestination().rotation, attachedObj._raftAlignSpeed, false);
		attachedObj._oneShotAudio.PlayOneShot(global::AudioType.Raft_Reel_Start, 1f);
		attachedObj._loopingAudio.FadeIn(0.2f, false, false, 1f);
		attachedObj._state = RaftCarrier.DockState.AligningBelow;

		if (WorldObject.AttachedObject is RaftDock dock)
		{
			if (dock._state == RaftCarrier.DockState.AligningBelow)
			{
				dock.enabled = true;
			}
		}
		else if (WorldObject.AttachedObject is DamRaftLift lift)
		{
			if (lift._state == RaftCarrier.DockState.AligningBelow)
			{
				lift.enabled = true;

				foreach (var node in lift._liftNodes)
				{
					node.localEulerAngles = lift.GetAlignDestination().localEulerAngles;
				}

				lift._nodeIndex = 1;
				lift._raftDockLights.SetLightsActivation(true, false);
			}
		}
	}
}
