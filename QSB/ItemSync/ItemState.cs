using QSB.Player;
using UnityEngine;

namespace QSB.ItemSync;

public class ItemState
{
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