using QSB.WorldSync;

namespace QSB.AuthoritySync;

/// <summary>
/// a world object that has an owner
/// </summary>
public interface IAuthWorldObject : IWorldObject
{
	public uint Owner { get; set; }
	/// <summary>
	/// can the world object have authority
	/// </summary>
	public bool CanOwn { get; }
}
