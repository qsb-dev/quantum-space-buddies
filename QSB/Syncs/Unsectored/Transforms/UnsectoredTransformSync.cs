using QSB.Utility;
using QSB.WorldSync;
using QuantumUNET.Transport;
using UnityEngine;

namespace QSB.Syncs.Unsectored.Transforms
{
	public abstract class UnsectoredTransformSync : BaseUnsectoredSync<Transform>
	{
		protected abstract Transform InitLocalTransform();
		protected abstract Transform InitRemoteTransform();

		protected override Transform SetAttachedObject()
			=> HasAuthority ? InitLocalTransform() : InitRemoteTransform();

		public override void SerializeTransform(QNetworkWriter writer, bool initialState)
		{
			base.SerializeTransform(writer, initialState);

			var worldPos = transform.position;
			var worldRot = transform.rotation;
			writer.Write(worldPos);
			SerializeRotation(writer, worldRot);
			_prevPosition = worldPos;
			_prevRotation = worldRot;
		}

		public override void DeserializeTransform(QNetworkReader reader, bool initialState)
		{
			if (!WorldObjectManager.AllObjectsReady)
			{
				reader.ReadVector3();
				DeserializeRotation(reader);
				return;
			}

			var pos = reader.ReadVector3();
			var rot = DeserializeRotation(reader);

			if (HasAuthority)
			{
				return;
			}

			transform.position = pos;
			transform.rotation = rot;
		}

		protected override bool UpdateTransform()
		{
			if (HasAuthority)
			{
				transform.position = ReferenceTransform.ToRelPos(AttachedObject.position);
				transform.rotation = ReferenceTransform.ToRelRot(AttachedObject.rotation);
				return true;
			}

			if (UseInterpolation)
			{
				AttachedObject.position = ReferenceTransform.FromRelPos(SmoothPosition);
				AttachedObject.rotation = ReferenceTransform.FromRelRot(SmoothRotation);
			}
			else
			{
				AttachedObject.position = ReferenceTransform.FromRelPos(transform.position);
				AttachedObject.rotation = ReferenceTransform.FromRelRot(transform.rotation);
			}

			return true;
		}
	}
}
