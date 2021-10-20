using OWML.Common;
using OWML.Utils;
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
				shape.OnShapeActivated -= (Shape s)
					=> QSBCore.UnityEvents.FireOnNextUpdate(() => OnEnable(s));

				shape.OnShapeDeactivated -= (Shape s)
					=> QSBCore.UnityEvents.FireOnNextUpdate(() => OnDisable(s));
			}
		}

		public override void Init(T attachedObject, int id)
		{
			var debugBundle = QSBCore.DebugAssetBundle;
			var sphere = debugBundle.LoadAsset<GameObject>("Assets/Sphere.prefab");
			var cube = debugBundle.LoadAsset<GameObject>("Assets/Cube.prefab");
			var capsule = debugBundle.LoadAsset<GameObject>("Assets/Capsule.prefab");

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
				// Firing next update to give time for shapes to actually be disabled

				shape.OnShapeActivated += (Shape s)
					=> QSBCore.UnityEvents.FireOnNextUpdate(() => OnEnable(s));

				shape.OnShapeDeactivated += (Shape s)
					=> QSBCore.UnityEvents.FireOnNextUpdate(() => OnDisable(s));

				if (QSBCore.DebugMode)
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

			if (GetAttachedShapes().Any(x => !x.enabled || !x.active))
			{
				ControllingPlayer = 0u;
				IsEnabled = false;
			}
			else
			{
				IsEnabled = true;
			}
		}

		private List<Shape> GetAttachedShapes()
		{
			if (AttachedObject == null)
			{
				return new List<Shape>();
			}

			var visibilityTrackers = AttachedObject.GetValue<VisibilityTracker[]>("_visibilityTrackers");
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
			foreach (var tracker in visibilityTrackers)
			{
				var shapes = tracker.GetValue<Shape[]>("_shapes");
				totalShapes.AddRange(shapes);
			}

			return totalShapes;
		}

		private void OnEnable(Shape s)
		{
			IsEnabled = true;
			if (!QSBCore.WorldObjectsReady && !QSBCore.IsHost)
			{
				return;
			}

			if (ControllingPlayer != 0)
			{
				// controlled by another player, dont care that we activate it
				return;
			}

			var id = QSBWorldSync.GetIdFromTypeSubset<IQSBQuantumObject>(this);
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
			if (!QSBCore.WorldObjectsReady && !QSBCore.IsHost)
			{
				return;
			}

			if (ControllingPlayer != QSBPlayerManager.LocalPlayerId)
			{
				// not being controlled by us, don't care if we leave area
				return;
			}

			var id = QSBWorldSync.GetIdFromTypeSubset<IQSBQuantumObject>(this);
			// send event to other players that we're releasing authority
			QSBEventManager.FireEvent(EventNames.QSBQuantumAuthority, id, 0u);
		}
	}
}
