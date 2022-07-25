using QSB.Player;
using UnityEngine;

namespace QSB.ItemSync;

/// <summary>
/// used for initial state sync.
/// we have to store this separately because its not saved in the item itself, unfortunately.
/// </summary>
public class ItemState
{
	/// <summary>
	/// if this is false, there's no need to sync initial state for this item
	/// </summary>
	public bool HasBeenInteractedWith;

	public ItemStateType State;

	// on ground
	public Transform Parent;
	public Vector3 LocalPosition;
	public Vector3 WorldPosition => Parent.TransformPoint(LocalPosition);
	public Vector3 LocalNormal;
	public Vector3 WorldNormal => Parent.TransformDirection(LocalNormal);
	public Sector Sector;
	public IItemDropTarget CustomDropTarget;
	public OWRigidbody Rigidbody;

	// held
	public PlayerInfo HoldingPlayer;

	// socketed
	public OWItemSocket Socket;
}

public enum ItemStateType
{
	OnGround,
	Held,
	Socketed
}
