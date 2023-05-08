using QSB.Messaging;
using QSB.Player;

namespace QSB.OwnershipSync;

public static class IOwnedWorldObject_Extensions
{
	/// <summary>
	/// try and gain authority over the object
	/// </summary>
	public static void RequestOwnership(this IOwnedWorldObject @this)
	{
		if (@this.Owner != 0)
		{
			return;
		}
		@this.SendMessage(new OwnedWorldObjectMessage(QSBPlayerManager.LocalPlayerId));
	}

	/// <summary>
	/// release authority over the object,
	/// potentially to giving it to someone else
	/// </summary>
	public static void ReleaseOwnership(this IOwnedWorldObject @this)
	{
		if (@this.Owner != QSBPlayerManager.LocalPlayerId)
		{
			return;
		}
		@this.SendMessage(new OwnedWorldObjectMessage(0));
	}
}
