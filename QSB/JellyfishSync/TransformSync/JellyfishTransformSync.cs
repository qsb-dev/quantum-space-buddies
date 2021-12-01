using System.Collections.Generic;
using QSB.JellyfishSync.WorldObjects;
using QSB.Syncs.Unsectored.Rigidbodies;
using QSB.WorldSync;
using QuantumUNET.Transport;

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

		private static int _nextId;
		private int _id;

		public override void Start()
		{
			_id = _nextId++;
			base.Start();
		}

		protected override void OnDestroy()
		{
			_nextId--;
			base.OnDestroy();
		}

		public override float GetNetworkSendInterval() => 1;

		protected override void Init()
		{
			_qsbJellyfish = QSBWorldSync.GetWorldFromId<QSBJellyfish>(_id);
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
	}
}
