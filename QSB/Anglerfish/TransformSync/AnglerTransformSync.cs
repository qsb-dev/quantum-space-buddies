using QSB.Anglerfish.WorldObjects;
using QSB.Syncs.Unsectored.Rigidbodies;
using QSB.WorldSync;
using QuantumUNET.Transport;
using System.Collections.Generic;
using UnityEngine;

namespace QSB.Anglerfish.TransformSync
{
	public class AnglerTransformSync : UnsectoredRigidbodySync
	{
		public override bool IsReady => WorldObjectManager.AllAdded;
		public override bool UseInterpolation => false;

		private QSBAngler _qsbAngler;
		private static readonly List<AnglerTransformSync> _instances = new();

		protected override OWRigidbody GetRigidbody()
			=> _qsbAngler.AttachedObject._anglerBody;

		public override void Start()
		{
			_instances.Add(this);
			base.Start();
		}

		protected override void OnDestroy()
		{
			_instances.Remove(this);
			base.OnDestroy();
		}

		public override float GetNetworkSendInterval() => 1;

		protected override void Init()
		{
			_qsbAngler = QSBWorldSync.GetWorldFromId<QSBAngler>(_instances.IndexOf(this));
			_qsbAngler.TransformSync = this;

			base.Init();
			SetReferenceTransform(_qsbAngler.AttachedObject._brambleBody.transform);
		}

		private bool _shouldUpdate;

		public override void DeserializeTransform(QNetworkReader reader, bool initialState)
		{
			base.DeserializeTransform(reader, initialState);

			if (!WorldObjectManager.AllReady || HasAuthority)
			{
				return;
			}

			_shouldUpdate = true;
		}

		protected override bool UpdateTransform()
		{
			if (HasAuthority)
			{
				return base.UpdateTransform();
			}

			if (!_shouldUpdate)
			{
				return false;
			}

			_shouldUpdate = false;
			return base.UpdateTransform();
		}

		protected override void OnRenderObject()
		{
			if (!WorldObjectManager.AllReady
				|| !QSBCore.ShowLinesInDebug
				|| !IsReady
				|| ReferenceTransform == null
				|| ((OWRigidbody)AttachedObject).IsSuspended())
			{
				return;
			}

			base.OnRenderObject();

			Popcron.Gizmos.Sphere(AttachedObject.transform.position, _qsbAngler.AttachedObject._arrivalDistance, Color.blue);
			Popcron.Gizmos.Sphere(AttachedObject.transform.position, _qsbAngler.AttachedObject._pursueDistance, Color.red);
			Popcron.Gizmos.Sphere(AttachedObject.transform.position, _qsbAngler.AttachedObject._escapeDistance, Color.yellow);
			Popcron.Gizmos.Sphere(AttachedObject.transform.position
				+ AttachedObject.transform.TransformDirection(_qsbAngler.AttachedObject._mouthOffset), 3, Color.grey);

			if (_qsbAngler.TargetTransform != null)
			{
				Popcron.Gizmos.Line(_qsbAngler.TargetTransform.position, ((OWRigidbody)AttachedObject).GetPosition(), Color.gray);
				Popcron.Gizmos.Line(_qsbAngler.TargetTransform.position, _qsbAngler.TargetTransform.position + _qsbAngler.TargetVelocity, Color.green);
				Popcron.Gizmos.Line(((OWRigidbody)AttachedObject).GetPosition(), _qsbAngler.AttachedObject._targetPos, Color.red);
				Popcron.Gizmos.Sphere(_qsbAngler.AttachedObject._targetPos, 5, Color.red);
			}

			// Popcron.Gizmos.Line(AttachedObject.transform.position, _qsbAngler.AttachedObject.GetTargetPosition(), Color.white);
		}
	}
}
