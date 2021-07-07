using OWML.Common;
using QSB.Player;
using QSB.Utility;
using QuantumUNET.Components;
using UnityEngine;

namespace QSB.Syncs
{
	public abstract class SyncBase<T> : QNetworkTransform
		where T : Component
	{
		public uint AttachedNetId
		{
			get
			{
				if (NetIdentity == null)
				{
					DebugLog.ToConsole($"Error - Trying to get AttachedNetId with null NetIdentity! Type:{GetType().Name} GrandType:{GetType().GetType().Name}", MessageType.Error);
					return uint.MaxValue;
				}

				return NetIdentity.NetId.Value;
			}
		}

		public uint PlayerId
		{
			get
			{
				if (NetIdentity == null)
				{
					DebugLog.ToConsole($"Error - Trying to get PlayerId with null NetIdentity! Type:{GetType().Name} GrandType:{GetType().GetType().Name}", MessageType.Error);
					return uint.MaxValue;
				}

				return NetIdentity.RootIdentity != null
					? NetIdentity.RootIdentity.NetId.Value
					: AttachedNetId;
			}
		}

		public PlayerInfo Player => QSBPlayerManager.GetPlayer(PlayerId);

		public abstract bool IsReady { get; }
		public abstract bool UseInterpolation { get; }

		public T AttachedObject { get; set; }
		public Transform ReferenceTransform { get; set; }

		protected string _logName => $"{PlayerId}.{GetType().Name}";
		protected virtual float DistanceLeeway { get; } = 5f;
		private float _previousDistance;
		protected const float SmoothTime = 0.1f;
		protected Vector3 _positionSmoothVelocity;
		protected Quaternion _rotationSmoothVelocity;
		protected IntermediaryTransform _intermediaryTransform;
		protected bool _isInitialized;

		protected abstract T InitLocalTransform();
		protected abstract T InitRemoteTransform();
		protected abstract void UpdateTransform();
		protected abstract void Init();

		protected Vector3 SmartSmoothDamp(Vector3 currentPosition, Vector3 targetPosition)
		{
			var distance = Vector3.Distance(currentPosition, targetPosition);
			if (distance > _previousDistance + DistanceLeeway)
			{
				_previousDistance = distance;
				return targetPosition;
			}

			_previousDistance = distance;
			return Vector3.SmoothDamp(currentPosition, targetPosition, ref _positionSmoothVelocity, SmoothTime);
		}

		public override void Update()
		{
			if (!_isInitialized && IsReady)
			{
				Init();
			}
			else if (_isInitialized && !IsReady)
			{
				_isInitialized = false;
				return;
			}

			if (!_isInitialized)
			{
				return;
			}

			if (AttachedObject == null)
			{
				DebugLog.ToConsole($"Warning - AttachedObject {_logName} is null.", MessageType.Warning);
				_isInitialized = false;
				return;
			}

			if (!AttachedObject.gameObject.activeInHierarchy)
			{
				return;
			}

			if (ReferenceTransform == null)
			{
				return;
			}

			if (AttachedObject.transform.parent != ReferenceTransform && !HasAuthority)
			{
				DebugLog.ToConsole($"Warning - For {_logName}, AttachedObject's ({AttachedObject.name}) parent is not the same as ReferenceTransform! " +
					$"({AttachedObject.transform.parent} v {ReferenceTransform.name})" +
					$"Did you try to manually reparent AttachedObject?", MessageType.Error);

			}

			UpdateTransform();

			base.Update();
		}

		protected virtual void OnRenderObject()
		{
			if (!QSBCore.WorldObjectsReady
				|| !QSBCore.DebugMode
				|| !QSBCore.ShowLinesInDebug
				|| !IsReady
				|| ReferenceTransform == null)
			{
				return;
			}

			/* Red Cube = Where visible object should be
			 * Green/Yellow Cube = Where visible object is
			 * Red Line = Connection between Red Cube and Green/Yellow Cube
			 * Cyan Line = Connection between Green/Yellow cube and reference transform
			 */

			Popcron.Gizmos.Cube(_intermediaryTransform.GetTargetPosition_Unparented(), _intermediaryTransform.GetTargetRotation_Unparented(), Vector3.one / 2, Color.red);
			Popcron.Gizmos.Line(_intermediaryTransform.GetTargetPosition_Unparented(), AttachedObject.transform.position, Color.red);
			var color = HasMoved() ? Color.green : Color.yellow;
			Popcron.Gizmos.Cube(AttachedObject.transform.position, AttachedObject.transform.rotation, Vector3.one / 2, color);
			Popcron.Gizmos.Line(AttachedObject.transform.position, ReferenceTransform.position, Color.cyan);
		}
	}
}
