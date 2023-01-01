using Cysharp.Threading.Tasks;
using QSB.ElevatorSync.Messages;
using QSB.Messaging;
using QSB.WorldSync;
using System.Threading;
using UnityEngine;

namespace QSB.ElevatorSync.WorldObjects;

public class QSBElevator : WorldObject<Elevator>
{
	private OWTriggerVolume _elevatorTrigger;

	public override async UniTask Init(CancellationToken ct)
	{
		// BUG : This won't work for the log lift! need to make a different trigger for that

		var boxShape = AttachedObject.gameObject.GetAddComponent<BoxShape>();
		boxShape.center = new Vector3(0, 1.75f, 0.25f);
		boxShape.size = new Vector3(3, 3.5f, 3);

		_elevatorTrigger = AttachedObject.gameObject.GetAddComponent<OWTriggerVolume>();
	}

	public override void SendInitialState(uint to) =>
		this.SendMessage(new ElevatorMessage(AttachedObject._goingToTheEnd) { To = to });

	public void RemoteCall(bool isGoingUp)
	{
		if (AttachedObject._goingToTheEnd == isGoingUp)
		{
			return;
		}

		SetDirection(isGoingUp);
		if (_elevatorTrigger.IsTrackingObject(Locator.GetPlayerDetector()))
		{
			AttachedObject._attachPoint.AttachPlayer();
			if (Locator.GetPlayerSuit().IsWearingSuit() && Locator.GetPlayerSuit().IsTrainingSuit())
			{
				Locator.GetPlayerSuit().RemoveSuit();
			}
		}

		AttachedObject.StartLift();
	}

	private void SetDirection(bool isGoingUp)
	{
		AttachedObject._interactVolume.transform.Rotate(0f, 180f, 0f);
		AttachedObject._goingToTheEnd = isGoingUp;
		AttachedObject._targetLocalPos = isGoingUp ? AttachedObject._endLocalPos : AttachedObject._startLocalPos;
	}

	public override void DisplayLines()
	{
		var boxShape = (BoxShape)_elevatorTrigger._shape;
		Popcron.Gizmos.Cube(
			ShapeUtil.Box.CalcWorldSpaceCenter(boxShape),
			boxShape.transform.rotation,
			ShapeUtil.Box.CalcWorldSpaceSize(boxShape),
			_elevatorTrigger.IsTrackingObject(Locator.GetPlayerDetector()) ? Color.green : Color.white
		);
	}
}
