using System.Linq;
using QSB.Events;
using QSB.MeteorSync.WorldObjects;
using QSB.Utility;
using QSB.WorldSync;
using UnityEngine;
using EventType = QSB.Events.EventType;

namespace QSB.MeteorSync.Events
{
	/// called when we request a resync on client join
	public class MeteorResyncEvent : QSBEvent<MeteorResyncMessage>
	{
		public override EventType Type => EventType.MeteorResync;

		public override void SetupListener()
			=> GlobalMessenger.AddListener(EventNames.QSBMeteorResync, Handler);

		public override void CloseListener()
			=> GlobalMessenger.RemoveListener(EventNames.QSBMeteorResync, Handler);

		private void Handler() => SendEvent(CreateMessage());

		private MeteorResyncMessage CreateMessage()
		{
			var qsbMeteors = QSBWorldSync.GetWorldObjects<QSBMeteor>().ToArray();
			var fragments = QSBWorldSync.GetUnityObjects<FragmentIntegrity>().ToArray();

			var msg = new MeteorResyncMessage
			{
				Suspended = new bool[qsbMeteors.Length],
				Damage = new float[qsbMeteors.Length],
				MeteorTransforms = new MeteorResyncMessage.TransformMessage[qsbMeteors.Length],

				Integrity = new float[fragments.Length],
				FragmentTransforms = new MeteorResyncMessage.TransformMessage[fragments.Length]
			};

			// var refBody = Locator._brittleHollow.GetOWRigidbody();
			// for (var i = 0; i < qsbMeteors.Length; i++)
			// {
			// 	msg.Suspended[i] = qsbMeteors[i].AttachedObject.isSuspended;
			// 	msg.Damage[i] = qsbMeteors[i].Damage;
			// 	var body = qsbMeteors[i].AttachedObject.owRigidbody;
			// 	msg.MeteorTransforms[i] = new MeteorResyncMessage.TransformMessage
			// 	{
			// 		pos = refBody.transform.InverseTransformPoint(body.transform.position),
			// 		rot = refBody.transform.InverseTransformRotation(body.transform.rotation),
			// 		vel = GetRelativeVelocity(body, refBody),
			// 		angVel = body.GetRelativeAngularVelocity(refBody)
			// 	};
			// }
			//
			// for (var i = 0; i < fragments.Length; i++)
			// {
			// 	msg.Integrity[i] = fragments[i].GetIntegrity();
			// 	var body = fragments[i].GetAttachedOWRigidbody();
			// 	msg.FragmentTransforms[i] = new MeteorResyncMessage.TransformMessage
			// 	{
			// 		pos = refBody.transform.InverseTransformPoint(body.transform.position),
			// 		rot = refBody.transform.InverseTransformRotation(body.transform.rotation),
			// 		vel = GetRelativeVelocity(body, refBody),
			// 		angVel = body.GetRelativeAngularVelocity(refBody)
			// 	};
			// }

			return msg;
		}

		public override void OnReceiveRemote(bool isHost, MeteorResyncMessage msg)
		{
			if (!MeteorManager.MeteorsReady)
			{
				return;
			}

			var qsbMeteors = QSBWorldSync.GetWorldObjects<QSBMeteor>().ToArray();
			var fragments = QSBWorldSync.GetUnityObjects<FragmentIntegrity>().ToArray();

			// var refBody = Locator._brittleHollow.GetOWRigidbody();
			// for (var i = 0; i < qsbMeteors.Length; i++)
			// {
			// 	if (!msg.Suspended[i] && qsbMeteors[i].AttachedObject.isSuspended)
			// 	{
			// 		// todo
			// 		DebugLog.DebugWrite($"{qsbMeteors[i].LogName} - TODO unsuspend");
			// 	}
			// 	else if (msg.Suspended[i] && !qsbMeteors[i].AttachedObject.isSuspended)
			// 	{
			// 		// todo
			// 		DebugLog.DebugWrite($"{qsbMeteors[i].LogName} - TODO suspend");
			// 	}
			//
			// 	qsbMeteors[i].Damage = msg.Damage[i];
			// 	var body = qsbMeteors[i].AttachedObject.owRigidbody;
			// 	var targetPos = refBody.transform.TransformPoint(msg.MeteorTransforms[i].pos);
			// 	var targetRot = refBody.transform.TransformRotation(msg.MeteorTransforms[i].rot);
			// 	var targetVel = refBody.GetPointVelocity(targetPos) + msg.MeteorTransforms[i].vel;
			// 	var targetAngVel = refBody.GetAngularVelocity() + msg.MeteorTransforms[i].angVel;
			// 	body.MoveToPosition(targetPos);
			// 	body.MoveToRotation(targetRot);
			// 	SetVelocity(body, targetVel);
			// 	body.SetAngularVelocity(targetAngVel);
			// }
			//
			// for (var i = 0; i < fragments.Length; i++)
			// {
			// 	fragments[i]._integrity = 0;
			// 	fragments[i].AddDamage(fragments[i]._origIntegrity - msg.Integrity[i]);
			//
			// 	var body = fragments[i].GetAttachedOWRigidbody();
			// 	var targetPos = refBody.transform.TransformPoint(msg.FragmentTransforms[i].pos);
			// 	var targetRot = refBody.transform.TransformRotation(msg.FragmentTransforms[i].rot);
			// 	var targetVel = refBody.GetPointVelocity(targetPos) + msg.FragmentTransforms[i].vel;
			// 	var targetAngVel = refBody.GetAngularVelocity() + msg.FragmentTransforms[i].angVel;
			// 	body.MoveToPosition(targetPos);
			// 	body.MoveToRotation(targetRot);
			// 	SetVelocity(body, targetVel);
			// 	body.SetAngularVelocity(targetAngVel);
			// }

			DebugLog.DebugWrite($"METEOR RESYNC REQUESTED - synced {qsbMeteors.Length} meteors and {fragments.Length} fragments");
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
