using QSB.WorldSync;

namespace QSB.EchoesOfTheEye.RaftSync.WorldObjects;

public class QSBRaftDock : WorldObject<RaftDock>
{
	public override void SendInitialState(uint to) { }

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

	public void Dock(QSBRaft qsbRaft) { }
}
