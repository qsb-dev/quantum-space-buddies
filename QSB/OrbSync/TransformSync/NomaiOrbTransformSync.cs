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
		public static List<NomaiOrbTransformSync> OrbTransformSyncs = new();

		private int _index => OrbTransformSyncs.IndexOf(this);

		public override void OnStartClient() => OrbTransformSyncs.Add(this);

		protected override void OnDestroy()
		{
			OrbTransformSyncs.Remove(this);
			base.OnDestroy();
		}

		protected override void Init()
		{
			if (!OrbTransformSyncs.Contains(this))
			{
				OrbTransformSyncs.Add(this);
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
				OrbTransformSyncs[_index] = null;
			}

			SetReferenceTransform(originalParent);
		}

		private Transform GetTransform()
		{
			if (_index == -1)
			{
				DebugLog.ToConsole($"Error - Index cannot be found. OrbTransformSyncs count : {OrbTransformSyncs.Count}", MessageType.Error);
				return null;
			}

			if (QSBWorldSync.OldOrbList == null || QSBWorldSync.OldOrbList.Count <= _index)
			{
				DebugLog.ToConsole($"Error - OldOrbList is null or does not contain index {_index}.", MessageType.Error);
				return null;
			}

			if (QSBWorldSync.OldOrbList[_index] == null)
			{
				DebugLog.ToConsole($"Error - OldOrbList index {_index} is null.", MessageType.Error);
				return null;
			}

			return QSBWorldSync.OldOrbList[_index].transform;
		}

		protected override Component InitLocalTransform() => GetTransform();
		protected override Component InitRemoteTransform() => GetTransform();

		protected override float DistanceLeeway => 1f;
		public override bool IsReady => WorldObjectManager.AllReady;
		public override bool UseInterpolation => false;
	}
}
