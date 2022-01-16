using OWML.Common;
using QSB.Player;
using QSB.Utility;
using QSB.WorldSync;
using System;
using System.Linq;
using UnityEngine;

namespace QSB.Syncs
{
	/*
	 * Rewrite number : 10
	 * God has cursed me for my hubris, and my work is never finished.
	 */

	public abstract class SyncBase : QSBNetworkTransform
	{
		/// <summary>
		/// valid if IsPlayerObject, otherwise null
		/// </summary>
		public PlayerInfo Player { get; private set; }

		private bool _baseIsReady
		{
			get
			{
				if (netId is uint.MaxValue or 0)
				{
					return false;
				}

				if (!WorldObjectManager.AllObjectsAdded)
				{
					return false;
				}

				if (IsPlayerObject)
				{
					if (!Player.IsReady && !isLocalPlayer)
					{
						return false;
					}
				}

				return true;
			}
		}

		public abstract bool IsReady { get; }
		public abstract bool UseInterpolation { get; }
		public abstract bool IgnoreDisabledAttachedObject { get; }
		public abstract bool IgnoreNullReferenceTransform { get; }
		public abstract bool DestroyAttachedObject { get; }
		public abstract bool IsPlayerObject { get; }

		public Transform AttachedTransform { get; set; }
		public Transform ReferenceTransform { get; set; }

		public string LogName => (IsPlayerObject ? $"{Player.PlayerId}." : string.Empty) + $"{netId}:{GetType().Name}";
		protected virtual float DistanceLeeway => 5f;
		private float _previousDistance;
		protected const float SmoothTime = 0.1f;
		private Vector3 _positionSmoothVelocity;
		private Quaternion _rotationSmoothVelocity;
		protected bool _isInitialized;
		protected Vector3 SmoothPosition;
		protected Quaternion SmoothRotation;

		protected abstract Transform SetAttachedTransform();
		protected abstract bool UpdateTransform();

		public virtual void Start()
		{
			if (IsPlayerObject)
			{
				// get player objects spawned before this object (or is this one)
				// and use the closest one
				Player = QSBPlayerManager.PlayerList
					.Where(x => x.PlayerId <= netId)
					.OrderBy(x => x.PlayerId).Last();
			}

			DontDestroyOnLoad(gameObject);
			QSBSceneManager.OnSceneLoaded += OnSceneLoaded;
		}

		protected virtual void OnDestroy()
		{
			if (DestroyAttachedObject)
			{
				if (!hasAuthority && AttachedTransform != null)
				{
					Destroy(AttachedTransform.gameObject);
				}
			}

			QSBSceneManager.OnSceneLoaded -= OnSceneLoaded;
		}

		protected virtual void Init()
		{
			if (!QSBSceneManager.IsInUniverse)
			{
				DebugLog.ToConsole($"Error - {LogName} is being init-ed when not in the universe!", MessageType.Error);
			}

			if (DestroyAttachedObject)
			{
				if (!hasAuthority && AttachedTransform != null)
				{
					Destroy(AttachedTransform.gameObject);
				}
			}

			AttachedTransform = SetAttachedTransform();
			_isInitialized = true;
		}

		protected virtual void OnSceneLoaded(OWScene oldScene, OWScene newScene, bool isInUniverse) => _isInitialized = false;

