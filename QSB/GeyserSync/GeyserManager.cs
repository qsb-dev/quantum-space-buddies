using QSB.GeyserSync.WorldObjects;
using QSB.Patches;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.GeyserSync
{
	public class GeyserManager : WorldObjectManager
	{
		public override void Awake()
		{
			base.Awake();
			QSBPatchManager.OnPatchType += OnPatchType;
		}

		public override void OnDestroy()
		{
			base.OnDestroy();
			QSBPatchManager.OnPatchType -= OnPatchType;
		}

		protected override void RebuildWorldObjects(OWScene scene)
			=> QSBWorldSync.Init<QSBGeyser, GeyserController>();

		public void OnPatchType(QSBPatchTypes type)
		{
			if (type != QSBPatchTypes.OnNonServerClientConnect)
			{
				return;
			}
			QSBCore.HarmonyHelper.EmptyMethod<GeyserController>("Update");
		}
	}
}