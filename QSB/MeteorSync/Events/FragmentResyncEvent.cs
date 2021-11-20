using OWML.Common;
using QSB.Events;
using QSB.MeteorSync.WorldObjects;
using QSB.Utility;
using QSB.WorldSync;
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
				LeashLength = qsbFragment.LeashLength,
				IsDetached = qsbFragment.IsDetached
			};

			if (msg.IsDetached)
			{
				msg.IsThruWhiteHole = qsbFragment.IsThruWhiteHole;

				var body = qsbFragment.Body;
				var refBody = qsbFragment.RefBody;
				var pos = body.GetPosition();
				msg.Pos = refBody.transform.InverseTransformPoint(pos);
				msg.Rot = refBody.transform.InverseTransformRotation(body.GetRotation());
				msg.Vel = body.GetVelocity() - refBody.GetPointVelocity(pos);
				msg.AngVel = body.GetAngularVelocity() - refBody.GetAngularVelocity();
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

			if (msg.IsDetached)
			{
				// the detach is delayed, so wait until that happens
				QSBCore.UnityEvents.RunWhen(() => qsbFragment.IsDetached, () =>
				{
					var body = qsbFragment.Body;

					if (msg.IsThruWhiteHole && !qsbFragment.IsThruWhiteHole)
					{
						var whiteHoleVolume = MeteorManager.WhiteHoleVolume;
						var attachedFluidDetector = body.GetAttachedFluidDetector();
						var attachedForceDetector = body.GetAttachedForceDetector();
						if (attachedFluidDetector is not null and ConstantFluidDetector constantFluidDetector)
						{
							constantFluidDetector.SetDetectableFluid(whiteHoleVolume._fluidVolume);
						}
						if (attachedForceDetector is not null and ConstantForceDetector constantForceDetector)
						{
							constantForceDetector.ClearAllFields();
						}
						qsbFragment.DetachableFragment.ChangeFragmentSector(whiteHoleVolume._whiteHoleSector,
							whiteHoleVolume._whiteHoleProxyShadowSuperGroup);

						qsbFragment.DetachableFragment.EndWarpScaling();
						body.gameObject.AddComponent<DebrisLeash>().Init(whiteHoleVolume._whiteHoleBody, qsbFragment.LeashLength);
						whiteHoleVolume._ejectedBodyList.Add(body);
					}
					else if (!msg.IsThruWhiteHole && qsbFragment.IsThruWhiteHole)
					{
						// should only happen if client is way too far ahead and they try to connect. we fail here.
						DebugLog.ToConsole($"{qsbFragment.LogName} is thru white hole, but msg is not. fuck", MessageType.Error);
						return;
					}
					if (qsbFragment.IsThruWhiteHole)
					{
						var debrisLeash = body.GetComponent<DebrisLeash>();
						debrisLeash._deccelerating = false;
						debrisLeash.enabled = true;
					}

					var refBody = qsbFragment.RefBody;
					var pos = refBody.transform.TransformPoint(msg.Pos);
					body.SetPosition(pos);
					body.SetRotation(refBody.transform.TransformRotation(msg.Rot));
					body.SetVelocity(msg.Vel + refBody.GetPointVelocity(pos));
					body.SetAngularVelocity(msg.AngVel + refBody.GetAngularVelocity());
				});
			}
			else if (!msg.IsDetached && qsbFragment.IsDetached)
			{
				// should only happen if client is way too far ahead and they try to connect. we fail here.
				DebugLog.ToConsole($"{qsbFragment.LogName} is detached, but msg is not. fuck", MessageType.Error);
			}
		}
	}
}
