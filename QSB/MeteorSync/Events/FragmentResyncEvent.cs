using System;
using OWML.Common;
using QSB.Events;
using QSB.MeteorSync.WorldObjects;
using QSB.Utility;
using QSB.WorldSync;
using UnityEngine;
using EventType = QSB.Events.EventType;

namespace QSB.MeteorSync.Events
{
	/// called when we request a resync on client join
	/// pain
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
			var msg = new FragmentResyncMessage
			{
				ObjectId = qsbFragment.ObjectId,
				Integrity = qsbFragment.AttachedObject._integrity,
				OrigIntegrity = qsbFragment.AttachedObject._origIntegrity,
				LeashLength = qsbFragment.LeashLength
			};

			if (msg.Integrity <= 0)
			{
				msg.IsThruWhiteHole = qsbFragment.IsThruWhiteHole;

				var refBody = qsbFragment.RefBody;
				var body = qsbFragment.Body;
				msg.Pos = refBody.transform.InverseTransformPoint(body.transform.position);
				msg.Rot = refBody.transform.InverseTransformRotation(body.transform.rotation);
				msg.Vel = GetRelativeVelocity(body, refBody);
				msg.AngVel = body.GetRelativeAngularVelocity(refBody);
			}

			return msg;
		}

		public override void OnReceiveRemote(bool isHost, FragmentResyncMessage msg)
		{
			if (!MeteorManager.Ready)
			{
				return;
			}

			var qsbFragment = QSBWorldSync.GetWorldFromId<QSBFragment>(msg.ObjectId);
			qsbFragment.AttachedObject._integrity = msg.Integrity;
			qsbFragment.AttachedObject._origIntegrity = msg.OrigIntegrity;
			qsbFragment.LeashLength = msg.LeashLength;
			qsbFragment.AttachedObject.CallOnTakeDamage();

			if (msg.Integrity <= 0)
			{
				// the detach is delayed, so wait even more until that happens lol
				QSBCore.UnityEvents.FireInNUpdates(() =>
				{
					if (msg.IsThruWhiteHole && !qsbFragment.IsThruWhiteHole)
					{
						qsbFragment.DetachableFragment.ChangeFragmentSector(MeteorManager.WhiteHoleVolume._whiteHoleSector,
							MeteorManager.WhiteHoleVolume._whiteHoleProxyShadowSuperGroup);
						qsbFragment.DetachableFragment.EndWarpScaling();
						qsbFragment.Body.gameObject.AddComponent<DebrisLeash>()
							.Init(MeteorManager.WhiteHoleVolume._whiteHoleBody, qsbFragment.LeashLength);
					}
					else if (!msg.IsThruWhiteHole && qsbFragment.IsThruWhiteHole)
					{
						// should only happen if client is way too far ahead and they try to connect. we fail here.
						DebugLog.ToConsole($"{qsbFragment.LogName} is thru white hole, but msg is not. goodbye", MessageType.Quit);
						Application.Quit();
					}

					var refBody = qsbFragment.RefBody;
					var body = qsbFragment.Body;
					var targetPos = refBody.transform.TransformPoint(msg.Pos);
					var targetRot = refBody.transform.TransformRotation(msg.Rot);
					var targetVel = refBody.GetPointVelocity(targetPos) + msg.Vel;
					var targetAngVel = refBody.GetAngularVelocity() + msg.AngVel;
					body.MoveToPosition(targetPos);
					body.MoveToRotation(targetRot);
					SetVelocity(body, targetVel);
					body.SetAngularVelocity(targetAngVel);
				}, 20);
			}
		}


		// code yoink from transform sync lol
		private static void SetVelocity(OWRigidbody rigidbody, Vector3 relativeVelocity)
		{
			var currentVelocity = rigidbody._currentVelocity;

			if (rigidbody.RunningKinematicSimulation())
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
