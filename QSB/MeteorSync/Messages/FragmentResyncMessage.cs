using Mirror;
using OWML.Common;
using QSB.Messaging;
using QSB.MeteorSync.WorldObjects;
using QSB.Utility;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.MeteorSync.Messages
{
	/// called when we request a resync on client join
	/// pain
	public class FragmentResyncMessage : QSBWorldObjectMessage<QSBFragment>
	{
		private float Integrity;
		private float OrigIntegrity;
		private float LeashLength;
		private bool IsDetached;

		private bool IsThruWhiteHole;
		private Vector3 RelPos;
		private Quaternion RelRot;
		private Vector3 RelVel;
		private Vector3 RelAngVel;

		public FragmentResyncMessage(QSBFragment qsbFragment)
		{
			Integrity = qsbFragment.AttachedObject._integrity;
			OrigIntegrity = qsbFragment.AttachedObject._origIntegrity;
			LeashLength = qsbFragment.LeashLength;
			IsDetached = qsbFragment.IsDetached;

			if (IsDetached)
			{
				IsThruWhiteHole = qsbFragment.IsThruWhiteHole;

				var body = qsbFragment.Body;
				var refBody = qsbFragment.RefBody;
				var pos = body.GetPosition();
				RelPos = refBody.transform.ToRelPos(pos);
				RelRot = refBody.transform.ToRelRot(body.GetRotation());
				RelVel = refBody.ToRelVel(body.GetVelocity(), pos);
				RelAngVel = refBody.ToRelAngVel(body.GetAngularVelocity());
			}
		}

		public override void Serialize(NetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(Integrity);
			writer.Write(OrigIntegrity);
			writer.Write(LeashLength);
			writer.Write(IsDetached);
			if (IsDetached)
			{
				writer.Write(IsThruWhiteHole);
				writer.Write(RelPos);
				writer.Write(RelRot);
				writer.Write(RelVel);
				writer.Write(RelAngVel);
			}
		}

		public override void Deserialize(NetworkReader reader)
		{
			base.Deserialize(reader);
			Integrity = reader.Read<float>();
			OrigIntegrity = reader.Read<float>();
			LeashLength = reader.Read<float>();
			IsDetached = reader.Read<bool>();
			if (IsDetached)
			{
				IsThruWhiteHole = reader.Read<bool>();
				RelPos = reader.ReadVector3();
				RelRot = reader.ReadQuaternion();
				RelVel = reader.ReadVector3();
				RelAngVel = reader.ReadVector3();
			}
		}

		public override void OnReceiveRemote()
		{
			var qsbFragment = ObjectId.GetWorldObject<QSBFragment>();
			qsbFragment.AttachedObject._origIntegrity = OrigIntegrity;
			qsbFragment.LeashLength = LeashLength;
			if (!OWMath.ApproxEquals(qsbFragment.AttachedObject._integrity, Integrity))
			{
				qsbFragment.AttachedObject._integrity = Integrity;
				qsbFragment.AttachedObject.CallOnTakeDamage();
			}

			if (IsDetached && !qsbFragment.IsDetached)
			{
				// the detach is delayed, so wait until that happens
				QSBCore.UnityEvents.RunWhen(() => qsbFragment.IsDetached, () =>
				{
					var body = qsbFragment.Body;

					if (IsThruWhiteHole && !qsbFragment.IsThruWhiteHole)
					{
						var whiteHoleVolume = MeteorManager.WhiteHoleVolume;
						var attachedFluidDetector = body.GetAttachedFluidDetector();
						var attachedForceDetector = body.GetAttachedForceDetector();
						if (attachedFluidDetector is ConstantFluidDetector constantFluidDetector)
						{
							constantFluidDetector.SetDetectableFluid(whiteHoleVolume._fluidVolume);
						}

						if (attachedForceDetector is ConstantForceDetector constantForceDetector)
						{
							constantForceDetector.ClearAllFields();
						}

						qsbFragment.DetachableFragment.ChangeFragmentSector(whiteHoleVolume._whiteHoleSector,
							whiteHoleVolume._whiteHoleProxyShadowSuperGroup);

						qsbFragment.DetachableFragment.EndWarpScaling();
						body.gameObject.AddComponent<DebrisLeash>().Init(whiteHoleVolume._whiteHoleBody, qsbFragment.LeashLength);
						whiteHoleVolume._ejectedBodyList.Add(body);
					}
					else if (!IsThruWhiteHole && qsbFragment.IsThruWhiteHole)
					{
						// should only happen if client is way too far ahead and they try to connect. we fail here.
						DebugLog.ToConsole($"{qsbFragment} is thru white hole, but msg is not. fuck", MessageType.Error);
						return;
					}

					if (qsbFragment.IsThruWhiteHole)
					{
						var debrisLeash = body.GetComponent<DebrisLeash>();
						debrisLeash._deccelerating = false;
						debrisLeash.enabled = true;
					}

					var refBody = qsbFragment.RefBody;
					var pos = refBody.transform.FromRelPos(RelPos);
					body.SetPosition(pos);
					body.SetRotation(refBody.transform.FromRelRot(RelRot));
					body.SetVelocity(refBody.FromRelVel(RelVel, pos));
					body.SetAngularVelocity(refBody.FromRelAngVel(RelAngVel));
				});
			}
			else if (!IsDetached && qsbFragment.IsDetached)
			{
				// should only happen if client is way too far ahead and they try to connect. we fail here.
				DebugLog.ToConsole($"{qsbFragment} is detached, but msg is not. fuck", MessageType.Error);
			}
		}
	}
}
