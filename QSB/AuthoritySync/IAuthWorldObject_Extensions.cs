using QSB.Messaging;
using QSB.Player;

namespace QSB.AuthoritySync;

public static class IAuthWorldObject_Extensions
{
	/// <summary>
	/// try and gain authority over the object
	/// </summary>
	public static void RequestOwnership(this IAuthWorldObject @this)
	{
		if (@this.Owner != 0)
		{
			return;
		}

		@this.SendMessage(new AuthWorldObjectMessage(QSBPlayerManager.LocalPlayerId));
	}

	/// <summary>
	/// release authority over the object,
	/// potentially to giving it to someone else
	/// </summary>
	public static void ReleaseOwnership(this IAuthWorldObject @this)
	{
		if (@this.Owner != QSBPlayerManager.LocalPlayerId)
		{
			return;
		}

		@this.SendMessage(new AuthWorldObjectMessage(0));
	}
}
