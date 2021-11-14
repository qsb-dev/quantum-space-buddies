using QSB.Events;
using QSB.MeteorSync.WorldObjects;
using QSB.Utility;
using QSB.WorldSync;
using UnityEngine;
using EventType = QSB.Events.EventType;

namespace QSB.MeteorSync.Events
{
	/// called when we request a resync on client join
	public class FragmentResyncEvent : QSBEvent<FragmentResyncMessage>
	{
		public override EventType Type => EventType.FragmentResync;

		public override void SetupListener()
			=> GlobalMessenger<QSBFragment>.AddListener(EventNames.QSBFragmentResync, Handler);

		public override void CloseListener()
			=> GlobalMessenger<QSBFragment>.RemoveListener(EventNames.QSBFragmentResync, Handler);

		private void Handler(QSBFragment qsbFragment) => SendEvent(CreateMessage(qsbFragment));

		private FragmentResyncMessage CreateMessage(QSBFragment qsbFragment)
		{
			return new FragmentResyncMessage();
			var msg = new FragmentResyncMessage
			{
				ObjectId = qsbFragment.ObjectId,
				Integrity = qsbFragment.AttachedObject.GetIntegrity()
			};
			if (msg.Integrity <= 0)
			{
				var refBody = Locator._brittleHollow.GetOWRigidbody();
				var body = qsbFragment.AttachedObject.GetAttachedOWRigidbody();
				msg.Pos = refBody.transform.InverseTransformPoint(body.transform.position);
				msg.Rot = refBody.transform.InverseTransformRotation(body.transform.rotation);
				msg.Vel = GetRelativeVelocity(body, refBody);
				msg.AngVel = body.GetRelativeAngularVelocity(refBody);
			}

			return msg;
		}

		public override void OnReceiveRemote(bool isHost, FragmentResyncMessage msg)
		{
			return;
			if (!MeteorManager.MeteorsReady)
			{
				return;
			}

			var qsbFragment = QSBWorldSync.GetWorldFromId<QSBFragment>(msg.ObjectId);
			qsbFragment.AttachedObject._integrity = msg.Integrity;
			if (msg.Integrity <= 0)
			{
				var refBody = Locator._brittleHollow.GetOWRigidbody();
				var body = qsbFragment.AttachedObject.GetAttachedOWRigidbody();
				var targetPos = refBody.transform.TransformPoint(msg.Pos);
				var targetRot = refBody.transform.TransformRotation(msg.Rot);
				var targetVel = refBody.GetPointVelocity(targetPos) + msg.Vel;
				var targetAngVel = refBody.GetAngularVelocity() + msg.AngVel;
				body.MoveToPosition(targetPos);
				body.MoveToRotation(targetRot);
				SetVelocity(body, targetVel);
				body.SetAngularVelocity(targetAngVel);
			}
		}


		// code yoink from transform sync lol
		private static void SetVelocity(OWRigidbody rigidbody, Vector3 relativeVelocity)
		{
			var isRunningKinematic = rigidbody.RunningKinematicSimulation();
			var currentVelocity = rigidbody._currentVelocity;

			if (isRunningKinematic)
			{
				rigidbody._kinematicRigidbody.velocity = relativeVelocity + Locator.GetCenterOfTheUniverse().GetStaticFrameVelocity_Internal();
			}
			else
			{
				rigidbody._rigidbody.velocity = relativeVelocity + Locator.GetCenterOfTheUniverse().GetStaticFrameVelocity_Internal();
			}

			rigidbody._lastVelocity = currentVelocity;
			rigidbody._currentVelocity = relativeVelocity;
		}

		private static Vector3 GetRelativeVelocity(OWRigidbody body, OWRigidbody refBody)
			=> body.GetVelocity() - refBody.GetPointVelocity(body.transform.position);
	}
}
