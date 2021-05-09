using QSB.Syncs.TransformSync;
using QSB.Utility;
using QSB.WorldSync;
using System.Collections.Generic;
using UnityEngine;

namespace QSB.OrbSync.TransformSync
{
	internal class NomaiOrbTransformSync : UnparentedBaseTransformSync
	{
		public static List<NomaiOrbTransformSync> OrbTransformSyncs = new List<NomaiOrbTransformSync>();

		private int _index => OrbTransformSyncs.IndexOf(this);
		private bool _isReady;

		public override void OnStartClient()
		{
			QSBSceneManager.OnSceneLoaded += (OWScene scene, bool inUniverse) => _isReady = false;
			OrbTransformSyncs.Add(this);
		}

		protected override void OnDestroy()
		{
			QSBSceneManager.OnSceneLoaded -= (OWScene scene, bool inUniverse) => _isReady = false;
			OrbTransformSyncs.Remove(this);
			QSBSceneManager.OnSceneLoaded -= OnSceneLoaded;
		}

		protected override void Init()
		{
			base.Init();
			SetReferenceTransform(AttachedObject.GetAttachedOWRigidbody().GetOrigParent());
		}

		private GameObject GetTransform()
		{
			if (QSBWorldSync.OldOrbList == null || QSBWorldSync.OldOrbList.Count <= _index)
			{
				DebugLog.ToConsole($"Error - OldOrbList is null or does not contain index {_index}.", OWML.Common.MessageType.Error);
				return null;
			}
			if (QSBWorldSync.OldOrbList[_index] == null)
			{
				DebugLog.ToConsole($"Error - OldOrbList index {_index} is null.", OWML.Common.MessageType.Error);
				return null;
			}
			return QSBWorldSync.OldOrbList[_index].gameObject;
		}

		protected override GameObject InitLocalTransform() => GetTransform();
		protected override GameObject InitRemoteTransform() => GetTransform();

		public override bool IsReady => QSBCore.WorldObjectsReady;
		public override bool UseInterpolation => false;
	}
}
