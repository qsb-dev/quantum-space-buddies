using OWML.Common;
using QSB.Syncs.Unsectored.Transforms;
using QSB.Utility;
using QSB.WorldSync;
using System.Collections.Generic;
using UnityEngine;

namespace QSB.OrbSync.TransformSync
{
	internal class NomaiOrbTransformSync : UnsectoredTransformSync
	{
		public static readonly List<NomaiOrbTransformSync> Instances = new();

		public override bool IsPlayerObject => false;

		public NomaiInterfaceOrb Orb;
		private int _index => Instances.IndexOf(this);

		public override void OnStartClient() => Instances.Add(this);

		protected override void OnDestroy()
		{
			Instances.Remove(this);
			base.OnDestroy();
		}

		protected override void Init()
		{
			if (!Instances.Contains(this))
			{
				Instances.Add(this);
			}

			base.Init();

			if (AttachedObject == null)
			{
				DebugLog.ToConsole($"Error - Trying to init orb with null AttachedObject.", MessageType.Error);
				return;
			}

			var originalParent = AttachedObject.GetAttachedOWRigidbody().GetOrigParent();
			if (originalParent == Locator.GetRootTransform())
			{
				DebugLog.DebugWrite($"{LogName} with AttachedObject {AttachedObject.name} had it's original parent as SolarSystemRoot - Disabling...");
				enabled = false;
				Instances[_index] = null;
			}

			SetReferenceTransform(originalParent);
			Orb = AttachedObject.GetRequiredComponent<NomaiInterfaceOrb>();
		}

		private Transform GetTransform()
		{
			if (_index == -1)
			{
				DebugLog.ToConsole($"Error - Index cannot be found. OrbTransformSyncs count : {Instances.Count}", MessageType.Error);
				return null;
			}

			if (OrbManager.Orbs.Count <= _index)
			{
				DebugLog.ToConsole($"Error - OldOrbList does not contain index {_index}.", MessageType.Error);
				return null;
			}

			if (OrbManager.Orbs[_index] == null)
			{
				DebugLog.ToConsole($"Error - OldOrbList index {_index} is null.", MessageType.Error);
				return null;
			}

			return OrbManager.Orbs[_index].transform;
		}

		protected override bool UpdateTransform()
		{
			if (HasAuthority)
			{
				transform.position = ReferenceTransform.ToRelPos(AttachedObject.position);
			}
			else
			{
				Orb.SetTargetPosition(ReferenceTransform.FromRelPos(transform.position));
			}

			return true;
		}

		protected override Transform InitLocalTransform() => GetTransform();
		protected override Transform InitRemoteTransform() => GetTransform();

		public override bool IsReady => WorldObjectManager.AllObjectsReady;
		public override bool UseInterpolation => false;
	}
}
