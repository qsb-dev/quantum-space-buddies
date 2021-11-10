using OWML.Common;
using QSB.Player;
using QSB.Player.TransformSync;
using QSB.Utility;
using QSB.WorldSync;
using QuantumUNET.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace QSB.Syncs
{
	/*
	 * Rewrite number : 9
	 * God has cursed me for my hubris, and my work is never finished.
	 */

	public abstract class SyncBase : QNetworkTransform
	{
		private static readonly Dictionary<uint, Dictionary<Type, SyncBase>> _storedTransformSyncs = new Dictionary<uint, Dictionary<Type, SyncBase>>();

		public static T GetPlayers<T>(PlayerInfo player)
			where T : SyncBase
		{
			var dictOfOwnedSyncs = _storedTransformSyncs[player.PlayerId];
			var wantedSync = dictOfOwnedSyncs[typeof(T)];
			if (wantedSync == default)
			{
				DebugLog.ToConsole($"Error -  _storedTransformSyncs does not contain type:{typeof(T)} under player {player.PlayerId}. Attempting to find manually...", MessageType.Error);
				var allSyncs = Resources.FindObjectsOfTypeAll<T>();
				wantedSync = allSyncs.First(x => x.Player == player);
				if (wantedSync == default)
				{
					DebugLog.ToConsole($"Error -  Could not find type:{typeof(T)} for player {player.PlayerId} manually!", MessageType.Error);
					return default;
				}
			}

			return (T)wantedSync;
		}

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
		private bool _baseIsReady => QSBPlayerManager.PlayerExists(PlayerId)
			&& Player != null
			&& Player.PlayerStates.IsReady
			&& NetId.Value != uint.MaxValue
			&& NetId.Value != 0U
			&& WorldObjectManager.AllReady;
		public abstract bool IsReady { get; }
		public abstract bool UseInterpolation { get; }
		public abstract bool IgnoreDisabledAttachedObject { get; }
		public abstract bool IgnoreNullReferenceTransform { get; }
		public abstract bool ShouldReparentAttachedObject { get; }

		public Component AttachedObject { get; set; }
		public Transform ReferenceTransform { get; set; }

		protected string _logName => $"{PlayerId}.{NetId.Value}:{GetType().Name}";
		protected virtual float DistanceLeeway { get; } = 5f;
		private float _previousDistance;
		protected const float SmoothTime = 0.1f;
		protected Vector3 _positionSmoothVelocity;
		protected Quaternion _rotationSmoothVelocity;
		protected IntermediaryTransform _intermediaryTransform;
		protected bool _isInitialized;

		protected abstract Component SetAttachedObject();
		protected abstract bool UpdateTransform();

		public virtual void Start()
		{
			var lowestBound = Resources.FindObjectsOfTypeAll<PlayerTransformSync>()
				.Where(x => x.NetId.Value <= NetId.Value).OrderBy(x => x.NetId.Value).Last();
			NetIdentity.SetRootIdentity(lowestBound.NetIdentity);

			DontDestroyOnLoad(gameObject);
			_intermediaryTransform = new IntermediaryTransform(transform);
			QSBSceneManager.OnSceneLoaded += OnSceneLoaded;

			if (Player == null)
			{
				DebugLog.ToConsole($"Error - Player in start of {_logName} was null!", MessageType.Error);
				return;
			}

			if (!_storedTransformSyncs.ContainsKey(PlayerId))
			{
				_storedTransformSyncs.Add(PlayerId, new Dictionary<Type, SyncBase>());
			}

			var playerDict = _storedTransformSyncs[PlayerId];
			playerDict[GetType()] = this;
		}

		protected virtual void OnDestroy()
		{
			if (ShouldReparentAttachedObject)
			{
				if (!HasAuthority && AttachedObject != null)
				{
					Destroy(AttachedObject.gameObject);
				}
			}

			QSBSceneManager.OnSceneLoaded -= OnSceneLoaded;

			if (!QSBPlayerManager.PlayerExists(PlayerId))
			{
				return;
			}

			var playerDict = _storedTransformSyncs[PlayerId];
			playerDict.Remove(GetType());
		}

		protected virtual void Init()
		{
			if (!QSBSceneManager.IsInUniverse)
			{
				DebugLog.ToConsole($"Error - {_logName} is being init-ed when not in the universe!", MessageType.Error);
			}

			// TODO : maybe make it's own option
			if (ShouldReparentAttachedObject)
			{
				if (!HasAuthority && AttachedObject != null)
				{
					Destroy(AttachedObject.gameObject);
				}
			}

			AttachedObject = SetAttachedObject();
			_isInitialized = true;
		}

		protected virtual void OnSceneLoaded(OWScene oldScene, OWScene newScene, bool isInUniverse) => _isInitialized = false;

		public override void Update()
		{
			if (!_isInitialized && IsReady && _baseIsReady)
			{
				Init();
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

			if (AttachedObject == null)
			{
				DebugLog.ToConsole($"Warning - AttachedObject {_logName} is null.", MessageType.Warning);
				_isInitialized = false;
				base.Update();
				return;
			}

			if (ReferenceTransform != null && ReferenceTransform.position == Vector3.zero)
			{
				DebugLog.ToConsole($"Warning - {_logName}'s ReferenceTransform is at (0,0,0). ReferenceTransform:{ReferenceTransform.name}, AttachedObject:{AttachedObject.name}", MessageType.Warning);
			}

			if (!AttachedObject.gameObject.activeInHierarchy && !IgnoreDisabledAttachedObject)
			{
				base.Update();
				return;
			}

			if (ReferenceTransform == null && !IgnoreNullReferenceTransform)
			{
				DebugLog.ToConsole($"Warning - {_logName}'s ReferenceTransform is null. AttachedObject:{AttachedObject.name}", MessageType.Warning);
				base.Update();
				return;
			}

			if (ReferenceTransform != _intermediaryTransform.GetReferenceTransform())
			{
				DebugLog.ToConsole($"Warning - {_logName}'s ReferenceTransform does not match the reference transform set for the intermediary. ReferenceTransform null : {ReferenceTransform == null}, Intermediary reference null : {_intermediaryTransform.GetReferenceTransform() == null}");
				base.Update();
				return;
			}

			if (ShouldReparentAttachedObject
				&& !HasAuthority
				&& AttachedObject.transform.parent != ReferenceTransform)
			{
				DebugLog.ToConsole($"Warning : {_logName} : AttachedObject's parent is different to ReferenceTransform. Correcting...", MessageType.Warning);
				ReparentAttachedObject(ReferenceTransform);
			}

			oh rUpdateTransform();

			base.Update();
		}

		protected Vector3 SmartSmoothDamp(Vector3 currentPosition, Vector3 targetPosition)
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

		public void SetReferenceTransform(Transform transform)
		{
			if (ReferenceTransform == transform)
			{
				return;
			}

			ReferenceTransform = transform;
			_intermediaryTransform.SetReferenceTransform(transform);

			if (ShouldReparentAttachedObject)
			{
				if (AttachedObject == null)
				{
					DebugLog.ToConsole($"Warning - AttachedObject was null for {_logName} when trying to set reference transform to {transform?.name}. Waiting until not null...", MessageType.Warning);
					QSBCore.UnityEvents.RunWhen(
						() => AttachedObject != null,
						() => ReparentAttachedObject(transform));
					return;
				}

				if (!HasAuthority)
				{
					ReparentAttachedObject(transform);
				}
			}

			if (HasAuthority)
			{
				_intermediaryTransform.EncodePosition(AttachedObject.transform.position);
				_intermediaryTransform.EncodeRotation(AttachedObject.transform.rotation);
			}
		}

		private void ReparentAttachedObject(Transform newParent)
		{
			if (AttachedObject.transform.parent != null && AttachedObject.transform.parent.GetComponent<Sector>() == null)
			{
				DebugLog.ToConsole($"Warning - Trying to reparent AttachedObject {AttachedObject.name} which wasnt attached to sector!", MessageType.Warning);
			}

			AttachedObject.transform.SetParent(newParent, true);
			AttachedObject.transform.localScale = Vector3.one;
		}

		protected virtual void OnRenderObject()
		{
			if (!QSBCore.WorldObjectsReady
				|| !QSBCore.DebugMode
				|| !QSBCore.ShowLinesInDebug
				|| !IsReady
				|| ReferenceTransform == null
				|| _intermediaryTransform.GetReferenceTransform() == null)
			{
				return;
			}

			/* Red Cube = Where visible object should be
			 * Green/Yellow Cube = Where visible object is
			 * Magenta cube = Reference transform
			 * Red Line = Connection between Red Cube and Green/Yellow Cube
			 * Cyan Line = Connection between Green/Yellow cube and reference transform
			 */

			Popcron.Gizmos.Cube(_intermediaryTransform.GetTargetPosition_Unparented(), _intermediaryTransform.GetTargetRotation_Unparented(), Vector3.one / 4, Color.red);
			Popcron.Gizmos.Line(_intermediaryTransform.GetTargetPosition_Unparented(), AttachedObject.transform.position, Color.red);
			var color = HasMoved() ? Color.green : Color.yellow;
			Popcron.Gizmos.Cube(AttachedObject.transform.position, AttachedObject.transform.rotation, Vector3.one / 4, color);
			Popcron.Gizmos.Cube(ReferenceTransform.position, ReferenceTransform.rotation, Vector3.one / 4, Color.magenta);
			Popcron.Gizmos.Line(AttachedObject.transform.position, ReferenceTransform.position, Color.cyan);
		}
	}
}
