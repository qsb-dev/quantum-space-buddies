using Mirror;
using NewHorizons;
using OWML.Common;
using OWML.ModHelper;
using QSB;
using QSB.Utility;
using UnityEngine;

namespace QSBNH
{
    public class QSBNH : MonoBehaviour
    {
	    public static QSBNH Instance;

	    public INewHorizons NewHorizonsAPI;

		private void Start()
	    {
			Instance = this;
			DebugLog.DebugWrite($"Start of QSB-NH compatibility code.", MessageType.Success);
			NewHorizonsAPI = QSBCore.Helper.Interaction.TryGetModApi<INewHorizons>("xen.NewHorizons");
		}

		public static string HashToMod(int hash)
		{
			foreach (var mod in NewHorizons.Main.MountedAddons)
			{
				var name = mod.ModHelper.Manifest.UniqueName;
				if (name.GetStableHashCode() == hash)
				{
					return name;
				}
			}

			return null;
		}

		public static int[] HashAddonsForSystem(string system)
		{
			if (NewHorizons.Main.BodyDict.TryGetValue(system, out var bodies))
			{
				var addonHashes = bodies
					.Where(x => x.Mod.ModHelper.Manifest.UniqueName != "xen.NewHorizons")
					.Select(x => x.Mod.ModHelper.Manifest.UniqueName.GetStableHashCode())
					.Distinct();

				var nhPlanetHashes = bodies
					.Where(x => x.Mod.ModHelper.Manifest.UniqueName == "xen.NewHorizons")
					.Select(x => x.Config.name.GetStableHashCode());

				return addonHashes.Concat(nhPlanetHashes).ToArray();
			}
			else
			{
				return null;
			}
		}
	}
}
