using OWML.Common;
using QSB.OrbSync.TransformSync;
using QSB.OrbSync.WorldObjects;
using QSB.Utility;
using QSB.WorldSync;
using QuantumUNET;
using System.Collections.Generic;
using System.Linq;

namespace QSB.OrbSync
{
	public class OrbManager : WorldObjectManager
	{
		public static readonly List<NomaiInterfaceOrb> Orbs = new();

		protected override void RebuildWorldObjects(OWScene scene)
		{
			QSBWorldSync.Init<QSBOrbSlot, NomaiInterfaceSlot>();
			DebugLog.DebugWrite($"Finished slot build with {QSBWorldSync.GetWorldObjects<QSBOrbSlot>().Count()} slots.", MessageType.Success);
			BuildOrbs();
		}

		private static void BuildOrbs()
		{
			Orbs.Clear();
			Orbs.AddRange(QSBWorldSync.GetUnityObjects<NomaiInterfaceOrb>());
			if (QSBCore.IsHost)
			{
				NomaiOrbTransformSync.Instances.ForEach(x => QNetworkServer.Destroy(x.gameObject));
				foreach (var _ in Orbs)
				{
					Instantiate(QSBNetworkManager.Instance.OrbPrefab).SpawnWithServerAuthority();
				}
			}

			DebugLog.DebugWrite($"Finished orb build with {Orbs.Count} orbs.", MessageType.Success);
		}

		public static void HandleSlotStateChange(NomaiInterfaceSlot slot, NomaiInterfaceOrb affectingOrb, bool state)
		{
			var slotList = QSBWorldSync.GetWorldObjects<QSBOrbSlot>().ToList();
			if (!slotList.Any())
			{
				return;
			}

			var qsbSlot = slotList.FirstOrDefault(x => x.AttachedObject == slot);
			if (qsbSlot == null)
			{
				DebugLog.ToConsole($"Error - No QSBOrbSlot found for {slot.name}!", MessageType.Error);
				return;
			}

			var orbSync = NomaiOrbTransformSync.Instances.Where(x => x != null).FirstOrDefault(x => x.AttachedObject == affectingOrb.transform);
			if (orbSync == null)
			{
				DebugLog.ToConsole($"Error - No NomaiOrbTransformSync found for {affectingOrb.name} (For slot {slot.name})!", MessageType.Error);
				return;
			}

			if (orbSync.HasAuthority)
			{
				qsbSlot.HandleEvent(state, Orbs.IndexOf(affectingOrb));
			}
		}
	}
}
