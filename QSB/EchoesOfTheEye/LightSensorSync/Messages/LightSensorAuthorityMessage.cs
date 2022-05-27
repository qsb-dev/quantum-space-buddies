using QSB.EchoesOfTheEye.LightSensorSync.WorldObjects;
using QSB.Messaging;
using QSB.Player;

namespace QSB.EchoesOfTheEye.LightSensorSync.Messages;

internal class LightSensorAuthorityMessage : QSBWorldObjectMessage<QSBLightSensor, uint>
{
	public LightSensorAuthorityMessage(uint authorityOwner) : base(authorityOwner) { }

	public override bool ShouldReceive
	{
		get
		{
			if (!base.ShouldReceive)
			{
				return false;
			}

			return (WorldObject.AuthorityOwner == 0 || Data == 0)
				&& WorldObject.AuthorityOwner != Data;
		}
	}

	public override void OnReceiveLocal() => WorldObject.AuthorityOwner = Data;

	public override void OnReceiveRemote()
	{
		WorldObject.AuthorityOwner = Data;
		if (WorldObject.AuthorityOwner == 0 && WorldObject.AttachedObject.enabled)
		{
			// object has no owner, but is still active for this player. request ownership
			WorldObject.SendMessage(new LightSensorAuthorityMessage(QSBPlayerManager.LocalPlayerId));
		}
	}
}
