using OWML.Common;
using OWML.Utils;
using QSB.Events;
using QSB.Player;
using QSB.Utility;
using QSB.WorldSync;
using System.Collections.Generic;
using System.Linq;

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
				DebugLog.ToConsole($"Warning - {AttachedObject.name} has null visibility trackers!", MessageType.Warning);
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
			if (!QSBCore.HasWokenUp && !QSBCore.IsServer)
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
			if (GetAttachedShapes().Any(x => x.gameObject.activeInHierarchy))
			{
				return;
			}
			IsEnabled = false;
			if (!QSBCore.HasWokenUp && !QSBCore.IsServer)
			{
				return;
			}
			if (ControllingPlayer == 0)
			{
				// not controlled by anyone, but was just disabled...?
				DebugLog.ToConsole($"Warning - {AttachedObject.name} was just disabled, but previously had no controller.", MessageType.Warning);
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
