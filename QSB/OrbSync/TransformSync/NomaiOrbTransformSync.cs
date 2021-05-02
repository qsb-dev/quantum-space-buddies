using QSB.TransformSync;
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
			OrbTransformSyncs.Add(this);

			QSBCore.UnityEvents.RunWhen(() => QSBCore.HasWokenUp, () => QSBCore.UnityEvents.FireOnNextUpdate(OnReady));
		}

		private void OnReady()
		{
			if (QSBWorldSync.OldOrbList == null || QSBWorldSync.OldOrbList.Count <= _index)
			{
				DebugLog.ToConsole($"Error - OldOrbList is null or does not contain index {_index}.", OWML.Common.MessageType.Error);
				return;
			}
			_isReady = true;
		}

		protected override void OnDestroy()
		{
			OrbTransformSyncs.Remove(this);
			QSBSceneManager.OnSceneLoaded -= OnSceneLoaded;
		}

		protected override void Init()
		{
			base.Init();
			SetReferenceTransform(AttachedObject.GetAttachedOWRigidbody().GetOrigParent());
		}

		protected override GameObject InitLocalTransform() => QSBWorldSync.OldOrbList[_index].gameObject;
		protected override GameObject InitRemoteTransform() => QSBWorldSync.OldOrbList[_index].gameObject;

		public override bool IsReady => _isReady;
		public override bool UseInterpolation => false;
	}
}
