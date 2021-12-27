using OWML.Common;
using QSB.Messaging;
using QSB.MeteorSync.WorldObjects;
using QSB.Utility;
using QSB.WorldSync;
using QuantumUNET.Transport;
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
		private Vector3 Pos;
		private Quaternion Rot;
		private Vector3 Vel;
		private Vector3 AngVel;

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
				Pos = refBody.transform.ToRelPos(pos);
				Rot = refBody.transform.ToRelRot(body.GetRotation());
				Vel = refBody.ToRelVel(body.GetVelocity(), pos);
				AngVel = refBody.ToRelAngVel(body.GetAngularVelocity());
			}
		}

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(Integrity);
			writer.Write(OrigIntegrity);
			writer.Write(LeashLength);
			writer.Write(IsDetached);
			if (IsDetached)
			{
				writer.Write(IsThruWhiteHole);
				writer.Write(Pos);
				writer.Write(Rot);
				writer.Write(Vel);
				writer.Write(AngVel);
			}
		}

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			Integrity = reader.ReadSingle();
			OrigIntegrity = reader.ReadSingle();
			LeashLength = reader.ReadSingle();
			IsDetached = reader.ReadBoolean();
			if (IsDetached)
			{
				IsThruWhiteHole = reader.ReadBoolean();
				Pos = reader.ReadVector3();
				Rot = reader.ReadQuaternion();
				Vel = reader.ReadVector3();
				AngVel = reader.ReadVector3();
			}
		}

		public override void OnReceiveRemote()
		{
			var qsbFragment = ObjectId.GetWorldObject<QSBFragment>();
			qsbFragment.AttachedObject._integrity = Integrity;
			qsbFragment.AttachedObject._origIntegrity = OrigIntegrity;
			qsbFragment.LeashLength = LeashLength;
			qsbFragment.AttachedObject.CallOnTakeDamage();

			if (IsDetached)
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
					var pos = refBody.transform.FromRelPos(Pos);
					body.SetPosition(pos);
					body.SetRotation(refBody.transform.FromRelRot(Rot));
					body.SetVelocity(refBody.FromRelVel(Vel, pos));
					body.SetAngularVelocity(refBody.FromRelAngVel(AngVel));
				});
			}
			else if (!IsDetached && qsbFragment.IsDetached)
			{
				// should only happen if client is way too far ahead and they try to connect. we fail here.
				DebugLog.ToConsole($"{qsbFragment.LogName} is detached, but msg is not. fuck", MessageType.Error);
			}
		}
	}
}