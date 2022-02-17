using Cysharp.Threading.Tasks;
using OWML.Common;
using QSB.Messaging;
using QSB.Player;
using QSB.QuantumSync.Messages;
using QSB.Utility;
using QSB.WorldSync;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

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

		public override async UniTask Init(CancellationToken ct)
		{
			await UniTask.DelayFrame(5, cancellationToken: ct);

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

		public override void SendInitialState(uint to) =>
			((IQSBQuantumObject)this).SendMessage(new QuantumAuthorityMessage(ControllingPlayer) { To = to });

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
					DebugLog.ToConsole($"Warning - a ShapeVisibilityTracker in {this} is null!", MessageType.Warning);
					continue;
				}

				// if the tracker is not active, this won't have been set, so just do it ourselves
				tracker._shapes ??= tracker.GetComponents<Shape>();
				totalShapes.AddRange(tracker._shapes.Where(x => x != null));
			}

			return totalShapes;
		}

		public void SetIsQuantum(bool isQuantum) => AttachedObject._isQuantum = isQuantum;

		private void OnEnable(Shape s)
		{
			if (IsEnabled)
			{
				return;
			}

			IsEnabled = true;
			if (!QSBWorldSync.AllObjectsReady && !QSBCore.IsHost)
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
			Delay.RunNextFrame(() =>
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
				if (!QSBWorldSync.AllObjectsReady && !QSBCore.IsHost)
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
