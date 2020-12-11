using QSB.WorldSync;
using QuantumUNET;
using UnityEngine;

namespace QSB.TransformSync
{
	public class NomaiOrbTransformSync : QSBNetworkBehaviour
	{
		public NomaiInterfaceOrb AttachedOrb { get; private set; }
		public Transform OrbTransform { get; private set; }

		private int Index => QSBWorldSync.OrbSyncList.IndexOf(this);

		private bool _isInitialized;
		private bool _isReady;
		private Transform _orbParent;

		public override void OnStartClient()
		{
			DontDestroyOnLoad(this);
			QSBWorldSync.OrbSyncList.Add(this);

			QSB.Helper.Events.Unity.RunWhen(() => QSB.HasWokenUp, () => QSB.Helper.Events.Unity.FireOnNextUpdate(OnReady));
		}

		private void OnReady()
		{
			AttachedOrb = QSBWorldSync.OldOrbList[Index];
			_isReady = true;
		}

		private void OnDestroy()
		{
			QSBWorldSync.OrbSyncList.Remove(this);
		}

		protected void Init()
		{
			OrbTransform = AttachedOrb.transform;
			_orbParent = AttachedOrb.GetAttachedOWRigidbody().GetOrigParent();
			_isInitialized = true;
		}

		private void Update()
		{
			if (!_isInitialized && _isReady)
			{
				Init();
			}
			else if (_isInitialized && !_isReady)
			{
				_isInitialized = false;
			}

			if (OrbTransform == null || !_isInitialized)
			{
				return;
			}

			UpdateTransform();
		}

		private void UpdateTransform()
		{
			if (HasAuthority)
			{
				transform.position = _orbParent.InverseTransformPoint(OrbTransform.position);
				transform.rotation = _orbParent.InverseTransformRotation(OrbTransform.rotation);
				return;
			}
			if (transform.position != Vector3.zero)
			{
				OrbTransform.position = _orbParent.TransformPoint(transform.position);
				OrbTransform.rotation = _orbParent.InverseTransformRotation(OrbTransform.rotation);
			}
		}
	}
}