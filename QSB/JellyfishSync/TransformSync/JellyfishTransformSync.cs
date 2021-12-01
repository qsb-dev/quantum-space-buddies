using System.Collections.Generic;
using QSB.JellyfishSync.WorldObjects;
using QSB.Syncs.Unsectored.Rigidbodies;
using QSB.WorldSync;
using QuantumUNET.Transport;
using UnityEngine;

namespace QSB.JellyfishSync.TransformSync
{
	public class JellyfishTransformSync : UnsectoredRigidbodySync
	{
		public override bool IsReady => QSBCore.WorldObjectsReady;
		public override bool UseInterpolation => false;

		private QSBJellyfish _qsbJellyfish;
		private static readonly List<JellyfishTransformSync> _instances = new();

		protected override OWRigidbody GetRigidbody()
			=> _qsbJellyfish.AttachedObject._jellyfishBody;

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

		public override float GetNetworkSendInterval() => 10;

		protected override void Init()
		{
			_qsbJellyfish = QSBWorldSync.GetWorldFromId<QSBJellyfish>(_instances.IndexOf(this));
			_qsbJellyfish.TransformSync = this;

			base.Init();
			SetReferenceTransform(_qsbJellyfish.AttachedObject._planetBody.transform);
		}

		private bool _shouldUpdate;

		public override void DeserializeTransform(QNetworkReader reader, bool initialState)
		{
			base.DeserializeTransform(reader, initialState);
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
			base.OnRenderObject();

			if (!QSBCore.WorldObjectsReady
				|| !QSBCore.ShowLinesInDebug
				|| !IsReady
				|| ReferenceTransform == null
				|| ((OWRigidbody)AttachedObject).IsSuspended())
			{
				return;
			}

			var jellyfish = _qsbJellyfish.AttachedObject;
			var position = ReferenceTransform.position;
			var dir = Vector3.Normalize(jellyfish.transform.position - position);
			Popcron.Gizmos.Line(position + dir * jellyfish._lowerLimit, position + dir * jellyfish._upperLimit, Color.magenta);
			Popcron.Gizmos.Sphere(position + dir * jellyfish._lowerLimit, 10f, Color.magenta);
			Popcron.Gizmos.Sphere(position + dir * jellyfish._upperLimit, 10f, Color.magenta);
		}
	}
}
