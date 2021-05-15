using OWML.Common;
using QSB.Syncs.TransformSync;
using QSB.Utility;
using QuantumUNET.Components;
using QuantumUNET.Transport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace QSB.Syncs.RigidbodySync
{
	public abstract class UnparentedBaseRigidbodySync : QNetworkTransform, ISync<OWRigidbody>
	{
		public Transform ReferenceTransform { get; set; }
		public OWRigidbody AttachedObject { get; set; }

		public abstract bool IsReady { get; }
		public abstract bool UseInterpolation { get; }

		protected bool _isInitialized;
		protected IntermediaryTransform _intermediaryTransform;
		protected Vector3 _velocity;
		protected Vector3 _angularVelocity;
		private string _logName => $"{NetId}:{GetType().Name}";

		protected abstract OWRigidbody GetRigidbody();

		public virtual void Start()
		{
			DontDestroyOnLoad(gameObject);
			_intermediaryTransform = new IntermediaryTransform(transform);
			QSBSceneManager.OnSceneLoaded += OnSceneLoaded;
		}

		protected virtual void OnDestroy()
		{
			QSBSceneManager.OnSceneLoaded -= OnSceneLoaded;
		}

		private void OnSceneLoaded(OWScene scene, bool isInUniverse)
			=> _isInitialized = false;

		protected virtual void Init()
		{
			if (!QSBSceneManager.IsInUniverse)
			{
				DebugLog.ToConsole($"Error - {_logName} is being init-ed when not in the universe!", MessageType.Error);
			}
			AttachedObject = GetRigidbody();
			_isInitialized = true;
		}

		public override void SerializeTransform(QNetworkWriter writer)
		{
			if (_intermediaryTransform == null)
			{
				_intermediaryTransform = new IntermediaryTransform(transform);
			}

			/* We need to send :
			 * - Position
			 * - Rotation
			 * - Velocity
			 * - Angular velocity
			 * We can't store the last two on the IntermediaryTransform, so they come from fields.
			 */

			// Get world position from IT.
			
			// Get world rotation from IT.

			// Get velocity from field.

			// Get angular velocity from field.

			// Send all.

			// Set _prev fields.

			var worldPos = _intermediaryTransform.GetPosition();
			var worldRot = _intermediaryTransform.GetRotation();
			var velocity = _velocity;
			var angularVelocity = _angularVelocity;

			writer.Write(worldPos);
			SerializeRotation(writer, worldRot);
			writer.Write(velocity);
			writer.Write(angularVelocity);
			_prevPosition = worldPos;
			_prevRotation = worldRot;
		}

		public override void DeserializeTransform(QNetworkReader reader)
		{
			if (!QSBCore.WorldObjectsReady)
			{
				reader.ReadVector3();
				DeserializeRotation(reader);
				reader.ReadVector3();
				reader.ReadVector3();
				return;
			}

			var pos = reader.ReadVector3();
			var rot = DeserializeRotation(reader);
			var vel = reader.ReadVector3();
			var angVel = reader.ReadVector3();

			if (HasAuthority)
			{
				return;
			}

			if (_intermediaryTransform == null)
			{
				_intermediaryTransform = new IntermediaryTransform(transform);
			}

			_intermediaryTransform.SetPosition(pos);
			_intermediaryTransform.SetRotation(rot);
			_velocity = vel;
			_angularVelocity = angVel;

			if (_intermediaryTransform.GetPosition() == Vector3.zero)
			{
				DebugLog.ToConsole($"Warning - {_logName} at (0,0,0)! - Given position was {pos}", MessageType.Warning);
			}
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
				DebugLog.ToConsole($"Warning - AttachedRigidbody {_logName} is null.", MessageType.Warning);
				return;
			}

			if (!AttachedObject.gameObject.activeInHierarchy)
			{
				return;
			}

			UpdateTransform();

			base.Update();
		}

		protected virtual void UpdateTransform()
		{
			if (HasAuthority)
			{
				_intermediaryTransform.EncodePosition(AttachedObject.transform.position);
				_intermediaryTransform.EncodeRotation(AttachedObject.transform.rotation);
				_velocity = AttachedObject.GetRelativeVelocity(ReferenceTransform.GetAttachedOWRigidbody());
				_angularVelocity = AttachedObject.GetRelativeAngularVelocity(ReferenceTransform.GetAttachedOWRigidbody());
				return;
			}

			var targetPos = _intermediaryTransform.GetTargetPosition_Unparented();
			var targetRot = _intermediaryTransform.GetTargetRotation_Unparented();

			if (targetPos == Vector3.zero || _intermediaryTransform.GetTargetPosition_ParentedToReference() == Vector3.zero)
			{
				return;
			}

			AttachedObject.SetPosition(targetPos);
			AttachedObject.SetRotation(targetRot);
			AttachedObject.SetVelocity(ReferenceTransform.GetAttachedOWRigidbody().GetVelocity() + _velocity);
			AttachedObject.SetAngularVelocity(ReferenceTransform.GetAttachedOWRigidbody().GetAngularVelocity() + _angularVelocity);
		}

		public void SetReferenceTransform(Transform transform)
		{
			if (ReferenceTransform == transform)
			{
				return;
			}
			ReferenceTransform = transform;
			_intermediaryTransform.SetReferenceTransform(transform);
		}

		private void OnRenderObject()
		{
			if (!QSBCore.WorldObjectsReady || !QSBCore.DebugMode || !QSBCore.ShowLinesInDebug || !IsReady)
			{
				return;
			}

			Popcron.Gizmos.Cube(_intermediaryTransform.GetTargetPosition_Unparented(), _intermediaryTransform.GetTargetRotation_Unparented(), Vector3.one / 2, Color.red);
			Popcron.Gizmos.Line(_intermediaryTransform.GetTargetPosition_Unparented(), AttachedObject.transform.position, Color.red);
			var color = HasMoved() ? Color.green : Color.yellow;
			Popcron.Gizmos.Cube(AttachedObject.transform.position, AttachedObject.transform.rotation, Vector3.one / 2, color);
			Popcron.Gizmos.Line(AttachedObject.transform.position, ReferenceTransform.position, Color.cyan);
		}
	}
}
