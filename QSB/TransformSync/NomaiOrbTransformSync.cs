﻿using QSB.Utility;
using QSB.WorldSync;
using QuantumUNET;
using UnityEngine;

namespace QSB.TransformSync
{
	public class NomaiOrbTransformSync : QSBNetworkBehaviour
	{
		public NomaiInterfaceOrb AttachedOrb { get; private set; }
		public Transform OrbTransform { get; private set; }

		private int Index => WorldRegistry.OrbSyncList.IndexOf(this);

		private const int MaxUpdatesBeforeDisable = 5;

		private bool _isInitialized;
		private bool _isReady;
		private Transform _orbParent;
		private int _updateCount;

		public override void OnStartClient()
		{
			DontDestroyOnLoad(this);
			WorldRegistry.OrbSyncList.Add(this);

			QSB.Helper.Events.Unity.RunWhen(() => QSB.HasWokenUp, () => QSB.Helper.Events.Unity.FireOnNextUpdate(OnReady));
		}

		public override void OnStartAuthority()
		{
			DebugLog.DebugWrite("START AUTHORITY - has auth? : " + HasAuthority);
		}

		public override void OnStopAuthority()
		{
			DebugLog.DebugWrite("END AUTHORITY - has auth? : " + HasAuthority);
		}

		private void OnReady()
		{
			AttachedOrb = WorldRegistry.OldOrbList[Index];
			_isReady = true;
		}

		private void OnDestroy()
		{
			WorldRegistry.OrbSyncList.Remove(this);
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

		protected virtual void UpdateTransform()
		{
			if (HasAuthority)
			{
				transform.position = _orbParent.InverseTransformPoint(OrbTransform.position);
				transform.rotation = _orbParent.InverseTransformRotation(OrbTransform.rotation);
				return;
			}
			OrbTransform.position = _orbParent.TransformPoint(transform.position);
			OrbTransform.rotation = _orbParent.InverseTransformRotation(OrbTransform.rotation);

			if (transform.localPosition == Vector3.zero)
			{
				_updateCount++;
			}
			if (_updateCount >= MaxUpdatesBeforeDisable)
			{
				enabled = false;
				_updateCount = 0;
			}
		}
	}
}