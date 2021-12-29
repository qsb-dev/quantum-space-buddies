using QSB.Messaging;
using QSB.Player;
using QSB.Player.TransformSync;
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
		private float DegreesY;
		private Vector3 Vel;
		private Vector3 AngVel;

		public DebugTeleportInfoMessage(uint to)
		{
			To = to;

			var body = Locator.GetPlayerBody();
			var refBody = PlayerTransformSync.LocalInstance.ReferenceSector.AttachedObject.GetOWRigidbody();

			DegreesY = Locator.GetPlayerCameraController().GetDegreesY();
			Vel = refBody.ToRelVel(body.GetVelocity(), body.GetPosition());
			AngVel = refBody.ToRelAngVel(body.GetAngularVelocity());
		}

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(DegreesY);
			writer.Write(Vel);
			writer.Write(AngVel);
		}

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			DegreesY = reader.ReadSingle();
			Vel = reader.ReadVector3();
			AngVel = reader.ReadVector3();
		}

		public override void OnReceiveRemote()
		{
			var otherPlayer = QSBPlayerManager.GetPlayer(From);

			var body = Locator.GetPlayerBody();
			var refBody = otherPlayer.TransformSync.ReferenceSector.AttachedObject.GetOWRigidbody();

			var pos = otherPlayer.Body.transform.position;
			var rot = otherPlayer.Body.transform.rotation;
			body.WarpToPositionRotation(pos, rot);
			Locator.GetPlayerCameraController().SetDegreesY(DegreesY);
			body.SetVelocity(refBody.FromRelVel(Vel, pos));
			body.SetAngularVelocity(refBody.FromRelAngVel(AngVel));
		}
	}
}
