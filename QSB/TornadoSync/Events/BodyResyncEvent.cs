using QSB.Events;
using QSB.Syncs;

namespace QSB.TornadoSync.Events
{
	public class BodyResyncEvent : QSBEvent<BodyResyncMessage>
	{
		public override bool RequireWorldObjectsReady => true;

		public override void SetupListener()
			=> GlobalMessenger<OWRigidbody, OWRigidbody>.AddListener(EventNames.QSBBodyResync, Handler);

		public override void CloseListener()
			=> GlobalMessenger<OWRigidbody, OWRigidbody>.RemoveListener(EventNames.QSBBodyResync, Handler);

		private void Handler(OWRigidbody body, OWRigidbody refBody) => SendEvent(CreateMessage(body, refBody));

		private BodyResyncMessage CreateMessage(OWRigidbody body, OWRigidbody refBody)
		{
			var pos = body.GetPosition();
			return new BodyResyncMessage
			{
				BodyIndex = CenterOfTheUniverse.s_rigidbodies.IndexOf(body),
				RefBodyIndex = CenterOfTheUniverse.s_rigidbodies.IndexOf(refBody),
				Pos = refBody.transform.EncodePos(pos),
				Rot = refBody.transform.EncodeRot(body.GetRotation()),
				Vel = refBody.EncodeVel(body.GetVelocity(), pos),
				AngVel = refBody.EncodeAngVel(body.GetAngularVelocity())
			};
		}

		public override void OnReceiveRemote(bool isHost, BodyResyncMessage message)
		{
			var body = CenterOfTheUniverse.s_rigidbodies[message.BodyIndex];
			var refBody = CenterOfTheUniverse.s_rigidbodies[message.RefBodyIndex];
			var pos = refBody.transform.DecodePos(message.Pos);
			body.SetPosition(pos);
			body.SetRotation(refBody.transform.DecodeRot(message.Rot));
			body.SetVelocity(refBody.DecodeVel(message.Vel, pos));
			body.SetAngularVelocity(refBody.DecodeAngVel(message.AngVel));
		}
	}
}
