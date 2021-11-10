using System.Collections.Generic;
using QSB.Anglerfish.WorldObjects;
using QSB.Syncs.Unsectored.Rigidbodies;
using QSB.WorldSync;

namespace QSB.Anglerfish.TransformSync
{
	public class AnglerTransformSync : UnsectoredRigidbodySync
	{
		public override bool IsReady => QSBCore.WorldObjectsReady;
		public override bool UseInterpolation => true;
		protected override OWRigidbody GetRigidbody() => qsbAngler.AttachedObject._anglerBody;

		public QSBAngler qsbAngler;
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

		public override float GetNetworkSendInterval() => 1;

		protected override void Init()
		{
			qsbAngler = QSBWorldSync.GetWorldFromId<QSBAngler>(_instances.IndexOf(this));
			qsbAngler.transformSync = this;

			base.Init();
			SetReferenceTransform(qsbAngler.AttachedObject._brambleBody._transform);
		}
	}
}
