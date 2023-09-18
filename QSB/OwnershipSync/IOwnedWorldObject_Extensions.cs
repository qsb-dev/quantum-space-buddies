using QSB.Messaging;
using QSB.Player;

namespace QSB.OwnershipSync;

public static class IOwnedWorldObject_Extensions
{
	/// <summary>
	/// try and gain ownership over the object
	///
	/// does nothing if object we cant own this object or there is already another owner
	/// </summary>
	public static void RequestOwnership(this IOwnedWorldObject @this)
	{
		if (!@this.CanOwn)
		{
			return;
		}
		if (@this.Owner != 0)
		{
			return;
		}
		@this.SendMessage(new OwnedWorldObjectMessage(QSBPlayerManager.LocalPlayerId));
	}

	/// <summary>
	/// forcibly gain ownership over the object
	/// </summary>
	public static void ForceOwnership(this IOwnedWorldObject @this)
	{
		@this.SendMessage(new OwnedWorldObjectMessage(QSBPlayerManager.LocalPlayerId));
	}

	/// <summary>
	/// release ownership over the object,
	/// potentially to giving it to someone else
	///
	/// does nothing if someone else already owns this 
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
