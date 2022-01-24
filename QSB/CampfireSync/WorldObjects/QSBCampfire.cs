using QSB.CampfireSync.Messages;
using QSB.Messaging;
using QSB.WorldSync;

namespace QSB.CampfireSync.WorldObjects
{
	public class QSBCampfire : WorldObject<Campfire>
	{
		public override void SendResyncInfo(uint to)
		{
			if (QSBCore.IsHost)
			{
				this.SendMessage(new CampfireStateMessage(GetState()) { To = to });
			}
		}

		public void StartRoasting()
			=> AttachedObject.StartRoasting();

		public Campfire.State GetState()
			=> AttachedObject.GetState();

		public void SetState(Campfire.State newState)
			=> AttachedObject.SetState(newState);
	}
}
