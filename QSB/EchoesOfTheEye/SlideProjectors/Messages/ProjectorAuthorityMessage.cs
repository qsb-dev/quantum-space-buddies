using Mirror;
using QSB.EchoesOfTheEye.SlideProjectors.WorldObjects;
using QSB.Messaging;

namespace QSB.EchoesOfTheEye.SlideProjectors.Messages;

public class ProjectorAuthorityMessage : QSBWorldObjectMessage<QSBSlideProjector>
{
	private uint AuthorityOwner;

	public ProjectorAuthorityMessage(uint authorityOwner) => AuthorityOwner = authorityOwner;

	public override void Serialize(NetworkWriter writer)
	{
		base.Serialize(writer);
		writer.Write(AuthorityOwner);
	}

	public override void Deserialize(NetworkReader reader)
	{
		base.Deserialize(reader);
		AuthorityOwner = reader.Read<uint>();
	}

	public override bool ShouldReceive
	{
		get
		{
			if (!base.ShouldReceive)
			{
				return false;
			}

			// Deciding if to change the object's owner
			//		  Message
			//	   | = 0 | > 0 |
			// = 0 | No  | Yes |
			// > 0 | Yes | No  |
			// if Obj==Message then No
			// Obj

			return (WorldObject.ControllingPlayer == 0 || AuthorityOwner == 0)
			       && WorldObject.ControllingPlayer != AuthorityOwner;
		}
	}

	public override void OnReceiveLocal() => OnReceiveRemote();

	public override void OnReceiveRemote()
	{
		WorldObject.ControllingPlayer = AuthorityOwner;
		WorldObject.OnChangeAuthority(AuthorityOwner);
	}
}