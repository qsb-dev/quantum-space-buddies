using Mirror;
using OWML.Common;
using QSB.Player;
using QSB.Utility;
using QSB.WorldSync;
using System;
using System.Linq;
using UnityEngine;

/*
* Rewrite number : 11
* God has cursed me for my hubris, and my work is never finished.
*/

namespace QSB.Syncs;

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

		if (!QSBWorldSync.AllObjectsReady)
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

	protected override bool CheckValid()
	{
		if (!IsInitialized)
		{
			return false;
		}

		if (!base.CheckValid())
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

		return true;
	}

	protected abstract bool UseInterpolation { get; }
	protected virtual bool AllowInactiveAttachedObject => false;
	protected abstract bool AllowNullReferenceTransform { get; }
	protected virtual bool IsPlayerObject => false;

	public Transform AttachedTransform { get; private set; }
	public Transform ReferenceTransform { get; private set; }

	public string Name => AttachedTransform ? AttachedTransform.name : "<NullObject!>";

	public override string ToString() => (IsPlayerObject ? $"{Player.PlayerId}." : string.Empty)
										 + $"{netId}:{GetType().Name} ({Name})";

	protected virtual float DistanceChangeThreshold => 5f;
	private float _prevDistance;
	private const float SmoothTime = 0.1f;
	private Vector3 _positionSmoothVelocity;
	private Quaternion _rotationSmoothVelocity;
	protected Vector3 SmoothPosition { get; private set; }
	protected Quaternion SmoothRotation { get; private set; }
	private bool _interpolating;

	protected abstract Transform InitAttachedTransform();

	public override void OnStartClient()
	{
		base.OnStartClient();
		if (IsPlayerObject)
		{
			// get player objects spawned before this object (or is this one)
			// and use the closest one
			_player = QSBPlayerManager.PlayerList
				.Where(x => x.PlayerId <= netId)
				.MaxBy(x => x.PlayerId);
		}

		QSBSceneManager.OnPreSceneLoad += OnPreSceneLoad;
	}

	public override void OnStopClient()
	{
		base.OnStopClient();
		QSBSceneManager.OnPreSceneLoad -= OnPreSceneLoad;
		if (IsInitialized)
		{
			SafeUninit();
		}
	}

	private void OnPreSceneLoad(OWScene oldScene, OWScene newScene)
	{
		if (IsInitialized)
		{
			SafeUninit();
		}
	}

	private const float _pauseTimerDelay = 10;
	private float _pauseTimer;

	private void SafeInit()
	{
		this.Try("initializing", () =>
		{
			Init();
			IsInitialized = true;
		});
		if (!IsInitialized)
		{
			_pauseTimer = _pauseTimerDelay;
		}
	}

	private void SafeUninit()
	{
		this.Try("uninitializing", () =>
		{
			Uninit();
			IsInitialized = false;
		});
		if (IsInitialized)
		{
			_pauseTimer = _pauseTimerDelay;
		}
	}

	protected virtual void Init() =>
		AttachedTransform = InitAttachedTransform();

	protected virtual void Uninit()
	{
		if (IsPlayerObject && !hasAuthority && AttachedTransform)
		{
			Destroy(AttachedTransform.gameObject);
		}
	}

	/// <summary>
	/// call the base method FIRST
	/// </summary>
	protected override bool HasChanged()
	{
		GetFromAttached();
		if (UseInterpolation)
		{
			SmoothPosition = transform.position;
			SmoothRotation = transform.rotation;
		}

		return base.HasChanged();
	}

	/// <summary>
	/// called right before checking HasChanged
	/// </summary>
	protected abstract void GetFromAttached();

	/// <summary>
	/// call the base method LAST
	/// </summary>
	protected override void Deserialize(NetworkReader reader)
	{
		base.Deserialize(reader);
		if (UseInterpolation)
		{
			_interpolating = true;
		}

		ApplyToAttached();
	}

	/// <summary>
	/// called right after deserializing
	/// </summary>
	protected abstract void ApplyToAttached();

	protected sealed override void Update()
	{
		if (_pauseTimer > 0)
		{
			_pauseTimer -= Time.unscaledDeltaTime;
			return;
		}

		if (!IsInitialized && CheckReady())
		{
			SafeInit();
		}
		else if (IsInitialized && !CheckReady())
		{
			SafeUninit();
		}

		base.Update();

		if (IsValid && _interpolating)
		{
			if (hasAuthority)
			{
				_interpolating = false;
				return;
			}

			Interpolate();
			ApplyToAttached();
		}
	}

	private void Interpolate()
	{
		var distance = Vector3.Distance(SmoothPosition, transform.position);
		var angle = Quaternion.Angle(SmoothRotation, transform.rotation);
		if (Mathf.Abs(distance - _prevDistance) > DistanceChangeThreshold ||
			distance < .001f && angle < .001f)
		{
			SmoothPosition = transform.position;
			SmoothRotation = transform.rotation;
			_interpolating = false;
		}
		else
		{
			SmoothPosition = Vector3.SmoothDamp(SmoothPosition, transform.position, ref _positionSmoothVelocity, SmoothTime);
			SmoothRotation = QuaternionHelper.SmoothDamp(SmoothRotation, transform.rotation, ref _rotationSmoothVelocity, SmoothTime);
		}

		_prevDistance = distance;
	}

	public virtual void SetReferenceTransform(Transform referenceTransform)
	{
		if (ReferenceTransform == referenceTransform)
		{
			return;
		}

		ReferenceTransform = referenceTransform;
		if (IsPlayerObject && !hasAuthority && AttachedTransform)
		{
			AttachedTransform.parent = ReferenceTransform;
			AttachedTransform.localScale = Vector3.one;
		}

		if (UseInterpolation && !hasAuthority && AttachedTransform && ReferenceTransform)
		{
			SmoothPosition = ReferenceTransform.ToRelPos(AttachedTransform.position);
			SmoothRotation = ReferenceTransform.ToRelRot(AttachedTransform.rotation);
		}
	}

	protected virtual void OnRenderObject()
	{
		if (!QSBCore.DebugSettings.DrawLines
			|| !IsValid
			|| !ReferenceTransform)
		{
			return;
		}

		/*
		 * Red Cube = Where visible object should be
		 * Green cube = Where visible object is
		 * Red Line = Connection between Red Cube and Green Cube
		 * Magenta cube = Reference transform
		 * Cyan Line = Connection between Green cube and Magenta cube
		 */

		var colorMul = _interpolating ? .5f : 1;
		Popcron.Gizmos.Cube(ReferenceTransform.FromRelPos(transform.position), ReferenceTransform.FromRelRot(transform.rotation), Vector3.one / 8, Color.red * colorMul);
		Popcron.Gizmos.Cube(AttachedTransform.position, AttachedTransform.rotation, Vector3.one / 6, Color.green * colorMul);
		Popcron.Gizmos.Line(ReferenceTransform.FromRelPos(transform.position), AttachedTransform.position, Color.red * colorMul);
		Popcron.Gizmos.Cube(ReferenceTransform.position, ReferenceTransform.rotation, Vector3.one / 8, Color.magenta * colorMul);
		Popcron.Gizmos.Line(AttachedTransform.position, ReferenceTransform.position, Color.cyan * colorMul);
	}

	private void OnGUI()
	{
		if (!QSBCore.DebugSettings.DrawLabels
			|| Event.current.type != EventType.Repaint
			|| !IsValid
			|| !ReferenceTransform)
		{
			return;
		}

		DebugGUI.DrawLabel(AttachedTransform, ToString());
	}
}