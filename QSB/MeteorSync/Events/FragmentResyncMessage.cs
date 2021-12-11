using OWML.Common;
using QSB.Messaging;
using QSB.MeteorSync.WorldObjects;
using QSB.Syncs;
using QSB.Utility;
using QuantumUNET.Transport;
using UnityEngine;

namespace QSB.MeteorSync.Events
{
	/// called when we request a resync on client join
	/// pain
	public class FragmentResyncMessage : QSBWorldObjectMessage<QSBFragment>
	{
		private float _integrity;
		private float _origIntegrity;
		private float _leashLength;
		private bool _isDetached;

		private bool _isThruWhiteHole;
		private Vector3 _pos;
		private Quaternion _rot;
		private Vector3 _vel;
		private Vector3 _angVel;

		public FragmentResyncMessage(QSBFragment qsbFragment)
		{
			_integrity = qsbFragment.AttachedObject._integrity;
			_origIntegrity = qsbFragment.AttachedObject._origIntegrity;
			_leashLength = qsbFragment.LeashLength;
			_isDetached = qsbFragment.IsDetached;

			if (_isDetached)
			{
				_isThruWhiteHole = qsbFragment.IsThruWhiteHole;

				var body = qsbFragment.Body;
				var refBody = qsbFragment.RefBody;
				var pos = body.GetPosition();
				_pos = refBody.transform.EncodePos(pos);
				_rot = refBody.transform.EncodeRot(body.GetRotation());
				_vel = refBody.EncodeVel(body.GetVelocity(), pos);
				_angVel = refBody.EncodeAngVel(body.GetAngularVelocity());
			}
		}

		public FragmentResyncMessage() { }

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(_integrity);
			writer.Write(_origIntegrity);
			writer.Write(_leashLength);
			writer.Write(_isDetached);
			if (_isDetached)
			{
				writer.Write(_isThruWhiteHole);
				writer.Write(_pos);
				writer.Write(_rot);
				writer.Write(_vel);
				writer.Write(_angVel);
			}
		}

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			_integrity = reader.ReadSingle();
			_origIntegrity = reader.ReadSingle();
			_leashLength = reader.ReadSingle();
			_isDetached = reader.ReadBoolean();
			if (_isDetached)
			{
				_isThruWhiteHole = reader.ReadBoolean();
				_pos = reader.ReadVector3();
				_rot = reader.ReadQuaternion();
				_vel = reader.ReadVector3();
				_angVel = reader.ReadVector3();
			}
		}

		public override void OnReceiveRemote()
		{
			WorldObject.AttachedObject._integrity = _integrity;
			WorldObject.AttachedObject._origIntegrity = _origIntegrity;
			WorldObject.LeashLength = _leashLength;
			WorldObject.AttachedObject.CallOnTakeDamage();

			if (_isDetached)
			{
				// the detach is delayed, so wait until that happens
				QSBCore.UnityEvents.RunWhen(() => WorldObject.IsDetached, () =>
				{
					var body = WorldObject.Body;

					if (_isThruWhiteHole && !WorldObject.IsThruWhiteHole)
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

						WorldObject.DetachableFragment.ChangeFragmentSector(whiteHoleVolume._whiteHoleSector,
							whiteHoleVolume._whiteHoleProxyShadowSuperGroup);

						WorldObject.DetachableFragment.EndWarpScaling();
						body.gameObject.AddComponent<DebrisLeash>().Init(whiteHoleVolume._whiteHoleBody, WorldObject.LeashLength);
						whiteHoleVolume._ejectedBodyList.Add(body);
					}
					else if (!_isThruWhiteHole && WorldObject.IsThruWhiteHole)
					{
						// should only happen if client is way too far ahead and they try to connect. we fail here.
						DebugLog.ToConsole($"{WorldObject.LogName} is thru white hole, but msg is not. fuck", MessageType.Error);
						return;
					}

					if (WorldObject.IsThruWhiteHole)
					{
						var debrisLeash = body.GetComponent<DebrisLeash>();
						debrisLeash._deccelerating = false;
						debrisLeash.enabled = true;
					}

					var refBody = WorldObject.RefBody;
					var pos = refBody.transform.DecodePos(_pos);
					body.SetPosition(pos);
					body.SetRotation(refBody.transform.DecodeRot(_rot));
					body.SetVelocity(refBody.DecodeVel(_vel, pos));
					body.SetAngularVelocity(refBody.DecodeAngVel(_angVel));
				});
			}
			else if (!_isDetached && WorldObject.IsDetached)
			{
				// should only happen if client is way too far ahead and they try to connect. we fail here.
				DebugLog.ToConsole($"{WorldObject.LogName} is detached, but msg is not. fuck", MessageType.Error);
			}
		}
	}
}
