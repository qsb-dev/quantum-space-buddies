using QSB.Messaging;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.AuthoritySync;

/// <summary>
/// helper implementation of the interface
/// </summary>
public abstract class AuthWorldObject<T> : WorldObject<T>, IAuthWorldObject
	where T : MonoBehaviour
{
	public uint Owner { get; set; }
	public abstract bool CanOwn { get; }

	public override void SendInitialState(uint to)
	{
		((IAuthWorldObject)this).SendMessage(new WorldObjectAuthMessage(Owner) { To = to });
	}
}
