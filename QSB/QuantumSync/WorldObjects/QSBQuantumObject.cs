﻿using OWML.Common;
using QSB.Events;
using QSB.Player;
using QSB.Utility;
using QSB.WorldSync;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace QSB.QuantumSync.WorldObjects
{
	internal abstract class QSBQuantumObject<T> : WorldObject<T>, IQSBQuantumObject
		where T : QuantumObject
	{
		public uint ControllingPlayer { get; set; }
		public bool IsEnabled { get; set; }

		public override void OnRemoval()
		{
			foreach (var shape in GetAttachedShapes())
			{
				shape.OnShapeActivated -= OnEnable;
				shape.OnShapeDeactivated -= OnDisable;
			}
		}

		public override void Init(T attachedObject, int id)
		{
			var debugBundle = QSBCore.DebugAssetBundle;
			var sphere = debugBundle.LoadAsset<GameObject>("Assets/Prefabs/Sphere.prefab");
			var cube = debugBundle.LoadAsset<GameObject>("Assets/Prefabs/Cube.prefab");
			var capsule = debugBundle.LoadAsset<GameObject>("Assets/Prefabs/Capsule.prefab");

			if (cube == null)
			{
				DebugLog.DebugWrite($"CUBE IS NULL");
			}

			if (sphere == null)
			{
				DebugLog.DebugWrite($"SPHERE IS NULL");
			}

			if (capsule == null)
			{
				DebugLog.DebugWrite($"CAPSULE IS NULL");
			}

			foreach (var shape in GetAttachedShapes())
			{
				if (shape == null)
				{
					break;
				}

				if (QSBCore.ShowQuantumVisibilityObjects)
				{
					if (shape is BoxShape boxShape)
					{
						var newCube = UnityEngine.Object.Instantiate(cube);
						newCube.transform.parent = shape.transform;
						newCube.transform.localPosition = Vector3.zero;
						newCube.transform.localRotation = Quaternion.Euler(0, 0, 0);
						newCube.transform.localScale = boxShape.size;
					}
					else if (shape is SphereShape sphereShape)
					{
						var newSphere = UnityEngine.Object.Instantiate(sphere);
						newSphere.transform.parent = shape.transform;
						newSphere.transform.localPosition = Vector3.zero;
						newSphere.transform.localRotation = Quaternion.Euler(0, 0, 0);
						newSphere.transform.localScale = Vector3.one * (sphereShape.radius * 2);
					}
					else if (shape is CapsuleShape capsuleShape)
					{
						var newCapsule = Object.Instantiate(capsule);
						newCapsule.transform.parent = shape.transform;
						newCapsule.transform.localPosition = Vector3.zero;
						newCapsule.transform.localRotation = Quaternion.Euler(0, 0, 0);
						newCapsule.transform.localScale = new Vector3(capsuleShape.radius * 2, capsuleShape.height, capsuleShape.radius * 2);
					}
				}
			}

			QSBCore.UnityEvents.FireInNUpdates(LateInit, 5);
		}

		private void LateInit()
		{
			foreach (var shape in GetAttachedShapes())
			{
				shape.OnShapeActivated += OnEnable;
				shape.OnShapeDeactivated += OnDisable;
			}

			var attachedShapes = GetAttachedShapes();

			if (attachedShapes.Count == 0)
			{
				IsEnabled = false;
				return;
			}

			if (attachedShapes.All(x => x.enabled && x.gameObject.activeInHierarchy && x.active))
			{
				IsEnabled = true;
			}
			else
			{
				ControllingPlayer = 0u;
				IsEnabled = false;
			}
		}

		public List<ShapeVisibilityTracker> GetVisibilityTrackers() 
			=> AttachedObject?._visibilityTrackers == null
				? new()
				: AttachedObject._visibilityTrackers.Select(x => (ShapeVisibilityTracker)x).ToList();

		public List<Shape> GetAttachedShapes()
		{
			if (AttachedObject == null)
			{
				return new List<Shape>();
			}

			var visibilityTrackers = AttachedObject._visibilityTrackers;
			if (visibilityTrackers == null || visibilityTrackers.Length == 0)
			{
				return new List<Shape>();
			}

			if (visibilityTrackers.Any(x => x.GetType() == typeof(RendererVisibilityTracker)))
			{
				DebugLog.ToConsole($"Warning - {AttachedObject.name} has a RendererVisibilityTracker!", MessageType.Warning);
				return new List<Shape>();
			}

			var totalShapes = new List<Shape>();
			foreach (ShapeVisibilityTracker tracker in visibilityTrackers)
			{
				if (tracker == null)
				{
					DebugLog.ToConsole($"Warning - a ShapeVisibilityTracker in {LogName} is null!", MessageType.Warning);
					continue;
				}

				var shapes = tracker._shapes;
				totalShapes.AddRange(shapes);
			}

			return totalShapes;
		}

		private void OnEnable(Shape s)
		{
			if (IsEnabled)
			{
				return;
			}

			IsEnabled = true;
			if (!WorldObjectManager.AllObjectsReady && !QSBCore.IsHost)
			{
				return;
			}

			if (ControllingPlayer != 0)
			{
				// controlled by another player, dont care that we activate it
				return;
			}

			var id = ObjectId;
			// no one is controlling this object right now, request authority
			QSBEventManager.FireEvent(EventNames.QSBQuantumAuthority, id, QSBPlayerManager.LocalPlayerId);
		}

		private void OnDisable(Shape s)
		{
			if (!IsEnabled)
			{
				return;
			}

			if (GetAttachedShapes().Any(x => x.isActiveAndEnabled))
			{
				return;
			}

			IsEnabled = false;
			if (!WorldObjectManager.AllObjectsReady && !QSBCore.IsHost)
			{
				return;
			}

			if (ControllingPlayer != QSBPlayerManager.LocalPlayerId)
			{
				// not being controlled by us, don't care if we leave area
				return;
			}

			var id = ObjectId;
			// send event to other players that we're releasing authority
			QSBEventManager.FireEvent(EventNames.QSBQuantumAuthority, id, 0u);
		}
	}
}
