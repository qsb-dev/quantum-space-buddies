using Mirror;
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
	 * Rewrite number : 11
	 * God has cursed me for my hubris, and my work is never finished.
	 */

	public abstract class SyncBase : QSBNetworkTransform
	{
		/// <summary>
		/// valid if IsPlayerObject, otherwise null
		/// </summary>
		public PlayerInfo Player
		{
			get
			{
				if (_player == null)
				{
					DebugLog.ToConsole($"Error - trying to get SyncBase.Player for {netId} before Start has been called! "
						+ "this really should not be happening!\n"
						+ Environment.StackTrace,
						MessageType.Error);
				}

				return _player;
			}
		}
		private PlayerInfo _player;

		private bool IsInitialized;

		protected virtual bool CheckReady()
		{
			if (netId is uint.MaxValue or 0)
			{
				return false;
			}

			if (!QSBWorldSync.AllObjectsAdded)
			{
				return false;
			}

			if (IsPlayerObject)
			{
				if (_player == null)
				{
					return false;
				}

				if (!isLocalPlayer && !_player.IsReady)
				{
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// can be true with null reference transform. <br/>
		/// can be true with inactive attached object.
		/// </summary>
		public bool IsValid { get; private set; }

		protected virtual bool CheckValid()
		{
			if (!IsInitialized)
			{
				return false;
			}

			if (!AttachedTransform)
			{
				DebugLog.ToConsole($"Error - AttachedObject {this} is null!", MessageType.Error);
				return false;
			}

			if (!AllowInactiveAttachedObject && !AttachedTransform.gameObject.activeInHierarchy)
			{
				return false;
			}

			if (!AllowNullReferenceTransform && !ReferenceTransform)
			{
				DebugLog.ToConsole($"Warning - {this}'s ReferenceTransform is null.", MessageType.Warning);
				return false;
			}

			if (ReferenceTransform == Locator.GetRootTransform())
			{
				return false;
			}

			return true;
		}

		protected abstract bool UseInterpolation { get; }
		protected virtual bool AllowInactiveAttachedObject => false;
		protected abstract bool AllowNullReferenceTransform { get; }
		protected virtual bool IsPlayerObject => false;
		protected virtual bool OnlyApplyOnDeserialize => false;

		public Transform AttachedTransform { get; private set; }
		public Transform ReferenceTransform { get; private set; }

		public string Name => AttachedTransform ? AttachedTransform.name : "<NullObject!>";

		public override string ToString() => (IsPlayerObject ? $"{Player.PlayerId}." : string.Empty)
			+ $"{netId}:{GetType().Name} ({Name})";

		protected virtual float DistanceLeeway => 5f;
		private float _previousDistance;
		protected const float SmoothTime = 0.1f;
		private Vector3 _positionSmoothVelocity;
		private Quaternion _rotationSmoothVelocity;
		protected Vector3 SmoothPosition { get; private set; }
		protected Quaternion SmoothRotation { get; private set; }

		protected abstract Transform InitAttachedTransform();
		protected abstract void GetFromAttached();
		protected abstract void ApplyToAttached();

		public override void OnStartClient()
		{
			if (IsPlayerObject)
			{
				// get player objects spawned before this object (or is this one)
				// and use the closest one
				_player = QSBPlayerManager.PlayerList
					.Where(x => x.PlayerId <= netId)
					.OrderBy(x => x.PlayerId).Last();
			}

			DontDestroyOnLoad(gameObject);
			QSBSceneManager.OnSceneLoaded += OnSceneLoaded;
		}

		public override void OnStopClient()
		{
			QSBSceneManager.OnSceneLoaded -= OnSceneLoaded;
			if (IsInitialized)
			{
				this.Try("uninitializing (from object destroy)", Uninit);
			}
		}

		private void OnSceneLoaded(OWScene oldScene, OWScene newScene, bool isInUniverse)
		{
			if (IsInitialized)
			{
				this.Try("uninitializing (from scene change)", Uninit);
			}
		}

		protected virtual void Init()
		{
			AttachedTransform = InitAttachedTransform();
			IsInitialized = true;
		}

		protected virtual void Uninit()
		{
			if (IsPlayerObject && !hasAuthority && AttachedTransform)
			{
				Destroy(AttachedTransform.gameObject);
			}

			AttachedTransform = null;
			ReferenceTransform = null;
			IsInitialized = false;
			IsValid = false;
		}

		private bool _shouldApply;

		protected override void Deserialize(NetworkReader reader, bool initialState)
		{
			base.Deserialize(reader, initialState);
			if (OnlyApplyOnDeserialize)
			{
				_shouldApply = true;
			}
		}

		protected sealed override void Update()
		{
			if (!IsInitialized && CheckReady())
			{
				this.Try("initializing", Init);
			}
			else if (IsInitialized && !CheckReady())
			{
				this.Try("uninitializing", Uninit);
				base.Update();
				return;
			}

			IsValid = CheckValid();
			if (!IsValid)
			{
				base.Update();
				return;
			}

			if (ReferenceTransform && ReferenceTransform.position == Vector3.zero)
			{
				DebugLog.ToConsole($"Warning - {this}'s ReferenceTransform is at (0,0,0). ReferenceTransform:{ReferenceTransform.name}", MessageType.Warning);
			}

			if (!hasAuthority && UseInterpolation)
			{
				SmoothPosition = SmartSmoothDamp(SmoothPosition, transform.position);
				SmoothRotation = QuaternionHelper.SmoothDamp(SmoothRotation, transform.rotation, ref _rotationSmoothVelocity, SmoothTime);
			}

			if (hasAuthority)
			{
				GetFromAttached();
			}
			else if (!OnlyApplyOnDeserialize || _shouldApply)
			{
				_shouldApply = false;
				ApplyToAttached();
			}

			base.Update();
		}

		private Vector3 SmartSmoothDamp(Vector3 currentPosition, Vector3 targetPosition)
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

		public void SetReferenceTransform(Transform referenceTransform)
		{
			if (ReferenceTransform == referenceTransform)
			{
				return;
			}

			ReferenceTransform = referenceTransform;

			if (!hasAuthority && UseInterpolation)
			{
				if (IsPlayerObject)
				{
					AttachedTransform.SetParent(ReferenceTransform, true);
					SmoothPosition = AttachedTransform.localPosition;
					SmoothRotation = AttachedTransform.localRotation;
				}
				else
				{
					SmoothPosition = ReferenceTransform.ToRelPos(AttachedTransform.position);
					SmoothRotation = ReferenceTransform.ToRelRot(AttachedTransform.rotation);
				}
			}
		}

		protected virtual void OnRenderObject()
		{
			if (!QSBCore.ShowLinesInDebug
				|| !IsValid
				|| !ReferenceTransform)
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
			if (!QSBCore.ShowDebugLabels
				|| Event.current.type != EventType.Repaint
				|| !IsValid
				|| !ReferenceTransform)
			{
				return;
			}

			DebugGUI.DrawLabel(AttachedTransform.transform, ToString());
		}
	}
}
