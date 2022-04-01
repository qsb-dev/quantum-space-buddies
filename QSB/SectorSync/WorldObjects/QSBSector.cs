using OWML.Common;
using QSB.Utility;
using QSB.WorldSync;
using System;
using System.Linq;
using UnityEngine;

namespace QSB.SectorSync.WorldObjects;

public class QSBSector : WorldObject<Sector>
{
	public Sector.Name Type => AttachedObject.GetName();
	public Transform Transform
	{
		get
		{
			if (AttachedObject == null)
			{
				DebugLog.ToConsole($"Error - Tried to get Transform from QSBSector {ObjectId} with null AttachedObject!\r\n{Environment.StackTrace}", MessageType.Error);
				return null;
			}

			return AttachedObject.transform;
		}
	}

	public override void SendInitialState(uint to) { }

	public bool ShouldSyncTo(DynamicOccupant occupantType)
	{
		if (occupantType == DynamicOccupant.Ship && Type == Sector.Name.Ship)
		{
			return false;
		}

		if (AttachedObject == null)
		{
			DebugLog.ToConsole($"Warning - AttachedObject for sector id:{ObjectId} is null!", MessageType.Warning);
			return false;
		}

		if (!AttachedObject.gameObject.activeInHierarchy)
		{
			return false;
		}

		if (AttachedObject.name is "Sector_Shuttle" or "Sector_NomaiShuttleInterior")
		{
			if (QSBSceneManager.CurrentScene == OWScene.SolarSystem)
			{
				var shuttleController = AttachedObject.gameObject.GetComponentInParent<NomaiShuttleController>();
				if (shuttleController == null)
				{
					DebugLog.ToConsole($"Warning - Expected to find a NomaiShuttleController for {AttachedObject.name}!", MessageType.Warning);
					return false;
				}

				if (!shuttleController.IsPlayerInside())
				{
					return false;
				}
			}
			else if (QSBSceneManager.CurrentScene == OWScene.EyeOfTheUniverse)
			{
				// TODO : Optimize this out.
				var shuttleController = QSBWorldSync.GetUnityObjects<EyeShuttleController>().First();
				if (shuttleController == null)
				{
					DebugLog.ToConsole($"Warning - Expected to find a EyeShuttleController for {AttachedObject.name}!", MessageType.Warning);
					return false;
				}

				if (!shuttleController._isPlayerInside)
				{
					return false;
				}
			}
		}

		return true;
	}

	public float GetScore(OWRigidbody rigidbody)
	{
		var sqrDistance = (Transform.position - rigidbody.GetPosition()).sqrMagnitude;
		var radius = GetRadius();
		var sqrVelocity = GetSqrVelocity(rigidbody);

		return sqrDistance + radius * radius + sqrVelocity;
	}

	private float GetRadius()
	{
		// TODO : make this work for other stuff, not just shaped triggervolumes
		var trigger = AttachedObject.GetTriggerVolume();
		if (trigger && trigger.GetShape())
		{
			return trigger.GetShape().CalcWorldBounds().radius;
		}

		return 0f;
	}

	private float GetSqrVelocity(OWRigidbody rigidbody)
	{
		var sectorRigidbody = AttachedObject.GetOWRigidbody();
		if (sectorRigidbody && rigidbody)
		{
			var relativeVelocity = rigidbody.GetVelocity() - sectorRigidbody.GetPointVelocity(rigidbody.GetPosition());
			return relativeVelocity.sqrMagnitude;
		}

		return 0;
	}
}
