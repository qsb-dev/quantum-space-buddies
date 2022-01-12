using OWML.Common;
using QSB.Messaging;
using QSB.Player;
using QSB.QuantumSync.Messages;
using QSB.Utility;
using QSB.WorldSync;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace QSB.QuantumSync.WorldObjects
{
	internal abstract class QSBQuantumObject<T> : WorldObject<T>, IQSBQuantumObject
		where T : QuantumObject
	{
		/// <summary>
		/// whether the controlling player is always the host <br/>
		/// also means this object is considered always enabled
		/// </summary>
		protected virtual bool HostControls => false;
		public uint ControllingPlayer { get; set; }
		public bool IsEnabled { get; private set; }

		public override void OnRemoval()
		{
			if (HostControls)
			{
				return;
			}

			foreach (var shape in GetAttachedShapes())
			{
				shape.OnShapeActivated -= OnEnable;
				shape.OnShapeDeactivated -= OnDisable;
			}
		}

		public override void Init()
		{
			if (QSBCore.ShowQuantumVisibilityObjects)
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
					if (shape is BoxShape boxShape)
					{
						var newCube = Object.Instantiate(cube);
						newCube.transform.parent = shape.transform;
						newCube.transform.localPosition = Vector3.zero;
						newCube.transform.localRotation = Quaternion.Euler(0, 0, 0);
						newCube.transform.localScale = boxShape.size;
					}
					else if (shape is SphereShape sphereShape)
					{
						var newSphere = Object.Instantiate(sphere);
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

			StartDelayedReady();
			QSBCore.UnityEvents.FireInNUpdates(LateInit, 5);
		}

		private void LateInit()
		{
			FinishDelayedReady();

			if (HostControls)
			{
				// smallest player id is the host
				ControllingPlayer = QSBPlayerManager.PlayerList.Min(x => x.PlayerId);
				IsEnabled = true;
				return;
			}

			var attachedShapes = GetAttachedShapes();
			if (attachedShapes.Count == 0)
			{
				IsEnabled = false;
				return;
			}

			foreach (var shape in attachedShapes)
			{
				shape.OnShapeActivated += OnEnable;
				shape.OnShapeDeactivated += OnDisable;
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

				// if the tracker is not active, this won't have been set, so just do it ourselves
				tracker._shapes ??= tracker.GetComponents<Shape>();
				totalShapes.AddRange(tracker._shapes.Where(x => x != null));
			}

			return totalShapes;
		}

		public void SetIsQuantum(bool isQuantum) => AttachedObject.SetIsQuantum(isQuantum);

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

			// no one is controlling this object right now, request authority
			((IQSBQuantumObject)this).SendMessage(new QuantumAuthorityMessage(QSBPlayerManager.LocalPlayerId));
		}

		private void OnDisable(Shape s) =>
			// we wait a frame here in case the shapes get disabled as we switch from 1 visibility tracker to another
			QSBCore.UnityEvents.FireOnNextUpdate(() =>
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

				// send event to other players that we're releasing authority
				((IQSBQuantumObject)this).SendMessage(new QuantumAuthorityMessage(0u));
			});

		public override void DisplayLines()
		{
			if (AttachedObject == null)
			{
				return;
			}

			var localPlayer = QSBPlayerManager.LocalPlayer;

			if (localPlayer == null)
			{
				return;
			}

			var body = localPlayer.Body;

			if (body == null)
			{
				return;
			}

			if (ControllingPlayer == 0)
			{
				if (IsEnabled)
				{
					Popcron.Gizmos.Line(AttachedObject.transform.position,
						body.transform.position,
						Color.magenta * 0.25f);
				}

				return;
			}

			var player = QSBPlayerManager.GetPlayer(ControllingPlayer);

			if (player == null)
			{
				return;
			}

			var playerBody = player.Body;

			if (playerBody == null)
			{
				return;
			}

			Popcron.Gizmos.Line(AttachedObject.transform.position,
				playerBody.transform.position,
				Color.magenta);
		}
	}
}
