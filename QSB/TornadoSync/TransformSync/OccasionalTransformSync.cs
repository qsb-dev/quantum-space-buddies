using QSB.Player.TransformSync;
using QSB.ShipSync.TransformSync;
using QSB.Syncs.Unsectored.Rigidbodies;
using QSB.Tools.ProbeTool.TransformSync;
using QSB.Utility;
using QSB.WorldSync;
using QuantumUNET.Transport;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace QSB.TornadoSync.TransformSync
{
	public class OccasionalTransformSync : UnsectoredRigidbodySync
	{
		public override bool IsReady => WorldObjectManager.AllObjectsReady
			&& CenterOfTheUniverse.s_rigidbodies.IsInRange(_bodyIndex)
			&& CenterOfTheUniverse.s_rigidbodies.IsInRange(_refBodyIndex);
		public override bool UseInterpolation => false;
		public override bool IsPlayerObject => false;

		protected override OWRigidbody GetRigidbody() => CenterOfTheUniverse.s_rigidbodies[_bodyIndex];

		private int _bodyIndex = -1;
		private int _refBodyIndex = -1;
		private Sector[] _sectors;
		private OWRigidbody[] _childBodies;

		public void InitBodyIndexes(OWRigidbody body, OWRigidbody refBody)
		{
			_bodyIndex = CenterOfTheUniverse.s_rigidbodies.IndexOf(body);
			_refBodyIndex = CenterOfTheUniverse.s_rigidbodies.IndexOf(refBody);
		}

		public override float GetNetworkSendInterval() => 20;

		protected override void Init()
		{
			base.Init();
			SetReferenceTransform(CenterOfTheUniverse.s_rigidbodies[_refBodyIndex].transform);

			_sectors = SectorManager.s_sectors
				.Where(x => x._attachedOWRigidbody == AttachedObject).ToArray();
			_childBodies = CenterOfTheUniverse.s_rigidbodies
				.Where(x => x._origParentBody == AttachedObject)
				.ToArray();
		}

		public override void SerializeTransform(QNetworkWriter writer, bool initialState)
		{
			base.SerializeTransform(writer, initialState);

			if (initialState)
			{
				writer.Write(_bodyIndex);
				writer.Write(_refBodyIndex);
			}
		}

		private bool _shouldUpdate;

		public override void DeserializeTransform(QNetworkReader reader, bool initialState)
		{
			base.DeserializeTransform(reader, initialState);

			if (initialState)
			{
				_bodyIndex = reader.ReadInt32();
				_refBodyIndex = reader.ReadInt32();
			}

			if (!WorldObjectManager.AllObjectsReady || HasAuthority)
			{
				return;
			}

			_shouldUpdate = true;
		}

		protected override bool UpdateTransform()
		{
			if (HasAuthority)
			{
				SetValuesToSync();
				return true;
			}

			if (!_shouldUpdate)
			{
				return false;
			}

			_shouldUpdate = false;

			var hasMoved = CustomHasMoved(
				transform.position,
				_localPrevPosition,
				transform.rotation,
				_localPrevRotation,
				_relativeVelocity,
				_localPrevVelocity,
				_relativeAngularVelocity,
				_localPrevAngularVelocity);

			_localPrevPosition = transform.position;
			_localPrevRotation = transform.rotation;
			_localPrevVelocity = _relativeVelocity;
			_localPrevAngularVelocity = _relativeAngularVelocity;

			if (!hasMoved)
			{
				return true;
			}

			if (_sectors.Contains(PlayerTransformSync.LocalInstance?.ReferenceSector?.AttachedObject))
			{
				QueueMove(Locator._playerBody);
			}
			if (_sectors.Contains(ShipTransformSync.LocalInstance?.ReferenceSector?.AttachedObject))
			{
				QueueMove(Locator._shipBody);
			}
			if (_sectors.Contains(PlayerProbeSync.LocalInstance?.ReferenceSector?.AttachedObject))
			{
				QueueMove(Locator._probe._owRigidbody);
			}
			foreach (var child in _childBodies)
			{
				QueueMove(child);
			}

			var pos = ReferenceTransform.FromRelPos(transform.position);
			AttachedObject.SetPosition(pos);
			AttachedObject.SetRotation(ReferenceTransform.FromRelRot(transform.rotation));
			AttachedObject.SetVelocity(ReferenceTransform.GetAttachedOWRigidbody().FromRelVel(_relativeVelocity, pos));
			AttachedObject.SetAngularVelocity(ReferenceTransform.GetAttachedOWRigidbody().FromRelAngVel(_relativeAngularVelocity));

			Move();

			return true;
		}


		private readonly List<MoveData> _toMove = new();

		private struct MoveData
		{
			public OWRigidbody Child;
			public Vector3 RelPos;
			public Quaternion RelRot;
			public Vector3 RelVel;
			public Vector3 RelAngVel;
		}

		private void QueueMove(OWRigidbody child)
		{
			if (!child)
			{
				return; // wtf
			}

			if (child.transform.parent != null)
			{
				// it's parented to AttachedObject or one of its children
				return;
			}

			var pos = child.GetPosition();
			_toMove.Add(new MoveData
			{
				Child = child,
				RelPos = AttachedObject.transform.ToRelPos(pos),
				RelRot = AttachedObject.transform.ToRelRot(child.GetRotation()),
				RelVel = AttachedObject.ToRelVel(child.GetVelocity(), pos),
				RelAngVel = AttachedObject.ToRelAngVel(child.GetAngularVelocity())
			});
		}

		private void Move()
		{
			foreach (var data in _toMove)
			{
				var pos = AttachedObject.transform.FromRelPos(data.RelPos);
				data.Child.SetPosition(pos);
				data.Child.SetRotation(AttachedObject.transform.FromRelRot(data.RelRot));
				data.Child.SetVelocity(AttachedObject.FromRelVel(data.RelVel, pos));
				data.Child.SetAngularVelocity(AttachedObject.FromRelAngVel(data.RelAngVel));
			}
			_toMove.Clear();
		}
	}
}
