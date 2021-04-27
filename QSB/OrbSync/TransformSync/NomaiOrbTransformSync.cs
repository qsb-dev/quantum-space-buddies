using QSB.Utility;
using QSB.WorldSync;
using QuantumUNET;
using UnityEngine;

namespace QSB.OrbSync.TransformSync
{
	public class NomaiOrbTransformSync : QNetworkBehaviour
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

			QSBCore.UnityEvents.RunWhen(() => QSBCore.HasWokenUp, () => QSBCore.UnityEvents.FireOnNextUpdate(OnReady));
		}

		private void OnReady()
		{
			if (QSBWorldSync.OldOrbList == null || QSBWorldSync.OldOrbList.Count < Index)
			{
				DebugLog.ToConsole($"Error - OldOrbList is null or does not contain index {Index}.", OWML.Common.MessageType.Error);
				return;
			}
			AttachedOrb = QSBWorldSync.OldOrbList[Index];
			_isReady = true;
		}

		public void OnDestroy()
		{
			DebugLog.DebugWrite($"OnDestroy - parented to {gameObject.transform.parent.name}");
			QSBWorldSync.OrbSyncList.Remove(this);
		}

		protected void Init()
		{
			OrbTransform = AttachedOrb.transform;
			_orbParent = AttachedOrb.GetAttachedOWRigidbody().GetOrigParent();
			_isInitialized = true;
		}

		public void Update()
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
				transform.rotation = OrbTransform.rotation;
				return;
			}
			if (transform.position != Vector3.zero)
			{
				OrbTransform.position = _orbParent.TransformPoint(transform.position);
				OrbTransform.rotation = transform.rotation;
			}
		}

		private void OnRenderObject()
		{
			if (!QSBCore.HasWokenUp || !QSBCore.DebugMode || !QSBCore.ShowLinesInDebug)
			{
				return;
			}

			Popcron.Gizmos.Cube(OrbTransform.position, OrbTransform.rotation, Vector3.one / 2, Color.blue);
			Popcron.Gizmos.Cube(_orbParent.TransformPoint(transform.position), transform.rotation, Vector3.one / 2, Color.red);
		}
	}
}