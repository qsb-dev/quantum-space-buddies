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
				shape.OnShapeActivated -= (Shape s) => OnEnable();
				shape.OnShapeDeactivated -= (Shape s) => OnDisable();
			}
		}

		public override void Init(T attachedObject, int id)
		{
			foreach (var shape in GetAttachedShapes())
			{
				if (shape == null)
				{
					break;
				}
				shape.OnShapeActivated += (Shape s) => OnEnable();
				shape.OnShapeDeactivated += (Shape s) => OnDisable();
			}
			ControllingPlayer = 0u;
		}

		private List<Shape> GetAttachedShapes()
		{
			var visibilityTrackers = AttachedObject.GetValue<VisibilityTracker[]>("_visibilityTrackers");
			if (visibilityTrackers == null || visibilityTrackers.Length == 0)
			{
				DebugLog.DebugWrite($"Error - {AttachedObject.name} has null visibility trackers!");
				return new List<Shape>();
			}
			if (visibilityTrackers.Any(x => x.GetType() == typeof(RendererVisibilityTracker)))
			{
				DebugLog.DebugWrite($"Error - {AttachedObject.name} has a renderervisibilitytracker!");
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

		private void OnEnable()
		{
			if (IsEnabled)
			{
				return;
			}
			IsEnabled = true;
			if (!QSBCore.HasWokenUp && !QSBCore.IsServer)
			{
				return;
			}
			if (ControllingPlayer != 0)
			{
				// controlled by another player, dont care that we activate it
				return;
			}
			var id = QSBWorldSync.GetIdFromTypeSubset(this);
			// no one is controlling this object right now, request authority
			QSBEventManager.FireEvent(EventNames.QSBQuantumAuthority, id, QSBPlayerManager.LocalPlayerId);
		}

		private void OnDisable()
		{
			if (!IsEnabled)
			{
				return;
			}
			IsEnabled = false;
			if (!QSBCore.HasWokenUp && !QSBCore.IsServer)
			{
				return;
			}
			if (ControllingPlayer != QSBPlayerManager.LocalPlayerId)
			{
				// not being controlled by us, don't care if we leave area
				return;
			}
			var id = QSBWorldSync.GetIdFromTypeSubset(this);
			// send event to other players that we're releasing authority
			QSBEventManager.FireEvent(EventNames.QSBQuantumAuthority, id, 0u);
		}
	}
}
