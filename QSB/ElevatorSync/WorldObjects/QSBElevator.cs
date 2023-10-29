using Cysharp.Threading.Tasks;
using QSB.ElevatorSync.Messages;
using QSB.Messaging;
using QSB.Patches;
using QSB.Utility;
using QSB.WorldSync;
using System.Threading;
using UnityEngine;

namespace QSB.ElevatorSync.WorldObjects;

public class QSBElevator : WorldObject<Elevator>
{
	private OWTriggerVolume _elevatorTrigger;

	// Used to reset attach position. This is in local space.
	public Vector3 originalAttachPosition;

	public override async UniTask Init(CancellationToken ct)
	{
		var boxShape = AttachedObject.gameObject.GetAddComponent<BoxShape>();

		if (Name == "LogLift")
		{
			boxShape.size = new Vector3(4.6f, 3.5f, 12);
			boxShape.center = new Vector3(0.1f, 1.75f, 1.3f);
		}
		else
		{
			boxShape.size = new Vector3(3, 3.5f, 3);
			boxShape.center = new Vector3(0, 1.75f, 0.25f);
		}

		_elevatorTrigger = AttachedObject.gameObject.GetAddComponent<OWTriggerVolume>();
		originalAttachPosition = AttachedObject._attachPoint.transform.localPosition;
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
			var attachPoint = AttachedObject._attachPoint;
			attachPoint.transform.position = Locator.GetPlayerTransform().position;

			attachPoint.AttachPlayer();
			if (Locator.GetPlayerSuit().IsWearingSuit() && Locator.GetPlayerSuit().IsTrainingSuit())
			{
				Locator.GetPlayerSuit().RemoveSuit();
			}
		}

		AttachedObject.StartLift();

		// Runs when the lift/elevator is done moving.
		// Reset the position of the attach point.
		Delay.RunWhen(() => !AttachedObject.enabled, () =>
		{
			AttachedObject._attachPoint.transform.localPosition = originalAttachPosition;
		});
	
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
