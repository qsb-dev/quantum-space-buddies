using QSB.WorldSync;

namespace QSB.OwnershipSync;

/// <summary>
/// a world object that has an owner
/// </summary>
public interface IOwnedWorldObject : IWorldObject
{
	/// <summary>
	/// 0 = owned by no one
	/// </summary>
	public uint Owner { get; set; }
	/// <summary>
	/// can the world object have authority
	/// </summary>
	public bool CanOwn { get; }
}
