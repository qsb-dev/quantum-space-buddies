using OWML.Common;
using QSB.Syncs.Unsectored.Transforms;
using QSB.Utility;
using QSB.WorldSync;
using System.Collections.Generic;
using QSB.OrbSync.WorldObjects;
using UnityEngine;

namespace QSB.OrbSync.TransformSync
{
	public class NomaiOrbTransformSync : UnsectoredTransformSync
	{
		private static readonly List<NomaiOrbTransformSync> _instances = new();

		public override bool IsPlayerObject => false;

		private QSBOrb _qsbOrb;

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

		protected override void Init()
		{
			var index = _instances.IndexOf(this);
			if (!OrbManager.Orbs.TryGet(index, out var orb))
			{
				DebugLog.ToConsole($"Error - No orb at index {index}.", MessageType.Error);
				return;
			}
			_qsbOrb = QSBWorldSync.GetWorldFromUnity<QSBOrb>(orb);
			_qsbOrb.TransformSync = this;

			base.Init();

			var origParent = AttachedObject.GetAttachedOWRigidbody().GetOrigParent();
			if (origParent == Locator.GetRootTransform())
			{
				DebugLog.DebugWrite($"{LogName} with AttachedObject {AttachedObject.name} had it's original parent as SolarSystemRoot - Disabling...");
				enabled = false;
			}

			SetReferenceTransform(origParent);
		}

		protected override Transform InitLocalTransform() => _qsbOrb.AttachedObject.transform;
		protected override Transform InitRemoteTransform() => _qsbOrb.AttachedObject.transform;

		protected override float DistanceLeeway => 1f;
		public override bool IsReady => WorldObjectManager.AllObjectsAdded;
		public override bool UseInterpolation => true;
	}
}
