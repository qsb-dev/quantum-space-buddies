using QSB.ElevatorSync.Messages;
using QSB.Messaging;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.ElevatorSync.WorldObjects
{
	public class QSBElevator : WorldObject<Elevator>
	{
		private OWTriggerVolume _elevatorTrigger;

		public override void Init()
		{
			StartDelayedReady();
			QSBCore.UnityEvents.RunWhen(() => AttachedObject._interactVolume, () =>
			{
				FinishDelayedReady();

				// BUG : This won't work for the log lift! need to make a different trigger for that

				var boxShape = AttachedObject.gameObject.GetAddComponent<BoxShape>();
				boxShape.center = new Vector3(0, 1.75f, 0.25f);
				boxShape.size = new Vector3(3, 3.5f, 3);

				_elevatorTrigger = AttachedObject.gameObject.GetAddComponent<OWTriggerVolume>();
			});
		}

		public override void SendResyncInfo(uint to)
		{
			if (QSBCore.IsHost)
			{
				this.SendMessage(new ElevatorMessage(AttachedObject._goingToTheEnd));
			}
		}

		public void RemoteCall(bool isGoingUp)
		{
			if (_elevatorTrigger.IsTrackingObject(Locator.GetPlayerDetector()))
			{
				SetDirection(isGoingUp);

				AttachedObject._attachPoint.AttachPlayer();

				if (Locator.GetPlayerSuit().IsWearingSuit() && Locator.GetPlayerSuit().IsTrainingSuit())
				{
					Locator.GetPlayerSuit().RemoveSuit();
				}

				AttachedObject.StartLift();
			}
			else
			{
				SetDirection(isGoingUp);
				AttachedObject.StartLift();
			}
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
}