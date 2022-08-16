using QSB.Messaging;
using QSB.Player;

namespace QSB.AuthoritySync;

public static class IAuthWorldObject_Extensions
{
	/// <summary>
	/// try and gain authority over the object
	/// </summary>
	public static void RequestOwnership(this IAuthWorldObject authWorldObject)
	{
		if (authWorldObject.Owner != 0)
		{
			return;
		}
		authWorldObject.SendMessage(new AuthWorldObjectMessage(QSBPlayerManager.LocalPlayerId));
	}

	/// <summary>
	/// release authority over the object,
	/// potentially to giving it to someone else
	/// </summary>
	public static void ReleaseOwnership(this IAuthWorldObject authWorldObject)
	{
		if (authWorldObject.Owner != QSBPlayerManager.LocalPlayerId)
		{
			return;
		}
		authWorldObject.SendMessage(new AuthWorldObjectMessage(0));
	}
}
