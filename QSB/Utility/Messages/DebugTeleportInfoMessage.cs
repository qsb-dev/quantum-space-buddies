using QSB.Messaging;
using QSB.Player.TransformSync;
using QSB.SectorSync.WorldObjects;
using QSB.WorldSync;
using QuantumUNET.Transport;
using UnityEngine;

namespace QSB.Utility.Messages
{
	public class DebugRequestTeleportInfoMessage : QSBMessage
	{
		public DebugRequestTeleportInfoMessage(uint to) => To = to;

		public override void OnReceiveRemote() => new DebugTeleportInfoMessage(From).Send();
	}

	public class DebugTeleportInfoMessage : QSBMessage
	{
		private int SectorId;
		private Vector3 RelPos;
		private Quaternion RelRot;
		private float DegreesY;
		private Vector3 RelVel;
		private Vector3 RelAngVel;

		public DebugTeleportInfoMessage(uint to)
		{
			To = to;

			var qsbSector = PlayerTransformSync.LocalInstance.ReferenceSector;
			SectorId = qsbSector.ObjectId;

			var body = Locator.GetPlayerBody();
			var refBody = qsbSector.AttachedObject.GetOWRigidbody();

			var pos = body.GetPosition();
			RelPos = refBody.transform.ToRelPos(pos);
			RelRot = refBody.transform.ToRelRot(body.GetRotation());
			DegreesY = Locator.GetPlayerCameraController().GetDegreesY();
			RelVel = refBody.ToRelVel(body.GetVelocity(), pos);
			RelAngVel = refBody.ToRelAngVel(body.GetAngularVelocity());
		}

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(SectorId);
			writer.Write(RelPos);
			writer.Write(RelRot);
			writer.Write(DegreesY);
			writer.Write(RelVel);
			writer.Write(RelAngVel);
		}

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			SectorId = reader.ReadInt32();
			RelPos = reader.ReadVector3();
			RelRot = reader.ReadQuaternion();
			DegreesY = reader.ReadSingle();
			RelVel = reader.ReadVector3();
			RelAngVel = reader.ReadVector3();
		}

		public override void OnReceiveRemote()
		{
			var qsbSector = SectorId.GetWorldObject<QSBSector>();

			var body = Locator.GetPlayerBody();
			var refBody = qsbSector.AttachedObject.GetOWRigidbody();

			var pos = refBody.transform.FromRelPos(RelPos);
			body.SetPosition(pos);
			body.SetRotation(refBody.transform.FromRelRot(RelRot));
			Locator.GetPlayerCameraController().SetDegreesY(DegreesY);
			body.SetVelocity(refBody.FromRelVel(RelVel, pos));
			body.SetAngularVelocity(refBody.FromRelAngVel(RelAngVel));
		}
	}
}
