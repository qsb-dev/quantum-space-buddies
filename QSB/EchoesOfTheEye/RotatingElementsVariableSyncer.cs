using Mirror;
using QSB.AuthoritySync;
using QSB.Utility.LinkedWorldObject;
using QSB.Utility.VariableSync;
using QSB.WorldSync;
using System;
using UnityEngine;

namespace QSB.EchoesOfTheEye;

public abstract class RotatingElementsVariableSyncer<TWorldObject> : BaseVariableSyncer<Quaternion[]>, ILinkedNetworkBehaviour
	where TWorldObject : IWorldObject
{
	public override void OnStartClient()
	{
		if (QSBCore.IsHost)
		{
			netIdentity.RegisterAuthQueue();
		}

		base.OnStartClient();
	}

	public override void OnStopClient()
	{
		if (QSBCore.IsHost)
		{
			netIdentity.UnregisterAuthQueue();
		}

		base.OnStopClient();
	}

	protected abstract Transform[] RotatingElements { get; }

	protected override bool HasChanged()
	{
		var rotatingElements = RotatingElements;
		Value ??= new Quaternion[rotatingElements.Length];
		PrevValue ??= new Quaternion[rotatingElements.Length];

		for (var i = 0; i < rotatingElements.Length; i++)
		{
			Value[i] = rotatingElements[i].localRotation;
		}

		for (var i = 0; i < rotatingElements.Length; i++)
		{
			if (Quaternion.Angle(Value[i], PrevValue[i]) > 1)
			{
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// copy array instead of setting, or else changes to values also happen on prev value
	/// </summary>
	protected override void UpdatePrevData() =>
		Array.Copy(Value, PrevValue, Value.Length);

	protected override void Deserialize(NetworkReader reader)
	{
		base.Deserialize(reader);

		var rotatingElements = RotatingElements;
		for (var i = 0; i < rotatingElements.Length; i++)
		{
			rotatingElements[i].localRotation = Value[i];
		}
	}

	protected TWorldObject WorldObject { get; private set; }
	public void SetWorldObject(IWorldObject worldObject) => WorldObject = (TWorldObject)worldObject;
}
