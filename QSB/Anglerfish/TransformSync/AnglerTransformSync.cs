using QSB.Anglerfish.WorldObjects;
using QSB.Syncs.Unsectored.Rigidbodies;
using QSB.WorldSync;
using System.Collections.Generic;
using UnityEngine;

namespace QSB.Anglerfish.TransformSync
{
	public class AnglerTransformSync : UnsectoredRigidbodySync
	{
		public override bool IsReady => QSBCore.WorldObjectsReady;
		public override bool UseInterpolation => true;
		protected override OWRigidbody GetRigidbody() => _qsbAngler.AttachedObject._anglerBody;

		private QSBAngler _qsbAngler;
		private static readonly List<AnglerTransformSync> _instances = new List<AnglerTransformSync>();
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

		public override float GetNetworkSendInterval() => 1 / 4f;

		protected override void Init()
		{
			_qsbAngler = QSBWorldSync.GetWorldFromId<QSBAngler>(_instances.IndexOf(this));
			_qsbAngler.transformSync = this;

			base.Init();
			SetReferenceTransform(_qsbAngler.AttachedObject._brambleBody.transform);
		}


		protected override void OnRenderObject()
		{
			base.OnRenderObject();

			if (!QSBCore.WorldObjectsReady
				|| !QSBCore.DebugMode
				|| !QSBCore.ShowLinesInDebug
				|| !IsReady
				|| ReferenceTransform == null
				|| _intermediaryTransform.GetReferenceTransform() == null)
			{
				return;
			}

			Popcron.Gizmos.Line(AttachedObject.transform.position, _qsbAngler.AttachedObject.GetTargetPosition(), Color.white);
		}
	}
}