		protected override void Update()
		{
			if (!_isInitialized && IsReady && _baseIsReady)
			{
				try
				{
					Init();
				}
				catch (Exception ex)
				{
					DebugLog.ToConsole($"Exception when initializing {name} : {ex}", MessageType.Error);
				}

				base.Update();
				return;
			}
			else if (_isInitialized && (!IsReady || !_baseIsReady))
			{
				_isInitialized = false;
				base.Update();
				return;
			}

			if (!_isInitialized)
			{
				base.Update();
				return;
			}

			if (AttachedTransform == null)
			{
				DebugLog.ToConsole($"Warning - AttachedObject {LogName} is null.", MessageType.Warning);
				_isInitialized = false;
				base.Update();
				return;
			}

			if (!AttachedTransform.gameObject.activeInHierarchy && !IgnoreDisabledAttachedObject)
			{
				base.Update();
				return;
			}
			else
			{
				if (ReferenceTransform != null && ReferenceTransform.position == Vector3.zero && ReferenceTransform != Locator.GetRootTransform())
				{
					DebugLog.ToConsole($"Warning - {LogName}'s ReferenceTransform is at (0,0,0). ReferenceTransform:{ReferenceTransform.name}, AttachedObject:{AttachedTransform.name}", MessageType.Warning);
				}
			}

			if (ReferenceTransform == Locator.GetRootTransform())
			{
				base.Update();
				return;
			}

			if (ReferenceTransform == null && !IgnoreNullReferenceTransform)
			{
				DebugLog.ToConsole($"Warning - {LogName}'s ReferenceTransform is null. AttachedObject:{AttachedTransform.name}", MessageType.Warning);
				base.Update();
				return;
			}

			if (!hasAuthority && UseInterpolation)
			{
				SmoothPosition = SmartSmoothDamp(SmoothPosition, transform.position);
				SmoothRotation = SmartSmoothDamp(SmoothRotation, transform.rotation);
			}

			UpdateTransform();

			base.Update();
		}

		private Vector3 SmartSmoothDamp(Vector3 currentPosition, Vector3 targetPosition)
		{
			var distance = Vector3.Distance(currentPosition, targetPosition);
			if (distance > _previousDistance + DistanceLeeway)
			{
				/*
				DebugLog.DebugWrite($"{_logName} moved too far!" +
					$"\r\n CurrentPosition:{currentPosition}," +
					$"\r\n TargetPosition:{targetPosition}");
				*/
				_previousDistance = distance;
				return targetPosition;
			}

			_previousDistance = distance;
			return Vector3.SmoothDamp(currentPosition, targetPosition, ref _positionSmoothVelocity, SmoothTime);
		}

		private Quaternion SmartSmoothDamp(Quaternion currentRotation, Quaternion targetRotation)
		{
			return QuaternionHelper.SmoothDamp(currentRotation, targetRotation, ref _rotationSmoothVelocity, SmoothTime);
		}

		public void SetReferenceTransform(Transform referenceTransform)
		{
			if (ReferenceTransform == referenceTransform)
			{
				return;
			}

			ReferenceTransform = referenceTransform;

			if (hasAuthority)
			{
				transform.position = ReferenceTransform.ToRelPos(AttachedTransform.position);
				transform.rotation = ReferenceTransform.ToRelRot(AttachedTransform.rotation);
			}
			else if (UseInterpolation)
			{
				SmoothPosition = ReferenceTransform.ToRelPos(AttachedTransform.position);
				SmoothRotation = ReferenceTransform.ToRelRot(AttachedTransform.rotation);
			}
		}

		protected virtual void OnRenderObject()
		{
			if (!QSBCore.ShowLinesInDebug
				|| !WorldObjectManager.AllObjectsReady
				|| !IsReady
				|| AttachedTransform == null
				|| ReferenceTransform == null)
			{
				return;
			}

			/* Red Cube = Where visible object should be
			 * Green cube = Where visible object is
			 * Magenta cube = Reference transform
			 * Red Line = Connection between Red Cube and Green Cube
			 * Cyan Line = Connection between Green cube and reference transform
			 */

			Popcron.Gizmos.Cube(ReferenceTransform.FromRelPos(transform.position), ReferenceTransform.FromRelRot(transform.rotation), Vector3.one / 8, Color.red);
			Popcron.Gizmos.Line(ReferenceTransform.FromRelPos(transform.position), AttachedTransform.transform.position, Color.red);
			Popcron.Gizmos.Cube(AttachedTransform.transform.position, AttachedTransform.transform.rotation, Vector3.one / 6, Color.green);
			Popcron.Gizmos.Cube(ReferenceTransform.position, ReferenceTransform.rotation, Vector3.one / 8, Color.magenta);
			Popcron.Gizmos.Line(AttachedTransform.transform.position, ReferenceTransform.position, Color.cyan);
		}

		private void OnGUI()
		{
			if (!QSBCore.ShowDebugLabels ||
				Event.current.type != EventType.Repaint)
			{
				return;
			}

			if (AttachedTransform != null)
			{
				DebugGUI.DrawLabel(AttachedTransform.transform, LogName);
			}
		}
	}
}
