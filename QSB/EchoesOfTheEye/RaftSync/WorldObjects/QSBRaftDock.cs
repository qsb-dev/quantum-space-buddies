using QSB.WorldSync;
using UnityEngine;

namespace QSB.EchoesOfTheEye.RaftSync.WorldObjects;

public class QSBRaftDock : WorldObject<RaftDock>
{
	public override void SendInitialState(uint to) { }

	public void Dock(QSBRaft qsbRaft)
	{
		AttachedObject._raft = qsbRaft.AttachedObject;
		AttachedObject._raft.OnArriveAtTarget += AttachedObject.OnArriveAtTarget;
		AttachedObject.GetAlignDestination().localEulerAngles = Vector3.zero;
		var to = AttachedObject.GetAlignDestination().InverseTransformDirection(AttachedObject._raft.transform.forward);
		to.y = 0f;
		var num = OWMath.Angle(Vector3.forward, to, Vector3.up);
		num = OWMath.RoundToNearestMultiple(num, 90f);
		AttachedObject.GetAlignDestination().localEulerAngles = new Vector3(0f, num, 0f);
		var vector = AttachedObject.GetAlignDestination().position - AttachedObject._raft.GetBody().GetPosition();
		vector = Vector3.Project(vector, AttachedObject._raft.transform.up);
		var position = AttachedObject.GetAlignDestination().position - AttachedObject.GetAlignDestination().up * vector.magnitude;
		AttachedObject._raft.MoveToTarget(position, AttachedObject.GetAlignDestination().rotation, AttachedObject._raftAlignSpeed, false);
		AttachedObject._oneShotAudio.PlayOneShot(AudioType.Raft_Reel_Start);
		AttachedObject._loopingAudio.FadeIn(0.2f);
		AttachedObject._state = RaftCarrier.DockState.AligningBelow;

		AttachedObject.enabled = true;
	}

	public void Undock()
	{
		if (AttachedObject._raft != null && AttachedObject._state == RaftCarrier.DockState.Docked)
		{
			AttachedObject._raftUndockCountDown = AttachedObject._raft.dropDelay;
			AttachedObject._state = RaftCarrier.DockState.WaitForExit;
			AttachedObject._raft.SetRailingRaised(true);
			if (AttachedObject._gearInterface != null)
			{
				AttachedObject._gearInterface.AddRotation(90f);
			}

			AttachedObject.enabled = true;
			return;
		}

		if (AttachedObject._gearInterface != null)
		{
			AttachedObject._gearInterface.PlayFailure();
		}
	}
}
